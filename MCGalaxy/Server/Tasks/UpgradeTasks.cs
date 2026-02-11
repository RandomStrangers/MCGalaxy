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
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy.Tasks
{
    internal static class UpgradeTasks
    {
        internal static void UpgradeOldAgreed()
        {
            if (!File.Exists("ranks/agreed.txt"))
            {
                return;
            }
            string data = null;
            using (FileStream fs = FileIO.TryOpenRead("ranks/agreed.txt"))
            {
                if (fs.ReadByte() != ' ')
                {
                    return;
                }
                data = new StreamReader(fs).ReadToEnd();
                data = data.Replace(" ", Environment.NewLine);
            }
            FileIO.TryWriteAllText("ranks/agreed.txt", data);
        }
        internal static void UpgradeOldTempranks(SchedulerTask _)
        {
            if (!File.Exists(Paths.TempRanksFile))
            {
                return;
            }
            using (StreamReader r = new(Paths.TempRanksFile))
            {
                string line = r.ReadLine();
                if (line == null)
                {
                    return;
                }
                string[] parts = line.SplitSpaces();
                if (parts.Length < 9)
                {
                    return;
                }
            }
            string[] lines = FileIO.TryReadAllLines(Paths.TempRanksFile);
            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].SplitSpaces();
                if (args.Length < 9)
                {
                    continue;
                }
                int min = NumberUtils.ParseInt32(args[4]),
                    hour = NumberUtils.ParseInt32(args[5]),
                    day = NumberUtils.ParseInt32(args[6]),
                    month = NumberUtils.ParseInt32(args[7]),
                    year = NumberUtils.ParseInt32(args[8]),
                    periodH = NumberUtils.ParseInt32(args[3]),
                    periodM = 0;
                if (args.Length > 10)
                {
                    periodM = NumberUtils.ParseInt32(args[10]);
                }
                DateTime assigned = new(year, month, day, hour, min, 0),
                    expiry = assigned.AddHours(periodH).AddMinutes(periodM);
                lines[i] = args[0] + " " + args[9] + " " + assigned.ToUnixTime() +
                    " " + expiry.ToUnixTime() + " " + args[2] + " " + args[1];
            }
            FileIO.TryWriteAllLines(Paths.TempRanksFile, lines);
        }
        internal static void UpgradeDBTimeSpent(SchedulerTask _)
        {
            string time = Database.ReadString("Players", "TimeSpent", "LIMIT 1");
            if (time == null)
            {
                return;
            }
            if (time.IndexOf(' ') == -1)
            {
                return;
            }
            Logger.Log(LogType.SystemActivity, "Upgrading TimeSpent column in database to new format..");
            DumpPlayerTimeSpents();
            UpgradePlayerTimeSpents();
            Logger.Log(LogType.SystemActivity, "Upgraded {0} rows. ({1} rows failed)", playerCount, playerFailed);
        }
        static List<int> playerIds;
        static List<long> playerSeconds;
        static int playerCount, playerFailed;
        static void DumpPlayerTimeSpents()
        {
            playerIds = new();
            playerSeconds = new();
            Database.ReadRows("Players", "ID,TimeSpent", ReadTimeSpent);
        }
        static void ReadTimeSpent(ISqlRecord record)
        {
            playerCount++;
            try
            {
                int id = record.GetInt32(0);
                TimeSpan span = Database.ParseOldDBTimeSpent(record.GetString(1));
                playerIds.Add(id);
                playerSeconds.Add((long)span.TotalSeconds);
            }
            catch
            {
                playerFailed++;
            }
        }
        static void UpgradePlayerTimeSpents()
        {
            using SqlTransaction bulk = new();
            for (int i = 0; i < playerIds.Count; i++)
            {
                bulk.Execute("UPDATE Players SET TimeSpent=@time WHERE ID=@pid",
                             new SqlArgument("@pid", playerIds[i]),
                             new SqlArgument("@time", playerSeconds[i]));
            }
            bulk.Commit();
        }
    }
}