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
using MCGalaxy.Events.ServerEvents;
using System.IO;
namespace MCGalaxy.Modules.Relay.Discord
{
    public class DiscordConfig
    {
        [ConfigBool("enabled", "General", false)]
        public bool Enabled;
        [ConfigString("bot-token", "General", "", true)]
        public string BotToken = "";
        [ConfigBool("use-nicknames", "General", true)]
        public bool UseNicks = true;
        [ConfigString("channel-ids", "General", "", true)]
        public string Channels = "";
        [ConfigString("op-channel-ids", "General", "", true)]
        public string OpChannels = "";
        [ConfigString("ignored-user-ids", "General", "", true)]
        public string IgnoredUsers = "";
        [ConfigBool("presence-enabled", "Presence (Status)", true)]
        public bool PresenceEnabled = true;
        [ConfigEnum("presence-status", "Presence (Status)", PresenceStatus.online, typeof(PresenceStatus))]
        public PresenceStatus Status = PresenceStatus.online;
        [ConfigEnum("presence-activity", "Presence (Status)", PresenceActivity.Playing, typeof(PresenceActivity))]
        public PresenceActivity Activity = PresenceActivity.Playing;
        [ConfigString("status-message", "Presence (Status)", "with {PLAYERS} players")]
        public string StatusMessage = "with {PLAYERS} players";
        [ConfigBool("can-mention-users", "Mentions", true)]
        public bool CanMentionUsers = true;
        [ConfigBool("can-mention-roles", "Mentions", true)]
        public bool CanMentionRoles = true;
        [ConfigBool("can-mention-everyone", "Mentions", false)]
        public bool CanMentionHere;
        [ConfigInt("embed-color", "Embeds", 9758051)]
        public int EmbedColor = 9758051;
        [ConfigBool("embed-show-game-statuses", "Embeds", true)]
        public bool EmbedGameStatuses = true;
        [ConfigInt("extra-intents", "Intents", 0)]
        public int ExtraIntents;
        [ConfigString("pluralkit-prefixes", "PluralKit", "pk;,rs;,se;,mw;", false)]
        public string PKPrefixes = "pk;,rs;,se;,mw;";
        [ConfigString("pluralkit-users", "PluralKit", "", true)]
        public string PKUsers = "";
        public const string PROPS_PATH = "props/discordbot" + Paths.PropertiesFileExt;
        public static ConfigElement[] cfg;
        public void Load()
        {
            if (!File.Exists(PROPS_PATH)) Save();
            cfg ??= ConfigElement.GetAll(typeof(DiscordConfig));
            ConfigElement.ParseFile(cfg, PROPS_PATH, this);
        }
        public void Save()
        {
            cfg ??= ConfigElement.GetAll(typeof(DiscordConfig));
            using StreamWriter w = FileIO.CreateGuarded(PROPS_PATH);
            w.WriteLine("# Discord relay bot configuration");
            w.WriteLine("# See https://github.com/ClassiCube/MCGalaxy/wiki/Discord-relay-bot/");
            w.WriteLine();
            ConfigElement.Serialise(cfg, w, this);
        }
    }
    public enum PresenceStatus { online, dnd, idle, invisible }
    public enum PresenceActivity { Playing = 0, Listening = 2, Watching = 3, Custom = 4, Competing = 5 }
    public class DiscordPlugin : Plugin
    {
        public override string Name => "DiscordRelay";
        public static DiscordConfig Config = new();
        public static DiscordBot Bot = new();
        public static readonly Command cmdDiscordBot = new CmdDiscordBot(),
            cmdDiscordCtrls = new CmdDiscordControllers();
        public override void Load(bool startup)
        {
            Server.EnsureDirectoryExists("text/discord");
            Server.EnsureDirectoryExists("text/discord/PK");
            Command.Register(cmdDiscordBot, cmdDiscordCtrls);
            Bot.Config = Config;
            Bot.ReloadConfig();
            Bot.Connect();
            OnConfigUpdatedEvent.Register(OnConfigUpdated, Priority.Low);
        }
        public override void Unload(bool shutdown)
        {
            Command.Unregister(cmdDiscordBot, cmdDiscordCtrls);
            OnConfigUpdatedEvent.Unregister(OnConfigUpdated);
            Bot.Disconnect("Disconnecting Discord bot");
        }
        public void OnConfigUpdated() => Bot.ReloadConfig();
    }
    public class CmdDiscordBot : RelayBotCmd
    {
        public override string Name => "DiscordBot";
        public override RelayBot Bot => DiscordPlugin.Bot;
    }
    public class CmdDiscordControllers : BotControllersCmd
    {
        public override string Name => "DiscordControllers";
        public override RelayBot Bot => DiscordPlugin.Bot;
    }
}