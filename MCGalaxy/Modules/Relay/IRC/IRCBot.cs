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
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MCGalaxy.Network;
namespace MCGalaxy.Modules.Relay.IRC
{
    public enum IRCControllerVerify { None, HalfOp, OpChannel };
    public class IRCBot : RelayBot
    {
        public TcpClient client;
        public StreamReader reader;
        public StreamWriter writer;
        public string botNick, curNick;
        public readonly IRCNickList nicks;
        public bool ready, registered;
        public readonly Random rnd = new();
        public override string RelayName => "IRC";
        public override bool Enabled => Server.Config.UseIRC;
        public override string UserID => curNick;
        public override void LoadControllers() => Controllers = PlayerList.Load("ranks/IRC_Controllers.txt");
        public IRCBot() => nicks = new()
        {
            bot = this
        };
        public override void DoSendMessage(string channel, string message)
        {
            if (!ready) return;
            message = ConvertMessage(message);
            if (message.IndexOf('\n') == -1)
            {
                SendPrivMsg(channel, message);
                return;
            }
            string[] parts = message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
                SendPrivMsg(channel, part.Replace("\r", ""));
        }
        public void Raw(string message)
        {
            if (!Enabled || !Connected) return;
            SendRaw(message);
        }
        public void Join(string channel)
        {
            if (string.IsNullOrEmpty(channel)) return;
            SendRaw(IRCCmds.Join(channel));
        }
        public override bool CanReconnect => canReconnect;
        public override void DoConnect()
        {
            ready = false;
            botNick = Server.Config.IRCNick.Replace(" ", "");
            string host = Server.Config.IRCServer;
            int port = Server.Config.IRCPort;
            bool useSSL = Server.Config.IRCSSL;
            if (port == 6697) useSSL = true;
            curNick = botNick;
            bool usePass = Server.Config.IRCIdentify && Server.Config.IRCPassword.Length > 0;
            string serverPass = usePass ? Server.Config.IRCPassword : "*";
            client = new();
            client.Connect(host, port);
            Stream s = client.GetStream();
            if (useSSL) s = HttpUtil.WrapSSLStream(s, host);
            Encoding encoding = new UTF8Encoding(false);
            registered = false;
            writer = new(s, encoding)
            {
                AutoFlush = true
            };
            reader = new(s, encoding);
            SendRaw(IRCCmds.Pass(serverPass));
            SendRaw(IRCCmds.User(botNick, Server.SoftwareNameVersioned));
            SendRaw(IRCCmds.Nick(curNick));
        }
        public override void DoReadLoop()
        {
            string line;

            try
            {
                while ((line = reader.ReadLine()) != null) 
                { 
                    ParseLine(line); 
                }
            }
            finally
            {
                client.Close();
            }
        }
        public override void DoDisconnect(string reason)
        {
            nicks.Clear();

            try
            {
                SendRaw(IRCCmds.Quit(reason));
                client.Close();
            }
            catch
            {
            }
        }
        public override void UpdateConfig()
        {
            Channels = Server.Config.IRCChannels.SplitComma();
            OpChannels = Server.Config.IRCOpChannels.SplitComma();
            IgnoredUsers = Server.Config.IRCIgnored.SplitComma();
            LoadBannedCommands();
        }
        public static readonly string[] ircColors = new string[] 
        {
            "\u000300", "\u000301", "\u000302", "\u000303", "\u000304", "\u000305",
            "\u000306", "\u000307", "\u000308", "\u000309", "\u000310", "\u000311",
            "\u000312", "\u000313", "\u000314", "\u000315",
        },
        ircSingle = new string[] 
        {
            "\u00030", "\u00031", "\u00032", "\u00033", "\u00034", "\u00035",
            "\u00036", "\u00037", "\u00038", "\u00039",
        },
        ircReplacements = new string[]
        {
            "&f", "&0", "&1", "&2", "&c", "&4", "&5", "&6",
            "&e", "&a", "&3", "&b", "&9", "&d", "&8", "&7",
        };
        public static readonly Regex ircTwoColorCode = new("(\x03\\d{1,2}),\\d{1,2}");
        public override string ParseMessage(string input)
        {
            input = ircTwoColorCode.Replace(input, "$1");
            StringBuilder sb = new(input);
            for (long i = 0; i < ircColors.LongLength; i++)
                sb.Replace(ircColors[i], ircReplacements[i]);
            for (long i = 0; i < ircSingle.LongLength; i++)
                sb.Replace(ircSingle[i], ircReplacements[i]);
            SimplifyCharacters(sb);
            sb.Replace("\x02", "");
            sb.Replace("\x1D", "");
            sb.Replace("\x1F", "");
            sb.Replace("\x03", "&f");
            sb.Replace("\x0f", "&f");
            return sb.ToString();
        }
        public string ConvertMessage(string message)
        {
            if (string.IsNullOrEmpty(message.Trim())) message = ".";
            message = ConvertMessageCommon(message);
            message = message.Replace("%S", "&f");
            message = message.Replace("&S", "&f");
            message = message.Replace("&f", "\x03\x0F");
            message = ToIRCColors(message);
            return message;
        }
        public static string ToIRCColors(string input)
        {
            input = Colors.Escape(input);
            input = LineWrapper.CleanupColors(input, true, false);
            StringBuilder sb = new(input);
            for (long i = 0; i < ircColors.LongLength; i++)
                sb.Replace(ircReplacements[i], ircColors[i]);
            return sb.ToString();
        }
        public override bool CheckController(string userID, ref string error)
        {
            bool foundAtAll = false;
            foreach (string chan in Channels)
                if (nicks.VerifyNick(chan, userID, ref error, ref foundAtAll)) return true;
            foreach (string chan in OpChannels)
                if (nicks.VerifyNick(chan, userID, ref error, ref foundAtAll)) return true;
            if (!foundAtAll)
                error = "You are not on the bot's list of known users for some reason, please leave and rejoin.";
            return false;
        }
        public void AnnounceJoinLeave(string nick, string verb, string channel)
        {
            Logger.Log(LogType.RelayActivity, "{0} {1} channel {2}", nick, verb, channel);
            MessageInGame(nick, string.Format("&I(IRC) {0} {1} the{2} channel", nick, verb, OpChannels.CaselessContains(channel) ? " operator" : ""));
        }
        public static RelayUser NickToUser(string nick) => new()
        {
            ID = nick,
            Nick = nick
        };
        public void JoinChannels()
        {
            Logger.Log(LogType.RelayActivity, "Joining IRC channels...");
            foreach (string chan in Channels)
                Join(chan);
            foreach (string chan in OpChannels)
                Join(chan);
            ready = true;
        }
        public void Authenticate()
        {
            string nickServ = Server.Config.IRCNickServName;
            if (nickServ.Length == 0) return;
            if (Server.Config.IRCIdentify && Server.Config.IRCPassword.Length > 0)
            {
                Logger.Log(LogType.RelayActivity, "Identifying with " + nickServ);
                SendPrivMsg(nickServ, "IDENTIFY " + Server.Config.IRCPassword);
            }
        }
        public string GenNewNick()
        {
            if (curNick.Length < 30) return curNick + "_";
            int idx = rnd.Next(30 / 3);
            char val = (char)('A' + rnd.Next(26));
            return curNick.Substring(0, idx) + val + curNick.Substring(idx + 1);
        }
        public readonly object sendLock = new();
        public void SendRaw(string msg)
        {
            if (msg.Length > 510)
                msg = msg.Substring(0, 510);

            try
            {
                lock (sendLock)
                    writer.WriteLine(msg);
            }
            catch 
            {
            }
        }
        public void SendPrivMsg(string target, string message)
        {
            string cmd = "PRIVMSG " + target + " :";
            int maxLen = 512 - 63 - 30 - cmd.Length - 2;
            lock (sendLock)
                for (int idx = 0; idx < message.Length;)
                {
                    int partLen = Math.Min(maxLen, message.Length - idx);
                    string part = message.Substring(idx, partLen);

                    SendRaw(cmd + part);
                    idx += partLen;
                }
        }
        public void ParseLine(string line)
        {
            int index = 0;
            string prefix = IRCUtils.ExtractPrefix(line, ref index),
                cmd = IRCUtils.NextParam(line, ref index);
            if (int.TryParse(cmd, out int code))
                ParseReply(code, line, index);
            else
                ParseCommand(prefix, cmd, line, index);
        }
        public void ParseCommand(string user, string cmd, string line, int index)
        {
            string nick = IRCUtils.ExtractNick(user),
                msg, channel, target, newNick;
            switch (cmd)
            {
                case "PING":
                    msg = IRCUtils.NextAll(line, ref index);
                    SendRaw(IRCCmds.Pong(msg));
                    break;
                case "ERROR":
                    msg = IRCUtils.NextAll(line, ref index);
                    Logger.Log(LogType.RelayActivity, "IRC Error: " + msg);
                    break;
                case "NOTICE":
                    target = IRCUtils.NextParam(line, ref index);
                    msg = IRCUtils.NextAll(line, ref index);
                    if (IRCUtils.IsValidChannel(target))
                    {
                    }
                    else if (msg.CaselessStarts("You are now identified"))
                        JoinChannels();
                    break;
                case "JOIN":
                    channel = IRCUtils.NextParam(line, ref index);
                    SendRaw(IRCCmds.Names(channel));
                    AnnounceJoinLeave(nick, "joined", channel);
                    break;
                case "PRIVMSG":
                    target = IRCUtils.NextParam(line, ref index);
                    msg = IRCUtils.NextAll(line, ref index);

                    if (msg.StartsWith("\u0001ACTION"))
                    {
                        msg = msg.Replace("\x01", "");
                        if (IRCUtils.IsValidChannel(target))
                            MessageInGame(nick, string.Format("&I(IRC) * {0} {1}", nick, msg));
                    }
                    else if (msg.StartsWith("\u0001"))
                    {
                    }
                    else if (IRCUtils.IsValidChannel(target))
                        HandleChannelMessage(NickToUser(nick), target, msg);
                    else
                        HandleDirectMessage(NickToUser(nick), nick, msg);
                    break;
                case "NICK":
                    newNick = IRCUtils.NextParam(line, ref index);
                    if (curNick == nick) curNick = newNick;
                    if (newNick == botNick) Authenticate();
                    nicks.OnChangedNick(nick, newNick);
                    MessageInGame(nick, "&I(IRC) " + nick + " &Sis now known as &I" + newNick);
                    break;
                case "PART":
                    channel = IRCUtils.NextParam(line, ref index);
                    nicks.OnLeftChannel(nick, channel);
                    if (nick == botNick) return;
                    AnnounceJoinLeave(nick, "left", channel);
                    break;
                case "QUIT":
                    nicks.OnLeft(nick);
                    if (nick == botNick)
                        SendRaw(IRCCmds.Nick(nick));
                    else
                    {
                        Logger.Log(LogType.RelayActivity, nick + " left IRC");
                        MessageInGame(nick, "&I(IRC) " + nick + " left");
                    }
                    break;
                case "KICK":
                    channel = IRCUtils.NextParam(line, ref index);
                    target = IRCUtils.NextParam(line, ref index);
                    msg = IRCUtils.NextAll(line, ref index);
                    nicks.OnLeftChannel(nick, channel);
                    if (msg.Length > 0) msg = " (" + msg + ")";
                    Logger.Log(LogType.RelayActivity, "{0} kicked {1} from IRC{2}", nick, target, msg);
                    MessageInGame(nick, "&I(IRC) " + nick + " kicked " + target + msg);
                    break;
                case "MODE":
                    target = IRCUtils.NextParam(line, ref index);
                    if (IRCUtils.IsValidChannel(target))
                        SendRaw(IRCCmds.Names(target));
                    break;
                case "KILL":
                    target = IRCUtils.NextParam(line, ref index);
                    nicks.OnLeft(target);
                    break;
            }
        }
        public void ParseReply(int code, string line, int index)
        {
            string channel;
            IRCUtils.NextParam(line, ref index);
            string[] names;
            switch (code)
            {
                case 001:
                    registered = true;
                    OnReady();
                    Authenticate();
                    JoinChannels();
                    break;
                case 353:
                    IRCUtils.NextParam(line, ref index);
                    channel = IRCUtils.NextParam(line, ref index);
                    names = IRCUtils.NextAll(line, ref index).Split(new char[] { ' ' });
                    nicks.UpdateFor(channel, names);
                    break;
                case 366:
                    break;
                case 432:
                    canReconnect = false;
                    throw new InvalidOperationException("Invalid characters in IRC bot nickname");
                case 433:
                case 436:
                    if (registered) return;
                    curNick = GenNewNick();
                    SendRaw(IRCCmds.Nick(curNick));
                    break;
                case >= 401 and <= 599:
                    Logger.Log(LogType.RelayActivity, "IRC Error #{0}: {1}", code,
                                   IRCUtils.NextAll(line, ref index));
                    break;
            }
        }
    }
}