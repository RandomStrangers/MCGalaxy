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
using MCGalaxy.Blocks.Physics;
namespace MCGalaxy.Blocks
{
    internal static class PlaceBehaviour
    {
        static bool SkipGrassDirt(Player p, ushort block) => !p.Level.Config.GrassGrow || p.ModeBlock == block || !(p.Level.LevelPhysics == 0 || p.Level.LevelPhysics == 5);
        internal static int GrassDie(Player p, ushort block, ushort x, ushort y, ushort z)
        {
            if (SkipGrassDirt(p, block)) return p.ChangeBlock(x, y, z, block);
            if (p.Level.GetBlock(x, (ushort)(y + 1), z) != 0xff && !p.Level.LightPasses(p.Level.GetBlock(x, (ushort)(y + 1), z)))
            {
                block = p.Level.Props[block].DirtBlock;
            }
            return p.ChangeBlock(x, y, z, block);
        }
        internal static int DirtGrow(Player p, ushort block, ushort x, ushort y, ushort z)
        {
            if (SkipGrassDirt(p, block)) return p.ChangeBlock(x, y, z, block);
            if (p.Level.GetBlock(x, (ushort)(y + 1), z) == 0xff || p.Level.LightPasses(p.Level.GetBlock(x, (ushort)(y + 1), z)))
            {
                block = p.Level.Props[block].GrassBlock;
            }
            return p.ChangeBlock(x, y, z, block);
        }
        internal static int Stack(Player p, ushort block, ushort x, ushort y, ushort z)
        {
            if (p.Level.GetBlock(x, (ushort)(y - 1), z) != block)
            {
                return p.ChangeBlock(x, y, z, block);
            }
            p.SendBlockchange(x, y, z, 0); // send the air block back only to the user
            p.ChangeBlock(x, (ushort)(y - 1), z, p.Level.Props[block].StackBlock);
            return 2;
        }
        internal static int C4(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            if (p.Level.LevelPhysics == 0 || p.Level.LevelPhysics == 5) return 0;
            // Use red wool to detonate c4
            if (p.BlockBindings[p.ClientHeldBlock] == 21)
            {
                p.Message("Placed detonator block, delete it to detonate.");
                return C4Det(p, 75, x, y, z);
            }
            if (p.c4circuitNumber == -1)
            {
                sbyte num = C4Physics.NextCircuit(p.Level);
                p.Level.C4list.Add(new(num));
                p.c4circuitNumber = num;
                p.Message("Place more blocks for more c4, then place a &c{0} &Sblock for the detonator.",
                               Block.GetName(p, 21));
            }
            C4Data c4 = C4Physics.Find(p.Level, p.c4circuitNumber);
            c4?.list.Add(p.Level.PosToInt(x, y, z));
            return p.ChangeBlock(x, y, z, 74);
        }
        internal static int C4Det(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            if (p.Level.LevelPhysics == 0 || p.Level.LevelPhysics == 5)
            {
                p.c4circuitNumber = -1;
                return 0;
            }
            C4Data c4 = C4Physics.Find(p.Level, p.c4circuitNumber);
            if (c4 != null) c4.detIndex = p.Level.PosToInt(x, y, z);
            p.c4circuitNumber = -1;
            return p.ChangeBlock(x, y, z, 75);
        }
    }
}
