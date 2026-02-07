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
namespace MCGalaxy.Commands.Chatting
{
    public class CmdTitle : EntityPropertyCmd
    {
        public override string Name => "Title";
        public override string Type => CommandTypes.Chat;
        public override sbyte DefaultRank => 100;
        public override CommandPerm[] ExtraPerms => new[] { new CommandPerm(100, "can change the title of others") };
        public override CommandAlias[] Aliases => new[] { new CommandAlias("XTitle", "-own") };
        public override void Use(Player p, string message, CommandData data) => UsePlayer(p, data, message, "title");
        protected override void SetPlayerData(Player p, string target, string title) => PlayerOperations.SetTitle(p, target, title);
        public override void Help(Player p)
        {
            p.Message("&T/Title [player] [title]");
            p.Message("&HSets the title of [player]");
            p.Message("&H  If [title] is not given, removes [player]'s title.");
        }
    }
}
