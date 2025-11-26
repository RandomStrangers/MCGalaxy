#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
//unknownshadow200: well player ids go from 0 up to 255. normal bots go from 127 down to 64, then 254 down to 127, then finally 63 down to 0.
//UnknownShadow200: FromRaw adds 256 if the block id is >= 66, and ToRaw subtracts 256 if the block id is >= 66
//"raw" is MCGalaxy's name for clientushort
///model |0.93023255813953488372093023255814
//gravestone drops upon death that contains your inventory
//different types of crafting stations
//furnace for smelting-style recipes
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
                return "JuneSolis"; //Zoey no longer supports NAS. 
            }
        }
        public static List<string> Devs = new()
        {
            //"zoeyvidae", //No longer supports.
            //"UnseenServant", //No longer involved.
            "JuneSolis",
            "HarmonyNetwork"
        };
        public const string textureURL = "https://dl.dropboxusercontent.com/s/2x5oxffkgpcyj16/nas.zip?dl=0",
            KeyPrefix = "nas_",
            PlayerKey = KeyPrefix + "NasPlayer",
            Path = "nas/",
            SavePath = Path + "playerdata/",
            CoreSavePath = Path + "coredata/",
            EffectsPath = Path + "effects/",
            NasVersion = "1.0.4.3";
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
            new CmdServerInfo2(),
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
            if (Directory.Exists("plugins/nas"))
            {
                string[] pluginfiles = FileUtils.TryGetFiles("plugins/nas");
                foreach (string pluginfile in pluginfiles)
                {
                    string[] files = FileUtils.TryGetFiles(Path);
                    foreach (string file in files)
                    {
                        if (!File.Exists(file))
                        {
                            MovePluginFile("nas/" + pluginfile, System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/nas/" + pluginfile);
                        }
                        else
                        {
                            Log("Duplicate file {0} found in plugins/nas folder, this should not have happened!", file);
                            FileIO.TryDelete(pluginfile);
                        }
                    }
                }
                FileUtils.TryDeleteDirectory("plugins/nas", true);
            }
            EnsureNasFilesExist();
            /*if (Block.Props.Length != 1024)
            { //check for TEN_BIT_BLOCKS. Value is 512 on a default instance of MCGalaxy.
                Log("NAS: FAILED to load plugin. In order to run NAS, you must be using a version of MCGalaxy which allows 767 blocks.");
                Log("NAS: You can find instructions for 767 blocks here: https://github.com/ClassiCube/MCGalaxy/tree/master/Uploads (infid)");
                return;
            // Unneeded since NAS compiles with TEN_BIT_BLOCKS
            }*/
            if (!File.Exists("Newtonsoft.Json.dll"))
            {
                Log("NAS: FAILED to load plugin. Could not find Newtonsoft.Json.dll");
                return;
            }
            string loadFile = "nas/loaded.txt";
            MoveFile("props/loaded.txt", loadFile); //loaded.txt
            //I HATE IT
            MovePluginFile("global.json", "blockdefs/global.json"); //blockdefs
            MovePluginFile("default.txt", "blockprops/default.txt"); //blockprops
            MovePluginFile("customcolors.txt", "text/customcolors.txt"); //custom chat colors
            MovePluginFile("command.properties", "props/command.properties"); //command permissions
            MovePluginFile("ExtraCommandPermissions.properties", "props/ExtraCommandPermissions.properties"); //extra command permissions
            MovePluginFile("ranks.properties", "props/ranks.properties"); //ranks
            MovePluginFile("faq.txt", "text/faq.txt"); //faq
            MovePluginFile("messages.txt", "text/messages.txt"); //messages
            MovePluginFile("welcome.txt", "text/welcome.txt"); //welcome
            string message = "Do not delete this file unless you are using the plugin for the first time!";
            if (File.Exists(loadFile))
            {
                firstEverPluginLoad = false;
            }
            else
            {
                firstEverPluginLoad = true;
                FileUtils.TryWriteAllText(loadFile, message);
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
            string msg = "NAS: FAILED to load plugin. Please report this to randomstrangers on Discord!";
            Log(msg);
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