#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using System;
using System.Collections.Generic;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class Nas : Plugin
    {
        public override string name
        {
            get
            {
                return "NAS";
            }
        }
        public override string MCGalaxy_Version
        {
            get
            {
                return "1.9.5.3";
            }
        }
        public override string creator
        {
            get
            {
                return "JuneSolis";
            }
        }
        public static List<string> Devs = new()
        {
            "JuneSolis",
            "HarmonyNetwork"
        };
        public const string textureURL = "https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/texturepack.zip",
            KeyPrefix = "nas_",
            PlayerKey = KeyPrefix + "NasPlayer",
            Path = "nas/",
            SavePath = Path + "playerdata/",
            CoreSavePath = Path + "coredata/",
            EffectsPath = Path + "effects/",
            NasVersion = "1.0.5.4";
        public static bool LoadedOnStartup = false,
            firstEverPluginLoad = false;
        public static Command[] Commands = new Command[]
        {
            new NasPlayer.CmdBarrelMode(),
            new NasPlayer.CmdGravestones(),
            new NasPlayer.CmdMyGravestones(),
            new NasPlayer.CmdNASSpawn(),
            new NasPlayer.CmdPVP(),
            new NasPlayer.CmdSpawnDungeon(),
            new CmdNewServerInfo(),
        };
        public static Command ServerInfoCommand;
        public static void EnsureNasFilesExist()
        {
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/selectorColors.png", Path + "selectorColors.png");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/terrain.png", Path + "terrain.png");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/effects/breakdust.properties", EffectsPath + "breakdust.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/effects/breakleaf.properties", EffectsPath + "breakleaf.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/effects/breakmeter.properties", EffectsPath + "breakmeter.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Uploads/nas/global.json", "blockdefs/global.json");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
        }
        public override void Load(bool startup)
        {
            if (startup)
            {
                LoadedOnStartup = true;
            }
            EnsureDirectoriesExists(Path, SavePath,
                CoreSavePath, EffectsPath,
                NasLevel.Path, NasBlock.Path,
                NasPlayer.DeathsPath, "blockprops",
                "blockdefs", "text");
            EnsureNasFilesExist();
            if (Block.Props.Length != 1024)
            {
                Log("NAS: FAILED to load plugin. In order to run NAS, you must be using a version of MCGalaxy which allows 767 blocks.");
                Log("NAS: You can find instructions for 767 blocks here: https://github.com/ClassiCube/MCGalaxy/tree/master/Uploads (infid)");
                return;
            }
            if (!File.Exists("Newtonsoft.Json.dll"))
            {
                Log("NAS: FAILED to load plugin. Could not find Newtonsoft.Json.dll");
                return;
            }
            if (File.Exists("nas/loaded.txt"))
            {
                firstEverPluginLoad = false;
            }
            else
            {
                firstEverPluginLoad = true;
                FileUtils.TryWriteAllText("nas/loaded.txt", "Do not delete this file unless you are using the plugin for the first time!");
            }
            if (firstEverPluginLoad)
            {
                LoadFirstTime();
            }
            NASUpdater.Setup();
            OnlineStat.Stats.Add(PvP);
            OnlineStat.Stats.Add(Kills);
            OnlineStat.Stats.Add(Dev);
            OfflineStat.Stats.Add(Dev);
            ServerInfoCommand = Command.Find("ServerInfo");
            Command.Unregister(ServerInfoCommand);
            Register(Commands);
            NasPlayer.Register();
            NasBlock.Setup();
            if (!NasEffect.Setup())
            {
                FailedLoad();
                return;
            }
            if (!NasBlockChange.Setup())
            {
                FailedLoad();
                return;
            }
            ItemProp.Setup();
            Crafting.Setup();
            if (!DynamicColor.Setup())
            {
                FailedLoad();
                return;
            }
            Collision.Setup();
            OnPlayerConnectEvent.Register(OnPlayerConnect, Priority.High);
            OnPlayerClickEvent.Register(OnPlayerClick, Priority.High);
            OnBlockChangingEvent.Register(OnBlockChanging, Priority.High);
            OnBlockChangedEvent.Register(OnBlockChanged, Priority.High);
            OnPlayerMoveEvent.Register(OnPlayerMove, Priority.High);
            OnPlayerChatEvent.Register(OnPlayerMessage, Priority.Normal);
            OnPlayerDisconnectEvent.Register(OnPlayerDisconnect, Priority.Low);
            OnPlayerCommandEvent.Register(OnPlayerCommand, Priority.High);
            OnShuttingDownEvent.Register(OnShutdown, Priority.Low);
            OnSentMapEvent.Register(HandleSentMap, Priority.Critical);
            NasGen.Setup();
            NasLevel.Setup();
            NasTimeCycle.Setup();
            if (firstEverPluginLoad)
            {
                GenLevel();
            }
        }
        public void PvP(Player p, Player target)
        {
            if (NasPlayer.GetNasPlayer(target).pvpEnabled)
            {
                p.Message("&S  " + target.pronouns.Subject.Capitalize() + " " + target.pronouns.PresentPerfectVerb + " PVP &2enabled&S.");
            }
            else
            {
                p.Message("&S  " + target.pronouns.Subject.Capitalize() + " " + target.pronouns.PresentPerfectVerb + " PVP &cdisabled&S.");
            }
        }
        public void Dev(Player p, Player target)
        {
            if (IsDev(target))
            {
                p.Message("&S  {0} developer.", name.Capitalize());
            }
        }
        public void Dev(Player p, PlayerData target)
        {
            if (IsDev(target))
            {
                p.Message("&S  {0} developer.", name.Capitalize());
            }
        }
        public void Kills(Player p, Player target)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(target);
            p.Message("&S  " + target.pronouns.Subject.Capitalize() + " " + target.pronouns.PresentPerfectVerb + " " + np.kills + " kills.");
        }
        public static void FailedLoad()
        {
            Log("NAS: FAILED to load plugin. Please report this to randomstrangers on Discord!");
        }
        public override void Unload(bool shutdown)
        {
            Chat.MessageAll("Attempting to unload NAS.");
            if (!shutdown && LoadedOnStartup)
            {
                InvalidOperationException ioex = new("You cannot unload NAS manually, it can only be unloaded on server shutdown.");
                throw ioex;
            }
            NASUpdater.TakeDown();
            NasPlayer.Unregister();
            DynamicColor.TakeDown();
            Command.Unregister(Commands);
            if (ServerInfoCommand != null)
            {
                Command.Register(ServerInfoCommand);
            }
            OnlineStat.Stats.Remove(PvP);
            OnlineStat.Stats.Remove(Kills);
            OnlineStat.Stats.Remove(Dev);
            OfflineStat.Stats.Remove(Dev);
            OnPlayerConnectEvent.Unregister(OnPlayerConnect);
            OnPlayerClickEvent.Unregister(OnPlayerClick);
            OnBlockChangingEvent.Unregister(OnBlockChanging);
            OnBlockChangedEvent.Unregister(OnBlockChanged);
            OnPlayerMoveEvent.Unregister(OnPlayerMove);
            OnPlayerDisconnectEvent.Unregister(OnPlayerDisconnect);
            OnJoinedLevelEvent.Register(OnLevelJoined, Priority.High);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
            OnShuttingDownEvent.Unregister(OnShutdown);
            OnSentMapEvent.Unregister(HandleSentMap);
            NasLevel.TakeDown();
            NasTimeCycle.TakeDown();
            NasGen.TakeDown();
        }
    }
}
#endif