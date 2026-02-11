/*
    Copyright 2015-2024 MCGalaxy
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Generator;
using System;
using System.Collections.Generic;
namespace MCGalaxy.Drawing.Brushes
{
    public sealed class CloudyBrush : Brush
    {
        readonly ushort[] blocks;
        readonly int[] counts;
        readonly float[] thresholds;
        readonly ImprovedNoise noise;
        public CloudyBrush(List<ushort> blocks, List<int> counts, NoiseArgs n)
        {
            this.blocks = blocks.ToArray();
            this.counts = counts.ToArray();
            thresholds = new float[counts.Count];
            Random r = n.Seed == int.MinValue ? new Random() : new Random(n.Seed);
            noise = new(r)
            {
                Frequency = n.Frequency,
                Amplitude = n.Amplitude,
                Octaves = n.Octaves,
                Lacunarity = n.Lacunarity,
                Persistence = n.Persistence
            };
        }
        public override string Name => "Cloudy";
        public override unsafe void Configure(DrawOp op, Player p)
        {
            if (!p.Ignores.DrawOutput)
            {
                p.Message("Calculating noise distribution...");
            }
            int* values = stackalloc int[10000];
            for (int i = 0; i < 10000; i++)
            {
                values[i] = 0;
            }
            for (int x = op.Min.X; x <= op.Max.X; x++)
            {
                for (int y = op.Min.Y; y <= op.Max.Y; y++)
                {
                    for (int z = op.Min.Z; z <= op.Max.Z; z++)
                    {
                        float N = noise.NormalisedNoise(x, y, z);
                        N = (N + 1) * 0.5f;
                        int index = (int)(N * 10000);
                        index = index < 0 ? 0 : index;
                        index = index >= 10000 ? 9999 : index;
                        values[index]++;
                    }
                }
            }
            float* coverage = stackalloc float[counts.Length];
            int totalBlocks = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                totalBlocks += counts[i];
            }
            float last = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                coverage[i] = last + (counts[i] / (float)totalBlocks);
                last = coverage[i];
            }
            int volume = op.SizeX * op.SizeY * op.SizeZ;
            float sum = 0;
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < counts.Length; j++)
                {
                    if (sum <= coverage[j])
                        thresholds[j] = i / (float)10000;
                }
                sum += values[i] / (float)volume;
            }
            thresholds[blocks.Length - 1] = 1;
            if (!p.Ignores.DrawOutput)
            {
                p.Message("Finished calculating, now drawing.");
            }
        }
        int next;
        public override ushort NextBlock(DrawOp op)
        {
            float N = noise.NormalisedNoise(op.Coords.X, op.Coords.Y, op.Coords.Z);
            N = (N + 1) * 0.5f;
            N = N < 0 ? 0 : N;
            N = N > 1 ? 1 : N;
            next = blocks.Length - 1;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (N <= thresholds[i]) 
                {
                    next = i; 
                    break;
                }
            }
            return blocks[next];
        }
    }
}
