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

    public static class FinitePhysics
    {

        public static unsafe void DoWaterOrLava(Level lvl, ref PhysInfo C)
        {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;
            ushort below = lvl.GetBlock(x, (ushort)(y - 1), z, out int index);

            if (below == 0)
            {
                lvl.AddUpdate(index, C.Block, C.Data);
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                C.Data.ResetTypes();
            }
            else if (below == 9 || below == 11)
            {
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                C.Data.ResetTypes();
            }
            else
            {
                int* indices = stackalloc int[25];
                for (int i = 0; i < 25; ++i)
                    indices[i] = i;

                for (int k = 24; k > 1; --k)
                {
                    int randIndx = rand.Next(k),
                        temp = indices[k];
                    indices[k] = indices[randIndx]; // move random num to end of list.
                    indices[randIndx] = temp;
                }

                for (int j = 0; j < 25; j++)
                {
                    int i = indices[j];
                    ushort posX = (ushort)(x + (i / 5) - 2),
                        posZ = (ushort)(z + (i % 5) - 2);

                    if (lvl.IsAirAt(posX, (ushort)(y - 1), posZ) && lvl.IsAirAt(posX, y, posZ))
                    {
                        if (posX < x)
                        {
                            posX = (ushort)((posX + x) / 2);
                        }
                        else
                        {
                            posX = (ushort)((posX + x + 1) / 2); // ceiling division
                        }

                        if (posZ < z)
                        {
                            posZ = (ushort)((posZ + z) / 2);
                        }
                        else
                        {
                            posZ = (ushort)((posZ + z + 1) / 2);
                        }

                        if (lvl.IsAirAt(posX, y, posZ, out index) && lvl.AddUpdate(index, C.Block, C.Data))
                        {
                            lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                            C.Data.ResetTypes();
                            return;
                        }
                    }
                }
            }
        }

        static bool Expand(Level lvl, ushort x, ushort y, ushort z)
        {
            return lvl.IsAirAt(x, y, z, out int index) && lvl.AddUpdate(index, 145, default(PhysicsArgs));
        }

        public static unsafe void DoFaucet(Level lvl, ref PhysInfo C)
        {
            Random rand = lvl.physRandom;
            ushort x = C.X, y = C.Y, z = C.Z;

            int* indices = stackalloc int[6];
            for (int i = 0; i < 6; ++i)
                indices[i] = i;

            for (int k = 5; k > 1; --k)
            {
                int randIndx = rand.Next(k),
                    temp = indices[k];
                indices[k] = indices[randIndx]; // move random num to end of list.
                indices[randIndx] = temp;
            }

            for (int j = 0; j < 6; j++)
            {
                int i = indices[j];
                switch (i)
                {
                    case 0:
                        if (Expand(lvl, (ushort)(x - 1), y, z)) return;
                        break;
                    case 1:
                        if (Expand(lvl, (ushort)(x + 1), y, z)) return;
                        break;
                    case 2:
                        if (Expand(lvl, x, (ushort)(y - 1), z)) return;
                        break;
                    case 3:
                        if (Expand(lvl, x, (ushort)(y + 1), z)) return;
                        break;
                    case 4:
                        if (Expand(lvl, x, y, (ushort)(z - 1))) return;
                        break;
                    case 5:
                        if (Expand(lvl, x, y, (ushort)(z + 1))) return;
                        break;
                }
            }
        }
    }
}