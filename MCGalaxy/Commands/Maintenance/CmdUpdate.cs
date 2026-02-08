/*
    Copyright 2011 MCForge
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
using System;
using System.IO;
using System.Net;
namespace MCGalaxy.Commands.Maintenance
{
    public class CmdUpdate : Command
    {
        public override string Name => "Update";
        public override string Type => CommandTypes.Moderation;
        public override LevelPermission DefaultRank => LevelPermission.Owner;
        public override void Use(Player p, string message)
        {
            if (message.CaselessEq("check"))
            {
                p.Message("Checking for updates..");
                string latest = new WebClient().DownloadString("https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/NAS/Uploads/version.txt");
                bool needsUpdate = true;
                if (!string.IsNullOrEmpty(latest))
                {
                    needsUpdate = new Version(latest) > new Version(Server.Version);
                }
                p.Message("Server {0}", needsUpdate ? "&cneeds updating" : "&ais up to date");
                if (needsUpdate)
                {
                    p.Message("Current version: {0}.", Server.Version);
                    if (!string.IsNullOrEmpty(latest))
                    {
                        p.Message("Latest version: {0}.", latest);
                    }
                }
            }
            else if (message.CaselessEq("latest"))
            {
                if (Environment.Version.Major == 4)
                {
                    try
                    {
                        try
                        {
                            FileIO.TryDelete("MCGalaxy.update");
                            FileIO.TryDelete("Prev_MCGalaxy.exe");
                        }
                        catch
                        {
                        }
                        Logger.Log(LogType.SystemActivity, "Downloading NAS update files");
                        WebClient client = new();
                        Logger.Log(LogType.SystemActivity, "Downloading {0} to {1}",
                            "https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/NAS/Uploads/MCGalaxy.exe", Path.GetFileName("MCGalaxy.update"));
                        client.DownloadFile("https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/NAS/Uploads/MCGalaxy.exe", "MCGalaxy.update");
                        Server.SaveAllLevels();
                        Player[] players = PlayerInfo.Online.Items;
                        foreach (Player pl in players)
                        {
                            pl.SaveStats();
                        }
                        FileIO.TryMove("MCGalaxy.exe", "Prev_MCGalaxy.exe");
                        FileIO.TryMove("MCGalaxy.update", "MCGalaxy.exe");
                        Server.Stop(true, "Updating server.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Error performing update", ex);
                    }
                }
            }
            else
            {
                Help(p);
            }
        }
        public override void Help(Player p)
        {
            p.Message("&T/Update check");
            p.Message("&HChecks whether the server needs updating");
            p.Message("&T/Update latest");
            p.Message("&HUpdates the server to the latest build");
        }
    }
}