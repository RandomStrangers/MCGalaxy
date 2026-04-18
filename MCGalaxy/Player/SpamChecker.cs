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
using MCGalaxy.Events;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public sealed class SpamChecker
    {
        public SpamChecker(Player p)
        {
            this.p = p;
            blockLog = new(Server.Config.BlockSpamCount);
            chatLog = new(Server.Config.ChatSpamCount);
            cmdLog = new(Server.Config.CmdSpamCount);
        }
        public readonly Player p;
        public readonly object chatLock = new(), cmdLock = new();
        public readonly List<DateTime> blockLog, chatLog, cmdLog;
        public void Clear()
        {
            blockLog.Clear();
            lock (chatLock)
                chatLog.Clear();
            lock (cmdLock)
                cmdLog.Clear();
        }
        public bool CheckBlockSpam()
        {
            if (p.ignoreGrief || !Server.Config.BlockSpamCheck || blockLog.AddSpamEntry(Server.Config.BlockSpamCount, Server.Config.BlockSpamInterval)) return false;
            Chat.MessageFromOps(p, "λNICK &Wwas kicked from " + p.Level.name + " for suspected griefing.");
            Logger.Log(LogType.SuspiciousActivity,
                       "{0} was kicked from {1} for block spam ({2} blocks in {3} seconds)",
                       p.name, p.Level.name, blockLog.Count, DateTime.UtcNow - blockLog[0]);
            p.Kick("You were kicked by antigrief system. Slow down.");
            return true;
        }
        public bool CheckChatSpam()
        {
            Player.lastMSG = p.name;
            if (!Server.Config.ChatSpamCheck || p.IsSuper) return false;
            lock (chatLock)
            {
                if (chatLog.AddSpamEntry(Server.Config.ChatSpamCount, Server.Config.ChatSpamInterval))
                    return false;
                OnModActionEvent.Call(new(p.name, Player.NASConsole, ModActionType.Muted, "&0Auto mute for spamming", Server.Config.ChatSpamMuteTime));
                return true;
            }
        }
        public bool CheckCommandSpam()
        {
            if (!Server.Config.CmdSpamCheck || p.IsSuper) return false;
            lock (cmdLock)
            {
                if (cmdLog.AddSpamEntry(Server.Config.CmdSpamCount, Server.Config.CmdSpamInterval))
                    return false;
                p.Message("You have been blocked from using commands for "
                          + Server.Config.CmdSpamBlockTime.Shorten(true, true) + " due to spamming");
                p.cmdUnblocked = DateTime.UtcNow.Add(Server.Config.CmdSpamBlockTime);
                return true;
            }
        }
    }
}