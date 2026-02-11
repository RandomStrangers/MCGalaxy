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
namespace MCGalaxy.DB
{
    /// <summary> Converts names to integer ids and back </summary>
    public static class NameConverter
    {
        /// <summary> Returns the name associated with the given ID, or ID#[id] if not found </summary>
        public static string FindName(int id)
        {
            // Only returns non-null if id > MaxPlayerID - invalid.Count
            string name = Server.invalidIds.GetAt(0x00FFFFFF - id);
            if (name != null) return name;
            name = Database.ReadString("Players", "Name", "WHERE ID=@0", id);
            return name ?? "ID#" + id;
        }
        /// <summary> Finds all the IDs associated with the given name. </summary>
        public static int[] FindIds(string name)
        {
            List<int> ids = new();
            int i = Server.invalidIds.IndexOf(name);
            if (i >= 0) ids.Add(0x00FFFFFF - i);
            Database.ReadRows("Players", "ID",
                                record => ids.Add(record.GetInt32(0)),
                                "WHERE Name=@0", name);
            return ids.ToArray();
        }
        /// <summary> Returns a non-database ID for the given name </summary>
        public static int InvalidNameID(string name)
        {
            if (Server.invalidIds.Add(name)) Server.invalidIds.Save();
            return 0x00FFFFFF - Server.invalidIds.IndexOf(name);
        }
    }
}
