/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
using MCGalaxy.Authentication;
using MCGalaxy.Blocks;
using MCGalaxy.Commands;
using MCGalaxy.DB;
using MCGalaxy.Drawing;
using MCGalaxy.Eco;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Games;
using MCGalaxy.Modules.Awards;
using MCGalaxy.Modules.Compiling;
using MCGalaxy.Network;
using MCGalaxy.Platform;
using MCGalaxy.SQL;
using MCGalaxy.Tasks;
using MCGalaxy.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
namespace MCGalaxy
{
    public sealed partial class Server
    {
        internal static ConfigElement[] serverConfig, levelConfig, zoneConfig;
        public static void Load()
        {
            PropertiesFile.Read(Paths.ServerPropsFile, (key, value) => ConfigElement.Parse(serverConfig, Config, key, value));
            if (!Directory.Exists(Config.BackupDirectory))
                Config.BackupDirectory = "levels/backups";
            SetMainLevel(Config.MainLevel);
            Save();
        }
        static readonly object saveLock = new();
        public static void Save()
        {
            try
            {
                lock (saveLock)
                {
                    using StreamWriter w = FileIO.CreateGuarded(Paths.ServerPropsFile);
                    SaveProps(w);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving " + Paths.ServerPropsFile, ex);
            }
        }
        static void SaveProps(StreamWriter w)
        {
            w.WriteLine("# Edit the settings below to modify how your server operates.");
            w.WriteLine("#");
            w.WriteLine("# Explanation of Server settings:");
            w.WriteLine("#   server-name                   = The name which displays on classicube.net");
            w.WriteLine("#   motd                          = The message which displays when a player connects");
            w.WriteLine("#   port                          = The port to list for connections on");
            w.WriteLine("#   public                        = Set to true to appear in the public server list");
            w.WriteLine("#   max-players                   = The maximum number of players allowed");
            w.WriteLine("#   max-guests                    = The maximum number of guests allowed");
            w.WriteLine("#   verify-names                  = Verify the validity of names");
            w.WriteLine("#   server-owner                  = The username of the owner of the server");
            w.WriteLine("# Explanation of Level settings:");
            w.WriteLine("#   world-chat                    = Set to true to enable world chat");
            w.WriteLine("# Explanation of IRC settings:");
            w.WriteLine("#   irc                           = Set to true to enable the IRC bot");
            w.WriteLine("#   irc-nick                      = The name of the IRC bot");
            w.WriteLine("#   irc-server                    = The server to connect to");
            w.WriteLine("#   irc-channel                   = The channel to join");
            w.WriteLine("#   irc-opchannel                 = The channel to join (posts OpChat)");
            w.WriteLine("#   irc-port                      = The port to use to connect");
            w.WriteLine("#   irc-identify                  = (true/false)    Do you want the IRC bot to Identify itself with nickserv. Note: You will need to register it's name with nickserv manually.");
            w.WriteLine("#   irc-password                  = The password you want to use if you're identifying with nickserv");
            w.WriteLine("# Explanation of Backup settings:");
            w.WriteLine("#   backup-time                   = The number of seconds between automatic backups");
            w.WriteLine("# Explanation of Color settings:");
            w.WriteLine("#   defaultColor                  = The color code of the default messages (Default = &e)");
            w.WriteLine("# Explanation of Other settings:");
            w.WriteLine("#   use-whitelist                 = Switch to allow use of a whitelist to override IP bans for certain players.  Default false.");
            w.WriteLine("#   profanity-filter              = Replace certain bad words in the chat.  Default false.");
            w.WriteLine("#   notify-on-join-leave          = Show a balloon popup in tray notification area when a player joins/leaves the server.  Default false.");
            w.WriteLine("#   allow-tp-to-higher-ranks      = Allows the teleportation to players of higher ranks");
            w.WriteLine("#   agree-to-rules-on-entry       = Forces all new players to the server to agree to the rules before they can build or use commands.");
            w.WriteLine("#   admins-join-silent            = Players who have adminchat permission join the game silently. Default true");
            w.WriteLine("#   guest-limit-notify            = Show -Too Many Guests- message in chat when maxGuests has been reached. Default false");
            w.WriteLine("#   guest-join-notify             = Shows when guests and lower ranks join server in chat and IRC. Default true");
            w.WriteLine("#   guest-leave-notify            = Shows when guests and lower ranks leave server in chat and IRC. Default true");
            w.WriteLine("#   announcement-interval         = The delay in between server announcements in minutes. Default 5");
            w.WriteLine();
            w.WriteLine("#   kick-on-hackrank              = Set to true if hackrank should kick players");
            w.WriteLine("#   hackrank-kick-time            = Number of seconds until player is kicked");
            w.WriteLine("# Explanation of Security settings:");
            w.WriteLine("#   admin-verification            = Determines whether admins have to verify on entry to the server.  Default true.");
            w.WriteLine("#   verify-admin-perm             = The minimum rank required for admin verification to occur.");
            w.WriteLine("# Explanation of Spam Control settings:");
            w.WriteLine("#   mute-on-spam                  = If enabled it mutes a player for spamming.  Default false.");
            w.WriteLine("#   spam-messages                 = The amount of messages that have to be sent \"consecutively\" to be muted.");
            w.WriteLine("#   spam-mute-time                = The amount of seconds a player is muted for spam.");
            w.WriteLine("#   spam-counter-reset-time       = The amount of seconds the \"consecutive\" messages have to fall between to be considered spam.");
            w.WriteLine();
            w.WriteLine("#   As an example, if you wanted the spam to only mute if a user posts 5 messages in a row within 2 seconds, you would use the folowing:");
            w.WriteLine("#   mute-on-spam                  = true");
            w.WriteLine("#   spam-messages                 = 5");
            w.WriteLine("#   spam-mute-time                = 60");
            w.WriteLine("#   spam-counter-reset-time       = 2");
            w.WriteLine();
            ConfigElement.Serialise(serverConfig, w, Config);
        }
        public static void Start()
        {
            serverConfig = ConfigElement.GetAll(typeof(ServerConfig));
            levelConfig = ConfigElement.GetAll(typeof(LevelConfig));
            zoneConfig = ConfigElement.GetAll(typeof(ZoneConfig));
            IOperatingSystem.DetectOS();
            StartTime = DateTime.UtcNow;
            Logger.Log(LogType.SystemActivity, "Starting Server");
            ServicePointManager.Expect100Continue = false;
            ForceEnableTLS();
            ExtraAuthenticator.SetActive();
            SQLiteBackend.Instance.LoadDependencies();
            EnsureFilesExist();
            Compiler.Init();
            LoadAllSettings(true);
            InitDatabase();
            Economy.LoadDatabase();
            Background.QueueOnce(LoadMainLevel);
            Background.QueueOnce(LoadNAS);
            Background.QueueOnce(LoadAllPlugins);
            Background.QueueOnce(LoadAutoloadMaps);
            Background.QueueOnce(InitPlayerLists);
            Background.QueueOnce(Pronouns.Init);
            Background.QueueOnce(SetupSocket);
            Background.QueueOnce(InitTimers);
            Background.QueueOnce(InitRest);
            Background.QueueOnce(InitHeartbeat);
            ServerTasks.QueueTasks();
            Background.QueueRepeat(ThreadSafeCache.DBCache.CleanupTask,
                                   null, TimeSpan.FromMinutes(5));
        }
        static void ForceEnableTLS()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
            }
            catch
            {
            }
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch
            {
            }
        }
        static void EnsureFilesExist()
        {
            FileIO.TryDeleteDirectory("properties", true);
            EnsureDirectoryExists("props");
            EnsureDirectoryExists("levels");
            EnsureDirectoryExists("bots");
            EnsureDirectoryExists("text");
            EnsureDirectoryExists("ranks");
            RankInfo.EnsureExists();
            Ban.EnsureExists();
            PlayerDB.EnsureDirectoriesExist();
            EnsureDirectoryExists("extra");
            EnsureDirectoryExists("extra/bots");
            EnsureDirectoryExists(Paths.ImportsDir);
            EnsureDirectoryExists("blockdefs");
        }
        public static void EnsureDirectoryExists(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                Logger.LogError("Creating directory " + dir, ex);
            }
        }
        public static void LoadAllSettings(bool commands = false)
        {
            Colors.Load();
            Alias.LoadCustom();
            BlockDefinition.LoadGlobal();
            ImagePalette.Load();
            Load();
            if (commands)
                Command.InitAll();
            AuthService.UpdateList();
            Heartbeat.ReloadDefault();
            Group.LoadAll();
            CommandPerms.Load();
            BlockNames.UpdateCore();
            Block.SetBlocks();
            BlockPerms.Load();
            AwardsList.Load();
            PlayerAwards.Load();
            Economy.Load();
            CommandExtraPerms.Load();
            ProfanityFilter.Init();
            Team.LoadList();
            ChatTokens.LoadCustom();
            CpeExtension.LoadDisabledList();
            TextFile announcementsFile = TextFile.Files["Announcements"];
            announcementsFile.EnsureExists();
            announcements = announcementsFile.GetText();
            OnConfigUpdatedEvent.Call();
        }
        static readonly object stopLock = new();
        static volatile Thread stopThread;
        public static Thread Stop(bool restart, string msg)
        {
            shuttingDown = true;
            lock (stopLock)
            {
                if (stopThread != null)
                    return stopThread;
                stopThread = new(() => ShutdownThread(restart, msg));
                stopThread.Start();
                return stopThread;
            }
        }
        static void ShutdownThread(bool restarting, string msg)
        {
            try
            {
                Logger.Log(LogType.SystemActivity, "Server shutting down ({0})", msg);
            }
            catch
            {
            }
            Listener.Close();
            try
            {
                foreach (Player p in PlayerInfo.Online.Items)
                    p.Leave(msg);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            byte[] kick = Packet.Kick(msg, false);
            try
            {
                foreach (INetSocket p in INetSocket.pending.Items)
                    p.Send(kick, SendFlags.None);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            OnShuttingDownEvent.Call(restarting, msg);
            Plugin.UnloadAll();
            NAS.Unload();
            try
            {
                if (SetupFinished && !Config.AutoLoadMaps)
                    FileIO.TryWriteAllText("text/autoload.txt", SaveAllLevels());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            try
            {
                Logger.Log(LogType.SystemActivity, "Server shutdown completed");
            }
            catch
            {
            }
            try
            {
                FileLogger.Flush(null);
            }
            catch
            {
            }
            if (restarting)
                IOperatingSystem.DetectOS().RestartProcess();
            Environment.Exit(0);
        }
        public static string SaveAllLevels()
        {
            string autoload = null;
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                if (!lvl.SaveChanges)
                {
                    Logger.Log(LogType.SystemActivity, "Skipping save for level {0}", lvl.ColoredName);
                    continue;
                }
                autoload = autoload + lvl.name + "=" + lvl.LevelPhysics + Environment.NewLine;
                lvl.Save();
                lvl.SaveBlockDBChanges();
            }
            return autoload;
        }
        /// <summary> Returns the full path to the server executable </summary>
        public static string GetPath() => Assembly.GetEntryAssembly().Location;
        /// <summary> Returns the full path to the runtime executable </summary>
        public static string GetRuntimeExePath() => Process.GetCurrentProcess().MainModule.FileName;
        static bool checkedOnMono, runningOnMono;
        public static bool RunningOnMono()
        {
            if (!checkedOnMono)
            {
                runningOnMono = Type.GetType("Mono.Runtime") != null;
                checkedOnMono = true;
            }
            return runningOnMono;
        }
        static void RandomMessage(SchedulerTask _)
        {
            if (PlayerInfo.Online.Count > 0 && announcements.Length > 0)
                Chat.MessageGlobal(announcements[new Random().Next(0, announcements.Length)]);
        }
        public static bool SetMainLevel(string map)
        {
            OnMainLevelChangingEvent.Call(ref map);
            string main = mainLevel != null ? mainLevel.name : Config.MainLevel;
            if (map.CaselessEq(main))
                return false;
            Level lvl = LevelInfo.FindExact(map) ?? LevelActions.Load(Player.NASConsole, map, false);
            if (lvl == null)
                return false;
            SetMainLevel(lvl);
            return true;
        }
        public static void SetMainLevel(Level lvl)
        {
            Level oldMain = mainLevel;
            mainLevel = lvl;
            oldMain.AutoUnload();
        }
        public static void DoGC()
        {
            Stopwatch sw = Stopwatch.StartNew();
            long start = GC.GetTotalMemory(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long end = GC.GetTotalMemory(false);
            double deltaKB = (start - end) / 1024.0;
            if (deltaKB < 100.0)
                return;
            Logger.Log(LogType.BackgroundActivity, "GC performed in {0:F2} ms (tracking {1:F2} KB, freed {2:F2} KB)",
                       sw.Elapsed.TotalMilliseconds, end / 1024.0, deltaKB);
        }
        /// <summary> Converts a formatted username into its original username </summary>
        /// <remarks> If ClassiCubeAccountPlus option is set, removes + </remarks>
        public static string ToRawUsername(string name) => Config.ClassicubeAccountPlus ? name.Replace("+", "") : name;
        /// <summary> Converts a username into its formatted username </summary>
        /// <remarks> If ClassiCubeAccountPlus option is set, adds trailing + </remarks>
        public static string FromRawUsername(string name)
        {
            if (!Config.ClassicubeAccountPlus)
                return name;
            if (!name.Contains("+"))
                name += "+";
            return name;
        }
    }
}
