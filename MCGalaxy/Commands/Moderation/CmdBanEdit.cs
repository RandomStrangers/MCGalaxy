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
namespace MCGalaxy.Commands.Moderation
{
    public sealed class CmdBanEdit : Command2
    {
        public override string Name => "BanEdit";
        public override string Shortcut => "be";
        public override string Type => CommandTypes.Moderation;
        public override sbyte DefaultRank => 80;
        public override void Use(Player p, string message, CommandData data)
        {
            string[] args = message.SplitSpaces(2);
            if (args.Length < 2) { Help(p); return; }
            if (!Ban.ChangeBanReason(args[0], args[1]))
            {
                p.Message("That player isn't banned.");
            }
            else
            {
                p.Message("Set ban reason for &0{0} &Sto: &2{1}", args[0], args[1]);
            }
        }
        public override void Help(Player p)
        {
            p.Message("&T/BanEdit [username] [reason]");
            p.Message("&HEdits reason of ban for the user.");
        }
    }
}
