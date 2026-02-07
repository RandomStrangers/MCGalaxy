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
using MCGalaxy.Blocks.Extended;
using MCGalaxy.Blocks.Physics;
using MCGalaxy.Maths;
using System;
namespace MCGalaxy.Blocks
{
    internal static class DeleteBehaviour
    {
        internal static int RocketStart(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            if (p.Level.LevelPhysics < 2 || p.Level.LevelPhysics == 5) return 0;
            DirUtils.EightYaw(p.Rot.RotY, out int dx, out int dz);
            DirUtils.Pitch(p.Rot.HeadX, out int dy);
            // Looking straight up or down
            byte pitch = p.Rot.HeadX;
            if (pitch >= 192 && pitch <= 196 || pitch >= 60 && pitch <= 64) 
            { 
                dx = 0; 
                dz = 0; 
            }
            Vec3U16 head = new((ushort)(x + dx * 2), (ushort)(y + dy * 2), (ushort)(z + dz * 2)),
                tail = new((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz));
            bool headFree = p.Level.IsAirAt(head.X, head.Y, head.Z) && p.Level.CheckClear(head.X, head.Y, head.Z),
                tailFree = p.Level.IsAirAt(tail.X, tail.Y, tail.Z) && p.Level.CheckClear(tail.X, tail.Y, tail.Z);
            if (headFree && tailFree)
            {
                p.Level.Blockchange(head.X, head.Y, head.Z, 188);
                p.Level.Blockchange(tail.X, tail.Y, tail.Z, 185);
            }
            return 0;
        }
        internal static int Firework(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            if (p.Level.LevelPhysics == 0 || p.Level.LevelPhysics == 5) return 0;
            Random rand = new();
            // Offset the firework randomly
            Vec3U16 pos = new(0, 0, 0)
            {
                X = (ushort)(x + rand.Next(0, 2) - 1),
                Z = (ushort)(z + rand.Next(0, 2) - 1)
            };
            ushort headY = (ushort)(y + 2), tailY = (ushort)(y + 1);
            bool headFree = p.Level.IsAirAt(pos.X, headY, pos.Z) && p.Level.CheckClear(pos.X, headY, pos.Z),
                tailFree = p.Level.IsAirAt(pos.X, tailY, pos.Z) && p.Level.CheckClear(pos.X, tailY, pos.Z);
            if (headFree && tailFree)
            {
                p.Level.Blockchange(pos.X, headY, pos.Z, 189);
                PhysicsArgs args = default;
                args.Type1 = 1;
                args.Value1 = 1;
                args.Type2 = 3; 
                args.Value2 = 100;
                p.Level.Blockchange(pos.X, tailY, pos.Z, 11, false, args);
            }
            return 0;
        }
        internal static int C4Det(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            C4Physics.BlowUp(p.Level.PosToInt(x, y, z), p.Level);
            return p.ChangeBlock(x, y, z, 0);
        }
        internal static int RevertDoor(Player _, ushort __, ushort ___, ushort ____, ushort _____) => 0;
        internal static int Door(Player p, ushort old, ushort x, ushort y, ushort z)
        {
            if (p.Level.LevelPhysics == 0) return p.ChangeBlock(x, y, z, 0);
            PhysicsArgs args = ActivateablePhysics.GetDoorArgs(old, out ushort physForm);
            p.Level.Blockchange(x, y, z, physForm, false, args);
            return 2;
        }
        internal static int ODoor(Player p, ushort old, ushort x, ushort y, ushort z)
        {
            if (old == 155 || old == 177)
            {
                p.Level.Blockchange(x, y, z, p.Level.Props[old].oDoorBlock);
                return 2;
            }
            return 0;
        }
        internal static int DoPortal(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            if (!Portal.Handle(p, x, y, z))
            {
                return p.ChangeBlock(x, y, z, 0);
            }
            return 0;
        }
        internal static int DoMessageBlock(Player p, ushort _, ushort x, ushort y, ushort z)
        {
            if (!MessageBlock.Handle(p, x, y, z, true))
            {
                return p.ChangeBlock(x, y, z, 0);
            }
            return 0;
        }
    }
}
