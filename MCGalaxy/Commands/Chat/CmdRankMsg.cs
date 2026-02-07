/*
    Written by Jack1312
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
    public sealed class CmdRankMsg : Command2
    {
        public override string Name => "RankMsg";
        public override string Shortcut => "rm";
        public override string Type => CommandTypes.Chat;
        public override LevelPermission DefaultRank => LevelPermission.AdvBuilder;
        public override bool UseableWhenFrozen => true;
        public override bool MessageBlockRestricted => true;
        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0) { Help(p); return; }
            if (!MessageCmd.CanSpeak(p, Name)) return;
            string[] args = message.SplitSpaces(2);
            string rank = args.Length == 1 ? p.group.Name : args[0];
            string text = args[args.Length - 1];
            Group grp = Matcher.FindRanks(p, rank);
            if (grp == null) return;
            string msg = grp.Color + "<" + grp.Name + ">λNICK: &f" + text;
            Chat.MessageChat(ChatScope.Rank, p, msg, grp.Permission, null);
        }
        public override void Help(Player p)
        {
            p.Message("&T/RankMsg [Rank] [Message]");
            p.Message("&HSends a message to the specified rank.");
            p.Message("&HNote: If no [rank] is given, player's rank is taken.");
        }
    }
}
