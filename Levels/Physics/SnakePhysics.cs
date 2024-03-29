﻿/*
    Copyright 2015 MCGalaxy
    Original level physics copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;

namespace MCGalaxy.BlockPhysics {
    
    public static class SnakePhysics {
        
        public static void Do(Level lvl, Check C, Random rand) {
            ushort x, y, z;
            lvl.IntToPos(C.b, out x, out y, out z);
            int dirsVisited = 0, index = 0;
            Player foundPlayer = AIPhysics.ClosestPlayer(lvl, C);
            
            if (foundPlayer != null && rand.Next(1, 20) < 19) {
                switch (rand.Next(1, 10)) {
                    case 1:
                    case 2:
                    case 3:
                        index = lvl.PosToInt((ushort)(x + Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                        if (index != C.b && MoveSnake(lvl, C, index)) return;
                        
                        dirsVisited++;
                        if (dirsVisited >= 3) break;
                        goto case 4;
                    case 4:
                    case 5:
                    case 6:
                        index = lvl.PosToInt(x, (ushort) (y + Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                        if (index != C.b && MoveSnakeY(lvl, C, index)) return;
                        
                        dirsVisited++;
                        if (dirsVisited >= 3) break;
                        goto case 7;
                    case 7:
                    case 8:
                    case 9:
                        index = lvl.PosToInt(x, y, (ushort)(z + Math.Sign((foundPlayer.pos[2] / 32) - z)));
                        if (index != C.b && MoveSnake(lvl, C, index)) return;
                        
                        dirsVisited++;
                        if (dirsVisited >= 3) break;
                        goto case 1;
                }
            }
            
            dirsVisited = 0;
            switch (rand.Next(1, 13)) {
                case 1:
                case 2:
                case 3:
                    index = lvl.IntOffset(C.b, -1, 0, 0);
                    if (MoveSnake(lvl, C, index)) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 4;
                case 4:
                case 5:
                case 6:
                    index = lvl.IntOffset(C.b, 1, 0, 0);
                    if (MoveSnake(lvl, C, index)) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 7;
                case 7:
                case 8:
                case 9:
                    index = lvl.IntOffset(C.b, 0, 0, 1);
                    if (MoveSnake(lvl, C, index)) return;

                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 10;
                case 10:
                case 11:
                case 12:
                default:
                    index = lvl.IntOffset(C.b, 0, 0, -1);
                    if (MoveSnake(lvl, C, index)) return;
                    
                    dirsVisited++;
                    if (dirsVisited >= 4) return;
                    goto case 1;
            }
        }
        
        static bool MoveSnake(Level lvl, Check C, int index) {
            if (
                lvl.GetTile(lvl.IntOffset(index, 0, -1, 0)) == Block.air &&
                lvl.GetTile(index) == Block.air) {
                index = lvl.IntOffset(index, 0, -1, 0);
            } else if (
                lvl.GetTile(index) == Block.air &&
                lvl.GetTile(lvl.IntOffset(index, 0, 1, 0)) == Block.air) {
            } else if (
                lvl.GetTile(lvl.IntOffset(index, 0, 2, 0)) == Block.air &&
                lvl.GetTile(lvl.IntOffset(index, 0, 1, 0)) == Block.air) {
                index = lvl.IntOffset(index, 0, 1, 0);
            }

            if (lvl.AddUpdate(index, lvl.blocks[C.b])) {
                lvl.AddUpdate(C.b, Block.snaketail, true, "wait 5 revert 0");
                return true;
            }
            return false;
        }
        
        static bool MoveSnakeY(Level lvl, Check C, int index ) {
            byte block = lvl.GetTile(index);
            byte blockAbove = lvl.GetTile(lvl.IntOffset(index, 0, 1, 0));
            byte block2Above = lvl.GetTile(lvl.IntOffset(index, 0, 2, 0));
            
            if (block == Block.air &&
                (blockAbove == Block.grass ||
                 blockAbove == Block.dirt && block2Above == Block.air)) {
                if (lvl.AddUpdate(index, lvl.blocks[C.b])) {
                    lvl.AddUpdate(C.b, Block.snaketail, true, "wait 5 revert 0");
                    return true;
                }            
            }
            return false;
        }
    }
}
