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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MCGalaxy.Platform;
namespace MCGalaxy.SQL
{
    public class SQLiteBackend : IDatabaseBackend
    {
        public static IDatabaseBackend Instance = new SQLiteBackend();
        public SQLiteBackend()
        {
            CaselessWhereSuffix = " COLLATE NOCASE";
            CaselessLikeSuffix = " COLLATE NOCASE";
        }
        public override bool EnforcesTextLength => false;
        public override bool EnforcesIntegerLimits => false;
        public override bool MultipleSchema => false;
        public override string EngineName => "SQLite";
        public override ISqlConnection CreateConnection() => new MCGSQLiteConnection();
        public override void LoadDependencies()
        {
            if (IOperatingSystem.DetectOS().IsWindows)
            {
                Server.CheckFile("sqlite3_x32.dll");
                Server.CheckFile("sqlite3_x64.dll");
                try
                {
                    string dll = IntPtr.Size == 8 ? "sqlite3_x64.dll" : "sqlite3_x32.dll";
                    if (File.Exists(dll))
                    {
                        FileIO.TryCopy(dll, "sqlite3.dll", true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error moving SQLite dll", ex);
                }
            }
        }
        public override void CreateDatabase() { }
        public override bool TableExists(string table) => Database.CountRows("sqlite_master",
                                      "WHERE type='table' AND name=@0", table) > 0;
        public override List<string> AllTables()
        {
            List<string> tables = GetStrings("SELECT name from sqlite_master WHERE type='table'");
            for (int i = tables.Count - 1; i >= 0; i--)
            {
                if (tables[i].StartsWith("sqlite_"))
                {
                    tables.RemoveAt(i);
                }
            }
            return tables;
        }
        public override List<string> ColumnNames(string table)
        {
            SqlUtils.ValidateName(table);
            List<string> columns = new();
            Database.Iterate("PRAGMA table_info(`" + table + "`)",
                             record => columns.Add(record.GetText("name")), null);
            return columns;
        }
        public override string RenameTableSql(string srcTable, string dstTable) => "ALTER TABLE `" + srcTable + "` RENAME TO `" + dstTable + "`";
        protected override void CreateTableColumns(StringBuilder sql, ColumnDesc[] columns)
        {
            string priKey = null;
            for (int i = 0; i < columns.Length; i++)
            {
                ColumnDesc col = columns[i];
                sql.Append(col.Column).Append(' ');
                sql.Append(col.FormatType());
                if (col.PrimaryKey)
                {
                    if (!col.AutoIncrement)
                    {
                        priKey = col.Column;
                    }
                    else
                    {
                        sql.Append(" PRIMARY KEY");
                    }
                }
                if (col.AutoIncrement)
                {
                    sql.Append(" AUTOINCREMENT");
                }
                if (col.NotNull)
                {
                    sql.Append(" NOT NULL");
                }
                if (i < columns.Length - 1)
                {
                    sql.Append(',');
                }
                else if (priKey != null)
                {
                    sql.Append(", PRIMARY KEY(").Append(priKey).Append(") ");
                }
                sql.AppendLine();
            }
        }
        public override void PrintSchema(string table, TextWriter w)
        {
            string sql = "SELECT sql from sqlite_master WHERE tbl_name = @0 AND type = 'table'";
            List<string> all = GetStrings(sql + CaselessWhereSuffix, table);
            for (int i = 0; i < all.Count; i++)
            {
                sql = all[i].Replace(" " + table, " `" + table + "`");
                sql = sql.Replace("CREATE TABLE `" + table + "`", "CREATE TABLE IF NOT EXISTS `" + table + "`");
                w.WriteLine(sql + ";");
            }
        }
        public override string AddColumnSql(string table, ColumnDesc col, string colAfter) => "ALTER TABLE `" + table + "` ADD COLUMN " + col.Column + " " + col.FormatType();
        public override string AddOrReplaceRowSql(string table, string columns, int numArgs) => InsertSql("INSERT OR REPLACE INTO", table, columns, numArgs);
    }
    sealed class MCGSQLiteConnection : SQLiteConnection
    {
        protected override bool ConnectionPooling => Server.Config.DatabasePooling;
        protected override string DBPath => "MCGalaxy.db";
    }
}
