/*
    Copyright 2011 MCForge
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
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Maths;
using System;
namespace MCGalaxy.Commands.Building
{
    public sealed class CmdFill : DrawCmd
    {
        public override string Name => "Fill";
        public override string Shortcut => "f";
        public override LevelPermission DefaultRank => LevelPermission.AdvBuilder;
        public override CommandAlias[] Aliases => new[] { new CommandAlias("F3D"), new CommandAlias("F2D", "2d"),
                    new CommandAlias("Fill3D"), new CommandAlias("Fill2D", "2d") };
        protected override int MarksCount => 1;
        protected override string SelectionType => "origin";
        protected override string PlaceMessage => "Place or break a block to mark the area you wish to fill.";
        protected override DrawMode GetMode(string[] parts)
        {
            string msg = parts[0];
            if (msg == "normal") return DrawMode.solid;
            if (msg == "up") return DrawMode.up;
            if (msg == "down") return DrawMode.down;
            return msg == "layer"
                ? DrawMode.layer
                : msg == "vertical_x"
                ? DrawMode.verticalX
                : msg == "vertical_z" ? DrawMode.verticalZ : msg == "2d" ? DrawMode.volcano : DrawMode.normal;
        }
        protected override DrawOp GetDrawOp(DrawArgs dArg) => new FillDrawOp();
        protected override void GetBrush(DrawArgs dArgs)
        {
            int endCount = 0;
            if (IsConfirmed(dArgs.Message)) endCount++;
            dArgs.BrushArgs = dArgs.Message.Splice(dArgs.ModeArgsCount, endCount);
        }
        protected override bool DoDraw(Player p, Vec3S32[] marks, object state, ushort block)
        {
            DrawArgs dArgs = (DrawArgs)state;
            ushort x = (ushort)marks[0].X, y = (ushort)marks[0].Y, z = (ushort)marks[0].Z;
            ushort old = p.Level.GetBlock(x, y, z);
            if (!CommandParser.IsBlockAllowed(p, "fill over", old)) return false;
            bool is2D = dArgs.Mode == DrawMode.volcano;
            if (is2D) dArgs.Mode = Calc2DFill(p, marks);
            FillDrawOp op = (FillDrawOp)dArgs.Op;
            op.Positions = FillDrawOp.FloodFill(p, p.Level.PosToInt(x, y, z), old, dArgs.Mode);
            int count = op.Positions.Count;
            bool confirmed = IsConfirmed(dArgs.Message), success = true;
            if (count < p.group.DrawLimit && count > p.Level.ReloadThreshold && !confirmed)
            {
                p.Message("This fill would affect {0} blocks.", count);
                p.Message("If you still want to fill, type &T/Fill {0} confirm", dArgs.Message);
            }
            else
            {
                success = base.DoDraw(p, marks, state, block);
            }
            if (is2D) dArgs.Mode = DrawMode.volcano;
            op.Positions = null;
            return success;
        }
        static DrawMode Calc2DFill(Player p, Vec3S32[] marks)
        {
            int lenX = Math.Abs(p.Pos.BlockX - marks[0].X),
                lenY = Math.Abs(p.Pos.BlockY - marks[0].Y),
                lenZ = Math.Abs(p.Pos.BlockZ - marks[0].Z);
            return lenY >= lenX && lenY >= lenZ ? DrawMode.layer : lenX >= lenZ ? DrawMode.verticalX : DrawMode.verticalZ;
        }
        static bool IsConfirmed(string message) => message.CaselessEq("confirm") || message.CaselessEnds(" confirm");
        public override void Help(Player p)
        {
            p.Message("&T/Fill <brush args>");
            p.Message("&HFills the area specified with the output of your current brush.");
            p.Message("&T/Fill [mode] <brush args>");
            p.Message("&HModes: &fnormal/up/down/layer/vertical_x/vertical_z/2d");
            p.Message(BrushHelpLine);
        }
    }
}
