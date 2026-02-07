/*
    Copyright 2011 MCForge
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
using MCGalaxy.Blocks.Physics;
using MCGalaxy.Games;
using System;
namespace MCGalaxy.Modules.Games.LS
{
    public partial class LSGame : RoundsGame
    {
        void UpdateBlockHandlers()
        {
            Map.UpdateBlockHandlers(Block.Sponge);
            Map.UpdateBlockHandlers(Block.StillWater);
            Map.UpdateBlockHandlers(Block.Water);
            Map.UpdateBlockHandlers(Block.Deadly_ActiveWater);
            Map.UpdateBlockHandlers(Block.Lava);
            Map.UpdateBlockHandlers(Block.Deadly_ActiveLava);
            Map.UpdateBlockHandlers(Block.Door_Log);
        }
        void HandleBlockHandlersUpdated(Level lvl, ushort block)
        {
            if (!Running || lvl != Map) return;
            switch (block)
            {
                case Block.Sponge:
                    lvl.PlaceHandlers[block] = PlaceSponge;
                    lvl.PhysicsHandlers[block] = DoSponge; break;
                case Block.StillWater:
                    lvl.PlaceHandlers[block] = PlaceWater; break;
                case Block.Water:
                case Block.Deadly_ActiveWater:
                    lvl.PhysicsHandlers[block] = DoWater; break;
                case Block.Lava:
                case Block.Deadly_ActiveLava:
                    lvl.PhysicsHandlers[block] = DoLava; break;
                case Block.Door_Log:
                    lvl.PlaceHandlers[block] = PlaceDoor; break;
            }
        }
        int PlaceSponge(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            LSData data = Get(p);
            bool placed = TryPlaceBlock(p, ref data.SpongesLeft, "Sponges", Block.Sponge, x, y, z);
            if (!placed) return 0;
            PhysInfo C = default;
            C.X = x; C.Y = y; C.Z = z;
            OtherPhysics.DoSponge(Map, ref C, !waterMode);
            return 2;
        }
        int PlaceWater(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            LSData data = Get(p);
            bool placed = TryPlaceBlock(p, ref data.WaterLeft, "Water blocks", 9, x, y, z);
            if (!placed) return 0;
            return 2;
        }
        int PlaceDoor(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            LSData data = Get(p);
            bool placed = TryPlaceBlock(p, ref data.DoorsLeft, "Door blocks", 111, x, y, z);
            if (!placed) return 0;
            return 2;
        }
        void DoSponge(Level lvl, ref PhysInfo C)
        {
            if (C.Data.Value2++ < Config.SpongeLife) return;
            lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
            OtherPhysics.DoSpongeRemoved(lvl, C.Index, !waterMode);
            C.Data.Data = 255;
        }
        void DoWater(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (!lvl.CheckSpongeWater(x, y, z))
            {
                ushort block = C.Block;
                SpreadWater(lvl, (ushort)(x + 1), y, z, block);
                SpreadWater(lvl, (ushort)(x - 1), y, z, block);
                SpreadWater(lvl, x, y, (ushort)(z + 1), block);
                SpreadWater(lvl, x, y, (ushort)(z - 1), block);
                SpreadWater(lvl, x, (ushort)(y - 1), z, block);
                if (floodUp) SpreadWater(lvl, x, (ushort)(y + 1), z, block);
            }
            else
            { //was placed near sponge
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
            }
            C.Data.Data = 255;
        }
        void DoLava(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            if (C.Data.Data < spreadDelay)
            {
                C.Data.Data++; 
                return;
            }
            if (!lvl.CheckSpongeWater(x, y, z))
            {
                ushort block = C.Block;
                SpreadLava(lvl, (ushort)(x + 1), y, z, block);
                SpreadLava(lvl, (ushort)(x - 1), y, z, block);
                SpreadLava(lvl, x, y, (ushort)(z + 1), block);
                SpreadLava(lvl, x, y, (ushort)(z - 1), block);
                SpreadLava(lvl, x, (ushort)(y - 1), z, block);
                if (floodUp) SpreadLava(lvl, x, (ushort)(y + 1), z, block);
            }
            else
            { //was placed near sponge
                lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
            }
            C.Data.Data = 255;
        }
        void SpreadWater(Level lvl, ushort x, ushort y, ushort z, ushort type)
        {
            ushort block = lvl.GetBlock(x, y, z, out int index);
            if (InSafeZone(x, y, z)) return;
            switch (block)
            {
                case 0:
                    if (!lvl.CheckSpongeWater(x, y, z))
                    {
                        lvl.AddUpdate(index, type);
                    }
                    break;
                case 10:
                case 112:
                case 194:
                    if (!lvl.CheckSpongeWater(x, y, z))
                    {
                        lvl.AddUpdate(index, 1, default(PhysicsArgs));
                    }
                    break;
                case 12:
                case 13:
                case 110:
                    lvl.AddCheck(index); break;
                case 16: // TODO
                case 8:
                case 193:
                    break;
                default:
                    SpreadLiquid(lvl, x, y, z, index, block, true);
                    break;
            }
        }
        void SpreadLava(Level lvl, ushort x, ushort y, ushort z, ushort type)
        {
            ushort block = lvl.GetBlock(x, y, z, out int index);
            if (InSafeZone(x, y, z)) return;
            // in LS, sponge should stop lava too
            switch (block)
            {
                case 0:
                    if (!lvl.CheckSpongeWater(x, y, z))
                    {
                        lvl.AddUpdate(index, type);
                    }
                    break;
                case 8:
                case 193:
                    if (!lvl.CheckSpongeWater(x, y, z))
                    {
                        lvl.AddUpdate(index, 1, default(PhysicsArgs));
                    }
                    break;
                case 12:
                    if (lvl.LevelPhysics > 1)
                    { //Adv physics changes sand to glass next to lava
                        lvl.AddUpdate(index, 20, default(PhysicsArgs));
                    }
                    else
                    {
                        lvl.AddCheck(index);
                    }
                    break;
                case 13:
                    lvl.AddCheck(index); break;
                case 16: // TODO
                case 10:
                case 112:
                case 194:
                    break;
                default:
                    SpreadLiquid(lvl, x, y, z, index, block, false);
                    break;
            }
        }
        void SpreadLiquid(Level lvl, ushort x, ushort y, ushort z, int index,
                          ushort block, bool isWater)
        {
            if (floodMode == 0) return;
            Random rand = lvl.physRandom;
            bool instaKills = isWater ?
                lvl.Props[block].WaterKills : lvl.Props[block].LavaKills;
            // TODO need to kill less often
            if (instaKills && floodMode > 1)
            {
                if (!lvl.CheckSpongeWater(x, y, z))
                {
                    lvl.AddUpdate(index, 0, default(PhysicsArgs));
                }
            }
            else if (!lvl.Props[block].OPBlock && rand.Next(1, 101) <= burnChance)
            {
                PhysicsArgs C = default;
                C.Type1 = 1; 
                C.Value1 = destroyDelay;
                C.Type2 = 3; 
                C.Value2 = dissipateChance;
                lvl.AddUpdate(index, 16, C);
            }
        }
        byte GetDestroyDelay()
        {
            int mode = floodMode;
            if (mode == 1) return 200;
            if (mode == 2) return 100;
            if (mode == 3) return 50;
            return 10;
        }
        byte GetDissipateChance()
        {
            int mode = floodMode;
            if (mode == 1) return 50;
            if (mode == 2) return 65;
            if (mode == 3) return 80;
            return 100;
        }
        byte GetBurnChance()
        {
            int mode = floodMode;
            if (mode == 1) return 50;
            if (mode == 2) return 70;
            if (mode == 3) return 85;
            return 100;
        }
    }
}
