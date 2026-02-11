using MCGalaxy.Maths;
using Newtonsoft.Json;
using System;
using System.Text;
namespace MCGalaxy
{
    public enum NASDamageSource 
    {
        Falling, Suffocating, Drowning, Entity, None, Murder
    }
    public partial class NASEntity
    {
        [JsonIgnore] public float AirPrev;
        [JsonIgnore] public NASLevel nl;
        [JsonIgnore] public AABB bounds = AABB.Make(new(0, 0, 0), new(16, 26 * 2, 16));
        [JsonIgnore] public AABB eyeBounds = AABB.Make(new(0, 24 * 2 - 2, 0), new(4, 4, 4));
        [JsonIgnore] public DateTime lastSuffocationDate = DateTime.MinValue;
        public float HP, Air;
        public bool holdingBreath = false;
        public string levelName;
        public Vec3S32 location, lastGroundedLocation;
        public byte yaw, pitch;
        public static void SetLocation(NASEntity ne, string levelName, Position pos, Orientation rot)
        {
            ne.levelName = levelName;
            ne.location.X = pos.X;
            ne.location.Y = pos.Y;
            ne.location.Z = pos.Z;
            ne.yaw = rot.RotY;
            ne.pitch = rot.HeadX;
        }
        public virtual void ChangeHealth(float diff)
        {
            HP += diff;
            if (HP < 0)
            {
                HP = 0;
            }
            if (HP > 10)
            {
                HP = 10;
            }
        }
        public string OxygenString()
        {
            if (Air == 10)
            {
                return "";
            }
            if (Air == 0)
            {
                return "&r?";
            }
            StringBuilder builder = new("", 16);
            for (int i = 0; i < Air; ++i)
            {
                builder.Append('°');
            }
            return builder.ToString();
        }
        public virtual bool CanTakeDamage(NASDamageSource source) => true;
        public virtual bool TakeDamage(float damage, NASDamageSource source, string customDeathReason = "") => !CanTakeDamage(source) && false;
        public virtual void UpdateAir()
        {
            AirPrev = Air;
            if (holdingBreath)
            {
                Air -= 0.03125f;
                if (Air < 0)
                {
                    Air = 0;
                }
            }
            else
            {
                Air += 0.03125f;
                if (Air > 10)
                {
                    Air = 10;
                }
            }
            if (Air == 0)
            {
                TakeDamage(0.125f, NASDamageSource.Drowning);
            }
        }
        public void DoNASBlockCollideActions(Position entityPos)
        {
            if (nl == null)
            {
                return;
            }
            AABB worldAABB = bounds.OffsetPosition(entityPos);
            worldAABB.Min.X++;
            worldAABB.Min.Y++;
            worldAABB.Min.Z++;
            AABB eyeAABB = eyeBounds.OffsetPosition(entityPos);
            eyeAABB.Min.X++;
            eyeAABB.Min.Y++;
            eyeAABB.Min.Z++;
            worldAABB = worldAABB.Expand(-1);
            Vec3S32 min = worldAABB.BlockMin, max = worldAABB.BlockMax;
            for (int y = min.Y; y <= max.Y; y++)
            {
                for (int z = min.Z; z <= max.Z; z++)
                {
                    for (int x = min.X; x <= max.X; x++)
                    {
                        foreach (Player pl in PlayerInfo.Online.Items)
                        {
                            ushort xP = (ushort)x, yP = (ushort)y, zP = (ushort)z;
                            nl.lvl ??= pl.Level;
                            ushort block = nl.lvl.GetBlock(xP, yP, zP);
                            if (block == 0)
                            {
                                block = 0;
                            }
                            if (block == 0xff)
                            {
                                continue;
                            }
                            NASBlock nb = NASBlock.blocksIndexedByServerushort[block];
                            AABB blockBB = nb.bounds.Offset(x * 32, y * 32, z * 32);
                            if (!AABB.Intersects(ref worldAABB, ref blockBB))
                            {
                                continue;
                            }
                            if (nb == null || nb.collideAction == null)
                            {
                                continue;
                            }
                            nb.collideAction(this, nb, AABB.Intersects(ref eyeAABB, ref blockBB), xP, yP, zP);
                        }
                    }
                }
            }
        }
    }
}
