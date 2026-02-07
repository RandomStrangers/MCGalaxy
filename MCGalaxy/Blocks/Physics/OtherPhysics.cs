/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCGalaxy.Generator.Foliage;
using System;
namespace MCGalaxy.Blocks.Physics
{
    public static class OtherPhysics
    {
        public static void DoFalling(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            int index = C.Index;
            bool movedDown = false;
            ushort yCur = y;
            do
            {
                index = lvl.IntOffset(index, 0, -1, 0); yCur--;// Get block below each loop
                ushort cur = lvl.GetBlock(x, yCur, z);
                if (cur == 0xff) break;
                bool hitBlock = false;
                switch (cur)
                {
                    case 0:
                    case 8:
                    case 10:
                        movedDown = true;
                        break;
                    //Adv physics crushes plants with sand
                    case 6:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                        if (lvl.LevelPhysics > 1) movedDown = true;
                        break;
                    default:
                        hitBlock = true;
                        break;
                }
                if (hitBlock || lvl.LevelPhysics > 1) break;
            } while (true);
            if (movedDown)
            {
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                if (lvl.LevelPhysics > 1)
                    lvl.AddUpdate(index, C.Block);
                else
                    lvl.AddUpdate(lvl.IntOffset(index, 0, 1, 0), C.Block);
                ActivateablePhysics.CheckNeighbours(lvl, x, y, z);
            }
            C.Data.Data = 255;
        }
        public static void DoFloatwood(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (lvl.GetBlock(x, (ushort)(y - 1), z, out int index) == 0)
            {
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                lvl.AddUpdate(index, 110, default(PhysicsArgs));
            }
            else
            {
                ushort above = lvl.GetBlock(x, (ushort)(y + 1), z, out index);
                if (above == 9 || Block.Convert(above) == 8)
                {
                    lvl.AddUpdate(C.Index, C.Block);
                    lvl.AddUpdate(index, 110, default(PhysicsArgs));
                }
            }
            C.Data.Data = 255;
        }
        public static void DoShrub(Level lvl, ref PhysInfo C)
        {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;
            if (lvl.LevelPhysics > 1)
            { //Adv physics kills flowers and mushroos in water/lava
                ActivateablePhysics.CheckNeighbours(lvl, x, y, z);
            }
            if (!lvl.Config.GrowTrees) { C.Data.Data = 255; return; }
            if (C.Data.Data < 20)
            {
                if (rand.Next(20) == 0) C.Data.Data++;
                return;
            }
            lvl.SetTile(x, y, z, 0);
            Tree tree = Tree.Find(lvl.Config.TreeType) ?? new NormalTree();
            tree.SetData(rand, tree.DefaultSize(rand));
            tree.Generate(x, y, z, (xT, yT, zT, bT) =>
                        {
                            if (!lvl.IsAirAt(xT, yT, zT)) return;
                            lvl.Blockchange(xT, yT, zT, bT);
                        });
            C.Data.Data = 255;
        }
        public static void DoDirtGrow(Level lvl, ref PhysInfo C)
        {
            if (!lvl.Config.GrassGrow) { C.Data.Data = 255; return; }
            ushort x = C.X, y = C.Y, z = C.Z;
            if (C.Data.Data > 20)
            {
                ushort above = lvl.GetBlock(x, (ushort)(y + 1), z);
                if (lvl.LightPasses(above))
                {
                    ushort block = lvl.GetBlock(x, y, z),
                        grass = lvl.Props[block].GrassBlock;
                    lvl.AddUpdate(C.Index, grass);
                }
                C.Data.Data = 255;
            }
            else
            {
                C.Data.Data++;
            }
        }
        public static void DoGrassDie(Level lvl, ref PhysInfo C)
        {
            if (!lvl.Config.GrassGrow) { C.Data.Data = 255; return; }
            ushort x = C.X, y = C.Y, z = C.Z;
            if (C.Data.Data > 20)
            {
                ushort above = lvl.GetBlock(x, (ushort)(y + 1), z);
                if (!lvl.LightPasses(above))
                {
                    ushort block = lvl.GetBlock(x, y, z);
                    ushort dirt = lvl.Props[block].DirtBlock;
                    lvl.AddUpdate(C.Index, dirt);
                }
                C.Data.Data = 255;
            }
            else
            {
                C.Data.Data++;
            }
        }
        public static void DoSponge(Level lvl, ref PhysInfo C, bool lava)
        {
            ushort target = lava ? (ushort)10 : (ushort)8,
                alt = lava ? (ushort)11 : (ushort)9,
                x = C.X, y = C.Y, z = C.Z;
            for (int yy = y - 2; yy <= y + 2; ++yy)
            {
                for (int zz = z - 2; zz <= z + 2; ++zz)
                {
                    for (int xx = x - 2; xx <= x + 2; ++xx)
                    {
                        ushort block = lvl.GetBlock((ushort)xx, (ushort)yy, (ushort)zz, out int index);
                        if (Block.Convert(block) == target || Block.Convert(block) == alt)
                        {
                            lvl.AddUpdate(index, 0, default(PhysicsArgs));
                        }
                    }
                }
            }
            C.Data.Data = 255;
        }
        public static void DoSpongeRemoved(Level lvl, int b, bool lava)
        {
            ushort target = lava ? (ushort)10 : (ushort)8,
                alt = lava ? (ushort)11 : (ushort)9;
            lvl.IntToPos(b, out ushort x, out ushort y, out ushort z);
            for (int yy = -3; yy <= +3; ++yy)
            {
                for (int zz = -3; zz <= +3; ++zz)
                {
                    for (int xx = -3; xx <= +3; ++xx)
                    {
                        if (Math.Abs(xx) == 3 || Math.Abs(yy) == 3 || Math.Abs(zz) == 3)
                        { // Calc only edge
                            ushort block = lvl.GetBlock((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), out int index);
                            if (Block.Convert(block) == target || Block.Convert(block) == alt)
                                lvl.AddCheck(index);
                        }
                    }
                }
            }
        }
        public static void DoOther(Level lvl, ref PhysInfo C)
        {
            if (lvl.LevelPhysics <= 1) { C.Data.Data = 255; return; }
            //Adv physics kills flowers and mushroos in water/lava
            ActivateablePhysics.CheckNeighbours(lvl, C.X, C.Y, C.Z);
            C.Data.Data = 255;
        }
    }
}
