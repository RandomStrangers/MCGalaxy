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
namespace MCGalaxy.Blocks.Physics
{
    public static class ActivateablePhysics
    {
        /// <summary> Activates fireworks, rockets, and TNT in 1 block radius around (x, y, z) </summary>
        public static void DoNeighbours(Level lvl, ushort x, ushort y, ushort z)
        {
            int bHead = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        ushort block = lvl.GetBlock((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz));
                        int bTail;
                        if (block == 187)
                        {
                            bool isFree =
                                lvl.GetBlock((ushort)(x + dx * 2), (ushort)(y + dy * 2), (ushort)(z + dz * 2), out bTail) == Block.Air &&
                                lvl.GetBlock((ushort)(x + dx * 3), (ushort)(y + dy * 3), (ushort)(z + dz * 3), out bHead) == Block.Air &&
                                !lvl.listUpdateExists.Get(x + dx * 3, y + dy * 3, z + dz * 3) &&
                                !lvl.listUpdateExists.Get(x + dx * 2, y + dy * 2, z + dz * 2);
                            if (isFree)
                            {
                                lvl.AddUpdate(bHead, 188, default(PhysicsArgs));
                                lvl.AddUpdate(bTail, 185, default(PhysicsArgs));
                            }
                        }
                        else if (block == 189)
                        {
                            bool isFree =
                                lvl.GetBlock((ushort)(x + dx), (ushort)(y + dy + 1), (ushort)(z + dz), out bTail) == Block.Air &&
                                lvl.GetBlock((ushort)(x + dx), (ushort)(y + dy + 2), (ushort)(z + dz), out bHead) == Block.Air &&
                                !lvl.listUpdateExists.Get(x + dx, y + dy + 1, z + dz) &&
                                !lvl.listUpdateExists.Get(x + dx, y + dy + 2, z + dz);
                            if (isFree)
                            {
                                lvl.AddUpdate(bHead, 189, default(PhysicsArgs));
                                PhysicsArgs args = default;
                                args.Type1 = 3;
                                args.Value1 = 100;
                                lvl.AddUpdate(bTail, 11, args);
                            }
                        }
                        else if (block == 46)
                        {
                            lvl.MakeExplosion((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), 0);
                        }
                    }
                }
            }
        }
        /// <summary> Activates doors, tdoors and toggles odoors at (x, y, z) </summary>
        public static void DoDoors(Level lvl, ushort x, ushort y, ushort z, bool instant)
        {
            ushort block = lvl.GetBlock(x, y, z, out int index);
            if (index == -1) return;
            if (lvl.Props[block].IsDoor)
            {
                PhysicsArgs args = GetDoorArgs(block, out ushort physForm);
                if (!instant) lvl.AddUpdate(index, physForm, args);
                else lvl.Blockchange(index, physForm, false, args);
            }
            else if (lvl.Props[block].IsTDoor)
            {
                PhysicsArgs args = GetTDoorArgs(block);
                lvl.AddUpdate(index, 0, args);
            }
            else
            {
                ushort oDoor = lvl.Props[block].oDoorBlock;
                if (oDoor != 0xff)
                {
                    lvl.AddUpdate(index, oDoor, true);
                }
            }
        }
        internal static PhysicsArgs GetDoorArgs(ushort block, out ushort physForm)
        {
            PhysicsArgs args = default;
            args.Type1 = 7; 
            args.Value1 = 16 - 1;
            args.Type2 = 2; 
            args.Value2 = (byte)block;
            args.ExtBlock = (byte)(block >> 8);
            physForm = 201; // air
            if (block == 164 || block == 165)
            {
                args.Value1 = 4 - 1;
            }
            else if (block == 119)
            {
                physForm = 211; // red wool
            }
            else if (block == 120)
            {
                args.Value1 = 4 - 1; 
                physForm = 212; // lava
            }
            return args;
        }
        internal static PhysicsArgs GetTDoorArgs(ushort block)
        {
            PhysicsArgs args = default;
            args.Type1 = 7; 
            args.Value1 = 16;
            args.Type2 = 2; 
            args.Value2 = (byte)block;
            args.ExtBlock = (byte)(block >> 8);
            return args;
        }
        internal static void CheckNeighbours(Level lvl, ushort x, ushort y, ushort z)
        {
            CheckAt(lvl, (ushort)(x + 1), y, z);
            CheckAt(lvl, (ushort)(x - 1), y, z);
            CheckAt(lvl, x, y, (ushort)(z + 1));
            CheckAt(lvl, x, y, (ushort)(z - 1));
            CheckAt(lvl, x, (ushort)(y + 1), z);
            // NOTE: omission of y-1 to match original behaviour
        }
        // TODO: Stop checking block type and just always call lvl.AddCheck
        internal static void CheckAt(Level lvl, ushort x, ushort y, ushort z)
        {
            ushort block = lvl.GetBlock(x, y, z, out int index);
            switch (block)
            {
                //case Block.water:
                //case Block.lava:
                case 6:
                case 12:
                case 13:
                case 17:
                case 18:
                case 110:
                    /*case Block.lava_fast:
                    case Block.WaterDown:
                    case Block.LavaDown:
                    case Block.deathlava:
                    case Block.deathwater:
                    case Block.geyser:
                    case Block.magma:*/
                    lvl.AddCheck(index);
                    break;
                default:
                    block = Block.Convert(block);
                    if (block == 8 || block == 10 || (block >= 21 && block <= 36))
                    {
                        lvl.AddCheck(index);
                    }
                    break;
            }
        }
    }
}
