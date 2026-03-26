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
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public sealed partial class Server
    {
        public static bool cancelcommand, SetupFinished,
            chatmod, flipHead, shuttingDown, voting;
        public static ServerConfig Config = new();
        public static DateTime StartTime;
        public static PlayerExtList AutoloadMaps, models, skins,
            reach, rotations, modelScales, bannedIP,
            frozen, muted, tempBans, tempRanks;
        public static PlayerMetaList RankInfo = new("text/rankinfo.txt"),
            Notes = new("text/notes.txt");
        public static PlayerList whiteList, invalidIds, ignored,
            hidden, agreed, vip, noEmotes, lockdown, reviewlist = new();
        public const string InternalVersion = "1.0.7.9";
        public static string Version => InternalVersion;
        public static string SoftwareName = "MCGalaxy-NAS (Standalone)";
        public static string fullName;
        public static string SoftwareNameVersioned
        {
            get { return fullName ?? SoftwareName + " " + Version; }
            set { fullName = value; }
        }
        public static INetListen Listener = new();
        public static readonly List<string> Devs = new()
        {
            "Hetal", "UclCommander"
        },
        Opstats = new()
        {
            "ban", "tempban", "xban", "banip", "kick",
            "warn", "mute", "freeze", "setrank"
        };
        public static Level mainLevel;
        public static string[] announcements = new string[0];
        public static ExtrasCollection Extras = new();
        public static int YesVotes, NoVotes;
        public static Scheduler MainScheduler = new("MCG_MainScheduler"),
            Background = new("MCG_BackgroundScheduler"),
            Critical = new("MCG_CriticalScheduler"),
            Heartbeats = new("MCG_HeartbeatsScheduler");
    }
}