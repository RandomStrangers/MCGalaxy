#if NAS && !NET_20 && TEN_BIT_BLOCKS
using MCGalaxy;
using NasBlockCollideAction =
    System.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
namespace NotAwesomeSurvival
{
    public partial class NasBlock
    {
        public static NasBlockCollideAction DefaultSolidCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                if (headSurrounded)
                {
                    ne.TakeDamage(1.5f, NasEntity.DamageSource.Suffocating);
                }
            };
        }
        public static NasBlockCollideAction LavaCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                if (headSurrounded)
                {
                    if (ne.CanTakeDamage(NasEntity.DamageSource.Drowning))
                    {
                        ne.holdingBreath = true;
                    }
                }
                ne.TakeDamage(1.5f, NasEntity.DamageSource.Suffocating, "@p &cmelted in lava.");
            };
        }
        public static NasBlockCollideAction FireCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                ne.TakeDamage(0.25f, NasEntity.DamageSource.None, "@p &cburned up");
            };
        }
        public static NasBlockCollideAction SpikeCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                ne.TakeDamage(3f, NasEntity.DamageSource.None, "@p &cgot impaled");
            };
        }
        public static NasBlockCollideAction PressureCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                ne.nl.SetBlock(x, y, z, Block.FromRaw(611));
                if (!ne.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    ne.nl.blockEntities.Add(x + " " + y + " " + z, new Entity());
                }
                ne.nl.blockEntities[x + " " + y + " " + z].strength = 15;
            };
        }
        public static NasBlockCollideAction LiquidCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                if (headSurrounded)
                {
                    if (ne.CanTakeDamage(NasEntity.DamageSource.Drowning))
                    {
                        ne.holdingBreath = true;
                    }
                }
            };
        }
        public static NasBlockCollideAction AirCollideAction()
        {
            return (ne, nasBlock, headSurrounded, x, y, z) =>
            {
                if (headSurrounded)
                {
                    ne.holdingBreath = false;
                }
            };
        }
    }
}
#endif