#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Events.LevelEvents;
using Newtonsoft.Json;
using System;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class NasLevel
    {
        public const string Path = Nas.Path + "leveldata/",
            Extension = ".json";
        public static bool IsNasLevel(Level lvl)
        {
            if (lvl.Config.Deletable && lvl.Config.Buildable)
            {
                return false;
            }
            if (Get(lvl) == null)
            {
                return false;
            }
            return true;
        }
        public static NasLevel Get(Level lvl)
        {
            if (all.ContainsKey(lvl.name))
            {
                return all[lvl.name];
            }
            return null;
        }
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
        public bool Save(string name = "")
        {
            if (name.IsNullOrEmpty())
            {
                name = lvl.name;
            }
            EndTickTask();
            lvl.Save(true);
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented),
                fileName = GetFileName(name);
            FileUtils.TryWriteAllText(fileName, jsonString);
            Log("Unloaded(saved) NasLevel {0}!", fileName);
            all.Remove(name);
            Server.DoGC();
            return true;
        }
        public static NasLevel Get(string name)
        {
            if (all.ContainsKey(name))
            {
                return all[name];
            }
            else
            {
                NasLevel nl = new();
                string fileName = GetFileName(name);
                if (File.Exists(fileName))
                {
                    string jsonString = FileUtils.TryReadAllText(fileName);
                    nl = JsonConvert.DeserializeObject<NasLevel>(jsonString);
                    return nl;
                }
                Log("NasLevel {0} does not exist, creating a new one!", name);
                return nl;
            }
        }
        public static void Unload(string name, NasLevel nl)
        {
            nl.Save(name);
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
            NasLevel nl;
            string fileName = GetFileName(lvl.name);
            if (File.Exists(fileName))
            {
                string jsonString = FileUtils.TryReadAllText(fileName);
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
                    Random rng = new(MakeInt(lvl.name));
                    int dungeonCount = rng.Next(3, 6);
                    for (int done = 0; done <= dungeonCount; done++)
                    {
                        NasGen.GenInstance.GenerateDungeon(rng, lvl, nl);
                    }
                    nl.dungeons = true;
                }
                Log("Loaded NasLevel {0}!", fileName);
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
                Log("Deleted NasLevel {0}!", fileName);
            }
        }
        public static void OnLevelRenamed(string srcMap, string dstMap)
        {
            string fileName = Path + srcMap + Extension;
            if (File.Exists(fileName))
            {
                string newFileName = Path + dstMap + Extension;
                FileIO.TryMove(fileName, newFileName);
                Log("Renamed NasLevel {0} to {1}!", fileName, newFileName);
            }
        }
    }
}
#endif