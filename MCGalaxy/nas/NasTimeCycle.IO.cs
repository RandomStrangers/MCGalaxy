#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using Newtonsoft.Json;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class NasTimeCycle
    {
        public static NasTimeCycle cyc = new();
        public int day = 0, minutes = 7 * hourMinutes;
        public DayCycles cycle = DayCycles.Sunrise;
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public static void StoreTimeData(int day, int minutes, DayCycles cycle)
        {
            cyc.day = day;
            cyc.minutes = minutes;
            cyc.cycle = cycle;
            if (!File.Exists(TimeFilePath))
            {
                File.Create(TimeFilePath).Dispose();
                Log("Created new json time file {0}!", TimeFilePath);
            }
            using StreamWriter sw = new(TimeFilePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, cyc);
        }
    }
}
#endif