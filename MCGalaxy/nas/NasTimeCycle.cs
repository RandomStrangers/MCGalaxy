#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class NasTimeCycle
    {
        public static float globalCurrentTime;
        public static DayCycles globalCurrentDayCycle;
        public static JsonSerializer serializer = new();
        public static Scheduler weatherScheduler;
        public static SchedulerTask task;
        public static string globalSkyColor, globalCloudColor,
            globalSunColor, globalShadowColor, 
            TimeFilePath = Nas.CoreSavePath + "time.json";
        public static DayCycles dayCycle = DayCycles.Sunrise;
        public static int cycleCurrentTime = 0,
            cycleMaxTime = 14400,
            hourMinutes = 600,
            gameday = 0;
        public enum DayCycles
        {
            Sunrise, Day, Sunset, Night, Midnight
        }
        public static void Setup()
        {
            weatherScheduler ??= new("WeatherScheduler");
            task = weatherScheduler.QueueRepeat(Update, null, new(0, 0, 7));
            dayCycle = DayCycles.Sunrise;
            if (!File.Exists(TimeFilePath))
            {
                File.Create(TimeFilePath).Dispose();
                Log("Created new json time file {0}!", TimeFilePath);
                using StreamWriter sw = new(TimeFilePath);
                using JsonWriter writer = new JsonTextWriter(sw);
                serializer.Serialize(writer, cyc);
            }
            string jsonString = FileUtils.TryReadAllText(TimeFilePath);
            NasTimeCycle ntc = JsonConvert.DeserializeObject<NasTimeCycle>(jsonString);
            dayCycle = ntc.cycle;
            gameday = ntc.day;
            cycleCurrentTime = ntc.minutes;
            cyc.cycle = ntc.cycle;
            cyc.day = ntc.day;
            cyc.minutes = ntc.minutes;
        }
        public static void TakeDown()
        {
            weatherScheduler.Cancel(task);
        }
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
                dayCycle = DayCycles.Sunrise;
            }
            if (cycleCurrentTime >= 8 * hourMinutes & cycleCurrentTime < 19 * hourMinutes)
            {
                dayCycle = DayCycles.Day;
            }
            if (cycleCurrentTime >= 19 * hourMinutes & cycleCurrentTime < 20 * hourMinutes)
            {
                dayCycle = DayCycles.Sunset;
            }
            if (cycleCurrentTime >= 20 * hourMinutes & cycleCurrentTime < 24 * hourMinutes)
            {
                dayCycle = DayCycles.Night;
            }
            if (cycleCurrentTime == 24 * hourMinutes | cycleCurrentTime == 0 | cycleCurrentTime < 7 * hourMinutes)
            {
                dayCycle = DayCycles.Midnight;
            }
            switch (dayCycle)
            {
                case DayCycles.Sunrise:
                    globalCloudColor = "#ff8c00";
                    globalSkyColor = "#FFA500";
                    globalSunColor = "#a9a9a9";
                    globalShadowColor = "#828282";
                    break;
                case DayCycles.Day:
                    globalCloudColor = "#ffffff";
                    globalSkyColor = "#ADD8E6";
                    globalSunColor = "#ffffff";
                    globalShadowColor = "#9B9B9B";
                    break;
                case DayCycles.Sunset:
                    globalCloudColor = "#cf5c00";
                    globalSkyColor = "#FFB500";
                    globalSunColor = "#a9a9a9";
                    globalShadowColor = "#828282";
                    break;
                case DayCycles.Night:
                    globalCloudColor = "#808080";
                    globalSkyColor = "#404040";
                    globalSunColor = "#808080";
                    globalShadowColor = "#595959";
                    break;
                case DayCycles.Midnight:
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
                if (NasLevel.Get(lvl.name).biome >= 0)
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
            foreach (Player p in PlayerInfo.Online.Items)
            {
                if (NasLevel.Get(p.level.name).biome >= 0)
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
#endif