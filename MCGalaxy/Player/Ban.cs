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
using System;
using System.Text;
namespace MCGalaxy
{
    /// <summary> Retrieves or updates a user's ban/unban information. </summary>
    /// <remarks> This is NOT the list of banned players (ranks/banned.txt) </remarks>
    public static class Ban
    {
        public static readonly PlayerMetaList bans = new("text/bans.txt"),
            unbans = new("text/unbans.txt");
        public static void EnsureExists()
        {
            bans.EnsureExists();
            unbans.EnsureExists();
        }
        public static string PackTempBanData(string reason, string banner, DateTime expiry) => banner + " " + expiry.ToUnixTime() + " " + (reason ??= "-");
        public static void UnpackTempBanData(string line, out string reason, out string banner, out DateTime expiry)
        {
            string[] parts = line.SplitSpaces(3);
            banner = Server.FromRawUsername(parts[0]);
            long timestamp = long.Parse(parts[1]);
            try
            {
                expiry = timestamp.FromUnixTime();
            }
            catch (ArgumentOutOfRangeException)
            {
                expiry = new(long.Parse(parts[1]), DateTimeKind.Utc);
            }
            reason = parts.Length > 2 ? parts[2] : "";
        }
        /// <summary> Adds a ban entry for the given user (who banned them and why they were banned) </summary>
        public static void BanPlayer(Player banner, string target, string reason, bool stealth, string oldrank)
        {
            if (reason.Length == 0) reason = Server.Config.DefaultBanMessage;
            reason = reason.Replace(" ", "%20");
            AddBanEntry(banner.name, target.ToLower(), reason, stealth, oldrank);
        }
        /// <summary> Adds an unban entry for the given user (who unbanned them and why they were unbanned) </summary>
        public static void UnbanPlayer(Player unbanner, string target, string reason)
        {
            if (reason.Length == 0) reason = "(none given)";
            reason = reason.Replace(" ", "%20");
            AddUnbanEntry(unbanner.name, target.ToLower(), reason);
        }
        public static void AddBanEntry(string pl, string target, string reason, bool stealth, string oldrank) => bans.Append(pl + " " + target + " " + reason + " " + stealth + " " + DateTime.UtcNow.ToUnixTime().ToString() + " " + oldrank);
        public static void AddUnbanEntry(string pl, string target, string reason) => unbans.Append(pl + " " + target + " " + reason + " " + DateTime.UtcNow.ToUnixTime().ToString());
        /// <summary> Returns info about the current or last ban of a user. </summary>
        public static void GetBanData(string who, out string banner, out string reason,
                                      out DateTime time, out string prevRank)
        {
            who = who.ToLower();
            foreach (string line in FileIO.TryReadAllLines(bans.file))
            {
                string[] parts = line.SplitSpaces();
                if (parts.Length <= 5 || parts[1] != who) continue;
                banner = Server.FromRawUsername(parts[0]);
                reason = parts[2].Replace("%20", " ");
                time = GetDate(parts[4]);
                prevRank = parts[5];
                return;
            }
            banner = null;
            reason = null; 
            time = DateTime.MinValue;
            prevRank = null;
        }
        /// <summary> Returns information about the last unban of a user. </summary>
        public static void GetUnbanData(string who, out string unbanner, out string reason,
                                        out DateTime time)
        {
            who = who.ToLower();
            unbanner = null; 
            reason = null;
            foreach (string line in FileIO.TryReadAllLines(unbans.file))
            {
                string[] parts = line.SplitSpaces();
                if (parts.Length <= 3 || parts[1] != who) continue;
                unbanner = Server.FromRawUsername(parts[0]);
                reason = parts[2].Replace("%20", " ");
                time = GetDate(parts[3]);
                return;
            }
            unbanner = null; 
            reason = null; 
            time = DateTime.MinValue;
        }
        public static DateTime GetDate(string raw)
        {
            raw = raw.Replace("%20", " ").Replace(",", "");
            if (long.TryParse(raw, out long timestap)) return timestap.FromUnixTime();
            string[] date = raw.SplitSpaces(),
                minuteHour = date[5].Split(':');
            int hour = NumberUtils.ParseInt32(minuteHour[0]),
                min = NumberUtils.ParseInt32(minuteHour[1]),
                day = NumberUtils.ParseInt32(date[1]),
                month = NumberUtils.ParseInt32(date[2]),
                year = NumberUtils.ParseInt32(date[3]);
            return new DateTime(year, month, day, hour, min, 0).ToUniversalTime();
        }
        public static bool DeleteBan(string name) => DeleteInfo(name, bans);
        public static bool DeleteUnban(string name) => DeleteInfo(name, unbans);
        public static bool DeleteInfo(string name, PlayerMetaList list)
        {
            name = name.ToLower();
            bool found = false;
            StringBuilder sb = new();
            foreach (string line in FileIO.TryReadAllLines(list.file))
            {
                string[] parts = line.SplitSpaces();
                if (parts.Length > 1 && parts[1] == name)
                    found = true;
                else
                    sb.AppendLine(line);
            }
            if (found) FileIO.TryWriteAllText(list.file, sb.ToString());
            return found;
        }
        public static bool ChangeBanReason(string who, string reason) => ChangeReason(who, reason, bans);
        public static bool ChangeReason(string who, string reason, PlayerMetaList list)
        {
            who = who.ToLower();
            reason = reason.Replace(" ", "%20");
            bool found = false;
            StringBuilder sb = new();
            foreach (string line in FileIO.TryReadAllLines(list.file))
            {
                string[] parts = line.SplitSpaces();
                if (parts.Length > 2 && parts[1] == who)
                {
                    found = true;
                    parts[2] = reason;
                    sb.AppendLine(string.Join(" ", parts));
                }
                else
                    sb.AppendLine(line);
            }
            if (found) FileIO.TryWriteAllText(list.file, sb.ToString());
            return found;
        }
    }
}
