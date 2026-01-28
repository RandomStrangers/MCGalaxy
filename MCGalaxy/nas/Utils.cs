#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.DB;
using MCGalaxy.Generator;
using MCGalaxy.Maths;
using MCGalaxy.Platform;
using MCGalaxy.SQL;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
namespace NotAwesomeSurvival
{
    public static class FileUtils
    {
        public static string TryReadAllText(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        public static string[] TryReadAllLines(string path)
        {
            try
            {
                return File.ReadAllLines(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        public static bool TryWriteAllText(string path, string contents)
        {
            try
            {
                File.WriteAllText(path, contents);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
        public static bool TryAppendAllText(string path, string contents)
        {
            try
            {
                File.AppendAllText(path, contents);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
        public static bool TryWriteAllLines(string path, string[] contents)
        {
            try
            {
                File.WriteAllLines(path, contents);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
    public static class OS
    {
        public static unsafe string Get()
        {
            string bitType, name;
            if (IntPtr.Size == 8)
            {
                bitType = " 64-bit";
            }
            else if (IntPtr.Size == 4)
            {
                bitType = " 32-bit";
            }
            else if (IntPtr.Size == 2)
            {
                bitType = " 16-bit";
            }
            else
            {
                bitType = " unknown bit type (IntPtr size is " + IntPtr.Size + ")";
            }
            if (Server.RunningOnMono())
            {
                name = "Mono";
            }
            else
            {
                PlatformID platform = Environment.OSVersion.Platform;
                if (platform == PlatformID.Win32S 
                    || platform == PlatformID.Win32Windows
                    || platform == PlatformID.Win32NT
                    || platform == PlatformID.WinCE
                    || platform == PlatformID.Xbox) 
                {
                    name = "Windows";
                }
                else if (platform == PlatformID.MacOSX)
                {
                    name = "Mac";
                }
                else
                {
                    sbyte* utsname = stackalloc sbyte[8192];
                    uname(utsname);
                    string kernel = new(utsname);
                    if (kernel.CaselessContains("linux"))
                    {
                        name = "Linux";
                    }
                    else if (kernel.CaselessContains("freeBSD"))
                    {
                        name = "FreeBSD";
                    }
                    else if (kernel.CaselessContains("netBSD"))
                    {
                        name = "NetBSD";
                    }
                    else if (kernel.CaselessContains("darwin"))
                    {
                        name = "Mac";
                    }
                    else
                    {
                        name = "Unix";
                    }
                }
            }
            return name + bitType;
        }
        [DllImport("libc")]
        static extern unsafe void uname(sbyte* uname_struct);
    }
    public class MonoOS : IOperatingSystem
    {
        public override bool IsWindows { get { return false; } }
        public override void RestartProcess()
        {
            try
            {
                execvp(Server.GetRuntimeExePath(), GetProcessCommandLineArgs());
            }
            catch (Exception ex)
            {
                Logger.LogError("Restarting process", ex);
            }
            execvp("mono", new string[] { "mono", Server.GetServerExePath(), null });
            Console.Out.WriteLine("execvp mono failed: {0}", Marshal.GetLastWin32Error());
        }
        static CPUTime ParseCpuLine(string line)
        {
            line = line.Replace("  ", " ");
            string[] bits = line.SplitSpaces();
            ulong user = ulong.Parse(bits[1]),
                nice = ulong.Parse(bits[2]),
                kern = ulong.Parse(bits[3]),
                idle = ulong.Parse(bits[4]);
            CPUTime all = new()
            {
                UserTime = user + nice,
                KernelTime = kern,
                IdleTime = idle
            };
            return all;
        }
        public override CPUTime MeasureAllCPUTime()
        {
            try
            {
                using StreamReader r = new("/proc/stat");
                string line = r.ReadLine();
                if (line.StartsWith("cpu "))
                {
                    return ParseCpuLine(line);
                }
                return new();
            }
            catch
            {
                return new();
            }
        }
        static string[] GetProcessCommandLineArgs()
        {
            using StreamReader r = new("/proc/self/cmdline");
            string[] args = r.ReadToEnd().Split('\0');
            args[args.Length - 1] = null;
            return args;
        }
        [DllImport("libc", SetLastError = true)]
        static extern int execvp(string path, string[] argv);
    }
    public class NasConsolePlayer : Player
    {
        public NasConsolePlayer() : base("(NAS)")
        {
            group = new()
            {
                Permission = LevelPermission.Console,
                DrawLimit = int.MaxValue,
                MaxUndo = TimeSpan.MaxValue,
                Name = "NAS",
                Color = "&S",
                GenVolume = int.MaxValue,
                OverseerMaps = int.MaxValue,
            };
            color = "&S";
            SuperName = "NAS";
        }
        public override string FullName
        {
            get { return "NAS [" + Server.Config.ConsoleName + "&S]"; }
        }
        public override void Message(string message)
        {
            Logger.Log(LogType.Debug, message);
        }
    }
    public partial class Nas
    {
        public static Player NasConsole = new NasConsolePlayer();
        public static ushort Convert(ushort block)
        {
            return block switch
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
                140 => 8,
                141 => 10,
                143 => 27,
                144 => 22,
                145 => 8,
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
                193 => 8,
                194 => 10,
                73 => 10,
                195 => 10,
                196 => 8,
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
                237 => 8,
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
        }
        public static ushort ToRaw(ushort raw)
        {
            return raw < 66 ? raw : (ushort)(raw - 256);
        }
        public static ushort FromRaw(ushort raw)
        {
            return raw < 66 ? raw : (ushort)(raw + 256);
        }
        public static bool IsPhysicsType(ushort block)
        {
            return block >= 66 && block < 256;
        }
        public class CmdNewServerInfo : Command
        {
            public override string name { get { return "NewServerInfo"; } }
            public override string shortcut { get { return "SInfo"; } }
            public override string type { get { return CommandTypes.Information; } }
            public override bool UseableWhenFrozen { get { return true; } }
            public override CommandAlias[] Aliases
            {
                get { return new[] { new CommandAlias("Host"), new("ZAll"), new("ServerInfo") }; }
            }
            public override CommandPerm[] ExtraPerms
            {
                get { return new[] { new CommandPerm(LevelPermission.Admin, "can see server host, operating system, CPU and memory usage") }; }
            }
            public override void Use(Player p, string message)
            {
                p.Message("About &b{0}&S", Server.Config.Name);
                p.Message("  &a{0} &Splayers total. (&a{1} &Sonline, &8{2} banned&S)",
                          Database.CountRows("Players"), PlayerInfo.GetOnlineCanSee(p, p.Rank).Count, Group.BannedRank.Players.Count);
                p.Message("  &a{0} &Slevels total (&a{1} &Sloaded). Currency is &3{2}&S.",
                          LevelInfo.AllMapFiles().Length, LevelInfo.Loaded.Count, Server.Config.Currency);
                TimeSpan up = DateTime.UtcNow - Server.StartTime;
                p.Message("  Been up for &a{0}&S, running &b{1} &a{2}",
                          up.Shorten(true), Server.SoftwareName, NasVersion);
                p.Message("&f" + NASUpdater.SourceURL);
                int updateInterval = 1000 / Server.Config.PositionUpdateInterval;
                p.Message("  Player positions are updated &a{0} &Stimes/second", updateInterval);
                string owner = Server.Config.OwnerName;
                if (!owner.CaselessEq("Notch") && !owner.CaselessEq("the owner"))
                {
                    p.Message("  Owner is &3{0}", owner);
                }
                if (HasExtraPerm(p, p.Rank, 1))
                {
                    OutputResourceUsage(p);
                }
            }
            static DateTime startTime;
            static ProcInfo startUsg;
            static void OutputResourceUsage(Player p)
            {
                p.Message("Host: {0}", Environment.MachineName);
                p.Message("OS: {0}", OS.Get());
                Process proc = Process.GetCurrentProcess();
                p.Message("Measuring resource usage...one second");
                IOperatingSystem os;
                if (Server.RunningOnMono())
                {
                    os = new MonoOS();
                }
                else
                {
                    os = IOperatingSystem.DetectOS();
                }
                if (startTime == default)
                {
                    startTime = DateTime.UtcNow;
                    startUsg = os.MeasureResourceUsage(proc, false);
                }
                CPUTime allBeg = os.MeasureAllCPUTime();
                ProcInfo begUsg = os.MeasureResourceUsage(proc, false);
                Thread.Sleep(1000);
                ProcInfo endUsg = os.MeasureResourceUsage(proc, true);
                CPUTime allEnd = os.MeasureAllCPUTime();
                p.Message("&a{0}% &SCPU usage now, &a{1}% &Soverall",
                    MeasureCPU(begUsg.ProcessorTime, endUsg.ProcessorTime, TimeSpan.FromSeconds(1)),
                    MeasureCPU(startUsg.ProcessorTime, endUsg.ProcessorTime, DateTime.UtcNow - startTime));
                ulong idl = allEnd.IdleTime - allBeg.IdleTime,
                    sys = allEnd.ProcessorTime - allBeg.ProcessorTime;
                double cpu = sys * 100.0 / (sys + idl);
                int cores = Environment.ProcessorCount;
                p.Message("  &a{0}% &Sby all processes across {1} CPU core{2}",
                    double.IsNaN(cpu) ? "(unknown)" : cpu.ToString("F2"),
                    cores, cores.Plural());
                int memory = (int)Math.Round(endUsg.PrivateMemorySize / 1048576.0);
                p.Message("&a{0} &Sthreads, using &a{1} &Smegabytes of memory",
                    endUsg.NumThreads, memory);
            }
            static string MeasureCPU(TimeSpan beg, TimeSpan end, TimeSpan interval)
            {
                if (end < beg)
                {
                    return "0.00";
                }
                int cores = Math.Max(1, Environment.ProcessorCount);
                TimeSpan used = end - beg;
                double elapsed = 100.0 * (used.TotalSeconds / interval.TotalSeconds);
                return (elapsed / cores).ToString("F2");
            }
            public override void Help(Player p)
            {
                p.Message("&T/ServerInfo");
                p.Message("&HDisplays the server information.");
            }
        }
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
            NasLevel nl = NasLevel.Get(lvl.name);
            string jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented),
                fileName = NasLevel.GetFileName(nl.lvl.name);
            bool saved = lvl.Save(true) && FileUtils.TryWriteAllText(fileName, jsonString);            
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
                NasLevel.GenerateMap(NasConsole,
                                           mapName,
                                           NasGen.mapWideness.ToString(),
                                           NasGen.mapTallness.ToString(),
                                           NasGen.mapWideness.ToString(),
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
        public static void EnsureDirectoriesExists(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
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
            return false;
        }
        public static bool IsDev(PlayerData data)
        {
            if (Devs.CaselessContains(data.Name))
            {
                return true;
            }
            return false;
        }
        public static void DisposeErrorResponse(Exception ex)
        {
            try
            {
                if (ex is WebException webEx && webEx.Response != null) webEx.Response.Close();
            }
            catch { }
        }
        public class CustomWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
                req.ServicePoint.BindIPEndPointDelegate = BindIPEndPointCallback;
                req.UserAgent = Server.SoftwareNameVersioned;
                return req;
            }
            static IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEP, int retryCount)
            {
                IPAddress localIP;
                if (Server.Listener.IP != null)
                {
                    localIP = Server.Listener.IP;
                }
                else if (!IPAddress.TryParse(Server.Config.ListenIP, out localIP))
                {
                    return null;
                }
                if (remoteEP.AddressFamily != localIP.AddressFamily)
                {
                    return null;
                }
                return new(localIP, 0);
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
                using (WebClient client = new CustomWebClient())
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
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public static string GetResponseText(WebResponse response)
        {
            using StreamReader r = new(response.GetResponseStream());
            return r.ReadToEnd().Trim();
        }
        public static string GetErrorResponse(Exception ex)
        {
            try
            {
                if (ex is WebException webEx && webEx.Response != null)
                {
                    return GetResponseText(webEx.Response);
                }
            }
            catch 
            { 
            }
            return null;
        }
        public static bool HandleErrorResponse(Exception ex, string msg, long retry)
        {
            return HandleErrorResponse((WebException)ex, msg, retry);
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
    public static class Extensions
    {
        public static DateTime Floor(this DateTime date, TimeSpan span)
        {
            return new((date.Ticks / span.Ticks) * span.Ticks);
        }
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