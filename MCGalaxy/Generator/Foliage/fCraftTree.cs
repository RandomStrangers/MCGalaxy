// Part of fCraft | Copyright 2009-2015 Matvei Stefarov <me@matvei.org> | BSD-3 | See LICENSE.txt
using MCGalaxy.Generator.Foliage;
using System;
namespace MCGalaxy.Generator.fCraft
{
    sealed class FCraftTree : Tree
    {
        public override long EstimateBlocksAffected() => height + 66;
        public override int DefaultSize(Random rnd) => rnd.Next(5, 8);
        public override void SetData(Random rnd, int value)
        {
            height = value;
            this.rnd = rnd;
        }
        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            for (int dy = 0; dy < height; dy++)
            {
                output(x, (ushort)(y + dy), z, 17);
            }
            for (int dy = -1; dy < height / 2; dy++)
            {
                int radius = (dy >= (height / 2) - 2) ? 1 : 2;
                for (int dx = -radius; dx < radius + 1; dx++)
                {
                    for (int dz = -radius; dz < radius + 1; dz++)
                    {
                        if (rnd.NextDouble() > 0.618 && Math.Abs(dx) == Math.Abs(dz) && Math.Abs(dx) == radius)
                            continue;
                        output((ushort)(x + dx), (ushort)(y + height + dy - 1), (ushort)(z + dz), 18);
                    }
                }
            }
        }
    }
}
