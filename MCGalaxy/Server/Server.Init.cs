/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Generator;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
namespace MCGalaxy
{
    public partial class Server
    {
        public static void LoadMainLevel(SchedulerTask _)
        {
            try
            {
                mainLevel = LevelActions.Load(Player.NASConsole, Config.MainLevel, false);
                if (mainLevel == null)
                {
                    Logger.Log(LogType.SystemActivity, "main level not found, generating..");
                    mainLevel = new(Config.MainLevel, 128, 64, 128);
                    MapGen.Find("Flat").Generate(Player.NASConsole, mainLevel, "");
                    mainLevel.Save();
                    Level.LoadMetadata(mainLevel);
                    LevelInfo.Add(mainLevel);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading main level", ex);
            }
        }
        public static void LoadAllPlugins(SchedulerTask _)
        {
            Plugin.LoadAll();
            OnPluginsLoadedEvent.Call();
        }
        public static void LoadNAS(SchedulerTask _) => NAS.Load();
        public static void InitPlayerLists(SchedulerTask _)
        {
            LoadPlayerLists();
            ModerationTasks.QueueTasks();
        }
        public static void LoadPlayerLists()
        {
            agreed = PlayerList.Load("ranks/agreed.txt");
            invalidIds = PlayerList.Load("extra/invalidids.txt");
            Player.NASConsole.DatabaseID = NameConverter.InvalidNameID("(NAS)");
            hidden = PlayerList.Load("ranks/hidden.txt");
            vip = PlayerList.Load("text/vip.txt");
            noEmotes = PlayerList.Load("text/emotelist.txt");
            lockdown = PlayerList.Load("text/lockdown.txt");
            models = PlayerExtList.Load("extra/models.txt");
            skins = PlayerExtList.Load("extra/skins.txt");
            reach = PlayerExtList.Load("extra/reach.txt");
            rotations = PlayerExtList.Load("extra/rotations.txt");
            modelScales = PlayerExtList.Load("extra/modelscales.txt");
            bannedIP = PlayerExtList.Load("ranks/banned-ip.txt");
            muted = PlayerExtList.Load("ranks/muted.txt");
            frozen = PlayerExtList.Load("ranks/frozen.txt");
            tempRanks = PlayerExtList.Load(Paths.TempRanksFile);
            tempBans = PlayerExtList.Load(Paths.TempBansFile);
            whiteList = PlayerList.Load("ranks/whitelist.txt");
        }
        public static void LoadAutoloadMaps(SchedulerTask _)
        {
            AutoloadMaps = PlayerExtList.Load("text/autoload.txt", '=');
            List<string> maps = AutoloadMaps.AllNames();
            foreach (string map in maps)
            {
                if (map.CaselessEq(Config.MainLevel))
                    continue;
                LevelActions.Load(Player.NASConsole, map, false);
            }
        }
        public static void SetupSocket(SchedulerTask _)
        {
            if (!IPAddress.TryParse(Config.ListenIP, out IPAddress ip))
            {
                Logger.Log(LogType.Warning, "Unable to parse listen IP config key, listening on any IP");
                ip = IPAddress.Any;
            }
            Listener.Listen(ip, Config.Port);
        }
        public static void InitHeartbeat(SchedulerTask _) => Heartbeat.Start();
        public static void InitTimers(SchedulerTask _)
        {
            MainScheduler.QueueRepeat(RandomMessage, null,
                                      Config.AnnouncementInterval);
            Critical.QueueRepeat(ServerTasks.UpdateEntityPositions, null,
                                 TimeSpan.FromMilliseconds(Config.PositionUpdateInterval));
        }
        public static void InitRest(SchedulerTask _)
        {
            MainScheduler.QueueRepeat(BlockQueue.Loop, null,
                                      TimeSpan.FromMilliseconds(BlockQueue.Interval));
            Critical.QueueRepeat(ServerTasks.TickPlayers, null,
                                 TimeSpan.FromMilliseconds(20));
            Logger.Log(LogType.SystemActivity, "Finished setting up server");
            SetupFinished = true;
        }
    }
}