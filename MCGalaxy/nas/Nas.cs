#if NAS && TEN_BIT_BLOCKS
using System;
using System.IO;
using MCGalaxy;
using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using System.Reflection;
using MCGalaxy.Events.ServerEvents;
using System.Collections.Generic;
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
        public static string GetVersion()
        {
            if (!Server.NasVersion.IsNullOrEmpty())
            {
                return Server.NasVersion;
            }
            else
            {
                return "1.9.5.3";
            }
        }
        public override string MCGalaxy_Version
        {
            get
            {
                return GetVersion();
            }
        }
        public override string creator
        {
            get
            {
                return "HarmonyNetwork"; //Goodly/Zoey no longer supports NAS. 
            }
        }
        public static List<string> Devs = new List<string>
        {
            //"goodlyay", //No longer supports.
            //"zoeyvidae", //No longer supports.
            //"UnseenServant", //No longer involved.
            "HarmonyNetwork"
        };
        public const string textureURL = "https://dl.dropboxusercontent.com/s/2x5oxffkgpcyj16/nas.zip?dl=0";
        public const string KeyPrefix = "nas_";
        public const string PlayerKey = KeyPrefix + "NasPlayer";
        public const string Path = "nas/";
        public const string SavePath = Path + "playerdata/";
        public const string CoreSavePath = Path + "coredata/";
        public const string EffectsPath = Path + "effects/";
        public static bool LoadedOnStartup = false;
        public static Command[] Commands = new Command[]
        {
            new NasPlayer.CmdBarrelMode(),
            new NasPlayer.CmdGravestones(),
            new NasPlayer.CmdMyGravestones(),
            new NasPlayer.CmdNASSpawn(),
            new NasPlayer.CmdPVP(),
            new NasPlayer.CmdSpawnDungeon()
        };
        public static bool firstEverPluginLoad = false;
        public static void EnsureNasFilesExist()
        {
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/selectorColors.png", Path + "selectorColors.png");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/terrain.png", Path + "terrain.png");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/effects/breakdust.properties", EffectsPath + "breakdust.properties");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/effects/breakleaf.properties", EffectsPath + "breakleaf.properties");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/effects/breakmeter.properties", EffectsPath + "breakmeter.properties");
            EnsureFileExists("https://github.com/cloverpepsi/place.cs/raw/main/global.json", "blockdefs/global.json");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/nas-rework/Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
            if (!File.Exists("text/BookTitles.txt"))
            {
                File.Create("text/BookTitles.txt").Dispose();
            }
            if (!File.Exists("text/BookAuthors.txt"))
            {
                File.Create("text/BookAuthors.txt").Dispose();
            }
        }
        public static void EnsureDirectoriesExist()
        {
            EnsureDirectoryExists(Path);
            EnsureDirectoryExists(SavePath);
            EnsureDirectoryExists(CoreSavePath);
            EnsureDirectoryExists(EffectsPath);
            EnsureDirectoryExists(NasLevel.Path);
            EnsureDirectoryExists(NasBlock.Path);
            EnsureDirectoryExists(NasPlayer.DeathsPath);
            EnsureDirectoryExists("blockprops");
            EnsureDirectoryExists("blockdefs");
            EnsureDirectoryExists("text");
        }
        public override void Load(bool startup)
        {
            if (startup)
            {
                LoadedOnStartup = true;
            }
            EnsureDirectoriesExist();
            if (Directory.Exists("plugins/nas"))
            {
                string[] pluginfiles = Directory.GetFiles("plugins/nas");
                foreach (string pluginfile in pluginfiles)
                {
                    string[] files = Directory.GetFiles(Path);
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
                Directory.Delete("plugins/nas", true);
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
                File.WriteAllText(loadFile, message);
            }
            if (firstEverPluginLoad)
            {
                LoadFirstTime();
            }
            OnlineStat.Stats.Add(PvP);
            OnlineStat.Stats.Add(Kills);
            OnlineStat.Stats.Add(Dev);
            OfflineStat.Stats.Add(Dev);
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
                InvalidOperationException ioex = new InvalidOperationException("You cannot unload NAS manually, it can only be unloaded on server shutdown.");
                throw ioex;
            }
            NasPlayer.Unregister();
            DynamicColor.TakeDown();
            Command.Unregister(Commands);
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