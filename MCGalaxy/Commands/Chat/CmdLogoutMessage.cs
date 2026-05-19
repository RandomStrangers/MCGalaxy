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
    public sealed class CmdLogoutMessage : EntityPropertyCmd
    {
        public override string Name => "LogoutMessage";
        public override string Shortcut => "LogoutMsg";
        public override string Type => CommandTypes.Chat;
        public override LevelPermission DefaultRank => LevelPermission.Operator;
        public override CommandPerm[] ExtraPerms => new[] { new CommandPerm(LevelPermission.Operator, "can change the logout message of others") };
        public override CommandAlias[] Aliases => new[] {
                new CommandAlias("OLogoutMessage", "-other")
            };
        public override void Use(Player p, string message, CommandData data) => UsePlayer(p, data, message, "logout message");
        protected override void SetPlayerData(Player p, string target, string msg) => PlayerOperations.SetLogoutMessage(p, target, msg);
        public override void Help(Player p)
        {
            p.Message("&T/LogoutMessage <message>");
            p.Message("&H Sets your logout message");
            p.Message("&T/OLogoutMessage [player] <message>");
            p.Message("&H Sets the logout message of another player");
            p.Message("&H  Leave <message> blank to reset it.");
            p.Message("&HYour logout message is currently: &S{0}", PlayerInfo.GetLogoutMessage(p));
        }
    }
}