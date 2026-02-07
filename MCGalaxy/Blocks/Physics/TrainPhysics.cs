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
using System;
namespace MCGalaxy.Blocks.Physics
{
    public static class TrainPhysics
    {
        public static void Do(Level lvl, ref PhysInfo C)
        {
            Random rand = lvl.physRandom;
            int dirX = rand.Next(1, 100 + 1) <= 50 ? 1 : -1,
                dirY = rand.Next(1, 100 + 1) <= 50 ? 1 : -1,
                dirZ = rand.Next(1, 100 + 1) <= 50 ? 1 : -1;
            ushort x = C.X, y = C.Y, z = C.Z;
            for (int dx = -dirX; dx != 2 * dirX; dx += dirX)
            {
                for (int dy = -dirY; dy != 2 * dirY; dy += dirY)
                {
                    for (int dz = -dirZ; dz != 2 * dirZ; dz += dirZ)
                    {
                        ushort below = lvl.GetBlock((ushort)(x + dx), (ushort)(y + dy - 1), (ushort)(z + dz)),
                            block = lvl.GetBlock((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), out int index);
                        bool isRails = lvl.Props[below].IsRails;
                        if (isRails && (block == 0 || block == 8) && !lvl.listUpdateExists.Get(x + dx, y + dy, z + dz))
                        {
                            lvl.AddUpdate(index, 230, default(PhysicsArgs));
                            lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                            ushort newBlock = below == 105 ? (ushort)20 : (ushort)49;
                            below = lvl.GetBlock(x, (ushort)(y - 1), z, out int belowIndex);
                            PhysicsArgs args = default;
                            args.Type1 = 1; 
                            args.Value1 = 5;
                            args.Type2 = 2; 
                            args.Value2 = (byte)below;
                            args.ExtBlock = (byte)(below >> 8);
                            lvl.AddUpdate(belowIndex, newBlock, args, true);
                            return;
                        }
                    }
                }
            }
        }
    }
}
