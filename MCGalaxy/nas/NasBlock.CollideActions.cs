#if NAS && TEN_BIT_BLOCKS
using NasBlockCollideAction =
    NotAwesomeSurvival.Action<NotAwesomeSurvival.NasEntity,
    NotAwesomeSurvival.NasBlock, bool, ushort, ushort, ushort>;
namespace NotAwesomeSurvival
{
    public partial class NasBlock
    {
        public static NasBlockCollideAction DefaultSolidCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                                    {
                                                                                        if (headSurrounded)
                                                                                        {
                                                                                            ne.TakeDamage(1.5f, NasEntity.DamageSource.Suffocating);
                                                                                        }
                                                                                    };
        public static NasBlockCollideAction LavaCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
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
        public static NasBlockCollideAction FireCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                            {
                                                                                ne.TakeDamage(0.25f, NasEntity.DamageSource.None, "@p &cburned up");
                                                                            };
        public static NasBlockCollideAction SpikeCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                             {
                                                                                 ne.TakeDamage(3f, NasEntity.DamageSource.None, "@p &cgot impaled");
                                                                             };
        public static NasBlockCollideAction PressureCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                                {
                                                                                    ne.nl.SetBlock(x, y, z, Nas.FromRaw(611));
                                                                                    if (!ne.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                    {
                                                                                        ne.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                    }
                                                                                    ne.nl.blockEntities[x + " " + y + " " + z].strength = 15;
                                                                                };
        public static NasBlockCollideAction LiquidCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                              {
                                                                                  if (headSurrounded)
                                                                                  {
                                                                                      if (ne.CanTakeDamage(NasEntity.DamageSource.Drowning))
                                                                                      {
                                                                                          ne.holdingBreath = true;
                                                                                      }
                                                                                  }
                                                                              };
        public static NasBlockCollideAction AirCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                           {
                                                                               if (headSurrounded)
                                                                               {
                                                                                   ne.holdingBreath = false;
                                                                               }
                                                                           };
    }
}
#endif