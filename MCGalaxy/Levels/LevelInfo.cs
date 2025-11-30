/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.DB;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.SQL;
using System;
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    public static class LevelInfo
    {
        /// <summary> Array of all current loaded levels. </summary>
        /// <remarks> Note this field is highly volatile, you should cache references to the items array. </remarks>
        public static VolatileArray<Level> Loaded = new();
        public static Level FindExact(string name)
        {
            Level[] loaded = Loaded.Items;
            foreach (Level lvl in loaded)
            {
                if (lvl.name.CaselessEq(name))
                {
                    return lvl;
                }
            }
            return null;
        }
        public static void Add(Level lvl)
        {
            Loaded.Add(lvl);
            OnLevelAddedEvent.Call(lvl);
        }
        public static void Remove(Level lvl)
        {
            Loaded.Remove(lvl);
            OnLevelRemovedEvent.Call(lvl);
        }
        public static string[] AllMapFiles()
        {
            List<string> Files = new();
            Files.AddRange(FileIO.TryGetFiles("levels", "*.lvl"));
            Files.AddRange(FileIO.TryGetFiles("levels", "*.mcf"));
            Files.AddRange(FileIO.TryGetFiles("levels", "*.map"));
            return Files.ToArray();
        }
        public static string[] AllMapNames()
        {
            string[] files = AllMapFiles();
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            return files;
        }
        public static bool MapExists(string name)
        {
            return File.Exists(MapPath(name));
        }
        public static string GetExt(string levelName)
        {
            bool mcf = File.Exists("levels/" + levelName.ToLower() + ".mcf"),
                map = File.Exists("levels/" + levelName.ToLower() + ".map");
            if (mcf)
            {
                return ".mcf";
            }
            else if (map)
            {
                return ".map";
            }
            else
            {
                return ".lvl";
            }
        }
        public static string Name(string name, string ext = ".lvl")
        {
            if (!ext.CaselessContains(".lvl"))
            {
                return name.ToLower() + ext;
            }
            else
            {
                bool mcf = File.Exists("levels/" + name.ToLower() + ".mcf"),
                    map = File.Exists("levels/" + name.ToLower() + ".map");
                if (mcf)
                {
                    return name.ToLower() + ".mcf";
                }
                else if (map)
                {
                    return name.ToLower() + ".map";
                }
                else
                {
                    return name.ToLower() + ".lvl";
                }
            }
        }
        /// <summary> Relative path of a level's map file </summary>
        public static string MapPath(string name, string ext = ".lvl")
        {
            return "levels/" + Name(name, ext);
        }
        /// <summary> Relative path of a level's backup folder </summary>
        public static string BackupBasePath(string name)
        {
            return Server.Config.BackupDirectory + "/" + name;
        }
        /// <summary> Relative path of a level's backup map directory </summary>
        public static string BackupDirPath(string name, string backup)
        {
            return BackupBasePath(name) + "/" + backup;
        }
        /// <summary> Relative path of a level's backup map file </summary>
        public static string BackupFilePath(string name, string backup, string ext = ".lvl")
        {
            if (!ext.CaselessContains(".lvl"))
            {
                return BackupDirPath(name, backup) + "/" + name + ext;
            }
            else
            {
                bool mcf = File.Exists("levels/" + name.ToLower() + ".mcf"),
                    map = File.Exists("levels/" + name.ToLower() + ".map");
                if (mcf)
                {
                    return BackupDirPath(name, backup) + "/" + name + ".mcf";
                }
                else if (map)
                {
                    return BackupDirPath(name, backup) + "/" + name + ".map";
                }
                else
                {
                    return BackupDirPath(name, backup) + "/" + name + ".lvl";
                }
            }
        }
        public static string BackupNameFrom(string path)
        {
            return path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }
        public static int LatestBackup(string map)
        {
            string root = BackupBasePath(map);
            string[] backups = FileIO.TryGetDirectories(root);
            int latest = 0;
            foreach (string path in backups)
            {
                string backupName = BackupNameFrom(path);

                if (!NumberUtils.TryParseInt32(backupName, out int num))
                {
                    continue;
                }
                latest = Math.Max(num, latest);
            }
            return latest;
        }
        public static string NextBackup(string map)
        {
            string root = BackupBasePath(map);
            Directory.CreateDirectory(root);
            return (LatestBackup(map) + 1).ToString();
        }
        public const string LATEST_MUSEUM_FLAG = "*latest";
        /// <summary>
        /// Returns true if a file was found for the given map with the given backup number.
        /// Supports LATEST_FLAG as backup number to return latest backup path.
        /// </summary>
        public static bool GetBackupPath(Player p, string map, string backupNumber, out string path)
        {
            if (!Directory.Exists(BackupBasePath(map)))
            {
                p.Message("Level \"{0}\" has no backups.", map);
                path = null;
                return false;
            }
            if (backupNumber == LATEST_MUSEUM_FLAG)
            {
                int latest = LatestBackup(map);
                if (latest == 0)
                {
                    p.Message("&WLevel \"{0}\" does not have any numbered backups, " +
                        "so the latest backup could not be determined.", map);
                    path = null;
                    return false;
                }
                backupNumber = latest.ToString();
            }
            path = BackupFilePath(map, backupNumber);
            if (!File.Exists(path))
            {
                p.Message("Backup \"{0}\" for {1} could not be found.", backupNumber, map);
                return false;
            }
            return true;
        }
        /// <summary> Relative path of a level's property file </summary>
        public static string PropsPath(string name)
        {
            return "levels/level properties/" + name + ".properties";
        }
        public static LevelConfig GetConfig(string map)
        {
            return GetConfig(map, out _);
        }
        internal static LevelConfig GetConfig(string map, out Level lvl)
        {
            lvl = FindExact(map);
            if (lvl != null)
            {
                return lvl.Config;
            }
            string propsPath = PropsPath(map);
            LevelConfig cfg = new();
            cfg.Load(propsPath);
            return cfg;
        }
        public static bool Check(Player p, LevelPermission plRank, string map, string action, out LevelConfig cfg)
        {
            cfg = GetConfig(map, out Level lvl);
            if (p.IsConsole)
            {
                return true;
            }
            if (lvl != null)
            {
                return Check(p, plRank, lvl, action);
            }
            AccessController visit = new LevelAccessController(cfg, map, true);
            AccessController build = new LevelAccessController(cfg, map, false);
            if (!visit.CheckDetailed(p, plRank) || !build.CheckDetailed(p, plRank))
            {
                p.Message("Hence, you cannot {0}.", action); 
                return false;
            }
            return true;
        }
        public static bool Check(Player p, LevelPermission plRank, string map, string action)
        {
            return Check(p, plRank, map, action, out _);
        }
        public static bool Check(Player p, LevelPermission plRank, Level lvl, string action)
        {
            if (p.IsConsole)
            {
                return true;
            }
            if (!lvl.VisitAccess.CheckDetailed(p, plRank) || !lvl.BuildAccess.CheckDetailed(p, plRank))
            {
                p.Message("Hence, you cannot {0}.", action); 
                return false;
            }
            return true;
        }
        public static bool ValidName(string map)
        {
            foreach (char c in map)
            {
                if (!Database.ValidNameChar(c))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsRealmOwner(string name, string map)
        {
            LevelConfig cfg = GetConfig(map);
            return IsRealmOwner(map, cfg, name);
        }
        public static bool IsRealmOwner(Level lvl, string name)
        {
            return IsRealmOwner(lvl.name, lvl.Config, name);
        }
        public static bool IsRealmOwner(string map, LevelConfig cfg, string name)
        {
            string[] owners = cfg.RealmOwner.SplitComma();
            if (owners.Length > 0)
            {
                foreach (string owner in owners)
                {
                    if (owner.CaselessEq(name))
                    {
                        return true;
                    }
                }
                return false;
            }
            return Server.Config.ClassicubeAccountPlus && map.CaselessStarts(name);
        }
        internal static string DefaultRealmOwner(string map)
        {
            bool plus = Server.Config.ClassicubeAccountPlus;
            if (!plus || map.IndexOf('+') == -1)
            {
                return null;
            }
            while (map.Length > 0 && char.IsNumber(map[map.Length - 1]))
            {
                map = map.Substring(0, map.Length - 1);
            }
            return PlayerDB.FindName(map);
        }
        /// <summary>
        /// If playerName owns levelName and levelName begins with playerName.
        /// </summary>
        internal static bool IsPersonalRealmOwner(string playerName, string levelName)
        {
            return levelName.CaselessStarts(playerName) && IsRealmOwner(playerName, levelName);
        }
        /// <summary>
        /// Returns all the os maps personally(level name begins with player name) owned by p, sorted alphabetically.
        /// </summary>
        internal static List<string> AllPersonalRealms(string playerName)
        {
            string[] allMaps = AllMapNames();
            List<string> owned = new();
            foreach (string lvlName in allMaps)
            {
                if (IsPersonalRealmOwner(playerName, lvlName))
                {
                    owned.Add(lvlName);
                }
            }
            owned.Sort(new AlphanumComparator());
            return owned;
        }
        public static void ListMaps(Player p, IList<string> maps, string levelsTitle, string listCmd, string itemName, string page, bool showVisitable = true)
        {
            p.Message("{0} (&c[no] &Sif not visitable):", levelsTitle);
            Paginator.Output(p, maps, (file) => FormatMap(p, file, showVisitable),
                             listCmd, itemName, page);
        }
        static string FormatMap(Player p, string map, bool showVisitable)
        {
            RetrieveProps(map, out LevelPermission visitP, out LevelPermission buildP, out bool loadOnGoto);
            LevelPermission maxPerm = visitP;
            if (maxPerm < buildP)
            {
                maxPerm = buildP;
            }
            string visit;
            if (showVisitable)
            {
                visit = loadOnGoto && p.Rank >= visitP ? "" : " &c[no]";
            }
            else
            {
                visit = "";
            }
            return Group.GetColor(maxPerm) + map + visit;
        }
        static void RetrieveProps(string level, out LevelPermission visit,
                                  out LevelPermission build, out bool loadOnGoto)
        {
            visit = LevelPermission.Guest;
            build = LevelPermission.Guest;
            loadOnGoto = true;
            string propsPath = PropsPath(level);
            SearchArgs args = new();
            if (!PropertiesFile.Read(propsPath, ref args, ProcessLine))
            {
                return;
            }
            visit = Group.ParsePermOrName(args.Visit, visit);
            build = Group.ParsePermOrName(args.Build, build);
            if (!bool.TryParse(args.LoadOnGoto, out loadOnGoto))
            {
                loadOnGoto = true;
            }
        }
        static void ProcessLine(string key, string value, ref SearchArgs args)
        {
            if (key.CaselessEq("pervisit"))
            {
                args.Visit = value;
            }
            else if (key.CaselessEq("perbuild"))
            {
                args.Build = value;
            }
            else if (key.CaselessEq("loadongoto"))
            {
                args.LoadOnGoto = value;
            }
        }
        struct SearchArgs 
        { 
            public string Visit, Build, LoadOnGoto; 
        }
    }
}