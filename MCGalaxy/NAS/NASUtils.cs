using MCGalaxy.Commands;
using MCGalaxy.DB;
using MCGalaxy.Maths;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
namespace MCGalaxy
{
    public partial class NASPlugin
    {
        public static ushort Convert(ushort block) => block switch
        {
            70 => 39,
            100 => 20,
            101 => 49,
            102 => 45,
            103 => 1,
            104 => 4,
            106 => 9,
            107 => 11,
            108 => 4,
            109 => 19,
            110 => 5,
            112 => 10,
            71 or 72 => 36,
            111 => 17,
            113 => 49,
            114 => 20,
            115 => 1,
            116 => 18,
            117 => 12,
            118 => 5,
            119 => 25,
            120 => 46,
            121 => 44,
            220 => 42,
            221 => 3,
            222 => 2,
            223 => 29,
            224 => 47,
            253 => 41,
            80 => 4,
            83 => 21,
            85 => 22,
            86 => 23,
            87 => 24,
            89 => 26,
            90 => 27,
            91 => 28,
            92 => 30,
            93 => 31,
            94 => 32,
            95 => 33,
            96 => 34,
            97 => 35,
            98 => 36,
            122 => 17,
            123 => 49,
            124 => 20,
            125 => 1,
            126 => 18,
            127 => 12,
            128 => 5,
            129 => 25,
            135 => 46,
            136 => 44,
            138 => 9,
            139 => 11,
            148 => 17,
            149 => 49,
            150 => 20,
            151 => 1,
            152 => 18,
            153 => 12,
            154 => 5,
            155 => 25,
            156 => 46,
            157 => 44,
            158 => 11,
            159 => 9,
            130 => 36,
            131 => 34,
            133 => 9,
            134 => 11,
            140 or 193 or 196 or 237 or 145 => 8,
            141 => 10,
            143 => 27,
            144 => 22,
            146 => 10,
            147 => 28,
            161 => 9,
            162 => 11,
            166 => 9,
            167 => 11,
            175 => 28,
            176 => 22,
            74 => 46,
            75 => 21,
            182 => 46,
            183 => 46,
            184 => 10,
            185 => 10,
            186 => 46,
            187 => 20,
            188 => 41,
            189 => 42,
            191 => 9,
            190 => 11,
            194 => 10,
            73 => 10,
            195 => 10,
            197 or 200 or 201 or 202 or 203 or 204
                or 205 or 206 or 207 or 208 or 209
                or 210 or 213 or 214 or 215 or 216
                or 217 or 225 or 254 or 81 or 226
                or 227 or 228 or 229 or 84 or 66
                or 67 or 68 or 69 or 137 or 105
                or 132 or 160 or 165 or 164 or 192
                or 168 or 169 or 170 or 171 or 172
                or 173 or 174 or 179 or 180 or 181 => 0,
            211 => 21,
            212 => 10,
            177 => 21,
            178 => 11,
            230 => 27,
            251 => 34,
            252 => 16,
            231 => 46,
            232 => 48,
            233 => 24,
            235 => 36,
            236 => 34,
            238 => 10,
            239 => 21,
            240 => 29,
            242 => 10,
            249 => 29,
            245 => 41,
            248 => 21,
            247 => 35,
            246 => 19,
            250 => 49,
            _ => block,
        };
        public static ushort ToRaw(ushort raw) => raw < 66 ? raw : (ushort)(raw - 256);
        public static ushort FromRaw(ushort raw) => raw < 66 ? raw : (ushort)(raw + 256);
        public static bool HasExtraPerm(NASPlayer np, string cmd, int num) => CommandExtraPerms.Find(cmd, num).UsableBy(np.p.Rank);
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
            NASLevel nl = NASLevel.Get(lvl.name);
            string jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented),
                fileName = NASLevel.GetFileName(nl.lvl.name);
            bool saved = lvl.Save(true) && FileIO.TryWriteAllText(fileName, jsonString);            
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
            if (!NASGen.GetSeedAndChunkOffset(Server.mainLevel.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ))
            {
                Log("NAS: {0} is not a NAS level, generating a NAS level to replace it!", Server.mainLevel.name);
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
                Chat.Message(ChatScope.All, "A server restart is required to initialize NAS plugin.", null, null, true);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                Server.Stop(true, "A server restart is required to initialize NAS plugin.");
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
        public static bool IsDev(Player p) => Devs.CaselessContains(p.truename);
        public static bool IsDev(PlayerData data) => Devs.CaselessContains(data.Name);
        public static void DisposeErrorResponse(Exception ex)
        {
            try
            {
                if (ex is WebException webEx && webEx.Response != null)
                {
                    webEx.Response.Close();
                }
            }
            catch 
            { 
            }
        }
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
                DisposeErrorResponse(ex);
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
        public static string GetErrorResponse(Exception ex)
        {
            try
            {
                if (ex is WebException webEx && webEx.Response != null)
                {
                    return new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd().Trim();
                }
            }
            catch 
            { 
            }
            return null;
        }
        public static bool HandleErrorResponse(WebException ex, string msg, long retry)
        {
            string err = GetErrorResponse(ex);
            HttpStatusCode status = GetStatus(ex);
            if (status == (HttpStatusCode)429)
            {
                Sleep();
                return true;
            }
            if (status >= HttpStatusCode.InternalServerError && status <= HttpStatusCode.GatewayTimeout)
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
        public static HttpStatusCode GetStatus(WebException ex) => ex.Response == null ? 0 : ((HttpWebResponse)ex.Response).StatusCode;
        public static void LogWarning(Exception ex, string target) => Logger.Log(LogType.Warning, "Error sending request to Github API {0}: {1}", target, ex.Message);
        public static void LogWarning(string message) => Logger.Log(LogType.Warning, message);
        public static void LogWarning(Exception ex) => Logger.Log(LogType.Warning, "Error sending request to Github API - {0}", ex.Message);
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
        public static void Sleep() => Thread.Sleep(TimeSpan.FromSeconds(30 + 0.5f));
    }
    public partial class NASPlayer
    {
        public static bool GetCoords(Player p, string[] args, int argsOffset, ref Vec3S32 P) => GetCoordInt(p, args[argsOffset + 0], "X coordinate", ref P.X) &&
                GetCoordInt(p, args[argsOffset + 1], "Y coordinate", ref P.Y) &&
                GetCoordInt(p, args[argsOffset + 2], "Z coordinate", ref P.Z);
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
        public static bool TryParseInt32(string s, out int result) => int.TryParse(s, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result);
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
    public delegate void NASAction<T1>(T1 arg1);
    public delegate void NASAction<T1, T2>(T1 arg1, T2 arg2);
    public delegate void NASAction<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void NASAction<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate void NASAction<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void NASAction<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void NASAction<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult NASFunc<T1, T2, out TResult>(T1 arg1, T2 arg2);
}
