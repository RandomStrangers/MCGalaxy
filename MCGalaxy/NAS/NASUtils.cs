using MCGalaxy.Commands;
using MCGalaxy.Network;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using NASBlockAction = MCGalaxy.NASAction<MCGalaxy.NASLevel, MCGalaxy.NASBlock, int, int, int>;
namespace MCGalaxy
{
    public enum NASDayCycles
    {
        Sunrise, Day, Sunset, Night, Midnight
    }
    public enum NASContainerType
    {
        Chest, Barrel, Crate,
        Gravestone, AutoCraft, Dispenser
    }
    public enum NASMaterial
    {
        None,
        Gas,
        Stone,
        Earth,
        Wood,
        Plant,
        Leaves,
        Organic,
        Glass,
        Metal,
        Liquid,
        Lava,
        Count
    }
    public class NASWayPoint
    {
        public Position Pos;
        public byte Yaw, Pitch;
        public string Name, Level;
    }
    public class NASDisplayInfo
    {
        public NASInventory inv;
        public NASBlock nasBlock;
        public int amountChanged;
        public bool showToNormalChat;
    }
    public class NASBlockStack
    {
        public int amount;
        public ushort ID;
        public NASBlockStack(ushort ID, int amount = 1)
        {
            this.ID = ID;
            this.amount = amount;
        }
    }
    public class NASBlockLocation
    {
        public int X, Y, Z;
        public NASBlockLocation() { }
        public NASBlockLocation(NASQueuedBlockUpdate qb)
        {
            X = qb.x;
            Y = qb.y;
            Z = qb.z;
        }
        public NASBlockLocation(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
    public class NASMobMetadata
    {
        public int waitTime, walkTime, lookTime, search;
        public Player chasing;
    }
    public class NASQueueException : InvalidOperationException
    {
        public NASQueueException(string message) : base(message)
        {
        }
    }
    public struct NASCoords
    {
        public int X, Y, Z;
        public byte RotX, RotY;
    }
    public struct NASQueuedBlockUpdate
    {
        public int x, y, z;
        public NASBlock nb;
        public DateTime date;
        public NASBlockAction da;
    }
    public partial class NAS
    {
        public static bool HasExtraPerm(NASPlayer np, string cmd, int num) => CommandExtraPerms.Find(cmd, num).UsableBy(np.p.Rank);
        public static void SaveAll(Player p)
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                if (!lvl.SaveChanges)
                    continue;
                NASLevel nl = NASLevel.Get(lvl.name);
                if (!lvl.Save(true) && FileIO.TryWriteAllText(NASLevel.GetFileName(nl.lvl.name), JsonConvert.SerializeObject(nl, Formatting.Indented)))
                    p.Message("Saving of level {0} &Swas cancelled", lvl.ColoredName);
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
                NASLevel.GenerateMap(Player.NASConsole,
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
                Server.EnsureDirectoryExists(path);
        }
        public static string GetSavePath(Player p) => NASPlayer.Path + p.name + ".json";
        public static string GetDeathPath(string name) => NASPlayer.DeathsPath + name + ".txt";
        public static bool EnsureFileExists(string url, string file)
        {
            if (File.Exists(file))
                return true;
            try
            {
                using (WebClient client = new())
                    client.DownloadFile(url, file);
                if (File.Exists(file))
                    return true;
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
                    EnsureFileExists(url, file);
            }
            return false;
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
                        err = err.Substring(0, 200) + "...";
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
                    err = err.Substring(0, 200) + "...";
                Logger.Log(LogType.Warning, "Github API returned: " + err);
            }
            return false;
        }
        public static void Sleep() => Thread.Sleep(TimeSpan.FromSeconds(30 + 0.5f));
    }
    public delegate void NASAction<NAS1>(NAS1 arg1);
    public delegate void NASAction<NAS1, NAS2>(NAS1 arg1, NAS2 arg2);
    public delegate void NASAction<NAS1, NAS2, NAS3>(NAS1 arg1, NAS2 arg2, NAS3 arg3);
    public delegate void NASAction<NAS1, NAS2, NAS3, NAS4>(NAS1 arg1, NAS2 arg2, NAS3 arg3, NAS4 arg4);
    public delegate void NASAction<NAS1, NAS2, NAS3, NAS4, NAS5>(NAS1 arg1, NAS2 arg2, NAS3 arg3, NAS4 arg4, NAS5 arg5);
    public delegate void NASAction<NAS1, NAS2, NAS3, NAS4, NAS5, NAS6>(NAS1 arg1, NAS2 arg2, NAS3 arg3, NAS4 arg4, NAS5 arg5, NAS6 arg6);
    public delegate void NASAction<NAS1, NAS2, NAS3, NAS4, NAS5, NAS6, NAS7>(NAS1 arg1, NAS2 arg2, NAS3 arg3, NAS4 arg4, NAS5 arg5, NAS6 arg6, NAS7 arg7);
    public delegate NASResult NASFunc<NAS1, NAS2, out NASResult>(NAS1 arg1, NAS2 arg2);
}