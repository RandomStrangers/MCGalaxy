using NASBlockCollideAction =
    MCGalaxy.NASAction<MCGalaxy.NASEntity,
    MCGalaxy.NASBlock, bool, ushort, ushort, ushort>;
namespace MCGalaxy
{
    public partial class NASBlock
    {
        public static NASBlockCollideAction DefaultSolidCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                                    {
                                                                                        if (headSurrounded)
                                                                                            ne.TakeDamage(1.5f, NASDamageSource.Suffocating);
                                                                                    };
        public static NASBlockCollideAction LavaCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                            {
                                                                                if (headSurrounded && ne.CanTakeDamage(NASDamageSource.Drowning))
                                                                                    ne.holdingBreath = true;
                                                                                ne.TakeDamage(1.5f, NASDamageSource.Suffocating, "@p &cmelted in lava.");
                                                                            };
        public static NASBlockCollideAction FireCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                            {
                                                                                ne.TakeDamage(0.25f, NASDamageSource.None, "@p &cburned up");
                                                                            };
        public static NASBlockCollideAction SpikeCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                             {
                                                                                 ne.TakeDamage(3f, NASDamageSource.None, "@p &cgot impaled");
                                                                             };
        public static NASBlockCollideAction PressureCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                                {
                                                                                    ne.nl.SetBlock(x, y, z, Block.FromRaw(611));
                                                                                    if (!ne.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                        ne.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                    ne.nl.blockEntities[x + " " + y + " " + z].strength = 15;
                                                                                };
        public static NASBlockCollideAction LiquidCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                              {
                                                                                  if (headSurrounded && ne.CanTakeDamage(NASDamageSource.Drowning))
                                                                                      ne.holdingBreath = true;
                                                                              };
        public static NASBlockCollideAction AirCollideAction() => (ne, nasBlock, headSurrounded, x, y, z) =>
                                                                           {
                                                                               if (headSurrounded)
                                                                                   ne.holdingBreath = false;
                                                                           };
    }
}
