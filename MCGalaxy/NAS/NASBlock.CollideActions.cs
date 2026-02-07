namespace MCGalaxy
{
    public partial class NASBlock
    {
        public static Action<NASEntity,
    NASBlock, bool, ushort, ushort, ushort> DefaultSolidCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                                    {
                                                                                        if (headSurrounded)
                                                                                        {
                                                                                            ne.TakeDamage(1.5f, 1);
                                                                                        }
                                                                                    };
        public static Action<NASEntity,
    NASBlock, bool, ushort, ushort, ushort> LavaCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                            {
                                                                                if (headSurrounded)
                                                                                {
                                                                                    if (ne.CanTakeDamage(2))
                                                                                    {
                                                                                        ne.holdingBreath = true;
                                                                                    }
                                                                                }
                                                                                ne.TakeDamage(1.5f, 1, "@p &cmelted in lava.");
                                                                            };
        public static Action<NASEntity,
    NASBlock, bool, ushort, ushort, ushort> FireCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                            {
                                                                                ne.TakeDamage(0.25f, 4, "@p &cburned up");
                                                                            };
        public static Action<NASEntity, NASBlock, bool, ushort, ushort, ushort> SpikeCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                             {
                                                                                 ne.TakeDamage(3f, 4, "@p &cgot impaled");
                                                                             };
        public static Action<NASEntity, NASBlock, bool, ushort, ushort, ushort> PressureCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                                {
                                                                                    ne.nl.SetBlock(x, y, z, NASPlugin.FromRaw(611));
                                                                                    if (!ne.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                    {
                                                                                        ne.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                    }
                                                                                    ne.nl.blockEntities[x + " " + y + " " + z].strength = 15;
                                                                                };
        public static Action<NASEntity, NASBlock, bool, ushort, ushort, ushort> LiquidCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                              {
                                                                                  if (headSurrounded)
                                                                                  {
                                                                                      if (ne.CanTakeDamage(2))
                                                                                      {
                                                                                          ne.holdingBreath = true;
                                                                                      }
                                                                                  }
                                                                              };
        public static Action<NASEntity, NASBlock, bool, ushort, ushort, ushort> AirCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                           {
                                                                               if (headSurrounded)
                                                                               {
                                                                                   ne.holdingBreath = false;
                                                                               }
                                                                           };
    }
}
