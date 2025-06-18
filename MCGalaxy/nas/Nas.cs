
#if NAS && !NET_20 && TEN_BIT_BLOCKS
using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Commands;
using System.Reflection;
using System.Net;

//unknownshadow200: well player ids go from 0 up to 255. normal bots go from 127 down to 64, then 254 down to 127, then finally 63 down to 0.
//UnknownShadow200: FromRaw adds 256 if the block id is >= 66, and ToRaw subtracts 256 if the block id is >= 66
//"raw" is MCGalaxy's name for clientushort
///model |0.93023255813953488372093023255814
//gravestone drops upon death that contains your inventory
//different types of crafting stations
//furnace for smelting-style recipes
namespace NotAwesomeSurvival
{
    public class NameGenerator
    {
        public int[] numSyllables = new int[] { 1, 2, 3, 4, 5 };
        public int[] numSyllablesChance = new int[] { 150, 500, 80, 10, 1 };
        public int[] numConsonants = new int[] { 0, 1, 2, 3, 4 };
        public int[] numConsonantsChance = new int[] { 80, 350, 25, 5, 1 };
        public int[] numVowels = new int[] { 1, 2, 3 };
        public int[] numVowelsChance = new int[] { 180, 25, 1 };
        public char[] vowel = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        public int[] vowelChance = new int[] { 10, 12, 10, 10, 8, 2 };
        public char[] consonant = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
        public int[] consonantChance = new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 12, 12, 12, 10, 5, 12, 12, 12, 8, 8, 3, 4, 3 };
        public Random random;

        /// <summary>
        /// Create an instance.
        /// </summary>
        public NameGenerator()
        {
            random = new Random();
        }

        public int IndexSelect(int[] intArray)
        {
            int totalPossible = 0;
            for (int i = 0; i < intArray.Length; i++)
            {
                totalPossible += intArray[i];
            }
            int chosen = random.Next(totalPossible);
            int chancesSoFar = 0;
            for (int j = 0; j < intArray.Length; j++)
            {
                chancesSoFar += intArray[j];
                if (chancesSoFar > chosen)
                {
                    return j;
                }
            }
            return 0;
        }
        public string MakeSyllable()
        {
            return MakeConsonantBlock() + MakeVowelBlock() + MakeConsonantBlock();
        }
        public string MakeConsonantBlock()
        {
            string newName = "";
            int numberConsonants = numConsonants[IndexSelect(numConsonantsChance)];
            for (int i = 0; i < numberConsonants; i++)
            {
                newName += consonant[IndexSelect(consonantChance)];
            }
            return newName;
        }
        public string MakeVowelBlock()
        {
            string newName = "";
            int numberVowels = numVowels[IndexSelect(numVowelsChance)];
            for (int i = 0; i < numberVowels; i++)
            {
                newName += vowel[IndexSelect(vowelChance)];
            }
            return newName;
        }

        /// <summary>
        /// Generates a name randomly using certain construction rules. The name
        /// will be different each time it is called.
        /// </summary>
        /// <returns>A name string.</returns>
        public string MakeName()
        {
            int numberSyllables = numSyllables[IndexSelect(numSyllablesChance)];
            string newName = "";
            for (int i = 0; i < numberSyllables; i++)
            {
                newName += MakeSyllable();
            }
            return char.ToUpper(newName[0]) + newName.Substring(1);
        }
    }

    public class Nas : Plugin
    {
        public override string name 
        { 
            get 
            { 
                return "Nas"; 
            } 
        }
        public override string MCGalaxy_Version 
        { 
            get 
            { 
                return Server.NasVersion; 
            } 
        }
        public override string creator 
        { 
            get 
            { 
                return "HarmonyNetwork"; //Goodly/Zoey no longer supports NAS. 
            } 
        }
        public const string textureURL = "https://dl.dropboxusercontent.com/s/2x5oxffkgpcyj16/nas.zip?dl=0";
        public const string KeyPrefix = "nas_";
        public const string PlayerKey = KeyPrefix + "NasPlayer";
        public const string Path = "nas/";
        public const string SavePath = Path + "playerdata/";
        public const string CoreSavePath = Path + "coredata/";
        public const string EffectsPath = Path + "effects/";
        public static string GetSavePath(Player p)
        {
            return SavePath + p.name + ".json";
        }
        public static string GetTextPath(Player p)
        {
            return SavePath + p.name + ".txt";
        }
        public static bool firstEverPluginLoad = true;
        public static bool EnsureFileExists(string url, string file)
        {
            if (File.Exists(file)) return true;

            Logger.Log(LogType.SystemActivity, file + " doesn't exist, Downloading..");
            try
            {
                using (WebClient client = HttpUtil.CreateWebClient())
                {
                    client.DownloadFile(url, file);
                }
                if (File.Exists(file))
                {
                    Logger.Log(LogType.SystemActivity, file + " download succesful!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                bool canRetry = HandleErrorResponse(ex, url, 30);
                HttpUtil.DisposeErrorResponse(ex);
                if (!canRetry)
                {
                    Logger.LogError("Downloading " + file + " failed, try again later", ex);
                    return false;
                }
                else
                {
                    Retry(url, file);
                }
            }
            return false;
        }
        public static void Retry(string url, string file)
        {
            EnsureFileExists(url, file);
        }
        public static bool HandleErrorResponse(Exception ex, string msg, int retry)
        {
            return HandleErrorResponse((WebException)ex, msg, retry);
        }
        public static bool HandleErrorResponse(WebException ex, string msg, int retry)
        {
            string err = HttpUtil.GetErrorResponse(ex);
            HttpStatusCode status = GetStatus(ex);
            // 429 errors simply require retrying after sleeping for a bit
            if (status == (HttpStatusCode)429)
            {
                Sleep();
                return true;
            }

            // 500 errors might be temporary outage, so still retry a few times
            if (status >= (HttpStatusCode)500 && status <= (HttpStatusCode)504)
            {
                LogWarning(ex);
                LogResponse(err);
                return retry < 2;
            }

            // If unable to reach site at all, immediately give up
            if (ex.Status == WebExceptionStatus.NameResolutionFailure)
            {
                LogWarning(ex);
                return false;
            }

            // May be caused by connection dropout/reset, so still retry a few times
            if (ex.InnerException is IOException)
            {
                LogWarning(ex);
                return retry < 2;
            }

            LogError(ex, msg);
            LogResponse(err);
            return false;
        }
        static HttpStatusCode GetStatus(WebException ex)
        {
            if (ex.Response == null) return 0;
            return ((HttpWebResponse)ex.Response).StatusCode;
        }
        static void LogError(Exception ex, string target)
        {
            Logger.Log(LogType.Warning, "Error sending request to Github API {0}: {1}", target, ex);
        }

        static void LogWarning(Exception ex)
        {
            Logger.Log(LogType.Warning, "Error sending request to Github API - " + ex.Message);
        }

        static void LogResponse(string err)
        {
            if (string.IsNullOrEmpty(err)) return;

            // Github sometimes returns <html>..</html> responses for internal server errors
            //  most of this is useless content, so just truncate these particular errors 
            if (err.Length > 200) err = err.Substring(0, 200) + "...";

            Logger.Log(LogType.Warning, "Github API returned: " + err);
        }


        static void Sleep()
        {
            float delay = 30;
            Logger.Log(LogType.SystemActivity, "Waiting to download files...");
            Thread.Sleep(TimeSpan.FromSeconds(delay + 0.5f));
        }
        public static void EnsureNasFilesExist()
        {
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/selectorColors.png", Path + "selectorColors.png");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/terrain.png", Path + "terrain.png");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/effects/breakdust.properties", EffectsPath + "breakdust.properties");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/effects/breakleaf.properties", EffectsPath + "breakleaf.properties");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/effects/breakmeter.properties", EffectsPath + "breakmeter.properties");
            EnsureFileExists("https://github.com/SuperNova-DeadNova/MCGalaxy/raw/debug/Uploads/nas/coredata/time.json", CoreSavePath + "time.json");
            EnsureFileExists("https://github.com/cloverpepsi/place.cs/raw/main/global.json", "blockdefs/global.json");
        }
        public override void Load(bool startup)
        {
            if (!Directory.Exists(Path)) 
            { 
                Directory.CreateDirectory(Path); 
            }
            if (!Directory.Exists(NasLevel.Path)) 
            { 
                Directory.CreateDirectory(NasLevel.Path); 
            }
            if (!Directory.Exists(SavePath)) 
            { 
                Directory.CreateDirectory(SavePath); 
            }
            if (!Directory.Exists(CoreSavePath)) 
            { 
                Directory.CreateDirectory(CoreSavePath); 
            }
            if (!Directory.Exists(NasEffect.Path)) 
            { 
                Directory.CreateDirectory(NasEffect.Path); 
            }
            if (!Directory.Exists("blockprops")) 
            { 
                Directory.CreateDirectory("blockprops"); 
            }
            if (Directory.Exists("plugins/nas"))
            {
                string[] pluginfiles = Directory.GetFiles("plugins/nas");
                foreach (string pluginfile in pluginfiles)
                {
                    string[] files = Directory.GetFiles("nas");
                    foreach (string file in files)
                    {
                        if (!File.Exists(file))
                        {
                            File.Copy(pluginfile, System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/nas/" + pluginfile, true);
                            File.Delete(pluginfile);
                        }
                        else
                        {
                            File.Delete(pluginfile);
                            Logger.Log(LogType.Warning, "Duplicate file {0} found in plugins/nas folder, this should not have happened!");
                        }
                    }
                }
            }
            if (!File.Exists("text/BookTitles.txt"))
            {
                File.WriteAllText("text/BookTitles.txt", "");
            }
            if (!File.Exists("text/BookAuthors.txt"))
            {
                File.WriteAllText("text/BookAuthors.txt", "");
            }
            if (Block.Props.Length != 1024)
            { //check for TEN_BIT_BLOCKS. Value is 512 on a default instance of MCGalaxy.
                Player.Console.Message("NAS: FAILED to load plugin. In order to run NAS, you must be using a version of MCGalaxy which allows 767 blocks.");
                Player.Console.Message("NAS: You can find instructions for 767 blocks here: https://github.com/ClassiCube/MCGalaxy/tree/master/Uploads (infid)");
                return;
            }
            if (File.Exists("plugins/Newtonsoft.Json"))
            {
                if (!File.Exists("Newtonsoft.Json.dll"))
                {
                    File.Move("plugins/Newtonsoft.Json", "Newtonsoft.Json.dll");
                }
                else
                {
                    File.Delete("plugins/Newtonsoft.Json");
                }
            }
            if (!File.Exists("Newtonsoft.Json.dll"))
            {
                Player.Console.Message("NAS: FAILED to load plugin. Could not find Newtonsoft.Json.dll"); 
                return;
            }
            //I HATE IT
            MoveFile("global.json", "blockdefs/global.json"); //blockdefs
            MoveFile("default.txt", "blockprops/default.txt"); //blockprops
            MoveFile("customcolors.txt", "text/customcolors.txt"); //custom chat colors
            MoveFile("command.properties", "props/command.properties"); //command permissions
            MoveFile("ExtraCommandPermissions.properties", "props/ExtraCommandPermissions.properties"); //extra command permissions
            MoveFile("ranks.properties", "props/ranks.properties"); //ranks
            MoveFile("faq.txt", "text/faq.txt"); //faq
            MoveFile("messages.txt", "text/messages.txt"); //messages
            MoveFile("welcome.txt", "text/welcome.txt"); //welcome
            string loadFile = "props/loaded.txt";
            string message = "Do not delete this file unless you are using the plugin for the first time!";
            if (!File.Exists(loadFile))
            {
                firstEverPluginLoad = true;
                File.WriteAllText(loadFile, message);
            }
            if (firstEverPluginLoad)
            {
                Server.Config.DefaultTexture = textureURL;
                Server.Config.DefaultColor = "&7";
                Server.Config.verifyadmins = false;
                Server.Config.EdgeLevel = 60;
                Server.Config.SidesOffset = -200;
                Server.Config.CloudsHeight = 200;
                Server.Config.MaxFogDistance = 512;
                Server.Config.SkyColor = "#1489FF";
                Server.Config.ShadowColor = "#888899";
                SrvProperties.Save();
            }
            EnsureNasFilesExist();
            //I HATE IT
            OnlineStat.Stats.Add(PvP);
            OnlineStat.Stats.Add(Kills);
            Command.Register(new NasPlayer.CmdPVP());
            Command.Register(new NasPlayer.CmdNASSpawn());
            Command.Register(new NasPlayer.CmdBarrelMode());
            Command.Register(new NasPlayer.CmdSpawnDungeon());
            NasPlayer.Setup();
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
            NasGen.Setup();
            NasLevel.Setup();
            NasTimeCycle.Setup();
            if (firstEverPluginLoad)
            {
                //Player.Console.Message("GENERATING NEW MAP FIRST TIME EVER also main is {0}", lvl.name);
                int chunkOffsetX = 0, chunkOffsetZ = 0;
                string seed = "DEFAULT";
                if (!NasGen.GetSeedAndChunkOffset(Server.mainLevel.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ))
                {
                    Player.Console.Message("NAS: main level is not a NAS level, generating a NAS level to replace it!");
                    seed = new NameGenerator().MakeName().ToLower();
                    string mapName = seed + "_0,0";
                    Command.Find("newlvl").Use(Player.Console,
                                               mapName +
                                               " " + NasGen.mapWideness +
                                               " " + NasGen.mapTallness +
                                               " " + NasGen.mapWideness +
                                               " nasgen " + seed);
                    Server.Config.MainLevel = mapName;
                    SrvProperties.Save();
                    //Server.SetMainLevel(mapName);
                    Thread.Sleep(1000);
                    Chat.Message(ChatScope.All, "A server restart is required to initialize NAS plugin.", null, null, true);
                    Server.Stop(true, "A server restart is required to initialize NAS plugin.");
                }
            }
        }
        public void PvP(Player p, Player target)
        {
            if (NasPlayer.GetNasPlayer(target).pvpEnabled)
            {
                p.Message("&7  &fPLAYER HAS PVP &2ENABLED");
            }
            else
            {
                p.Message("&7  &fPLAYER HAS PVP &cDISABLED");
            }
        }
        public void Kills(Player p, Player target)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(target);
            p.Message("&7  &7Player has " + np.kills + " kills.");
        }
        public static void MoveFile(string pluginFile, string destFile)
        {
            pluginFile = "plugins/" + pluginFile;
            if (File.Exists(pluginFile))
            {
                if (File.Exists(destFile)) 
                { 
                    File.Delete(destFile); 
                }
                File.Move(pluginFile, destFile);
            }
            else
            {
                //firstEverPluginLoad = false;
            }
        }
        public static void FailedLoad()
        {
            string msg = "NAS: FAILED to load plugin. Please report this to randomstrangers on Discord!";
            //string msg = "NAS: FAILED to load plugin. Please follow the instructions found on github.";
            Player.Console.Message(msg);
        }
        public override void Unload(bool shutdown)
        {
            NasPlayer.TakeDown();
            DynamicColor.TakeDown();
            Command.Unregister(Command.Find("PVP"));
            Command.Unregister(Command.Find("NASSpawn"));
            Command.Unregister(Command.Find("BarrelMode"));
            Command.Unregister(Command.Find("SpawnDungeon"));
            OnlineStat.Stats.Remove(PvP);
            OnlineStat.Stats.Remove(Kills);
            OnPlayerConnectEvent.Unregister(OnPlayerConnect);
            OnPlayerClickEvent.Unregister(OnPlayerClick);
            OnBlockChangingEvent.Unregister(OnBlockChanging);
            OnBlockChangedEvent.Unregister(OnBlockChanged);
            OnPlayerMoveEvent.Unregister(OnPlayerMove);
            OnPlayerDisconnectEvent.Unregister(OnPlayerDisconnect);
            OnJoinedLevelEvent.Register(OnLevelJoined, Priority.High);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
            NasLevel.TakeDown();
            NasTimeCycle.TakeDown();
        }
        public static void OnLevelJoined(Player p, Level prevLevel, Level level, ref bool announce)
        {
            level.Config.SkyColor = NasTimeCycle.globalSkyColor;
            level.Config.CloudColor = NasTimeCycle.globalCloudColor;
            level.Config.LightColor = NasTimeCycle.globalSunColor;
        }
        /*public static void OnPlayerConnect(Player p)
        {
            //Player.Console.Message("OnPlayerConnect");
            string path = GetSavePath(p);
            string pathText = GetTextPath(p);
            NasPlayer np = new NasPlayer(p);
            if (!File.Exists(pathText))
            {
                File.Create(pathText).Dispose();
            }
            if (File.Exists(path))
            {
                ConfigElement[] configElements = ConfigElement.GetAll(typeof(NasPlayer));
                foreach (ConfigElement config in configElements)
                {
                    string jsonString = config.Format(typeof(NasPlayer));
                    np = JsonConvert.DeserializeObject<NasPlayer>(jsonString);
                }
                if (np == null)
                {
                    np = new NasPlayer(p);
                    Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                    NasEntity.SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                    p.Extras[PlayerKey] = np;
                    Logger.Log(LogType.Debug, "Created new save file for " + p.name + "!");
                }
                else
                {
                    np.SetPlayer(p);
                    p.Extras[PlayerKey] = np;
                    Logger.Log(LogType.Debug, "Loaded save file " + path + "!");
                }
            }
            else
            {
                np = new NasPlayer(p);
                Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                NasEntity.SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Created new save file for " + p.name + "!");
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NasBlock.Default);
            if (!np.bigUpdate || np.resetCount != 1)
            { //tick up the one whenever you want to do a reset
                np.UpdateValues();
            }
            //Q and E
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 16, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 18, 0, true));
            //arrow keys
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar up◙", 200, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar down◙", 208, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 203, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 205, 0, true));
            //WASD (lol)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen up◙", 17, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen down◙", 31, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen left◙", 30, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen right◙", 32, 0, true));
            //M and R
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar move◙", 50, 0, true)); //was 50 (M) was 42 (shift)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar inv◙", 19, 0, true)); //was 23 (i)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar delete◙", 45, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar confirmdelete◙", 25, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar toolinfo◙", 23, 0, true));
        }*/
        static void OnPlayerConnect(Player p)
        {
            //Player.Console.Message("OnPlayerConnect");
            string path = GetSavePath(p);
            string pathText = GetTextPath(p);

            NasPlayer np;

            if (!File.Exists(pathText)) File.Create(pathText).Dispose();
            if (File.Exists(path))
            {
                string jsonString = File.ReadAllText(path);
                np = JsonConvert.DeserializeObject<NasPlayer>(jsonString);
                np.SetPlayer(p);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Loaded save file " + path + "!");
            }
            else
            {
                np = new NasPlayer(p);
                Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                NasEntity.SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[PlayerKey] = np;
                Logger.Log(LogType.Debug, "Created new save file for " + p.name + "!");
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NasBlock.Default);
            if (!np.bigUpdate || np.resetCount != 1)
            { //tick up the one whenever you want to do a reset
                np.UpdateValues();
            }
            //Q and E
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 16, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 18, 0, true));
            //arrow keys
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar up◙", 200, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar down◙", 208, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 203, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 205, 0, true));

            //WASD (lol)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen up◙", 17, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen down◙", 31, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen left◙", 30, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar bagopen right◙", 32, 0, true));

            //M and R
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar move◙", 50, 0, true)); //was 50 (M) was 42 (shift)
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar inv◙", 19, 0, true)); //was 23 (i)

            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar delete◙", 45, 0, true));
            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar confirmdelete◙", 25, 0, true));

            p.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar toolinfo◙", 23, 0, true));

        }
        public static void OnPlayerCommand(Player p, string cmd, string message, CommandData data)
        {
            if (cmd.CaselessEq("setall"))
            {
                if (p.Rank < LevelPermission.Operator) 
                { 
                    return; 
                }
                foreach (Command _cmd in Command.allCmds)
                {
                    //p.Message("name {0}", _cmd.name);
                    Command.Find("cmdset").Use(p, _cmd.name + " Operator");
                }
                p.cancelcommand = true;
                return;
            }
            if (cmd.CaselessEq("gentree"))
            {
                if (p.Rank < LevelPermission.Operator) 
                { 
                    return; 
                }
                string[] messageString = message.SplitSpaces();
                NasTree.GenSwampTree(NasLevel.Get(p.level.name), new Random(), int.Parse(messageString[0]), int.Parse(messageString[1]), int.Parse(messageString[2]));
                p.cancelcommand = true;
                return;
            }
            if (cmd.CaselessEq("time") && message.SplitSpaces()[0] == "set")
            {
                if (message.SplitSpaces().Length > 1)
                {
                    int time = 0;
                    string setTime = message.SplitSpaces()[1];
                    if (setTime == "sunrise") 
                    { 
                        time = 8 * NasTimeCycle.hourMinutes; 
                    }
                    else
                    {
                        if (setTime == "day") 
                        { 
                            time = 7 * NasTimeCycle.hourMinutes;
                        }
                        else
                        {
                            if (setTime == "sunset") 
                            { 
                                time = 19 * NasTimeCycle.hourMinutes; 
                            }
                            else
                            {
                                if (setTime == "night") 
                                { 
                                    time = 20 * NasTimeCycle.hourMinutes; 
                                }
                                else
                                {
                                    if (setTime == "midnight") 
                                    { 
                                        time = 0; 
                                    }
                                    else
                                    {
                                        if (!CommandParser.GetInt(p, setTime, "Amount", ref time, 0)) 
                                        { 
                                            return; 
                                        }
                                    }
                                }
                            }
                        }
                    }
                    NasTimeCycle.cycleCurrentTime = time % NasTimeCycle.cycleMaxTime;
                    p.cancelcommand = true;
                }
            }
            if (cmd.CaselessEq("goto") && p.Rank < LevelPermission.Operator && data.Context != CommandContext.SendCmd)
            {
                p.Message("You cannot use /goto manually. It is triggered automatically when you go to map borders.");
                p.cancelcommand = true;
                return;
            }
            if (cmd.CaselessEq("deleteall") && p.Rank >= LevelPermission.Operator)
            {
                if (message.Length == 0) 
                { 
                    return; 
                }
                string[] allMaps = LevelInfo.AllMapNames();
                Command deleteLvl = Command.Find("deletelvl");
                foreach (string mapName in allMaps)
                {
                    if (mapName.StartsWith(message))
                    {
                        deleteLvl.Use(p, mapName);
                    }
                }
                p.cancelcommand = true;
                return;
            }
            if (cmd.CaselessEq("color"))
            {
                if (message.Length == 0) 
                { 
                    return; 
                }
                string[] args = message.Split(' ');
                string color = args[args.Length - 1];
                if (Matcher.FindColor(p, color) == "&h")
                {
                    p.Message("That color isn't allowed in names.");
                    p.cancelcommand = true; 
                    return;
                }
                return;
            }
            if (cmd.CaselessEq("staff"))
            {
                if (message != "alt")
                {
                    NasPlayer npl = NasPlayer.GetNasPlayer(p);
                    npl.staff = !npl.staff;
                    return;
                }
            }
            if (cmd.CaselessEq("smite"))
            {
                if (p.Rank < LevelPermission.Operator) 
                { 
                    return; 
                }
                NasPlayer nw = NasPlayer.GetNasPlayer(PlayerInfo.FindMatches(p, message));
                nw.lastAttackedPlayer = p;
                nw.TakeDamage(50, NasEntity.DamageSource.Entity, "@p &fwas smote by " + p.ColoredName);
                return;
            }
            if (!cmd.CaselessEq("nas")) 
            { 
                return; 
            }
            p.cancelcommand = true;
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            string[] words = message.Split(' ');
            if (words.Length > 1 && words[0] == "hotbar")
            {
                string hotbarFunc = words[1];
                if (words.Length > 2)
                {
                    string func2 = words[2];
                    if (hotbarFunc == "bagopen")
                    {
                        if (!np.inventory.bagOpen) 
                        {
                            return; 
                        }
                        if (func2 == "left") 
                        { 
                            np.inventory.MoveItemBarSelection(-1); 
                            return; 
                        }
                        if (func2 == "right") 
                        { 
                            np.inventory.MoveItemBarSelection(1); 
                            return; 
                        }
                        if (func2 == "up") 
                        { 
                            np.inventory.MoveItemBarSelection(-Inventory.itemBarLength); 
                            return; 
                        }
                        if (func2 == "down") 
                        { 
                            np.inventory.MoveItemBarSelection(Inventory.itemBarLength); 
                            return; 
                        }
                    }
                    return;
                }
                if (hotbarFunc == "left") 
                {
                    np.inventory.MoveItemBarSelection(-1); 
                    return; 
                }
                if (hotbarFunc == "right") 
                { 
                    np.inventory.MoveItemBarSelection(1); 
                    return; 
                }
                if (hotbarFunc == "up") 
                { 
                    np.inventory.MoveItemBarSelection(-Inventory.itemBarLength); 
                    return; 
                }
                if (hotbarFunc == "down") 
                { 
                    np.inventory.MoveItemBarSelection(Inventory.itemBarLength); 
                    return; 
                }
                if (hotbarFunc == "move") 
                { 
                    np.inventory.DoItemMove(); 
                    return;
                }
                if (hotbarFunc == "inv") 
                { 
                    np.inventory.ToggleBagOpen(); 
                    return; 
                }
                if (hotbarFunc == "delete") 
                { 
                    np.inventory.DeleteItem(); 
                    return; 
                }
                if (hotbarFunc == "confirmdelete") 
                { 
                    np.inventory.DeleteItem(true); 
                    return; 
                }
                if (hotbarFunc == "toolinfo") 
                {
                    np.inventory.ToolInfo(); 
                    return; 
                }
                return;
            }
        }
        public static void OnPlayerMessage(Player p, string message)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (np.isInserting)
            {
                int items = 0;
                if (!CommandParser.GetInt(p, message, "Amount", ref items, 0)) 
                { 
                    return; 
                }
                if (items == 0) 
                { 
                    p.cancelchat = true; 
                    p.SendMapMotd(); 
                    np.isInserting = false;
                    return; 
                }
                ushort clientushort = np.ConvertBlock(p.ClientHeldBlock);
                NasBlock nasBlock = NasBlock.Get(clientushort);
                int amount = np.inventory.GetAmount(nasBlock.parentID);
                if (items > amount)
                {
                    items = amount;
                }
                if (amount < 1)
                {
                    p.Message("You don't have any {0} to store.", nasBlock.GetName(p));
                    return;
                }
                int x = np.interactCoords[0]; 
                int y = np.interactCoords[1]; 
                int z = np.interactCoords[2];
                NasBlock.Entity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
                if (bEntity.drop == null)
                {
                    np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                    bEntity.drop = new Drop(nasBlock.parentID, items);
                    p.cancelchat = true;
                    p.SendMapMotd();
                    np.isInserting = false;
                    return;
                }
                foreach (BlockStack stack in bEntity.drop.blockStacks)
                {
                    //if a stack exists in the container already, add to that stack
                    if (stack.ID == nasBlock.parentID)
                    {
                        np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                        stack.amount += items;
                        p.cancelchat = true;
                        p.SendMapMotd();
                        np.isInserting = false;
                        return;
                    }
                }
                if (bEntity.drop.blockStacks.Count >= NasBlock.Container.BlockStackLimit)
                {
                    p.Message("It can't contain more than {0} stacks of blocks.", NasBlock.Container.BlockStackLimit);
                    p.cancelchat = true;
                    p.SendMapMotd();
                    np.isInserting = false;
                    return;

                }
                np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                bEntity.drop.blockStacks.Add(new BlockStack(nasBlock.parentID, items));
                p.cancelchat = true;
                p.SendMapMotd();
                np.isInserting = false;
            }
        }
        /*public static void OnPlayerDisconnect(Player p, string reason)
        {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            //np.hasBeenSpawned = false;
            if (p == null)
            {
                Logger.Log(LogType.Debug, "Player was null!");
                return;
            }
            try
            {
                string jsonString = JsonConvert.SerializeObject(np, Formatting.Indented);
                File.WriteAllText(GetSavePath(p), jsonString);
                if (File.Exists(GetTextPath(p))) 
                {
                    File.Delete(GetTextPath(p));
                }
                File.Copy(GetSavePath(p), GetTextPath(p));
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving playerdata for " + p.name, ex);
            }
        }*/
        static void OnPlayerDisconnect(Player p, string reason)
        {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];

            //np.hasBeenSpawned = false;
            string jsonString;
            jsonString = JsonConvert.SerializeObject(np, Formatting.Indented);
            File.WriteAllText(GetSavePath(p), jsonString);
            File.WriteAllText(GetTextPath(p), jsonString);
        }
        public static void OnPlayerClick(Player p, MouseButton button, 
            MouseAction action, ushort yaw, ushort pitch, byte entity, 
            ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (p.level.Config.Deletable && p.level.Config.Buildable) 
            { 
                return; 
            }
            NasPlayer.ClickOnPlayer(p, entity, button, action);
            if (button == MouseButton.Left) 
            { 
                NasBlockChange.HandleLeftClick(p, button, action, yaw, pitch, entity, x, y, z, face); 
            }
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (button == MouseButton.Middle && action == MouseAction.Pressed)
            {
                //np.ChangeHealth(0.5f);
                //ushort here = p.level.GetBlock(x, y, z);
                //p.Message("nasBlock {0}", NasBlock.blocksIndexedByServerushort[here].GetName(p));
                //NasBlock.blocksIndexedByServerushort[here].collideAction(np, NasBlock.blocks[1], true, x, y, z);
            }
            if (button == MouseButton.Right && action == MouseAction.Pressed)
            {
                //np.TakeDamage(0.5f);
            }
            if (!np.justBrokeOrPlaced)
            {
                np.HandleInteraction(button, action, x, y, z, entity, face);
            }
            if (action == MouseAction.Released)
            {
                np.justBrokeOrPlaced = false;
            }
        }
        public static void OnBlockChanging(Player p, ushort x, ushort y, ushort z, ushort block, bool placing, ref bool cancel)
        {
            NasBlockChange.PlaceBlock(p, x, y, z, block, placing, ref cancel);
        }
        public static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result)
        {
            NasBlockChange.OnBlockChanged(p, x, y, z, result);
        }
        public static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            NasPlayer np = (NasPlayer)p.Extras[PlayerKey];
            np.DisplayHealth();
            np.DoMovement(next, yaw, pitch);
        }
    }
}
#endif
