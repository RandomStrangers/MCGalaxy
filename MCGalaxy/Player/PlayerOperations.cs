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
using MCGalaxy.DB;
using MCGalaxy.Network;
namespace MCGalaxy
{
    /// <summary>
    /// Higher level operations that send feedback messages and may fail on invalid input.
    /// </summary>
    /// <remarks> See PlayerActions.cs for lower level operations. (TODO: Actually respect this distinction across both classes) </remarks>
    public static class PlayerOperations
    {
        /// <summary>
        /// Attempts to set the skin for the given target, which will be saved across play sessions.
        /// </summary>
        public static void SetSkin(Player p, string target, string skin)
        {
            string rawName = Server.ToRawUsername(target);
            skin = HttpUtil.FilterSkin(p, skin, rawName);
            if (skin == null) return;
            Player who = PlayerInfo.FindExact(target);
            if (p == who)
                p.Message("Changed your own skin to &c" + skin);
            else
                MessageAction(p, target, who, "λACTOR &Schanged λTARGET skin to &c" + skin);
            PlayerActions.SetSkin(target, skin);
        }
        /// <summary> Attempts to change the login message of the target player </summary>
        /// <remarks> Not allowed when players who cannot speak (e.g. muted) </remarks>
        public static bool SetLoginMessage(Player p, string target, string message)
        {
            switch (message.Length)
            {
                case 0:
                    p.Message("Login message of {0} &Swas removed", p.FormatNick(target));
                    break;
                default:
                    if (!p.CheckCanSpeak("change login messages")) return false;
                    p.Message("Login message of {0} &Swas changed to: {1}",
                              p.FormatNick(target), message);
                    break;
            }
            PlayerDB.SetLoginMessage(target, message);
            return true;
        }
        /// <summary> Attempts to change the logout message of the target player </summary>
        /// <remarks> Not allowed when players who cannot speak (e.g. muted) </remarks>
        public static bool SetLogoutMessage(Player p, string target, string message)
        {
            switch (message.Length)
            {
                case 0:
                    p.Message("Logout message of {0} &Swas removed", p.FormatNick(target));
                    break;
                default:
                    if (!p.CheckCanSpeak("change logout messages")) return false;
                    p.Message("Logout message of {0} &Swas changed to: {1}",
                              p.FormatNick(target), message);
                    break;
            }
            PlayerDB.SetLogoutMessage(target, message);
            return true;
        }
        /// <summary> Attempts to change the nickname of the target player </summary>
        /// <remarks> Not allowed when players who cannot speak (e.g. muted) </remarks>
        public static bool SetNick(Player p, string target, string nick)
        {
            if (Colors.Strip(nick).Length >= 30)
            {
                p.Message("Nick must be under 30 letters.");
                return false;
            }
            Player who = PlayerInfo.FindExact(target);
            switch (nick.Length)
            {
                case 0:
                    MessageAction(p, target, who, "λACTOR &Sremoved λTARGET nick");
                    nick = Server.ToRawUsername(target);
                    break;
                default:
                    {
                        if (!p.CheckCanSpeak("change nicks")) return false;
                        string color = who != null ? who.color : Group.GroupIn(target).Color;
                        MessageAction(p, target, who, "λACTOR &Schanged λTARGET nick to " + color + nick);
                        break;
                    }
            }
            if (who != null)
            {
                who.DisplayName = nick;
                TabList.Update(who, true);
            }
            PlayerDB.SetNick(target, nick);
            return true;
        }
        /// <summary> Attempts to change the title of the target player </summary>
        /// <remarks> Not allowed when players who cannot speak (e.g. muted) </remarks>
        public static bool SetTitle(Player p, string target, string title)
        {
            if (title.Length >= 20)
            {
                p.Message("&WTitle must be under 20 characters.");
                return false;
            }
            Player who = PlayerInfo.FindExact(target);
            switch (title.Length)
            {
                case 0:
                    MessageAction(p, target, who, "λACTOR &Sremoved λTARGET title");
                    break;
                default:
                    if (!p.CheckCanSpeak("change titles")) return false;
                    MessageAction(p, target, who, "λACTOR &Schanged λTARGET title to &b[" + title + "&b]");
                    break;
            }
            if (who != null) who.title = title;
            who?.SetPrefix();
            PlayerDB.Update(target, PlayerData.ColumnTitle, title.UnicodeToCp437());
            return true;
        }
        /// <summary> Attempts to change the title color of the target player </summary>
        public static bool SetTitleColor(Player p, string target, string name)
        {
            string color = "";
            Player who = PlayerInfo.FindExact(target);
            switch (name.Length)
            {
                case 0:
                    MessageAction(p, target, who, "λACTOR &Sremoved λTARGET title color");
                    break;
                default:
                    color = Matcher.FindColor(p, name);
                    if (color == null) return false;
                    MessageAction(p, target, who, "λACTOR &Schanged λTARGET title color to " + color + Colors.Name(color));
                    break;
            }
            if (who != null) who.titlecolor = color;
            who?.SetPrefix();
            PlayerDB.Update(target, PlayerData.ColumnTColor, color);
            return true;
        }
        /// <summary> Attempts to change the color of the target player </summary>
        public static bool SetColor(Player p, string target, string name)
        {
            Player who = PlayerInfo.FindExact(target);
            string color;
            switch (name.Length)
            {
                case 0:
                    color = Group.GroupIn(target).Color;
                    PlayerDB.Update(target, PlayerData.ColumnColor, "");
                    MessageAction(p, target, who, "λACTOR &Sremoved λTARGET color");
                    break;
                default:
                    color = Matcher.FindColor(p, name);
                    if (color == null) return false;
                    PlayerDB.Update(target, PlayerData.ColumnColor, color);
                    MessageAction(p, target, who, "λACTOR &Schanged λTARGET color to " + color + Colors.Name(color));
                    break;
            }
            who?.UpdateColor(color);
            return true;
        }
        /// <remarks> λACTOR is replaced with nick of player performing the action.
        /// λTARGET is replaced with either "their" or "[target nick]'s", depending
        /// on whether the actor is the same player as the target or not </remarks>
        internal static void MessageAction(Player actor, string target, Player who, string message)
        {
            bool global = who == null || actor.IsSuper
                            || (!actor.Level.SeesServerWideChat && actor.Level != who.Level);
            if (actor == who)
            {
                message = message.Replace("λACTOR", "λNICK")
                                 .Replace("λTARGET", actor.Pronouns.Object);
                Chat.MessageFrom(who, message);
            }
            else if (!global)
            {
                message = message.Replace("λACTOR", actor.ColoredName)
                                 .Replace("λTARGET", "λNICK&S's");
                Chat.MessageFrom(who, message);
            }
            else
            {
                message = message.Replace("λACTOR", actor.ColoredName)
                                 .Replace("λTARGET", Player.NASConsole.FormatNick(target) + "&S's");
                Chat.MessageAll(message);
            }
        }
        public static void TeleportToCoords(Player p, Position pos, Orientation ori)
        {
            SavePreTeleportState(p);
            p.SendPosition(pos, ori);
        }
        public static void TeleportToEntity(Player p, Entity dst)
        {
            SavePreTeleportState(p);
            Level lvl = dst.Level;
            if (dst is Player target && target.Loading)
            {
                p.Message("Waiting for {0} &Sto spawn..", p.FormatNick(target));
                target.BlockUntilLoad(10);
            }
            if (p.Level != lvl && !PlayerActions.ChangeMap(p, lvl.name)) return;
            p.BlockUntilLoad(10);
            p.SendPosition(dst.Pos, dst.Rot);
        }
        public static void SavePreTeleportState(Player p)
        {
            p.PreTeleportMap = p.Level.name;
            p.PreTeleportPos = p.Pos;
            p.PreTeleportRot = p.Rot;
        }
    }
}
