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
using MCGalaxy.Events.PlayerEvents;
namespace MCGalaxy.Commands.Moderation
{
    public sealed class CmdHide : Command2
    {
        public override string Name => "Hide";
        public override string Type => CommandTypes.Moderation;
        public override sbyte DefaultRank => 80;
        public override bool SuperUseable => false;
        public override bool UpdatesLastCmd => false;
        public override CommandPerm[] ExtraPerms => new[] { new CommandPerm(100, "can hide silently") };
        public override CommandAlias[] Aliases => new CommandAlias[] { new("XHide", "silent") };
        static void AnnounceOps(Player p, string msg) => Chat.MessageFrom(4, p, msg, new ItemPerms(p.hideRank), null, true);
        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length > 0 && p.possess.Length > 0)
            {
                p.Message("Stop your current possession first."); return;
            }
            bool silent = false;
            if (message.CaselessEq("silent"))
            {
                if (!CheckExtraPerm(p, data, 1)) return;
                silent = true;
            }
            Command adminchat = Find("AdminChat");
            Command opchat = Find("OpChat");
            Entities.GlobalDespawn(p, false);
            p.hidden = !p.hidden;
            if (p.hidden)
            {
                p.hideRank = data.Rank;
                AnnounceOps(p, "To Ops -λNICK&S- is now &finvisible");
                if (!silent)
                {
                    Chat.MessageFrom(0, p, "&c- λFULL &S" + PlayerInfo.GetLogoutMessage(p), null, null, true);
                }
                if (!p.opchat) opchat.Use(p, "", data);
                Server.hidden.Add(p.name);
                OnPlayerActionEvent.Call(p, 5);
            }
            else
            {
                AnnounceOps(p, "To Ops -λNICK&S- is now &fvisible");
                p.hideRank = -20;
                if (!silent)
                {
                    Chat.MessageFrom(0, p, "&a+ λFULL &S" + PlayerInfo.GetLoginMessage(p), null, null, true);
                }
                if (p.opchat) opchat.Use(p, "", data);
                if (p.adminchat) adminchat.Use(p, "", data);
                Server.hidden.Remove(p.name);
                OnPlayerActionEvent.Call(p, 6);
            }
            Entities.GlobalSpawn(p, false);
            TabList.Add(p, p);
            Server.hidden.Save(false);
        }
        public override void Help(Player p)
        {
            p.Message("&T/Hide &H- Toggles your visibility to other players, also toggles opchat.");
            p.Message("&T/Hide silent &H- Hides without showing join/leave message");
            p.Message("&HUse &T/OHide &Hto hide other players.");
        }
    }
}
