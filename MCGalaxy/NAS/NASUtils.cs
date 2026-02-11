using MCGalaxy.Commands;
using MCGalaxy.Network;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
namespace MCGalaxy
{
    public partial class NAS
    {
        public static bool HasExtraPerm(NASPlayer np, string cmd, int num) => CommandExtraPerms.Find(cmd, num).UsableBy(np.p.Rank);
        public static void SaveAll(Player p)
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                if (!lvl.SaveChanges)
                {
                    continue;
                }
                NASLevel nl = NASLevel.Get(lvl.name);
                string jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented),
                    fileName = NASLevel.GetFileName(nl.lvl.name);
                bool saved = lvl.Save(true) && FileIO.TryWriteAllText(fileName, jsonString);
                if (!saved)
                {
                    p.Message("Saving of level {0} &Swas cancelled", lvl.ColoredName);
                }
            }
            Chat.MessageGlobal("All levels have been saved.");
        }
        public static void GenLevel()
        {
            int chunkOffsetX = 0, chunkOffsetZ = 0;
            string seed = "DEFAULT";
            if (!NASGen.GetSeedAndChunkOffset(Server.mainLevel.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ))
            {
                Log("NAS: {0} is not a valid NAS level, generating a new NAS level to replace it!", Server.mainLevel.name);
                seed = new NASNameGenerator().MakeName().ToLower();
                string mapName = seed + "_0,0";
                NASLevel.GenerateMap(Player.Console,
                                           mapName,
                                           NASGen.mapWideness.ToString(),
                                           NASGen.mapTallness.ToString(),
                                           NASGen.mapWideness.ToString(),
                                           seed);
                Server.Config.MainLevel = mapName;
                Server.Save();
                Chat.Message(ChatScope.All, "A server restart is required to initialize NAS.", null, null, true);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                Server.Stop(true, "A server restart is required to initialize NAS.");
            }
        }
        public static void LoadFirstTime()
        {
            Server.Config.DefaultTexture = "https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/texturepack.zip";
            Server.Config.DefaultColor = "&7";
            Server.Config.verifyadmins = false;
            Server.Config.EdgeLevel = 60;
            Server.Config.SidesOffset = -200;
            Server.Config.CloudsHeight = 200;
            Server.Config.MaxFogDistance = 512;
            Server.Config.SkyColor = "#1489FF";
            Server.Config.ShadowColor = "#888899";
            Server.Save();
        }
        public static void EnsureDirectoriesExist(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
        public static string GetSavePath(Player p) => NASPlayer.Path + p.name + ".json";
        public static string GetDeathPath(string name) => NASPlayer.DeathsPath + name + ".txt";
        public static string GetTextPath(Player p) => NASPlayer.Path + p.name + ".txt";
        public static bool EnsureFileExists(string url, string file)
        {
            if (File.Exists(file))
            {
                return true;
            }
            try
            {
                using (WebClient client = new())
                {
                    client.DownloadFile(url, file);
                }
                if (File.Exists(file))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                bool canRetry = HandleErrorResponse((WebException)ex, url, 30);
                HttpUtil.DisposeErrorResponse(ex);
                if (!canRetry)
                {
                    Logger.LogError("Downloading " + file + " failed, try again later", ex);
                    return false;
                }
                else
                {
                    EnsureFileExists(url, file);
                }
            }
            return false;
        }
        public static void Register(params Command[] commands)
        {
            foreach (Command cmd in commands)
            {
                Command.Register(cmd);
            }
        }
        public static void Log(string format, params object[] args) => Logger.Log(LogType.Warning, string.Format(format, args));
        public static bool HandleErrorResponse(WebException ex, string msg, long retry)
        {
            string err = HttpUtil.GetErrorResponse(ex);
            HttpStatusCode status = ex.Response == null ? 0 : ((HttpWebResponse)ex.Response).StatusCode;
            if (status == (HttpStatusCode)429)
            {
                Sleep();
                return true;
            }
            if (status >= HttpStatusCode.InternalServerError && status <= HttpStatusCode.GatewayTimeout)
            {
                Logger.Log(LogType.Warning, "Error sending request to Github API - {0}", ex.Message);
                if (!string.IsNullOrEmpty(err))
                {
                    if (err.Length > 200)
                    {
                        err = err.Substring(0, 200) + "...";
                    }
                    Logger.Log(LogType.Warning, "Github API returned: " + err);
                }
                return retry < 2;
            }
            if (ex.Status == WebExceptionStatus.NameResolutionFailure)
            {
                Logger.Log(LogType.Warning, "Error sending request to Github API - {0}", ex.Message);
                return false;
            }
            if (ex.InnerException is IOException)
            {
                Logger.Log(LogType.Warning, "Error sending request to Github API - {0}", ex.Message);
                return retry < 2;
            }
            Logger.Log(LogType.Warning, "Error sending request to Github API {0}: {1}", msg, ex.Message);
            if (!string.IsNullOrEmpty(err))
            {
                if (err.Length > 200)
                {
                    err = err.Substring(0, 200) + "...";
                }
                Logger.Log(LogType.Warning, "Github API returned: " + err);
            }
            return false;
        }
        public static void Sleep() => Thread.Sleep(TimeSpan.FromSeconds(30 + 0.5f));
    }
    public delegate void NASAction<T1>(T1 arg1);
    public delegate void NASAction<T1, T2>(T1 arg1, T2 arg2);
    public delegate void NASAction<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void NASAction<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void NASAction<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void NASAction<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void NASAction<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult NASFunc<T1, T2, out TResult>(T1 arg1, T2 arg2);
}
