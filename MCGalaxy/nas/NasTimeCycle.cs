#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class NasTimeCycle
    {
        // Vars
        public static float globalCurrentTime;
        public static DayCycles globalCurrentDayCycle;
        public static int gameday = 0;
        public static string TimeFilePath = Nas.CoreSavePath + "time.json";
        public static JsonSerializer serializer = new JsonSerializer();
        public static Scheduler weatherScheduler;
        public static SchedulerTask task;
        public static string globalSkyColor, globalCloudColor, 
            globalSunColor, globalShadowColor; // self explanatory
        // Cycle Settings
        public static DayCycles dayCycle = DayCycles.Sunrise; // default cycle
        public static int cycleCurrentTime = 0, /* current cycle time (must be zero to start)*/
            cycleMaxTime = 14400, /* duration a whole day*/ 
            hourMinutes = 600; //seconds in an hour
        public enum DayCycles // Enum with day and night cycles
        {
            Sunrise, Day, Sunset, Night, Midnight
        }
        public static void Setup()
        {
            if (weatherScheduler == null)
            {
                weatherScheduler = new Scheduler("WeatherScheduler");
            }
            task = weatherScheduler.QueueRepeat(Update, null, new TimeSpan(0, 0, 7));
            dayCycle = DayCycles.Sunrise; // start with sunrise state
            // Static variables to keep time after switching scenes
            if (!File.Exists(TimeFilePath))
            {
                File.Create(TimeFilePath).Dispose();
                Log("Created new json time file {0}!", TimeFilePath);
                using (StreamWriter sw = new StreamWriter(TimeFilePath))
                { // To help you better understand, this is the stream writer
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    { // this is the json writer that will help me to serialize and deserialize items in the file
                        serializer.Serialize(writer, cyc);
                    }
                }
            }
            string jsonString = File.ReadAllText(TimeFilePath);
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
            Server.MainScheduler.Cancel(task);
        }
        public static void Update(SchedulerTask task) // this gets executed each time a second has passed.
        {
            // Update cycle time
            cycleCurrentTime += 6 * 7;
            // Static variables to keep time after switching scenes
            globalCurrentTime = cycleCurrentTime;
            globalCurrentDayCycle = dayCycle;
            // Check if cycle time reach cycle duration time
            if (cycleCurrentTime >= cycleMaxTime)
            {
                cycleCurrentTime = 0; // back to 0 (restarting cycle time)
                gameday += 1; // one more in-game day just passed
                dayCycle++; // change cycle state
            }
            //when to change cycles
            if (cycleCurrentTime >= 7 * hourMinutes & cycleCurrentTime < 8 * hourMinutes) 
            { 
                dayCycle = DayCycles.Sunrise; 
            } // 7am
            if (cycleCurrentTime >= 8 * hourMinutes & cycleCurrentTime < 19 * hourMinutes) 
            { 
                dayCycle = DayCycles.Day; 
            } // 8am
            if (cycleCurrentTime >= 19 * hourMinutes & cycleCurrentTime < 20 * hourMinutes) 
            { 
                dayCycle = DayCycles.Sunset; 
            } // 6pm
            if (cycleCurrentTime >= 20 * hourMinutes & cycleCurrentTime < 24 * hourMinutes) 
            { 
                dayCycle = DayCycles.Night; 
            } // 8pm
            if (cycleCurrentTime == 24 * hourMinutes | cycleCurrentTime == 0 | cycleCurrentTime < 7 * hourMinutes) 
            { 
                dayCycle = DayCycles.Midnight; 
            } // 0 am
            switch (dayCycle)
            {
                // Sunrise state (you can do a lot of stuff based on every cycle state, like enable monster spawning only when dark)
                case DayCycles.Sunrise:
                    globalCloudColor = "#ff8c00"; // Dark Orange
                    globalSkyColor = "#FFA500"; // Orange
                    globalSunColor = "#a9a9a9"; // Dark Gray
                    globalShadowColor = "#828282";
                    break;
                // Mid Day state
                case DayCycles.Day:
                    globalCloudColor = "#ffffff"; // White
                    globalSkyColor = "#ADD8E6"; // Light Blue
                    globalSunColor = "#ffffff"; // White
                    globalShadowColor = "#9B9B9B";
                    break;
                // Sunset state
                case DayCycles.Sunset:
                    globalCloudColor = "#cf5c00"; // Dark Orange
                    globalSkyColor = "#FFB500"; // Orange
                    globalSunColor = "#a9a9a9"; // Dark Gray
                    globalShadowColor = "#828282";
                    break;
                // Night state
                case DayCycles.Night:
                    globalCloudColor = "#808080"; // Gray
                    globalSkyColor = "#404040"; // Darker Gray
                    globalSunColor = "#808080"; // Gray
                    globalShadowColor = "#595959";
                    break;
                // Midnight state
                case DayCycles.Midnight:
                    globalCloudColor = "#404040"; // Darker Gray
                    globalSkyColor = "#000000"; // Black
                    globalSunColor = "#404040"; // Darker Gray
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
                        lvl.Config.LightColor = sun; // Sun Colour
                    }
                    if (lvl.Config.CloudColor != cloud)
                    {
                        changed = true;
                        lvl.Config.CloudColor = cloud; // Cloud Colour
                    }
                    if (lvl.Config.SkyColor != sky)
                    {
                        changed = true;
                        lvl.Config.SkyColor = sky; // Sky
                    }
                    if (lvl.Config.ShadowColor != shadow)
                    {
                        changed = true;
                        lvl.Config.ShadowColor = shadow; // Shadow
                    }
                    if (changed)
                    {
                        lvl.SaveSettings(); // We save these settings after
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
                        //p.SendCurrentEnv();
                    }
                }
            }
        }
    }
}
#endif