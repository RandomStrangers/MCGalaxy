/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    public sealed partial class Server
    {
        static readonly ColumnDesc[] playersTable = new ColumnDesc[]
        {
            new("ID", ColumnType.Integer, priKey: true, autoInc: true, notNull: true),
            new("Name", ColumnType.VarChar, 17),
            new("IP", ColumnType.Char, 15),
            new("FirstLogin", ColumnType.DateTime),
            new("LastLogin", ColumnType.DateTime),
            new("totalLogin", ColumnType.Int24),
            new("Title", ColumnType.Char, 20),
            new("TotalDeaths", ColumnType.Int16),
            new("Money", ColumnType.UInt24),
            new("totalBlocks", ColumnType.Int64),
            new("totalCuboided", ColumnType.Int64),
            new("totalKicked", ColumnType.Int24),
            new("TimeSpent", ColumnType.VarChar, 20),
            new("color", ColumnType.VarChar, 6),
            new("title_color", ColumnType.VarChar, 6),
            new("Messages", ColumnType.UInt24),
        },
        opstatsTable = new ColumnDesc[]
        {
            new("ID", ColumnType.Integer, priKey: true, autoInc: true, notNull: true),
            new("Time", ColumnType.DateTime),
            new("Name", ColumnType.VarChar, 17),
            new("Cmd", ColumnType.VarChar, 40),
            new("Cmdmsg", ColumnType.VarChar, 40),
        };
        static void InitDatabase()
        {
            if (!Directory.Exists("blockdb"))
            {
                Directory.CreateDirectory("blockdb");
            }
            Logger.Log(LogType.SystemActivity, "Using SQLite for database backend");
            Database.CreateTable("Opstats", opstatsTable);
            Database.CreateTable("Players", playersTable);
            //since MCForge 5.5.11 we are cleaning up the table Playercmds
            //if Playercmds exists copy-filter to Opstats and remove Playercmds
            if (Database.TableExists("Playercmds"))
            {
                const string sql = "INSERT INTO Opstats (Time, Name, Cmd, Cmdmsg) SELECT Time, Name, Cmd, Cmdmsg FROM Playercmds WHERE {0};";
                foreach (string cmd in Opstats)
                {
                    Database.Execute(string.Format(sql, "cmd = '" + cmd + "'"));
                }
                Database.Execute(string.Format(sql, "cmd = 'review' AND cmdmsg = 'next'"));
                Database.DeleteTable("Playercmds");
            }
            List<string> columns = SQLiteBackend.Instance.ColumnNames("Players");
            if (columns.Count == 0)
            {
                return;
            }
            if (!columns.CaselessContains("Color"))
            {
                Database.AddColumn("Players", new("color", ColumnType.VarChar, 6));
            }
            if (!columns.CaselessContains("Title_Color"))
            {
                Database.AddColumn("Players", new("title_color", ColumnType.VarChar, 6));
            }
            if (!columns.CaselessContains("TimeSpent"))
            {
                Database.AddColumn("Players", new("TimeSpent", ColumnType.VarChar, 20));
            }
            if (!columns.CaselessContains("TotalCuboided"))
            {
                Database.AddColumn("Players", new("totalCuboided", ColumnType.Int64));
            }
            if (!columns.CaselessContains("Messages"))
            {
                Database.AddColumn("Players", new("Messages", ColumnType.UInt24));
            }
        }
    }
}
