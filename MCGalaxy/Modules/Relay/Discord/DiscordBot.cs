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
using MCGalaxy.Config;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Tasks;
using MCGalaxy.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace MCGalaxy.Modules.Relay.Discord
{
    sealed class DiscordUser : RelayUser
    {
        public string ReferencedUser;
        public override string GetMessagePrefix() => string.IsNullOrEmpty(ReferencedUser) ? "" : "@" + ReferencedUser + " ";
    }
    public class DiscordBot : RelayBot
    {
        protected DiscordApiClient api;
        protected DiscordWebsocket socket;
        protected DiscordSession session;
        protected string botUserID;
        readonly Dictionary<string, byte> channelTypes = new();
        readonly List<string> filter_triggers = new();
        readonly List<string> filter_replacements = new();
        JsonArray allowed;
        public override string RelayName => "Discord";
        public override bool Enabled => Config.Enabled;
        public override string UserID => botUserID;
        public DiscordConfig Config;
        readonly TextFile replacementsFile = new("text/discord/replacements.txt",
                                        "// This file is used to replace words/phrases sent to Discord",
                                        "// Lines starting with // are ignored",
                                        "// Lines should be formatted like this:",
                                        "// example:http://example.org",
                                        "// That would replace 'example' in messages sent to Discord with 'http://example.org'");
        public string APIHost = "https://discord.com/api/v10";
        public string WSHost = "gateway.discord.gg";
        public string WSPath = "/?v=10&encoding=json";
        protected override bool CanReconnect => canReconnect && (socket == null || socket.CanReconnect);
        protected override void DoConnect()
        {
            socket = new(WSPath)
            {
                Session = session,
                Token = Config.BotToken,
                Host = WSHost,
                Presence = Config.PresenceEnabled,
                Status = Config.Status,
                Activity = Config.Activity,
                GetStatus = GetStatusMessage,
                OnReady = HandleReadyEvent,
                OnResumed = HandleResumedEvent,
                OnMessageCreate = HandleMessageEvent,
                OnChannelCreate = HandleChannelEvent,
                OnGatewayEvent = HandleGatewayEvent
            };
            socket.Connect();
        }
        static Exception UnpackError(Exception ex) => ex.InnerException is ObjectDisposedException ? ex.InnerException : ex.InnerException is IOException ? ex.InnerException : null;
        protected override void DoReadLoop()
        {
            try
            {
                socket.ReadLoop();
            }
            catch (Exception ex)
            {
                Exception unpacked = UnpackError(ex);
                if (unpacked != null) throw unpacked;
                throw;
            }
        }
        protected override void DoDisconnect(string reason)
        {
            try
            {
                socket.Disconnect();
            }
            catch
            {
            }
        }
        public override void ReloadConfig()
        {
            Config.Load();
            base.ReloadConfig();
            LoadReplacements();
            if (!Config.CanMentionHere) return;
            Logger.Log(LogType.Warning, "can-mention-everyone option is enabled in {0}, " +
                       "which allows pinging all users on Discord from in-game. " +
                       "It is recommended that this option be disabled.", DiscordConfig.PROPS_PATH);
        }
        protected override void UpdateConfig()
        {
            Channels = Config.Channels.SplitComma();
            OpChannels = Config.OpChannels.SplitComma();
            IgnoredUsers = Config.IgnoredUsers.SplitComma();
            UpdateAllowed();
            LoadBannedCommands();
        }
        void UpdateAllowed()
        {
            JsonArray mentions = new();
            if (Config.CanMentionUsers) mentions.Add("users");
            if (Config.CanMentionRoles) mentions.Add("roles");
            if (Config.CanMentionHere) mentions.Add("everyone");
            allowed = mentions;
        }
        void LoadReplacements()
        {
            replacementsFile.EnsureExists();
            string[] lines = replacementsFile.GetText();
            filter_triggers.Clear();
            filter_replacements.Clear();
            ChatTokens.LoadTokens(lines, (phrase, replacement) =>
                                  {
                                      filter_triggers.Add(phrase);
                                      filter_replacements.Add(DiscordUtils.MarkdownToSpecial(replacement));
                                  });
        }
        public override void LoadControllers() => Controllers = PlayerList.Load("text/discord/controllers.txt");
        DiscordUser ExtractUser(JsonObject data)
        {
            JsonObject author = (JsonObject)data["author"];
            DiscordUser user = new()
            {
                Nick = GetNick(data) ?? GetUser(author),
                ID = (string)author["id"],
                ReferencedUser = ExtractReferencedUser(data)
            };
            return user;
        }
        string GetNick(JsonObject data)
        {
            if (!Config.UseNicks) return null;
            if (!data.TryGetValue("member", out object raw)) return null;
            if (raw is not JsonObject member) return null;
            member.TryGetValue("nick", out raw);
            return raw as string;
        }
        string GetUser(JsonObject author)
        {
            author.TryGetValue("global_name", out object name);
            return name != null ? (string)name : (string)author["username"];
        }
        string ExtractReferencedUser(JsonObject data)
        {
            data.TryGetValue("referenced_message", out object refMsgRaw);
            if (refMsgRaw is not JsonObject refMsgData) return null;
            refMsgData.TryGetValue("author", out object authorRaw);
            return authorRaw == null ? null : GetUser((JsonObject)authorRaw);
        }
        void HandleMessageEvent(JsonObject data)
        {
            DiscordUser user = ExtractUser(data);
            if (user.ID == botUserID) return;
            string channel = (string)data["channel_id"],
                message = (string)data["content"];
            if (!channelTypes.TryGetValue(channel, out byte type))
            {
                type = GuessChannelType(data);
                if (type == 1) channelTypes[channel] = type;
            }
            if (type == 0)
                HandleDirectMessage(user, channel, message);
            else
            {
                HandleChannelMessage(user, channel, message);
                PrintAttachments(user, data, channel);
            }
        }
        void PrintAttachments(RelayUser user, JsonObject data, string channel)
        {
            if (!data.TryGetValue("attachments", out object raw) || raw is not JsonArray list) return;
            foreach (object entry in list)
            {
                if (entry is not JsonObject attachment) continue;
                string url = (string)attachment["url"];
                HandleChannelMessage(user, channel, url);
            }
        }
        void HandleChannelEvent(JsonObject data)
        {
            if ((string)data["type"] == "1") channelTypes[(string)data["id"]] = 0;
        }
        byte GuessChannelType(JsonObject data) => data.ContainsKey("member") ? (byte)1 : data.ContainsKey("webhook_id") ? (byte)1 : (byte)0;
        void HandleReadyEvent(JsonObject data)
        {
            botUserID = (string)((JsonObject)data["user"])["id"];
            HandleResumedEvent(data);
        }
        void HandleResumedEvent(JsonObject data)
        {
            if (api == null)
            {
                api = new()
                {
                    Token = Config.BotToken,
                    Host = APIHost
                };
                api.RunAsync();
            }
            OnReady();
        }
        void HandleGatewayEvent(string eventName, JsonObject data) => OnGatewayEventReceivedEvent.Call(this, eventName, data);
        static bool IsEscaped(char c) => (c > ' ' && c <= '/') || (c >= ':' && c <= '@') || (c >= '[' && c <= '`') || (c >= '{' && c <= '~');
        protected override string ParseMessage(string input)
        {
            StringBuilder sb = new(input);
            SimplifyCharacters(sb);
            sb.Replace("\uFE0F", "");
            int length = sb.Length - 1;
            for (int i = 0; i < length; i++)
            {
                if (sb[i] != '\\' || !IsEscaped(sb[i + 1])) continue;
                sb.Remove(i, 1); 
                length--;
            }
            sb.Replace("**", "");
            return sb.ToString();
        }
        readonly object updateLocker = new();
        volatile bool updateScheduled;
        DateTime nextUpdate;
        public void UpdateDiscordStatus()
        {
            TimeSpan delay = default;
            DateTime now = DateTime.UtcNow;
            lock (updateLocker)
            {
                if (updateScheduled) return;
                updateScheduled = true;
                if (nextUpdate > now) delay = nextUpdate - now;
            }
            Server.MainScheduler.QueueOnce(DoUpdateStatus, null, delay);
        }
        void DoUpdateStatus(SchedulerTask task)
        {
            DateTime now = DateTime.UtcNow;
            lock (updateLocker)
            {
                updateScheduled = false;
                nextUpdate = now.AddSeconds(0.5);
            }
            DiscordWebsocket s = socket;
            if (s == null || !s.SentIdentify) return;
            try 
            { 
                s.SendMessage(3, s.MakePresence()); 
            }
            catch 
            { 
            }
        }
        string GetStatusMessage()
        {
            fakeGuest.group = Group.DefaultRank;
            return Config.StatusMessage.Replace("{PLAYERS}", NumberUtils.StringifyInt(PlayerInfo.GetOnlineCanSee(fakeGuest, fakeGuest.Rank).Count));
        }
        protected override void OnStart()
        {
            DiscordSession s = new()
            {
                Intents = (1 << 9) | (1 << 12) | (1 << 15) | Config.ExtraIntents
            };
            session = s;
            base.OnStart();
            OnPlayerConnectEvent.Register(HandlePlayerConnect, Priority.Low);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.Low);
            OnPlayerActionEvent.Register(HandlePlayerAction, Priority.Low);
        }
        protected override void OnStop()
        {
            socket = null;
            if (api != null)
            {
                api.StopAsync();
                api = null;
            }
            base.OnStop();
            OnPlayerConnectEvent.Unregister(HandlePlayerConnect);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
            OnPlayerActionEvent.Unregister(HandlePlayerAction);
        }
        void HandlePlayerConnect(Player p) => UpdateDiscordStatus();
        void HandlePlayerDisconnect(Player p, string reason) => UpdateDiscordStatus();
        void HandlePlayerAction(Player p, PlayerAction action, string message, bool stealth)
        {
            if (action != PlayerAction.Hide && action != PlayerAction.Unhide) return;
            UpdateDiscordStatus();
        }
        /// <summary> Asynchronously sends a message to the discord API </summary>
        public void Send(DiscordApiMessage msg) => api?.QueueAsync(msg);
        protected override void DoSendMessage(string channel, string message)
        {
            message = ConvertMessage(message);
            for (int offset = 0; offset < message.Length; offset += 2000)
                Send(new ChannelSendMessage(channel, message.Substring(offset, Math.Min(message.Length - offset, 2000)))
                {
                    Allowed = allowed
                });
        }
        /// <summary> Formats a message for displaying on Discord </summary>
        /// <example> Escapes markdown characters such as _ and * </example>
        protected string ConvertMessage(string message)
        {
            message = ConvertMessageCommon(message);
            message = Colors.StripUsed(message);
            message = DiscordUtils.EscapeMarkdown(message);
            message = DiscordUtils.SpecialToMarkdown(message);
            return message;
        }
        protected override string PrepareMessage(string message)
        {
            for (int i = 0; i < filter_triggers.Count; i++)
                message = message.Replace(filter_triggers[i], filter_replacements[i]);
            return message;
        }
        protected override bool CheckController(string userID, ref string error) => true;
        protected override string UnescapeFull(Player p) => "\uEDC3\uEDC3" + base.UnescapeFull(p) + "\uEDC3\uEDC3";
        protected override string UnescapeNick(Player p) => "\uEDC3\uEDC3" + base.UnescapeNick(p) + "\uEDC3\uEDC3";
        protected override void MessagePlayers(RelayPlayer p)
        {
            ChannelSendEmbed embed = new(p.ChannelID);
            List<OnlineListEntry> entries = PlayerInfo.GetOnlineList(p, p.Rank, out int total);
            embed.Color = Config.EmbedColor;
            embed.Title = string.Format("{0} player{1} currently online",
                                        total, total.Plural());
            foreach (OnlineListEntry e in entries)
            {
                if (e.players.Count == 0) continue;
                embed.Fields.Add(
                    ConvertMessage(FormatRank(e)),
                    ConvertMessage(FormatPlayers(p, e))
                );
            }
            OnSendingWhoEmbedEvent.Call(this, p.User, ref embed);
            Send(embed);
        }
        static string FormatPlayers(Player p, OnlineListEntry e) => e.players.Join(pl => FormatNick(p, pl), ", ");
        static string FormatRank(OnlineListEntry e) => string.Format("\uEDC1\uEDC1{0}\uEDC1\uEDC1 (\uEDC4{1}\uEDC4)",
                                 e.group.GetFormattedName(), e.players.Count);
        static string FormatNick(Player p, Player pl)
        {
            string flags = OnlineListEntry.GetFlags(pl),
                format = flags.Length > 0
                ? "\uEDC3\uEDC3{0}\uEDC3\uEDC3\uEDC1{2}\uEDC1 (\uEDC4{1}\uEDC4)"
                : "\uEDC3\uEDC3{0}\uEDC3\uEDC3 (\uEDC4{1}\uEDC4)";
            return string.Format(format, p.FormatNick(pl), pl.Level.name.Replace('_', '\uEDC1'), flags);
        }
    }
}
