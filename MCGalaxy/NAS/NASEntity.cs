using MCGalaxy.Maths;
using Newtonsoft.Json;
using System;
namespace MCGalaxy
{
    public class NASEntity
    {
        [JsonIgnore] public float AirPrev;
        [JsonIgnore] public NASLevel nl;
        [JsonIgnore] public AABB bounds = AABB.Make(new(0, 0, 0), new(16, 26 * 2, 16));
        [JsonIgnore] public AABB eyeBounds = AABB.Make(new(0, 24 * 2 - 2, 0), new(4, 4, 4));
        [JsonIgnore] public DateTime lastSuffocationDate = DateTime.MinValue;
        public const int SuffocationMilliseconds = 500;
        public float HP, Air;
        public const float maxHP = 10, maxAir = 10;
        public bool holdingBreath = false;
        public string levelName;
        public Vec3S32 location, lastGroundedLocation;
        public byte yaw, pitch;
        public virtual bool CanTakeDamage(int source) => true;
        public virtual bool TakeDamage(float damage, int source, string customDeathReason = "")
        {
            if (!CanTakeDamage(source))
            {
                return false;
            }
            return false;
        }
    }
}