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
    public static class SimpleLiquidPhysics
    {
        public static void DoWater(Level lvl, ref PhysInfo C)
        {
            if (lvl.Config.FiniteLiquids)
            {
                FinitePhysics.DoWaterOrLava(lvl, ref C);
            }
            else if (lvl.Config.RandomFlow)
            {
                DoWaterRandowFlow(lvl, ref C);
            }
            else
            {
                DoWaterUniformFlow(lvl, ref C);
            }
        }
        public static void DoLava(Level lvl, ref PhysInfo C)
        {
            // upper 3 bits are time delay
            if (C.Data.Data < (4 << 5))
            {
                C.Data.Data += 1 << 5;
                return;
            }
            if (lvl.Config.FiniteLiquids)
            {
                FinitePhysics.DoWaterOrLava(lvl, ref C);
            }
            else if (lvl.Config.RandomFlow)
            {
                DoLavaRandowFlow(lvl, ref C, true);
            }
            else
            {
                DoLavaUniformFlow(lvl, ref C, true);
            }
        }
        public static void DoFastLava(Level lvl, ref PhysInfo C)
        {
            if (lvl.Config.RandomFlow)
            {
                DoLavaRandowFlow(lvl, ref C, false);
                if (C.Data.Data != 255)
                    C.Data.Data = 0; // no lava delay
            }
            else
            {
                DoLavaUniformFlow(lvl, ref C, false);
            }
        }
        static void DoWaterRandowFlow(Level lvl, ref PhysInfo C)
        {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;
            if (!lvl.CheckSpongeWater(x, y, z))
            {
                byte data = C.Data.Data;
                ushort block = C.Block;
                if (y < lvl.Height - 1)
                {
                    CheckFallingBlocks(lvl, C.Index + lvl.Width * lvl.Length);
                }
                if ((data & (1 << 0)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysWater(lvl, (ushort)(x + 1), y, z, block);
                    data |= 1 << 0;
                }
                if ((data & (1 << 1)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysWater(lvl, (ushort)(x - 1), y, z, block);
                    data |= 1 << 1;
                }
                if ((data & (1 << 2)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysWater(lvl, x, y, (ushort)(z + 1), block);
                    data |= 1 << 2;
                }
                if ((data & (1 << 3)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysWater(lvl, x, y, (ushort)(z - 1), block);
                    data |= 1 << 3;
                }
                if ((data & (1 << 4)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysWater(lvl, x, (ushort)(y - 1), z, block);
                    data |= 1 << 4;
                }
                if ((data & (1 << 0)) == 0 && WaterBlocked(lvl, (ushort)(x + 1), y, z))
                {
                    data |= 1 << 0;
                }
                if ((data & (1 << 1)) == 0 && WaterBlocked(lvl, (ushort)(x - 1), y, z))
                {
                    data |= 1 << 1;
                }
                if ((data & (1 << 2)) == 0 && WaterBlocked(lvl, x, y, (ushort)(z + 1)))
                {
                    data |= 1 << 2;
                }
                if ((data & (1 << 3)) == 0 && WaterBlocked(lvl, x, y, (ushort)(z - 1)))
                {
                    data |= 1 << 3;
                }
                if ((data & (1 << 4)) == 0 && WaterBlocked(lvl, x, (ushort)(y - 1), z))
                {
                    data |= 1 << 4;
                }
                // Have we spread now (or been blocked from spreading) in all directions?
                C.Data.Data = data;
                if (!C.Data.HasWait && (data & 0x1F) == 0x1F)
                {
                    C.Data.Data = 255;
                }
            }
            else
            { //was placed near sponge
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                if (!C.Data.HasWait)
                {
                    C.Data.Data = 255;
                }
            }
        }
        static void DoWaterUniformFlow(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (!lvl.CheckSpongeWater(x, y, z))
            {
                ushort block = C.Block;
                if (y < lvl.Height - 1)
                {
                    CheckFallingBlocks(lvl, C.Index + lvl.Width * lvl.Length);
                }
                LiquidPhysics.PhysWater(lvl, (ushort)(x + 1), y, z, block);
                LiquidPhysics.PhysWater(lvl, (ushort)(x - 1), y, z, block);
                LiquidPhysics.PhysWater(lvl, x, y, (ushort)(z + 1), block);
                LiquidPhysics.PhysWater(lvl, x, y, (ushort)(z - 1), block);
                LiquidPhysics.PhysWater(lvl, x, (ushort)(y - 1), z, block);
            }
            else
            { //was placed near sponge
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
            }
            if (!C.Data.HasWait) C.Data.Data = 255;
        }
        static bool WaterBlocked(Level lvl, ushort x, ushort y, ushort z)
        {
            ushort block = lvl.GetBlock(x, y, z);
            switch (block)
            {
                case 0:
                case 10:
                case 112:
                case 194:
                    if (!lvl.CheckSpongeWater(x, y, z)) return false;
                    break;
                case 12:
                case 13:
                case 110:
                    return false;
                case 0xff:
                    return true;
                default:
                    // Adv physics kills flowers, mushroom blocks in water
                    if (!lvl.Props[block].WaterKills) break;
                    if (lvl.LevelPhysics > 1 && !lvl.CheckSpongeWater(x, y, z)) return false;
                    break;
            }
            return true;
        }
        static void DoLavaRandowFlow(Level lvl, ref PhysInfo C, bool checkWait)
        {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;
            if (!lvl.CheckSpongeLava(x, y, z))
            {
                byte data = C.Data.Data;
                // Upper 3 bits are time flags - reset random delay
                data &= 0x1F;
                data |= (byte)(rand.Next(3) << 5);
                ushort block = C.Block;
                if ((data & (1 << 0)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysLava(lvl, (ushort)(x + 1), y, z, block);
                    data |= 1 << 0;
                }
                if ((data & (1 << 1)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysLava(lvl, (ushort)(x - 1), y, z, block);
                    data |= 1 << 1;
                }
                if ((data & (1 << 2)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysLava(lvl, x, y, (ushort)(z + 1), block);
                    data |= 1 << 2;
                }
                if ((data & (1 << 3)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysLava(lvl, x, y, (ushort)(z - 1), block);
                    data |= 1 << 3;
                }
                if ((data & (1 << 4)) == 0 && rand.Next(4) == 0)
                {
                    LiquidPhysics.PhysLava(lvl, x, (ushort)(y - 1), z, block);
                    data |= 1 << 4;
                }
                if ((data & (1 << 0)) == 0 && LavaBlocked(lvl, (ushort)(x + 1), y, z))
                {
                    data |= 1 << 0;
                }
                if ((data & (1 << 1)) == 0 && LavaBlocked(lvl, (ushort)(x - 1), y, z))
                {
                    data |= 1 << 1;
                }
                if ((data & (1 << 2)) == 0 && LavaBlocked(lvl, x, y, (ushort)(z + 1)))
                {
                    data |= 1 << 2;
                }
                if ((data & (1 << 3)) == 0 && LavaBlocked(lvl, x, y, (ushort)(z - 1)))
                {
                    data |= 1 << 3;
                }
                if ((data & (1 << 4)) == 0 && LavaBlocked(lvl, x, (ushort)(y - 1), z))
                {
                    data |= 1 << 4;
                }
                // Have we spread now (or been blocked from spreading) in all directions?
                C.Data.Data = data;
                if ((!checkWait || !C.Data.HasWait) && (data & 0x1F) == 0x1F)
                {
                    C.Data.Data = 255;
                }
            }
            else
            { //was placed near sponge
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                if (!checkWait || !C.Data.HasWait)
                {
                    C.Data.Data = 255;
                }
            }
        }
        static void DoLavaUniformFlow(Level lvl, ref PhysInfo C, bool checkWait)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (!lvl.CheckSpongeLava(x, y, z))
            {
                ushort block = C.Block;
                LiquidPhysics.PhysLava(lvl, (ushort)(x + 1), y, z, block);
                LiquidPhysics.PhysLava(lvl, (ushort)(x - 1), y, z, block);
                LiquidPhysics.PhysLava(lvl, x, y, (ushort)(z + 1), block);
                LiquidPhysics.PhysLava(lvl, x, y, (ushort)(z - 1), block);
                LiquidPhysics.PhysLava(lvl, x, (ushort)(y - 1), z, block);
            }
            else
            { //was placed near sponge
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
            }
            if (!checkWait || !C.Data.HasWait)
            {
                C.Data.Data = 255;
            }
        }
        static bool LavaBlocked(Level lvl, ushort x, ushort y, ushort z)
        {
            ushort block = lvl.GetBlock(x, y, z);
            switch (block)
            {
                case 0:
                    return false;
                case 8:
                case 193:
                    if (!lvl.CheckSpongeLava(x, y, z)) return false;
                    break;
                case 12:
                case 13:
                    return false;
                case 0xff:
                    return true;
                default:
                    // Adv physics kills flowers, wool, mushrooms, and wood type blocks in lava
                    if (!lvl.Props[block].LavaKills) break;
                    if (lvl.LevelPhysics > 1 && !lvl.CheckSpongeLava(x, y, z)) return false;
                    break;
            }
            return true;
        }
        static void CheckFallingBlocks(Level lvl, int b)
        {
            switch (lvl.blocks[b])
            {
                case 12:
                case 13:
                case 110:
                    lvl.AddCheck(b); 
                    break;
                default:
                    break;
            }
        }
    }
}