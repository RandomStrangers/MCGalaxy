#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Maths;
using Newtonsoft.Json;
using System;
namespace NotAwesomeSurvival
{
    public partial class NasEntity
    {
        public enum DamageSource { Falling, Suffocating, Drowning, Entity, None, Murder }
        public static string DeathReason(DamageSource source, Player p)
        {
            switch (source)
            {
                case DamageSource.Entity:
                    return "@p &cdied.";
                case DamageSource.Falling:
                    if (p == null) return "@p &cfell to their death.";
                    else return "@p &cfell to " + p.pronouns.Object + " death.";
                case DamageSource.Suffocating:
                    return "@p &esuffocated.";
                case DamageSource.Drowning:
                    return "@p &rdrowned.";
                case DamageSource.None:
                    return "@p &adied from unknown causes.";
                case DamageSource.Murder:
                    return "@p &8" + p.pronouns.PastVerb + "&8 murdered by &S@s";
                default:
                    return Enum.GetName(typeof(DamageSource), source).ToLower();
            }
        }
        [JsonIgnore] public float AirPrev;
        [JsonIgnore] public NasLevel nl;
        [JsonIgnore] public AABB bounds = AABB.Make(new Vec3S32(0, 0, 0), new Vec3S32(16, 26 * 2, 16));
        [JsonIgnore] public AABB eyeBounds = AABB.Make(new Vec3S32(0, 24 * 2 - 2, 0), new Vec3S32(4, 4, 4));
        [JsonIgnore] public DateTime lastSuffocationDate = DateTime.MinValue;
        public const int SuffocationMilliseconds = 500;
        public float HP;
        public const float maxHP = 10;
        public float Air;
        public const float maxAir = 10;
        public bool holdingBreath = false;
        public string levelName;
        public Vec3S32 location;
        public Vec3S32 lastGroundedLocation;
        public byte yaw,
            pitch;
        public static void SetLocation(NasEntity ne, string levelName, Position pos, Orientation rot)
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
            //TODO threadsafe
            HP += diff;
            if (HP < 0)
            {
                HP = 0;
            }
        }
        public virtual bool CanTakeDamage(DamageSource source)
        {
            return true;
        }
        public virtual bool TakeDamage(float damage, DamageSource source, string customDeathReason = "")
        {
            if (!CanTakeDamage(source))
            {
                return false;
            }
            return false;
        }
        public virtual void Die(string source)
        {
        }
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
                if (Air > maxAir)
                {
                    Air = maxAir;
                }
            }
            if (Air == 0)
            {
                TakeDamage(0.125f, DamageSource.Drowning);
            }
        }
        public virtual void DoNasBlockCollideActions(Position entityPos)
        {
            if (nl == null)
            {
                return;
            }
            AABB worldAABB = bounds.OffsetPosition(entityPos);
            //counteract client rounding
            worldAABB.Min.X++;
            worldAABB.Min.Y++;
            worldAABB.Min.Z++;
            AABB eyeAABB = eyeBounds.OffsetPosition(entityPos);
            eyeAABB.Min.X++;
            eyeAABB.Min.Y++;
            eyeAABB.Min.Z++;
            //sometimes client subtly clips through things like when walking up stairs...
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
                                block = Block.Air;
                            }
                            if (block == Block.Invalid)
                            {
                                continue;
                            }
                            NasBlock nb = NasBlock.blocksIndexedByServerushort[block];
                            AABB blockBB = nb.bounds.Offset(x * 32, y * 32, z * 32);
                            if (!AABB.Intersects(ref worldAABB, ref blockBB))
                            {
                                continue;
                            }
                            if (nb == null || nb.collideAction == null)
                            {
                                continue;
                            }
                            bool surroundsHead = AABB.Intersects(ref eyeAABB, ref blockBB);
                            nb.collideAction(this, nb, surroundsHead, xP, yP, zP);
                        }
                    }
                }
            }
        }
    }
}
#endif