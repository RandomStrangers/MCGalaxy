#if NAS && TEN_BIT_BLOCKS
using System.IO;
using System;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.LevelEvents;
namespace NotAwesomeSurvival
{
    public partial class NasLevel
    {
        public const string Path = Nas.Path + "leveldata/";
        public const string Extension = ".json";
        public static void Setup()
        {
            OnLevelLoadedEvent.Register(OnLevelLoaded, Priority.High);
            OnLevelUnloadEvent.Register(OnLevelUnload, Priority.Low);
            OnLevelDeletedEvent.Register(OnLevelDeleted, Priority.Low);
            OnLevelRenamedEvent.Register(OnLevelRenamed, Priority.Low);
        }
        public static void TakeDown()
        {
            Level[] loadedLevels = LevelInfo.Loaded.Items;
            foreach (Level lvl in loadedLevels)
            {
                if (!all.ContainsKey(lvl.name))
                {
                    return;
                }
                Unload(lvl.name, all[lvl.name]);
            }
            OnLevelLoadedEvent.Unregister(OnLevelLoaded);
            OnLevelUnloadEvent.Unregister(OnLevelUnload);
            OnLevelDeletedEvent.Unregister(OnLevelDeleted);
            OnLevelRenamedEvent.Unregister(OnLevelRenamed);
        }
        public static string GetFileName(string name)
        {
            return Path + name + Extension;
        }
        public static NasLevel ForceGet(string name)
        {
            NasLevel nl = new NasLevel();
            string fileName = GetFileName(name);
            if (File.Exists(fileName))
            {
                string jsonString = File.ReadAllText(fileName);
                nl = JsonConvert.DeserializeObject<NasLevel>(jsonString);
                return nl;
            }
            return nl;//Never return null, results in error
        }
        public static NasLevel Get(string name)
        {
            if (all.ContainsKey(name))
            {
                return all[name];
            }
            return ForceGet(name); //Failsafe.
        }
        public static void Unload(string name, NasLevel nl)
        {
            nl.EndTickTask();
            nl.lvl.Save(true);
            string jsonString;
            jsonString = JsonConvert.SerializeObject(nl, Formatting.Indented);
            string fileName = GetFileName(name);
            File.WriteAllText(fileName, jsonString);
            Logger.Log(LogType.Debug, "Unloaded(saved) NasLevel " + fileName + "!");
            all.Remove(name);
        }
        public static int MakeInt(string seed)
        {
            if (seed.Length == 0)
            {
                return new Random().Next();
            }
            if (!int.TryParse(seed, out int value))
            {
                value = seed.GetHashCode();
            }
            return value;
        }
        public static void OnLevelLoaded(Level lvl)
        {
            if (NasBlock.blocksIndexedByServerushort == null)
            {
                return;
            }
            NasLevel nl = new NasLevel();
            string fileName = GetFileName(lvl.name);
            if (File.Exists(fileName))
            {
                string jsonString = File.ReadAllText(fileName);
                nl = JsonConvert.DeserializeObject<NasLevel>(jsonString);
                nl.lvl = lvl;
                if (!all.ContainsKey(lvl.name))
                {
                    all.Add(lvl.name, nl);
                }
                nl.BeginTickTask();
                if (nl.biome < 0)
                {
                    nl.dungeons = false;
                }
                if (nl.dungeons)
                {
                    Random rng = new Random(MakeInt(lvl.name));
                    int dungeonCount = rng.Next(3, 6);
                    for (int done = 0; done <= dungeonCount; done++)
                    {
                        NasGen.GenInstance.GenerateDungeon(rng, lvl, nl);
                    }
                    nl.dungeons = true;
                }
                Logger.Log(LogType.Debug, "Loaded NasLevel " + fileName + "!");
            }
        }
        public static void OnLevelUnload(Level lvl, ref bool cancel)
        {
            if (!all.ContainsKey(lvl.name)) 
            { 
                return; 
            }
            Unload(lvl.name, all[lvl.name]);
        }
        public static void OnLevelDeleted(string name)
        {
            string fileName = Path + name + Extension;
            if (File.Exists(fileName))
            {
                FileIO.TryDelete(fileName);
                Logger.Log(LogType.Debug, "Deleted NasLevel " + fileName + "!");
            }
        }
        public static void OnLevelRenamed(string srcMap, string dstMap)
        {
            string fileName = Path + srcMap + Extension;
            if (File.Exists(fileName))
            {
                string newFileName = Path + dstMap + Extension;
                FileIO.TryMove(fileName, newFileName);
                Logger.Log(LogType.Debug, "Renamed NasLevel " + fileName + " to " + newFileName + "!");
            }
        }
    }
}
#endif