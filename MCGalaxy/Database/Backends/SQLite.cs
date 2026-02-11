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
using MCGalaxy.Network;
using MCGalaxy.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
namespace MCGalaxy.SQL
{
    public class SQLiteBackend
    {
        public static SQLiteBackend Instance = new();
        protected static List<string> GetStrings(string sql, params object[] args)
        {
            List<string> values = new();
            Database.Iterate(sql,
                            record => values.Add(record.GetText(0)),
                            args);
            return values;
        }
        public string CreateTableSql(string table, ColumnDesc[] columns)
        {
            StringBuilder sql = new();
            sql.AppendLine("CREATE TABLE if not exists `" + table + "` (");
            CreateTableColumns(sql, columns);
            sql.AppendLine(");");
            return sql.ToString();
        }
        public string DeleteTableSql(string table) => "DROP TABLE if exists `" + table + "`";
        public string CopyAllRowsSql(string srcTable, string dstTable) => "INSERT INTO `" + dstTable + "` SELECT * FROM `" + srcTable + "`";
        /// <summary> Returns SQL for reading rows from the given table. </summary>
        public string ReadRowsSql(string table, string columns, string modifier)
        {
            string sql = "SELECT " + columns + " FROM `" + table + "`";
            if (modifier.Length > 0) sql += " " + modifier;
            return sql;
        }
        /// <summary> Returns SQL for updating rows for the given table. </summary>
        public string UpdateRowsSql(string table, string columns, string modifier)
        {
            string sql = "UPDATE `" + table + "` SET " + columns;
            if (modifier.Length > 0) sql += " " + modifier;
            return sql;
        }
        /// <summary> Returns SQL for deleting rows for the given table. </summary>
        public string DeleteRowsSql(string table, string modifier)
        {
            string sql = "DELETE FROM `" + table + "`";
            if (modifier.Length > 0) sql += " " + modifier;
            return sql;
        }
        /// <summary> Returns SQL for adding a row to the given table. </summary>
        public string AddRowSql(string table, string columns, int numArgs) => InsertSql("INSERT INTO", table, columns, numArgs);
        protected string InsertSql(string cmd, string table, string columns, int numArgs)
        {
            StringBuilder sql = new(cmd);
            sql.Append(" `").Append(table).Append("` ");
            sql.Append('(').Append(columns).Append(')');
            string[] names = GetNames(numArgs);
            sql.Append(" VALUES (");
            for (int i = 0; i < numArgs; i++)
            {
                sql.Append(names[i]);
                if (i < numArgs - 1) sql.Append(", ");
                else sql.Append(")");
            }
            return sql.ToString();
        }
        #region Raw SQL functions
        /// <summary> Executes an SQL command and returns the number of affected rows. </summary>
        public int Execute(string sql, object[] parameters)
        {
            int rows = 0;
            using (SQLiteConnection conn = new())
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand(sql))
                {
                    FillParams(cmd, parameters);
                    rows = cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            return rows;
        }
        /// <summary> Excecutes an SQL query, invoking a callback on the returned rows one by one. </summary>
        public int Iterate(string sql, object[] parameters, ReaderCallback callback)
        {
            int rows = 0;
            using (SQLiteConnection conn = new())
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand(sql))
                {
                    FillParams(cmd, parameters);
                    using ISqlReader reader = cmd.ExecuteReader();
                    while (reader.Read()) 
                    { 
                        callback(reader); 
                        rows++; 
                    }
                }
                conn.Close();
            }
            return rows;
        }
        /// <summary> Sets the SQL command's parameter values to the given arguments </summary>
        public static void FillParams(SQLiteCommand cmd, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return;
            string[] names = GetNames(parameters.Length);
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.AddParameter(names[i], parameters[i]);
            }
        }
        static volatile string[] ids;
        internal static string[] GetNames(int count)
        {
            // Avoid allocation overhead from string concat every query by caching
            string[] names = ids;
            if (names == null || count > names.Length)
            {
                names = new string[count];
                for (int i = 0; i < names.Length; i++) 
                {
                    names[i] = "@" + i;
                }
                ids = names;
            }
            return names;
        }
        #endregion
        static void CheckFile(string file)
        {
            if (!File.Exists(file))
            {
                Logger.Log(LogType.SystemActivity, file + " doesn't exist, Downloading..");
                try
                {
                    using (WebClient client = HttpUtil.CreateWebClient())
                    {
                        client.DownloadFile("https://raw.githubusercontent.com/ClassiCube/MCGalaxy/master/" + file, file);
                    }
                    if (File.Exists(file))
                    {
                        Logger.Log(LogType.SystemActivity, file + " download succesful!");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Downloading " + file + " failed, try again later", ex);
                }
            }
        }
        public void LoadDependencies()
        {
            // on macOS/Linux, system provided sqlite3 native library is used
            if (!IOperatingSystem.DetectOS().IsWindows) return;
            CheckFile("sqlite3_x32.dll");
            CheckFile("sqlite3_x64.dll");
            // sqlite3.dll is the .DLL that MCGalaxy will actually load on Windows
            try
            {
                if (File.Exists(IntPtr.Size == 8 ? "sqlite3_x64.dll" : "sqlite3_x32.dll")) FileIO.TryCopy(IntPtr.Size == 8 ? "sqlite3_x64.dll" : "sqlite3_x32.dll", "sqlite3.dll", true);
            }
            catch (Exception ex)
            {
                // e.g. can happen when multiple server instances running
                Logger.LogError("Error moving SQLite dll", ex);
            }
        }
        public bool TableExists(string table) => Database.CountRows("sqlite_master",
                                      "WHERE type='table' AND name=@0", table) > 0;
        public List<string> AllTables()
        {
            List<string> tables = GetStrings("SELECT name from sqlite_master WHERE type='table'");
            // exclude sqlite built-in database tables
            for (int i = tables.Count - 1; i >= 0; i--)
            {
                if (tables[i].StartsWith("sqlite_")) tables.RemoveAt(i);
            }
            return tables;
        }
        public List<string> ColumnNames(string table)
        {
            SqlUtils.ValidateName(table);
            List<string> columns = new();
            Database.Iterate("PRAGMA table_info(`" + table + "`)",
                             record => columns.Add(record.GetText("name")), null);
            return columns;
        }
        public string RenameTableSql(string srcTable, string dstTable) => "ALTER TABLE `" + srcTable + "` RENAME TO `" + dstTable + "`";
        protected void CreateTableColumns(StringBuilder sql, ColumnDesc[] columns)
        {
            string priKey = null;
            for (int i = 0; i < columns.Length; i++)
            {
                ColumnDesc col = columns[i];
                sql.Append(col.Column).Append(' ');
                sql.Append(col.FormatType());
                // When the primary key isn't autoincrement, we use the same form as mysql
                // Otherwise we have to use sqlite's 'PRIMARY KEY AUTO_INCREMENT' form
                if (col.PrimaryKey)
                {
                    if (!col.AutoIncrement) priKey = col.Column;
                    else sql.Append(" PRIMARY KEY");
                }
                if (col.AutoIncrement) sql.Append(" AUTOINCREMENT");
                if (col.NotNull) sql.Append(" NOT NULL");
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
        public void PrintSchema(string table, TextWriter w)
        {
            string sql = "SELECT sql from sqlite_master WHERE tbl_name = @0 AND type = 'table'";
            List<string> all = GetStrings(sql + " COLLATE NOCASE", table);
            for (int i = 0; i < all.Count; i++)
            {
                sql = all[i].Replace(" " + table, " `" + table + "`");
                sql = sql.Replace("CREATE TABLE `" + table + "`", "CREATE TABLE IF NOT EXISTS `" + table + "`");
                w.WriteLine(sql + ";");
            }
        }
        public string AddColumnSql(string table, ColumnDesc col) => "ALTER TABLE `" + table + "` ADD COLUMN " + col.Column + " " + col.FormatType();
        public string AddOrReplaceRowSql(string table, string columns, int numArgs) => InsertSql("INSERT OR REPLACE INTO", table, columns, numArgs);
    }
}
