/*
    Copyright 2015-2024 MCGalaxy
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
using System.Collections.Generic;
using System.IO;
using System.Net;
namespace MCGalaxy.Commands.Moderation
{
    /// <summary> Provides common helper methods for moderation commands. </summary>
    public static class ModActionCmd
    {
        /// <summary> Expands @[rule number] to the actual rule with that number. </summary>
        public static string ExpandReason(Player p, string reason)
        {
            string expanded = TryExpandReason(reason, out int ruleNum);
            if (expanded != null) return expanded;
            Dictionary<int, string> sections = GetRuleSections();
            p.Message("No rule has number \"{0}\". Current rule numbers are: {1}",
                      ruleNum, sections.Keys.Join(n => n.ToString()));
            return null;
        }
        public static string TryExpandReason(string reason, out int ruleNum)
        {
            ruleNum = 0;
            if (reason.Length == 0 || reason[0] != '@') return reason;
            reason = reason.Substring(1);
            if (!NumberUtils.TryParseInt32(reason, out ruleNum)) return "@" + reason;
            Dictionary<int, string> sections = GetRuleSections();
            sections.TryGetValue(ruleNum, out string rule); 
            return rule;
        }
        static Dictionary<int, string> GetRuleSections()
        {
            Dictionary<int, string> sections = new();
            if (!File.Exists(Paths.RulesFile)) return sections;
            List<string> rules = Utils.ReadAllLinesList(Paths.RulesFile);
            foreach (string rule in rules)
                ParseRule(rule, sections);
            return sections;
        }
        static void ParseRule(string rule, Dictionary<int, string> sections)
        {
            int ruleNum = -1;
            rule = Colors.Strip(rule);
            for (int i = 0; i < rule.Length; i++)
            {
                char c = rule[i];
                bool isNumber = c >= '0' && c <= '9',
                    isLetter = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
                if (!isNumber && !isLetter) continue;
                if (isLetter && ruleNum == -1) return;
                if (isNumber)
                {
                    if (ruleNum == -1) ruleNum = 0;
                    ruleNum *= 10;
                    ruleNum += c - '0';
                }
                else
                {
                    sections[ruleNum] = rule.Substring(i);
                    return;
                }
            }
        }
        static void ChangeOnlineRank(Player who, Group newRank)
        {
            who.group = newRank;
            who.AllowBuild = who.Level.BuildAccess.CheckAllowed(who);
            if (who.hidden && who.hideRank < who.Rank) who.hideRank = who.Rank;
            who.SetColor(PlayerInfo.DefaultColor(who));
            who.SetPrefix();
            Entities.DespawnEntities(who, false);
            who.Session.SendSetUserType(who.UserType());
            who.SendCurrentBlockPermissions();
            Entities.SpawnEntities(who, false);
            CheckBlockBindings(who);
            who.CheckIsUnverified();
        }
        /// <summary> Changes the rank of the given player from the old to the new rank. </summary>
        internal static void ChangeRank(string name, Group oldRank, Group newRank,
                                        Player who, bool saveToNewRank = true)
        {
            if (who != null) ChangeOnlineRank(who, newRank);
            Server.reviewlist.Remove(name);
            oldRank.Players.Remove(name);
            oldRank.Players.Save();
            if (!saveToNewRank) return;
            newRank.Players.Add(name);
            newRank.Players.Save();
        }
        static void CheckBlockBindings(Player who)
        {
            ushort block = who.ModeBlock;
            if (block != Block.Invalid && !CommandParser.IsBlockAllowed(who, "place", block))
            {
                who.ModeBlock = Block.Invalid;
                who.Message("   Hence, &b{0} &Smode was turned &cOFF",
                            Block.GetName(who, block));
            }
            for (int b = 0; b < who.BlockBindings.Length; b++)
            {
                block = who.BlockBindings[b];
                if (block == b) continue;
                if (!CommandParser.IsBlockAllowed(who, "place", block))
                {
                    who.BlockBindings[b] = (ushort)b;
                    who.Message("   Hence, binding for &b{0} &Swas unbound",
                                Block.GetName(who, (ushort)b));
                }
            }
        }
        internal static Group CheckTarget(Player p, CommandData data, string action, string target)
        {
            if (p.name.CaselessEq(target))
            {
                p.Message("You cannot {0} yourself", action);
                return null;
            }
            Group group = PlayerInfo.GetGroup(target);
            return !Command.CheckRank(p, data, target, group.Permission, action, false) ? null : group;
        }
        /// <summary> Finds the matching name(s) for the input name,
        /// and requires a confirmation message for non-existent players. </summary>
        internal static string FindName(Player p, string action, string cmd,
                                        string cmdSuffix, string name, ref string reason)
        {
            if (!Formatter.ValidPlayerName(p, name)) return null;
            string match = MatchName(p, ref name);
            string confirmed = IsConfirmed(reason);
            if (confirmed != null) reason = confirmed;
            if (match != null)
            {
                if (Server.ToRawUsername(match).CaselessEq(Server.ToRawUsername(name)))
                    return match;
                p.Message("1 player matches \"{0}\": {1}", name, match);
            }
            if (confirmed != null) return name;
            string msgReason = string.IsNullOrEmpty(reason) ? "" : " " + reason;
            p.Message("If you still want to {0} \"{1}\", use &T/{3} {1}{4}{2} confirm",
                           action, name, msgReason, cmd, cmdSuffix);
            return null;
        }
        static string MatchName(Player p, ref string name)
        {
            Player target = PlayerInfo.FindMatches(p, name, out int matches);
            if (matches > 1) return null;
            if (matches == 1) 
            { 
                name = target.name; 
                return name; 
            }
            p.Message("Searching PlayerDB...");
            return PlayerDB.MatchNames(p, name);
        }
        static string IsConfirmed(string reason) => reason == null
                ? null
                : reason.CaselessEq("confirm")
                ? ""
                : reason.CaselessEnds(" confirm") ? reason.Substring(0, reason.Length - " confirm".Length) : null;
        static bool ValidIP(string str) => str.IndexOf(':') >= 0 || str.Split('.').Length == 4;
        /// <summary> Attempts to either parse the message directly as an IP,
        /// or finds the IP of the account whose name matches the message. </summary>
        /// <remarks> "@input" can be used to always find IP by matching account name. <br/>
        /// Warns the player if the input matches both an IP and an account name. </remarks>
        internal static string FindIP(Player p, string message, string cmd, out string name)
        {
            name = null;
            if (IPAddress.TryParse(message, out _) && ValidIP(message))
            {
                string account = Server.FromRawUsername(message);
                if (PlayerDB.FindName(account) == null) return message;
                p.Message("Note: \"{0}\" is both an IP and an account name. "
                          + "If you meant the account, use &T/{1} @{0}", message, cmd);
                return message;
            }
            if (message[0] == '@') message = message.Remove(0, 1);
            Player who = PlayerInfo.FindMatches(p, message);
            if (who != null) 
            { 
                name = who.name; 
                return who.ip;
            }
            p.Message("Searching PlayerDB..");
            name = PlayerDB.FindOfflineIPMatches(p, message, out string dbIP);
            return dbIP;
        }
    }
}
