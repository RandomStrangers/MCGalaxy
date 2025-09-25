#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.DB;
using MCGalaxy.Generator;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
namespace NotAwesomeSurvival
{
    public partial class Nas
    {
        public static bool HasExtraPerm(NasPlayer np, string cmd, int num)
        {
            return CommandExtraPerms.Find(cmd, num).UsableBy(np.p.Rank);
        }
        public static void SaveAll(Player p)
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                TrySave(p, lvl);
            }
            Chat.MessageGlobal("All levels have been saved.");
        }
        public static bool TrySave(Player p, Level lvl)
        {
            if (!lvl.SaveChanges)
            {
                p.Message("Saving {0} &Sis currently disabled (most likely because a game is or was running on the level)", lvl.ColoredName);
                return false;
            }
            bool saved = lvl.Save(true);
            if (!saved)
            {
                p.Message("Saving of level {0} &Swas cancelled", lvl.ColoredName);
            }
            return saved;
        }
        public static void GenLevel()
        {
            int chunkOffsetX = 0, chunkOffsetZ = 0;
            string seed = "DEFAULT";
            if (!NasGen.GetSeedAndChunkOffset(Server.mainLevel.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ))
            {
                Log("NAS: {0} is not a NAS level, generating a NAS level to replace it!", Server.mainLevel.name);
                seed = new NameGenerator().MakeName().ToLower();
                string mapName = seed + "_0,0";
                NasLevel.GenerateMap(Player.Console,
                                           mapName,
                                           NasGen.mapWideness,
                                           NasGen.mapTallness,
                                           NasGen.mapWideness,
                                           seed);
                Server.Config.MainLevel = mapName;
                SrvProperties.Save();
                Chat.Message(ChatScope.All, "A server restart is required to initialize NAS plugin.", null, null, true);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                Server.Stop(true, "A server restart is required to initialize NAS plugin.");
            }
        }
        public static void LoadFirstTime()
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
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static void MoveFile(string file, string destFile)
        {
            if (File.Exists(file))
            {
                if (File.Exists(destFile))
                {
                    FileIO.TryDelete(destFile);
                }
                FileIO.TryMove(file, destFile);
            }
            else
            {
            }
        }
        public static string GetSavePath(Player p)
        {
            return SavePath + p.name + ".json";
        }
        public static string GetDeathPath(string name)
        {
            return NasPlayer.DeathsPath + name + ".txt";
        }
        public static string GetTextPath(Player p)
        {
            return SavePath + p.name + ".txt";
        }
        public static bool IsDev(Player p)
        {
            if (Devs.CaselessContains(p.truename))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsDev(PlayerData data)
        {
            if (Devs.CaselessContains(data.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool EnsureFileExists(string url, string file)
        {
            if (File.Exists(file))
            {
                return true;
            }
            Log("{0} doesn't exist, Downloading..", file);
            try
            {
                using (WebClient client = HttpUtil.CreateWebClient())
                {
                    client.DownloadFile(url, file);
                }
                if (File.Exists(file))
                {
                    Log("{0} download successful!", file);
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
        public static void Register(params Command[] commands)
        {
            foreach (Command cmd in commands)
            {
                Command.Register(cmd);
            }
        }
        public static void MovePluginFile(string pluginFile, string destFile)
        {
            MoveFile("plugins/" + pluginFile, destFile);
        }
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public static void Retry(string url, string file)
        {
            EnsureFileExists(url, file);
        }
        public static bool HandleErrorResponse(Exception ex, string msg, long retry)
        {
            return HandleErrorResponse((WebException)ex, msg, retry);
        }
        public static bool HandleErrorResponse(WebException ex, string msg, long retry)
        {
            string err = HttpUtil.GetErrorResponse(ex);
            HttpStatusCode status = GetStatus(ex);
            if (status == (HttpStatusCode)429)
            {
                Sleep();
                return true;
            }
            if (status >= (HttpStatusCode)500 && status <= (HttpStatusCode)504)
            {
                LogWarning(ex);
                LogResponse(err);
                return retry < 2;
            }
            if (ex.Status == WebExceptionStatus.NameResolutionFailure)
            {
                LogWarning(ex);
                return false;
            }
            if (ex.InnerException is IOException)
            {
                LogWarning(ex);
                return retry < 2;
            }
            LogWarning(ex, msg);
            LogResponse(err);
            return false;
        }
        public static HttpStatusCode GetStatus(WebException ex)
        {
            if (ex.Response == null)
            {
                return 0;
            }
            return ((HttpWebResponse)ex.Response).StatusCode;
        }
        public static void LogWarning(Exception ex, string target)
        {
            Logger.Log(LogType.Warning, "Error sending request to Github API {0}: {1}", target, ex.Message);
        }
        public static void LogWarning(string message)
        {
            Logger.Log(LogType.Warning, message);
        }
        public static void LogWarning(Exception ex)
        {
            Logger.Log(LogType.Warning, "Error sending request to Github API - {0}", ex.Message);
        }
        public static void LogResponse(string err)
        {
            if (string.IsNullOrEmpty(err))
            {
                return;
            }
            if (err.Length > 200)
            {
                err = err.Substring(0, 200) + "...";
            }
            LogWarning("Github API returned: " + err);
        }
        public static void Sleep()
        {
            float delay = 30;
            Log("Waiting to download files...");
            Thread.Sleep(TimeSpan.FromSeconds(delay + 0.5f));
        }
    }
    public partial class NasPlayer
    {
        public static bool GetCoords(Player p, string[] args, int argsOffset, ref Vec3S32 P)
        {
            return
                GetCoordInt(p, args[argsOffset + 0], "X coordinate", ref P.X) &&
                GetCoordInt(p, args[argsOffset + 1], "Y coordinate", ref P.Y) &&
                GetCoordInt(p, args[argsOffset + 2], "Z coordinate", ref P.Z);
        }
        public static bool ParseRelative(ref string arg)
        {
            bool relative = arg.Length > 0 && (arg[0] == '~' || arg[0] == '#');
            if (relative)
            {
                arg = arg.Substring(1);
            }
            return relative;
        }
        public static bool GetCoordInt(Player p, string arg, string argName, ref int value)
        {
            bool relative = ParseRelative(ref arg);
            if (relative && arg.Length == 0)
            {
                return true;
            }
            int cur = value;
            if (!GetInt(p, arg, argName, ref value))
            {
                return false;
            }
            if (relative)
            {
                value += cur;
            }
            return true;
        }
        public static bool TryParseInt32(string s, out int result)
        {
            return int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
        }
        public static bool CheckRange(Player p, int value, string argName, int min, int max)
        {
            if (value >= min && value <= max)
            {
                return true;
            }
            if (max == int.MaxValue)
            {
                p.Message("&W{0} must be {1} or greater", argName, min);
            }
            else if (min == int.MinValue)
            {
                p.Message("&W{0} must be {1} or less", argName, max);
            }
            else
            {
                p.Message("&W{0} must be between {1} and {2}", argName, min, max);
            }
            return false;
        }
        public static bool GetInt(Player p, string input, string argName, ref int result,
                                  int min = int.MinValue, int max = int.MaxValue)
        {
            if (!TryParseInt32(input, out int value))
            {
                p.Message("&W\"{0}\" is not a valid integer for {1}.", input, argName.ToLowerInvariant());
                return false;
            }
            if (!CheckRange(p, value, argName, min, max))
            {
                return false;
            }
            result = value;
            return true;
        }
    }
    public partial class NasLevel
    {
        public static Level GenerateMap(Player p, string mapName, int width, int height, int length, string seed)
        {
            return GenerateMap(p, mapName, width.ToString(), height.ToString(), length.ToString(), seed);
        }
        public static Level GenerateMap(Player p, string mapName, string width, string height, string length, string seed)
        {
            string[] args = new string[] { mapName, width, height, length, seed };
            MapGen gen = MapGen.Find("NASGen");
            ushort x = 0, y = 0, z = 0;
            if (!MapGen.GetDimensions(p, args, 1, ref x, ref y, ref z, false))
            {
                return null;
            }
            return MapGen.Generate(p, gen, mapName, x, y, z, seed);
        }
    }
    public static class DateExtensions
    {
        public static DateTime Floor(this DateTime date, TimeSpan span)
        {
            long ticks = date.Ticks / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
    }
    public static class Extensions
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value as object == null)
            {
                return true;
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsNullOrEmpty(this string value)
        {
            if (value as object != null)
            {
                return value.Length == 0;
            }
            return true;
        }
    }
    public delegate void Action<T1>(T1 arg1);
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult Func<T1, T2, out TResult>(T1 arg1, T2 arg2);
}
#endif