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
using System;
namespace MCGalaxy.DB
{
    /// <summary> Retrieves or sets player stats in the database. </summary>
    public class PlayerData
    {
        public const string ColumnDeaths = "totalDeaths";
        public const string ColumnLogins = "totalLogin";
        public const string ColumnMoney = "Money";
        public const string ColumnKicked = "totalKicked";
        public const string ColumnColor = "color";
        public const string ColumnTitle = "title";
        public const string ColumnTColor = "title_color";
        public const string ColumnName = "Name";
        public const string ColumnIP = "IP";
        public const string ColumnID = "ID";
        public const string ColumnFirstLogin = "FirstLogin";
        public const string ColumnLastLogin = "LastLogin";
        public const string ColumnTimeSpent = "TimeSpent";
        public const string ColumnBlocks = "totalBlocks";
        public const string ColumnDrawn = "totalCuboided";
        public const string ColumnMessages = "Messages";
        public string Name, Color, Title, TitleColor, IP;
        public DateTime FirstLogin, LastLogin;
        public int DatabaseID, Money, Deaths, Logins, Kicks, Messages;
        public long TotalModified, TotalDrawn, TotalPlaced, TotalDeleted;
        public TimeSpan TotalTime;
        internal static void Create(Player p)
        {
            p.prefix = "";
            p.SetColor(p.group.Color);
            p.FirstLogin = DateTime.Now;
            p.TimesVisited = 1;
            string now = DateTime.Now.ToInvariantDateString();
            Database.AddRow("Players", "Name, IP, FirstLogin, LastLogin, totalLogin, Title, " +
                            "totalDeaths, Money, totalBlocks, totalKicked, Messages, TimeSpent",
                            p.name, p.ip, now, now, 1, "", 0, 0, 0, 0, 0, (long)p.TotalTime.TotalSeconds);
            int id = -200;
            Database.ReadRows("Players", "ID",
                                record => id = record.GetInt32(0),
                                "WHERE Name=@0", p.name);
            p.DatabaseID = id != -200 ? id : NameConverter.InvalidNameID(p.name);
        }
        /// <summary> Initialises the given player's stats from this instance. </summary>
        public void ApplyTo(Player p)
        {
            p.TimesVisited = Logins + 1;
            p.TotalTime = TotalTime;
            p.DatabaseID = DatabaseID;
            p.FirstLogin = FirstLogin;
            p.title = Title;
            p.titlecolor = TitleColor;
            string col = Color;
            if (col.Length == 0) col = p.group.Color;
            p.SetColor(col);
            p.SetBaseTotalModified(TotalModified);
            p.TotalDrawn = TotalDrawn;
            p.TotalPlaced = TotalPlaced;
            p.TotalDeleted = TotalDeleted;
            p.TimesDied = Deaths;
            p.TotalMessagesSent = Messages;
            p.money = Money;
            p.TimesBeenKicked = Kicks;
        }
        internal static PlayerData Parse(ISqlRecord record)
        {
            PlayerData data = new()
            {
                Name = record.GetText(ColumnName),
                IP = record.GetText(ColumnIP),
                DatabaseID = record.GetInt(ColumnID)
            };
            string rawTime = record.GetText(ColumnTimeSpent);
            try
            {
                data.TotalTime = TimeSpan.FromSeconds(long.Parse(rawTime));
            }
            catch
            {
                data.TotalTime = Database.ParseOldDBTimeSpent(rawTime);
            }
            data.FirstLogin = ParseDateTime(record, ColumnFirstLogin);
            data.LastLogin = ParseDateTime(record, ColumnLastLogin);
            data.Title = record.GetText(ColumnTitle);
            data.Title = data.Title.Cp437ToUnicode();
            data.TitleColor = ParseColor(record.GetText(ColumnTColor));
            data.Color = ParseColor(record.GetText(ColumnColor));
            data.Money = record.GetInt(ColumnMoney);
            data.Deaths = record.GetInt(ColumnDeaths);
            data.Logins = record.GetInt(ColumnLogins);
            data.Kicks = record.GetInt(ColumnKicked);
            data.Messages = record.GetInt(ColumnMessages);
            long blocks = record.GetLong(ColumnBlocks),
                drawn = record.GetLong(ColumnDrawn);
            data.TotalModified = UnpackLo(blocks);
            data.TotalPlaced = UnpackHi(blocks);
            data.TotalDrawn = UnpackLo(drawn);
            data.TotalDeleted = UnpackHi(drawn);
            return data;
        }
        internal static long ParseLong(string value) => (value.Length == 0 || value.CaselessEq("null")) ? 0 : long.Parse(value);
        internal static string ParseColor(string raw)
        {
            if (raw.Length == 0) return raw;
            string col = Colors.Parse(raw);
            return col.Length > 0 ? col : Colors.Name(raw).Length == 0 ? "" : raw;
        }
        static DateTime ParseDateTime(ISqlRecord record, string name)
        {
            int i = record.GetOrdinal(name);
            string raw = record.GetStringValue(i);
            if (raw.TryParseInvariantDateString(out DateTime dt)) return dt;
            try
            {
                return record.GetDateTime(i);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error parsing date", ex);
                return DateTime.MinValue;
            }
        }
        internal static long UnpackHi(long value) => (value >> 38) & ((1L << 26) - 1);
        internal static long UnpackLo(long value) => value & ((1L << 38) - 1);
        internal static long Pack(long hi, long lo) => hi << 38 | lo;
    }
}
