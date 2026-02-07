/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
namespace MCGalaxy.Blocks.Physics
{
    public delegate bool TNTImmuneFilter(ushort x, ushort y, ushort z);
    public static class TntPhysics
    {
        internal static void ToggleFuse(Level lvl, ushort x, ushort y, ushort z)
        {
            if (lvl.GetBlock(x, y, z) == 11)
            {
                lvl.Blockchange(x, y, z, 0);
            }
            else
            {
                lvl.Blockchange(x, y, z, 11);
            }
        }
        public static void DoTntExplosion(Level lvl, ref PhysInfo C)
        {
            Random rand = lvl.physRandom;
            if (rand.Next(1, 11) <= 7)
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
        }
        public static void DoSmallTnt(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (lvl.LevelPhysics < 3)
            {
                lvl.Blockchange(x, y, z, 0);
            }
            else
            {
                if (C.Data.Data < 5 && lvl.LevelPhysics == 3)
                {
                    C.Data.Data++;
                    ToggleFuse(lvl, x, (ushort)(y + 1), z);
                    return;
                }
                MakeExplosion(lvl, x, y, z, 0);
            }
        }
        public static void DoBigTnt(Level lvl, ref PhysInfo C) => DoLargeTnt(lvl, ref C, 1);
        public static void DoNukeTnt(Level lvl, ref PhysInfo C) => DoLargeTnt(lvl, ref C, 4);
        public static void DoLargeTnt(Level lvl, ref PhysInfo C, int power)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (lvl.LevelPhysics < 3)
            {
                lvl.Blockchange(x, y, z, Block.Air);
            }
            else
            {
                if (C.Data.Data < 5 && lvl.LevelPhysics == 3)
                {
                    C.Data.Data++;
                    ToggleFuse(lvl, x, (ushort)(y + 1), z);
                    ToggleFuse(lvl, x, (ushort)(y - 1), z);
                    ToggleFuse(lvl, (ushort)(x + 1), y, z);
                    ToggleFuse(lvl, (ushort)(x - 1), y, z);
                    ToggleFuse(lvl, x, y, (ushort)(z + 1));
                    ToggleFuse(lvl, x, y, (ushort)(z - 1));
                    return;
                }
                MakeExplosion(lvl, x, y, z, power);
            }
        }
        public static void MakeExplosion(Level lvl, ushort x, ushort y, ushort z, int size,
                                         bool force = false, TNTImmuneFilter filter = null)
        {
            Random rand = new();
            if ((lvl.LevelPhysics < 2 || lvl.LevelPhysics == 5) && !force) return;
            ushort block = lvl.GetBlock(x, y, z, out int index);
            if (index >= 0 && !lvl.Props[block].OPBlock)
            {
                lvl.AddUpdate(index, 184, default, true);
            }
            Explode(lvl, x, y, z, size + 1, rand, -1, filter);
            Explode(lvl, x, y, z, size + 2, rand, 7, filter);
            Explode(lvl, x, y, z, size + 3, rand, 3, filter);
        }
        static bool IsFuse(ushort b, int dx, int dy, int dz) => dx == 0 && dy == 1 && dz == 0 && b == Block.StillLava;
        static void Explode(Level lvl, ushort x, ushort y, ushort z,
                            int size, Random rand, int prob, TNTImmuneFilter filter)
        {
            for (int xx = x - size; xx <= (x + size); ++xx)
            {
                for (int yy = y - size; yy <= (y + size); ++yy)
                {
                    for (int zz = z - size; zz <= (z + size); ++zz)
                    {
                        ushort b = lvl.GetBlock((ushort)xx, (ushort)yy, (ushort)zz, out int index);
                        if (b == 0xff) continue;
                        bool doDestroy = prob < 0 || rand.Next(1, 10) < prob;
                        if (doDestroy && Block.Convert(b) != 46)
                        {
                            if (filter != null && b != 0 && !IsFuse(b, xx - x, yy - y, zz - z))
                            {
                                if (filter((ushort)xx, (ushort)yy, (ushort)zz)) continue;
                            }
                            int mode = rand.Next(1, 11);
                            if (mode <= 4)
                            {
                                lvl.AddUpdate(index, 184, default(PhysicsArgs));
                            }
                            else if (mode <= 8)
                            {
                                lvl.AddUpdate(index, 0, default(PhysicsArgs));
                            }
                            else
                            {
                                PhysicsArgs args = default;
                                args.Type1 = 4;
                                args.Value1 = 50;
                                args.Type2 = 3;
                                args.Value2 = 8;
                                lvl.AddCheck(index, false, args);
                            }
                        }
                        else if (b == 46)
                        {
                            lvl.AddUpdate(index, 182, default(PhysicsArgs));
                        }
                        else if (b == 182 || b == 183 || b == 186)
                        {
                            lvl.AddCheck(index);
                        }
                    }
                }
            }
        }
    }
}
