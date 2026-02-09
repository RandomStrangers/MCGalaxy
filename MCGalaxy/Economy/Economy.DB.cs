/*
    Copyright 2011 MCForge
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
using MCGalaxy.SQL;
namespace MCGalaxy.Eco
{
    public static partial class Economy
    {
        static readonly ColumnDesc[] ecoTable = new ColumnDesc[] {
            new("player", ColumnType.VarChar, 20, priKey: true),
            new("money", ColumnType.Int32),
            new("total", ColumnType.Int32),
            new("purchase", ColumnType.VarChar, 255),
            new("payment", ColumnType.VarChar, 255),
            new("salary", ColumnType.VarChar, 255),
            new("fine", ColumnType.VarChar, 255),
        };
        public static void LoadDatabase()
        {
            Database.CreateTable("Economy", ecoTable);
            Database.ReadRows("Economy", "*",
                                record => ParseStats(record),
                                "WHERE money > 0");
        }
        public static string FindMatches(Player p, string name, out int money)
        {
            string[] match = PlayerDB.MatchValues(p, name, "Name,Money");
            money = match == null ? 0 : NumberUtils.ParseInt32(match[1]);
            return match?[0];
        }
        public static void UpdateMoney(string name, int money) => PlayerDB.Update(name, PlayerData.ColumnMoney,
                            NumberUtils.StringifyInt(money));
        public struct EcoStats
        {
            public string Player, Purchase, Payment, Salary, Fine;
            public int TotalSpent;
        }
        public static void UpdateStats(EcoStats stats) => Database.AddOrReplaceRow("Economy", "player, money, total, purchase, payment, salary, fine",
                                     stats.Player, 0, stats.TotalSpent, stats.Purchase,
                                     stats.Payment, stats.Salary, stats.Fine);
        static EcoStats ParseStats(ISqlRecord record) => new()
        {
            Player = record.GetText("player"),
            Payment = Parse(record.GetText("payment")),
            Purchase = Parse(record.GetText("purchase")),
            Salary = Parse(record.GetText("salary")),
            Fine = Parse(record.GetText("fine")),
            TotalSpent = record.GetInt("total"),
        };
        static string Parse(string raw) => raw == null || raw.Length == 0 || raw.CaselessEq("NULL") ? null : raw.CaselessEq("%cNone") ? null : raw;
        public static EcoStats RetrieveStats(string name)
        {
            EcoStats stats = default;
            stats.Player = name;
            Database.ReadRows("Economy", "*",
                                record => stats = ParseStats(record),
                                "WHERE player=@0", name);
            return stats;
        }
    }
}
