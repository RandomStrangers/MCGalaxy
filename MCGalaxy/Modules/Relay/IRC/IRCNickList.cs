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
using System.Collections.Generic;
namespace MCGalaxy.Modules.Relay.IRC
{
    public class IRCNickList
    {
        public readonly Dictionary<string, List<string>> userMap = new();
        public IRCBot bot;
        public void Clear() => userMap.Clear();
        public void OnLeftChannel(string userNick, string channel) => RemoveNick(userNick, GetNicks(channel));
        public void OnLeft(string userNick)
        {
            foreach (KeyValuePair<string, List<string>> chans in userMap)
                RemoveNick(userNick, chans.Value);
        }
        public void OnChangedNick(string userNick, string newNick)
        {
            foreach (KeyValuePair<string, List<string>> chans in userMap)
            {
                int index = FindNick(userNick, chans.Value);
                if (index >= 0)
                    chans.Value[index] = GetPrefix(chans.Value[index]) + newNick;
                else
                    bot.SendRaw(IRCCmds.Names(chans.Key));
            }
        }
        public void UpdateFor(string channel, string[] nicks)
        {
            List<string> chanNicks = GetNicks(channel);
            foreach (string n in nicks)
                UpdateNick(n, chanNicks);
        }
        public List<string> GetNicks(string channel)
        {
            foreach (KeyValuePair<string, List<string>> chan in userMap)
                if (chan.Key.CaselessEq(channel)) return chan.Value;
            List<string> nicks = new();
            userMap[channel] = nicks;
            return nicks;
        }
        public bool VerifyNick(string channel, string userNick, ref string error, ref bool foundAtAll)
        {
            List<string> chanNicks = GetNicks(channel);
            if (chanNicks.Count == 0) return false;
            int index = FindNick(userNick, chanNicks);
            if (index == -1) return false;
            foundAtAll = true;
            IRCControllerVerify verify = Server.Config.IRCVerify;
            if (verify == IRCControllerVerify.None) return true;
            if (verify == IRCControllerVerify.HalfOp)
            {
                string prefix = GetPrefix(chanNicks[index]);
                if (prefix.Length == 0 || prefix == "+")
                {
                    error = "You must be at least a half-op on the channel to use commands from IRC.";
                    return false;
                }
                return true;
            }
            else
            {
                foreach (string chan in bot.OpChannels)
                {
                    chanNicks = GetNicks(chan);
                    if (chanNicks.Count == 0) continue;
                    index = FindNick(userNick, chanNicks);
                    if (index != -1) return true;
                }
                error = "You must have joined the opchannel to use commands from IRC.";
                return false;
            }
        }
        public static void UpdateNick(string n, List<string> chanNicks)
        {
            int index = FindNick(n, chanNicks);
            if (index >= 0)
                chanNicks[index] = n;
            else
                chanNicks.Add(n);
        }
        public static void RemoveNick(string n, List<string> chanNicks)
        {
            int index = FindNick(n, chanNicks);
            if (index >= 0) chanNicks.RemoveAt(index);
        }
        public static int FindNick(string n, List<string> chanNicks)
        {
            if (chanNicks == null) return -1;
            string unprefixNick = Unprefix(n);
            for (int i = 0; i < chanNicks.Count; i++)
                if (unprefixNick == Unprefix(chanNicks[i]))
                    return i;
            return -1;
        }
        public static string Unprefix(string nick) => nick.Substring(GetPrefixLength(nick));
        public static string GetPrefix(string nick) => nick.Substring(0, GetPrefixLength(nick));
        public static int GetPrefixLength(string nick)
        {
            int prefixChars = 0;
            for (int i = 0; i < nick.Length; i++)
            {
                if (!IsNickChar(nick[i]))
                    prefixChars++;
                else
                    return prefixChars;
            }
            return prefixChars;
        }
        public static bool IsNickChar(char c) => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                c == '[' || c == ']' || c == '{' || c == '}' || c == '^' || c == '`' || c == '_' || c == '|';
    }
}