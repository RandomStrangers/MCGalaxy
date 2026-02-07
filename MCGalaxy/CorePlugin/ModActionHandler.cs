/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
using MCGalaxy.Commands;
using MCGalaxy.Commands.Moderation;
using MCGalaxy.DB;
using MCGalaxy.Events;
using MCGalaxy.Tasks;
using System;
namespace MCGalaxy.Core
{
    internal static class ModActionHandler
    {
        internal static void HandleModAction(ModAction action)
        {
            switch (action.Type)
            {
                case 6: DoFreeze(action); break;
                case 7: DoUnfreeze(action); break;
                case 4: DoMute(action); break;
                case 5: DoUnmute(action); break;
                case 0: DoBan(action); break;
                case 1: DoUnban(action); break;
                case 2: DoBanIP(action); break;
                case 3: DoUnbanIP(action); break;
                case 8: DoWarn(action); break;
                case 9: DoRank(action); break;
                case 12: DoNote(action); break;
                case 13: DoNote(action); break;
            }
        }
        static void LogAction(ModAction e, Player _, string action)
        {
            // TODO should use per-player nick settings
            string targetNick = e.Actor.FormatNick(e.Target);
            if (e.Announce)
            {
                // TODO: Chat.MessageFrom if target is online?
                Player who = PlayerInfo.FindExact(e.Target);
                // TODO: who.SharesChatWith
                Chat.Message(1, e.FormatMessage(targetNick, action),
                             null, null, true);
            }
            else
            {
                Chat.Message(4, "To Ops: " + e.FormatMessage(targetNick, action),
                             Chat.OpchatPerms, null, true);
            }
            action = Colors.StripUsed(action);
            string suffix = "";
            if (e.Duration.Ticks != 0) suffix = " &Sfor " + e.Duration.Shorten();
            Logger.Log(3, "{0} was {1} by {2}",
                       e.Target, action, e.Actor.name + suffix);
        }
        static void DoFreeze(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            if (who != null) who.frozen = true;
            LogAction(e, who, "&bfrozen");
            Server.frozen.Update(e.Target, FormatModTaskData(e));
            ModerationTasks.FreezeCalcNextRun();
            Server.frozen.Save();
        }
        static void DoUnfreeze(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            if (who != null) who.frozen = false;
            LogAction(e, who, "&adefrosted");
            Server.frozen.Remove(e.Target);
            ModerationTasks.FreezeCalcNextRun();
            Server.frozen.Save();
        }
        static void DoMute(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            if (who != null) who.muted = true;
            LogAction(e, who, "&8muted");
            Server.muted.Update(e.Target, FormatModTaskData(e));
            ModerationTasks.MuteCalcNextRun();
            Server.muted.Save();
        }
        static void DoUnmute(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            if (who != null) who.muted = false;
            LogAction(e, who, "&aun-muted");
            Server.muted.Remove(e.Target);
            ModerationTasks.MuteCalcNextRun();
            Server.muted.Save();
        }
        static void DoBan(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            LogAction(e, who, "&8banned");
            if (e.Duration.Ticks != 0)
            {
                Server.tempBans.Update(e.Target, Ban.PackTempBanData(e.Reason, e.Actor.name, DateTime.UtcNow.Add(e.Duration)));
                Server.tempBans.Save();
                who?.Kick("Banned for " + e.Duration.Shorten(true) + "." + e.ReasonSuffixed);
            }
            else
            {
                Ban.DeleteBan(e.Target);
                Ban.BanPlayer(e.Actor, e.Target, e.Reason, !e.Announce, e.TargetGroup.Name);
                ModActionCmd.ChangeRank(e.Target, e.targetGroup, Group.BannedRank, who);
                who?.Kick("Banned by " + e.Actor.ColoredName + ": &S" + (e.Reason.Length == 0 ? Server.Config.DefaultBanMessage : e.Reason),
                             "Banned by " + e.Actor.ColoredName + ": &f" + (e.Reason.Length == 0 ? Server.Config.DefaultBanMessage : e.Reason));
            }
        }
        static void DoUnban(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            LogAction(e, who, "&8unbanned");
            if (Server.tempBans.Remove(e.Target))
            {
                Server.tempBans.Save();
            }
            if (!Group.BannedRank.Players.Contains(e.Target)) return;
            Ban.DeleteUnban(e.Target);
            Ban.UnbanPlayer(e.Actor, e.Target, e.Reason);
            ModActionCmd.ChangeRank(e.Target, Group.BannedRank, Group.DefaultRank, who, false);
            string ip = PlayerDB.FindIP(e.Target);
            if (ip != null && Server.bannedIP.Contains(ip))
            {
                e.Actor.Message("NOTE: {0} IP is still banned.", Pronouns.GetFor(e.Target)[0].Object);
            }
        }
        static void LogIPAction(ModAction e, string type)
        {
            ItemPerms perms = CommandExtraPerms.Find("WhoIs", 1);
            Chat.Message(1, e.FormatMessage("An IP", type), perms,
                         FilterNotItemPerms, true);
            Chat.Message(1, e.FormatMessage(e.Target, type), perms,
                         Chat.FilterPerms, true);
        }
        static bool FilterNotItemPerms(Player pl, object arg) => !Chat.FilterPerms(pl, arg);
        static void DoBanIP(ModAction e)
        {
            LogIPAction(e, "&8IP banned");
            Logger.Log(3, "IP-BANNED: {0} by {1}.{2}",
                       e.Target, e.Actor.name, e.ReasonSuffixed);
            Server.bannedIP.Update(e.Target, e.Reason);
            Server.bannedIP.Save();
        }
        static void DoUnbanIP(ModAction e)
        {
            LogIPAction(e, "&8IP unbanned");
            Logger.Log(3, "IP-UNBANNED: {0} by {1}.",
                       e.Target, e.Actor.name);
            Server.bannedIP.Remove(e.Target);
            Server.bannedIP.Save();
        }
        static void DoWarn(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            if (who != null)
            {
                LogAction(e, who, "&ewarned");
                if (who.warn == 0)
                {
                    who.Message("Do it again twice and you will get kicked!");
                }
                else if (who.warn == 1)
                {
                    who.Message("Do it one more time and you will get kicked!");
                }
                else if (who.warn == 2)
                {
                    Chat.MessageGlobal("{0} &Swas warn-kicked by {1}", who.ColoredName, e.Actor.ColoredName);
                    who.Kick("by " + e.Actor.ColoredName + "&S: " + e.Reason, "Kicked by " + e.Actor.ColoredName + ": &f" + e.Reason);
                }
                who.warn++;
            }
            else
            {
                if (!Server.Config.LogNotes)
                {
                    e.Actor.Message("Notes logging must be enabled to warn offline players."); 
                    return;
                }
                LogAction(e, who, "&ewarned");
            }
        }
        static void DoRank(ModAction e)
        {
            Player who = PlayerInfo.FindExact(e.Target);
            Group newRank = (Group)e.Metadata;
            string action = newRank.Permission >= e.TargetGroup.Permission ? "promoted to " : "demoted to ";
            LogAction(e, who, action + newRank.ColoredName);
            if (who != null && e.Announce)
            {
                who.Message("You are now ranked " + newRank.ColoredName + "&S, type /Help for your new set of commands.");
            }
            if (Server.tempRanks.Remove(e.Target))
            {
                ModerationTasks.TemprankCalcNextRun();
                Server.tempRanks.Save();
            }
            WriteRankInfo(e, newRank);
            if (e.Duration != TimeSpan.Zero) AddTempRank(e, newRank);
            ModActionCmd.ChangeRank(e.Target, e.TargetGroup, newRank, who);
        }
        static void WriteRankInfo(ModAction e, Group newRank) => Server.RankInfo.Append(e.Target + " " + e.Actor.name + " " + DateTime.UtcNow.ToUnixTime() + " " + newRank.Name
                + " " + e.TargetGroup.Name + " " + e.Reason.Replace(" ", "%20"));
        static void AddTempRank(ModAction e, Group newRank)
        {
            Server.tempRanks.Update(e.Target, FormatModTaskData(e) + " " + e.TargetGroup.Name + " " + newRank.Name);
            ModerationTasks.TemprankCalcNextRun();
            Server.tempRanks.Save();
        }
        static string FormatModTaskData(ModAction e)
        {
            DateTime end = DateTime.MaxValue.AddYears(-1);
            if (e.Duration != TimeSpan.Zero)
            {
                try
                {
                    end = DateTime.UtcNow.Add(e.Duration);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
            return e.Actor.name + " " + DateTime.UtcNow.ToUnixTime() + " " + end.ToUnixTime();
        }
        static void DoNote(ModAction e)
        {
            if (!Server.Config.LogNotes)
            {
                e.Actor.Message("Notes logging must be enabled to note players.");
                return;
            }
            LogAction(e, PlayerInfo.FindExact(e.Target), "&egiven a note");
        }
    }
}
