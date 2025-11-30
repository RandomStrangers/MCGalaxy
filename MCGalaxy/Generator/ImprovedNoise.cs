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
using System;

namespace MCGalaxy.Generator
{
    /// <summary> Implements improved perlin noise as described in http://mrl.nyu.edu/~perlin/noise/ </summary>
    public sealed class ImprovedNoise
    {
        public float Frequency = 1,
            Amplitude = 1,
            Lacunarity = 2,
            Persistence = 2;
        public int Octaves = 1;

        readonly byte[] p = new byte[512];

        public ImprovedNoise(Random rnd)
        {
            for (int i = 0; i < 256; i++)
                p[i] = (byte)i;

            for (int i = 0; i < 256; i++)
            {
                byte temp;
                int j = rnd.Next(i, 256);
                temp = p[i]; 
                p[i] = p[j]; 
                p[j] = temp;
            }
            for (int i = 0; i < 256; i++)
                p[i + 256] = p[i];
        }

        public float NormalisedNoise(float x, float y, float z)
        {
            float sum = 0,
                freq = Frequency, amp = Amplitude,
                scale = 0;

            for (int i = 0; i < Octaves; i++)
            {
                sum += Noise(x * freq, y * freq, z * freq) * amp;
                scale += amp;
                amp *= Persistence;
                freq *= Lacunarity;
            }
            return sum / scale;
        }

        public float OctaveNoise(float x, float y, float z)
        {
            float sum = 0,
                freq = Frequency, amp = Amplitude;
            for (int i = 0; i < Octaves; i++)
            {
                sum += Noise(x * freq, y * freq, z * freq) * amp;

                amp *= Persistence;
                freq *= Lacunarity;
            }
            return sum;
        }

        public float Noise(float x, float y, float z)
        {
            int xFloor = x >= 0 ? (int)x : (int)x - 1,
                yFloor = y >= 0 ? (int)y : (int)y - 1,
                zFloor = z >= 0 ? (int)z : (int)z - 1,
                X = xFloor & 0xFF, Y = yFloor & 0xFF, Z = zFloor & 0xFF;
            x -= xFloor; 
            y -= yFloor; 
            z -= zFloor;

            float u = Fade(x), v = Fade(y), w = Fade(z);
            int A = p[X] + Y, AA = p[A] + Z, AB = p[A + 1] + Z,
                B = p[X + 1] + Y, BA = p[B] + Z, BB = p[B + 1] + Z;

            return Lerp(
                Lerp(
                    Lerp(Grad(p[AA], x, y, z),
                         Grad(p[BA], x - 1, y, z),
                         u),
                    Lerp(Grad(p[AB], x, y - 1, z),
                         Grad(p[BB], x - 1, y - 1, z),
                         u),
                    v),
                Lerp(
                    Lerp(Grad(p[AA + 1], x, y, z - 1),
                         Grad(p[BA + 1], x - 1, y, z - 1),
                         u),
                    Lerp(Grad(p[AB + 1], x, y - 1, z - 1),
                         Grad(p[BB + 1], x - 1, y - 1, z - 1),
                         u),
                    v),
                w);
        }


        static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        static float Grad(int hash, float x, float y, float z)
        {
            return (hash & 0xF) switch
            {
                0x0 => x + y,
                0x1 => -x + y,
                0x2 => x - y,
                0x3 => -x - y,
                0x4 => x + z,
                0x5 => -x + z,
                0x6 => x - z,
                0x7 => -x - z,
                0x8 => y + z,
                0x9 => -y + z,
                0xA => y - z,
                0xB => -y - z,
                0xC => y + x,
                0xD => -y + z,
                0xE => y - x,
                0xF => -y - z,
                _ => 0,// never happens
            };
        }

        static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}
