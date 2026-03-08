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
namespace MCGalaxy.Commands.Building
{
    public sealed class CmdOutline : DrawCmd
    {
        public override string Name => "Outline";
        public override LevelPermission DefaultRank => LevelPermission.AdvBuilder;
        protected override DrawOp GetDrawOp(DrawArgs dArgs)
        {
            Player p = dArgs.Player;
            if (dArgs.Message.Length == 0)
            {
                p.Message("Block name is required.");
                return null;
            }
            string[] parts = dArgs.Message.SplitSpaces(2);
            if (!CommandParser.GetBlock(p, parts[0], out ushort target)) return null;
            OutlineDrawOp op = new()
            {
                side = GetSides(dArgs.Message.SplitSpaces())
            };
            if (op.side == OutlineDrawOp.Side.Unspecified) op.side = OutlineDrawOp.Side.All;
            op.Target = target;
            return op;
        }
        OutlineDrawOp.Side GetSides(string[] parts)
        {
            if (parts.Length == 1) return OutlineDrawOp.Side.Unspecified;
            string type = parts[1];
            return type switch
            {
                "left" => OutlineDrawOp.Side.Left,
                "right" => OutlineDrawOp.Side.Right,
                "front" => OutlineDrawOp.Side.Front,
                "back" => OutlineDrawOp.Side.Back,
                _ => type == "down"
                ? OutlineDrawOp.Side.Down
                : type == "up"
                ? OutlineDrawOp.Side.Up
                : type == "layer" ? OutlineDrawOp.Side.Layer : type == "all" ? OutlineDrawOp.Side.All : OutlineDrawOp.Side.Unspecified
            };
        }
        protected override DrawMode GetMode(string[] parts) => GetSides(parts) == OutlineDrawOp.Side.Unspecified ? DrawMode.normal : DrawMode.solid;
        protected override void GetBrush(DrawArgs dArgs) => dArgs.BrushArgs = dArgs.Message.Splice(dArgs.ModeArgsCount + 1, 0);
        public override void Help(Player p)
        {
            p.Message("&T/Outline [block] <brush args>");
            p.Message("&HOutlines [block] with output of your current brush.");
            p.Message("&T/Outline [block] [mode] <brush args>");
            p.Message("&HModes: &fall/up/layer/down/left/right/front/back (default all)");
            p.Message(BrushHelpLine);
        }
    }
}
