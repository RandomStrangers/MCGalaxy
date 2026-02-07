using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System.IO;
namespace MCGalaxy
{
    public partial class NASTimeCycle
    {
        public const string Path = NASPlugin.Path + "CoreData/";
        public static float globalCurrentTime;
        public static JsonSerializer serializer = new();
        public static Scheduler weatherScheduler;
        public static SchedulerTask task;
        public static string globalSkyColor, globalCloudColor,
            globalSunColor, globalShadowColor, 
            TimeFilePath = Path + "time.json";
        public static int globalCurrentDayCycle,
            dayCycle = 0,
            cycleCurrentTime = 0,
            cycleMaxTime = 14400,
            hourMinutes = 600,
            gameday = 0;
        public int cycle = 0, day = 0, minutes = 7 * hourMinutes;
        public static NASTimeCycle cyc = new();
        public static void StoreTimeData(int day, int minutes, int cycle)
        {
            cyc.day = day;
            cyc.minutes = minutes;
            cyc.cycle = cycle;
            if (!File.Exists(TimeFilePath))
            {
                File.Create(TimeFilePath).Dispose();
                Logger.Log(15, "Created new json time file {0}!", TimeFilePath);
            }
            using StreamWriter sw = new(TimeFilePath);
            using JsonTextWriter writer = new(sw);
            serializer.Serialize(writer, cyc);
        }
        public static void Setup()
        {
            weatherScheduler ??= new("WeatherScheduler");
            task = weatherScheduler.QueueRepeat(Update, null, new(0, 0, 7));
            dayCycle = 0;
            if (!File.Exists(TimeFilePath))
            {
                File.Create(TimeFilePath).Dispose();
                Logger.Log(15, "Created new json time file {0}!", TimeFilePath);
                using StreamWriter sw = new(TimeFilePath);
                using JsonTextWriter writer = new(sw);
                serializer.Serialize(writer, cyc);
            }
            NASTimeCycle ntc = JsonConvert.DeserializeObject<NASTimeCycle>(FileIO.TryReadAllText(TimeFilePath));
            dayCycle = ntc.cycle;
            gameday = ntc.day;
            cycleCurrentTime = ntc.minutes;
            cyc.cycle = ntc.cycle;
            cyc.day = ntc.day;
            cyc.minutes = ntc.minutes;
        }
        public static void TakeDown() => weatherScheduler.Cancel(task);
        public static void Update(SchedulerTask task)
        {
            cycleCurrentTime += 6 * 7;
            globalCurrentTime = cycleCurrentTime;
            globalCurrentDayCycle = dayCycle;
            if (cycleCurrentTime >= cycleMaxTime)
            {
                cycleCurrentTime = 0;
                gameday += 1;
                dayCycle++;
            }
            if (cycleCurrentTime >= 7 * hourMinutes & cycleCurrentTime < 8 * hourMinutes)
            {
                dayCycle = 0;
            }
            if (cycleCurrentTime >= 8 * hourMinutes & cycleCurrentTime < 19 * hourMinutes)
            {
                dayCycle = 1;
            }
            if (cycleCurrentTime >= 19 * hourMinutes & cycleCurrentTime < 20 * hourMinutes)
            {
                dayCycle = 2;
            }
            if (cycleCurrentTime >= 20 * hourMinutes & cycleCurrentTime < 24 * hourMinutes)
            {
                dayCycle = 3;
            }
            if (cycleCurrentTime == 24 * hourMinutes | cycleCurrentTime == 0 | cycleCurrentTime < 7 * hourMinutes)
            {
                dayCycle = 4;
            }
            switch (dayCycle)
            {
                case 0:
                    globalCloudColor = "#ff8c00";
                    globalSkyColor = "#FFA500";
                    globalSunColor = "#a9a9a9";
                    globalShadowColor = "#828282";
                    break;
                case 1:
                    globalCloudColor = "#ffffff";
                    globalSkyColor = "#ADD8E6";
                    globalSunColor = "#ffffff";
                    globalShadowColor = "#9B9B9B";
                    break;
                case 2:
                    globalCloudColor = "#cf5c00";
                    globalSkyColor = "#FFB500";
                    globalSunColor = "#a9a9a9";
                    globalShadowColor = "#828282";
                    break;
                case 3:
                    globalCloudColor = "#808080";
                    globalSkyColor = "#404040";
                    globalSunColor = "#808080";
                    globalShadowColor = "#595959";
                    break;
                case 4:
                    globalCloudColor = "#404040";
                    globalSkyColor = "#000000";
                    globalSunColor = "#404040";
                    globalShadowColor = "#494949";
                    break;
            }
            UpdateEnvSettings(globalCloudColor, globalSkyColor, globalSunColor, globalShadowColor);
            StoreTimeData(gameday, cycleCurrentTime, dayCycle);
        }
        public static void UpdateEnvSettings(string cloud, string sky, string sun, string shadow)
        {
            bool changed = false;
            foreach (Level lvl in LevelInfo.Loaded.Items)
            {
                NASLevel nl = NASLevel.Get(lvl);
                if (nl != null)
                {
                    if (nl.biome >= 0)
                    {
                        if (lvl.Config.LightColor != sun)
                        {
                            changed = true;
                            lvl.Config.LightColor = sun;
                        }
                        if (lvl.Config.CloudColor != cloud)
                        {
                            changed = true;
                            lvl.Config.CloudColor = cloud;
                        }
                        if (lvl.Config.SkyColor != sky)
                        {
                            changed = true;
                            lvl.Config.SkyColor = sky;
                        }
                        if (lvl.Config.ShadowColor != shadow)
                        {
                            changed = true;
                            lvl.Config.ShadowColor = shadow;
                        }
                        if (changed)
                        {
                            lvl.SaveSettings();
                        }
                    }
                }
            }
            foreach (Player p in PlayerInfo.Online.Items)
            {
                NASLevel nl = NASLevel.Get(p.Level);
                if (nl != null)
                {
                    if (nl.biome >= 0)
                    {
                        if (changed)
                        {
                            if (p.Supports(CpeExt.EnvColors))
                            {
                                if (Colors.TryParseHex(sky, out ColorDesc c))
                                {
                                    p.Send(Packet.EnvColor(0, c.R, c.G, c.B));
                                }
                                if (Colors.TryParseHex(cloud, out c))
                                {
                                    p.Send(Packet.EnvColor(1, c.R, c.G, c.B));
                                }
                                if (Colors.TryParseHex(shadow, out c))
                                {
                                    p.Send(Packet.EnvColor(3, c.R, c.G, c.B));
                                }
                                if (Colors.TryParseHex(sun, out c))
                                {
                                    p.Send(Packet.EnvColor(4, c.R, c.G, c.B));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
