/*
    Copyright 2015-2024 MCGalaxy
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
using MCGalaxy.SQL;
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy.DB
{
    /// <summary> Stores per-player persistent data. </summary>
    public static class PlayerDB
    {
        static string LoginPath(string name) => "text/login/" + name.ToLower() + ".txt";
        static string LogoutPath(string name) => "text/logout/" + name.ToLower() + ".txt";
        public static string LoadNick(string name)
        {
            string path = "players/" + name + "DB.txt";
            if (!File.Exists(path)) return null;
            foreach (string line in FileIO.TryReadAllLines(path))
            {
                if (!line.CaselessStarts("Nick = ")) continue;
                return line.Substring("Nick = ".Length).Trim();
            }
            return null;
        }
        public static void SetNick(string name, string nick)
        {
            EnsureDirectoriesExist();
            using StreamWriter sw = new("players/" + name + "DB.txt", false);
            sw.WriteLine("Nick = " + nick);
        }
        public static string GetLoginMessage(string name)
        {
            string path = LoginPath(name);
            if (File.Exists(path)) return FileIO.TryReadAllText(path);
            path = "text/login/" + name + ".txt";
            return File.Exists(path) ? FileIO.TryReadAllText(path) : "";
        }
        public static string GetLogoutMessage(string name)
        {
            string path = LogoutPath(name);
            if (File.Exists(path)) return FileIO.TryReadAllText(path);
            path = "text/logout/" + name + ".txt";
            return File.Exists(path) ? FileIO.TryReadAllText(path) : "";
        }
        static void SetMessage(string path, string msg)
        {
            EnsureDirectoriesExist();
            if (msg.Length > 0)
            {
                FileIO.TryWriteAllText(path, msg);
            }
            else if (File.Exists(path))
            {
                FileIO.TryDelete(path);
            }
        }
        public static void SetLoginMessage(string name, string msg) => SetMessage(LoginPath(name), msg);
        public static void SetLogoutMessage(string name, string msg) => SetMessage(LogoutPath(name), msg);
        /// <summary> Returns the Name field of the row whose Name field caselessly equals the given name </summary>
        public static string FindName(string name) => Database.ReadString("Players", "Name", "WHERE Name=@0 COLLATE NOCASE", name);
        /// <summary> Returns the IP field of the row whose Name field caselessly equals the given name </summary>
        public static string FindIP(string name) => Database.ReadString("Players", "IP", "WHERE Name=@0 COLLATE NOCASE", name);
        public static string FindOfflineIPMatches(Player p, string name, out string ip)
        {
            string[] match = MatchValues(p, name, "Name,IP");
            ip = match?[1];
            return match?[0];
        }
        public static void Update(string name, string column, string value) => Database.UpdateRows("Players", column + "=@1", "WHERE Name=@0", name, value);
        public static string FindColor(Player p) => Database.ReadString("Players", "Color", "WHERE ID=@0", p.DatabaseID) == null ? "" : PlayerData.ParseColor(Database.ReadString("Players", "Color", "WHERE ID=@0", p.DatabaseID));
        public static string MatchNames(Player p, string name) => Matcher.Find(p, name, out int matches, MatchMulti(name, "Name", r => r.GetText(0)),
                                null, n => n, "players", 20);
        public static string[] MatchValues(Player p, string name, string columns) => Matcher.Find(p, name, out int matches, MatchMulti(name, columns, Database.ParseFields),
                                null, n => n[0], "players", 20);
        public static PlayerData Match(Player p, string name) => Matcher.Find(p, name, out int matches, MatchMulti(name, "*", PlayerData.Parse),
                                null, stat => stat.Name, "players", 20);
        public delegate T RecordParser<T>(ISqlRecord record);
        static List<T> MatchMulti<T>(string name, string columns, RecordParser<T> parseRecord) where T : class
        {
            List<T> list = FindPartial(name, columns, parseRecord);
            if (list.Count < 25) return list;
            // As per the LIMIT in FindPartial, the SQL backend stops after finding 25 matches
            // However, this means that e.g. in the case of 30 partial matches and
            //    then 1 exact match, the exact WON'T be part of the returned list
            // So explicitly check for this case
            T exact = FindExact(name, columns, parseRecord);
            if (exact == null) return list;
            list.Clear();
            list.Add(exact);
            return list;
        }
        public static List<T> FindPartial<T>(string name, string columns, RecordParser<T> parseRecord)
        {
            List<T> list = new();
            Database.ReadRows("Players", columns, r => list.Add(parseRecord(r)),
                              "WHERE Name LIKE @0 ESCAPE '#' LIMIT 25 COLLATE NOCASE",
                              "%" + name.Replace("_", "#_") + "%");
            return list;
        }
        public static T FindExact<T>(string name, string columns, RecordParser<T> parseRecord) where T : class
        {
            T exact = null;
            Database.ReadRows("Players", columns, r => exact = parseRecord(r),
                              "WHERE Name=@0  COLLATE NOCASE LIMIT 1", name);
            return exact;
        }
        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists("text/login"))
                Directory.CreateDirectory("text/login");
            if (!Directory.Exists("text/logout"))
                Directory.CreateDirectory("text/logout");
            if (!Directory.Exists("players"))
                Directory.CreateDirectory("players");
        }
    }
}
