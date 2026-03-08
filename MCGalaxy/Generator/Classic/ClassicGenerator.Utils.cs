// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
namespace MCGalaxy.Generator.Classic
{
    public sealed partial class ClassicGenerator
    {
        static int Floor(float value) => value < (int)value ? (int)value - 1 : (int)value;
        void FillOblateSpheroid(int x, int y, int z, float radius, byte block)
        {
            int xBeg = Floor(Math.Max(x - radius, 0)),
                xEnd = Floor(Math.Min(x + radius, Width - 1)),
                yBeg = Floor(Math.Max(y - radius, 0)),
                yEnd = Floor(Math.Min(y + radius, Height - 1)),
                zBeg = Floor(Math.Max(z - radius, 0)),
                zEnd = Floor(Math.Min(z + radius, Length - 1));
            float radiusSq = radius * radius;
            for (int yy = yBeg; yy <= yEnd; yy++)
                for (int zz = zBeg; zz <= zEnd; zz++)
                    for (int xx = xBeg; xx <= xEnd; xx++)
                    {
                        int dx = xx - x, dy = yy - y, dz = zz - z;
                        if ((dx * dx + 2 * dy * dy + dz * dz) < radiusSq)
                        {
                            int index = (yy * Length + zz) * Width + xx;
                            if (blocks[index] == Block.Stone)
                                blocks[index] = block;
                        }
                    }
        }
        void FloodFill(int startIndex, byte block)
        {
            if (startIndex < 0) return;
            FastIntStack stack = new(4);
            stack.Push(startIndex);
            while (stack.Size > 0)
            {
                int index = stack.Pop();
                if (blocks[index] != Block.Air) continue;
                blocks[index] = block;
                int x = index % Width,
                    y = index / oneY,
                    z = index / Width % Length;
                if (x > 0) stack.Push(index - 1);
                if (x < Width - 1) stack.Push(index + 1);
                if (z > 0) stack.Push(index - Width);
                if (z < Length - 1) stack.Push(index + Width);
                if (y > 0) stack.Push(index - oneY);
            }
        }
        sealed class FastIntStack
        {
            public int[] Values;
            public int Size;
            public FastIntStack(int capacity)
            {
                Values = new int[capacity];
                Size = 0;
            }
            public int Pop() => Values[--Size];
            public void Push(int item)
            {
                if (Size == Values.Length)
                {
                    int[] array = new int[Values.Length * 2];
                    Buffer.BlockCopy(Values, 0, array, 0, Size * sizeof(int));
                    Values = array;
                }
                Values[Size++] = item;
            }
        }
    }
    public sealed class JavaRandom
    {
        long seed;
        public JavaRandom(int seed) => SetSeed(seed);
        public void SetSeed(int seed) => this.seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
        public int Next(int min, int max) => min + Next(max - min);
        public int Next(int n)
        {
            if ((n & -n) == n)
            {
                seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
                long raw = (long)((ulong)seed >> (48 - 31));
                return (int)((n * raw) >> 31);
            }
            int bits, val;
            do
            {
                seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
                bits = (int)((ulong)seed >> (48 - 31));
                val = bits % n;
            } while (bits - val + (n - 1) < 0);
            return val;
        }
        public float NextFloat()
        {
            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
            int raw = (int)((ulong)seed >> (48 - 24));
            return raw / ((float)(1 << 24));
        }
    }
}
