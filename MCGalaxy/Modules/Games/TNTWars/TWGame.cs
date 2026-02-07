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
///////--|----------------------------------|--\\\\\\\
//////---|  TNT WARS - Coded by edh649      |---\\\\\\
/////----|                                  |----\\\\\
////-----|  Note: Double click on // to see |-----\\\\
using MCGalaxy.Blocks.Physics;
using MCGalaxy.Games;
using MCGalaxy.Maths;
///------|        them in the sidebar!!     |------\\\
//-------|__________________________________|-------\\
using System;
using System.Collections.Generic;
namespace MCGalaxy.Modules.Games.TW
{
    public sealed class PlayerAndScore { public Player p; public int Score; }
    internal sealed class TWData
    {
        public int Score, Health = 2, KillStreak, TNTCounter;
        public float ScoreMultiplier = 1f;
        public int LastKillStreakAnnounced;
        public Player HarmedBy; // For Assists
        public void Reset(int diff)
        {
            Score = 0;
            Health = (diff == 0 || diff == 1) ? 2 : 1;
            KillStreak = 0;
            LastKillStreakAnnounced = 0;
            TNTCounter = 0;
            ScoreMultiplier = 1f;
            HarmedBy = null;
        }
    }
    sealed class TWTeam
    {
        public string Name, Color;
        public string ColoredName => Color + Name;
        public int Score;
        public Vec3U16 SpawnPos;
        public VolatileArray<Player> Members = new();
        public TWTeam(string name, string color) { Name = name; Color = color; }
    }
    public partial class TWGame : RoundsGame
    {
        TWMapConfig cfg = new();
        public TWConfig Config = new();
        public override string GameName => "TNT Wars";
        public override RoundsGameConfig GetConfig() => Config;
        protected override string WelcomeMessage => "&4TNT Wars &Sis running! Type &T/TW go &Sto join";
        readonly TWTeam Red = new("Red", Colors.red);
        readonly TWTeam Blue = new("Blue", Colors.blue);
        public List<TWZone> tntFreeZones = new();
        public List<TWZone> tntImmuneZones = new();
        readonly VolatileArray<Player> allPlayers = new();
        TNTImmuneFilter tntImmuneFilter;
        public static TWGame Instance = new();
        public TWGame() => Picker = new SimpleLevelPicker();
        static TWData Get(Player p)
        {
            TWData data = TryGet(p);
            if (data != null) return data;
            data = new();
            // TODO: Is this even thread-safe
            p.Extras["MCG_TW_DATA"] = data;
            return data;
        }
        static TWData TryGet(Player p)
        {
            p.Extras.TryGet("MCG_TW_DATA", out object data); return (TWData)data;
        }
        public override void UpdateMapConfig()
        {
            TWMapConfig cfg = new();
            cfg.SetDefaults(Map);
            cfg.Load(Map.name);
            this.cfg = cfg;
            Red.SpawnPos = cfg.RedSpawn;
            Blue.SpawnPos = cfg.BlueSpawn;
            if (Running)
            {
                UpdateAllStatus1();
                UpdateAllStatus2();
            }
        }
        protected override List<Player> GetPlayers()
        {
            List<Player> playing = new();
            playing.AddRange(Red.Members.Items);
            playing.AddRange(Blue.Members.Items);
            return playing;
        }
        public override void OutputStatus(Player p)
        {
            if (Config.Mode == 1)
            {
                p.Message("{0} team score: &f{1}/{2} points",
                               Red.ColoredName, Red.Score, cfg.ScoreRequired);
                p.Message("{0} team score: &f{1}/{2} points",
                               Blue.ColoredName, Blue.Score, cfg.ScoreRequired);
            }
            p.Message("Your score: &f{0}/{1} &Spoints, health: &f{2} &SHP",
                           Get(p).Score, cfg.ScoreRequired, Get(p).Health);
        }
        protected override void StartGame()
        {
            ResetTeams();
            tntImmuneFilter = (x, y, z) => InZone(x, y, z, tntImmuneZones);
        }
        protected override void EndGame()
        {
            RestoreBuildPerms();
            ResetTeams();
            // Reset block handlers
            UpdateBlockHandlers();
            Map.UpdateBlockProps();
        }
        void ResetTeams()
        {
            Blue.Members.Clear();
            Red.Members.Clear();
            Blue.Score = 0;
            Red.Score = 0;
            Player[] players = allPlayers.Items;
            foreach (Player pl in players)
            {
                RestoreColor(pl);
            }
        }
        public override void PlayerJoinedGame(Player p)
        {
            bool announce = false;
            HandleSentMap(p, Map, Map);
            HandleJoinedLevel(p, Map, Map, ref announce);
        }
        public override void PlayerLeftGame(Player p)
        {
            allPlayers.Remove(p);
            TWTeam team = TeamOf(p);
            if (team != null)
            {
                team.Members.Remove(p);
                RestoreColor(p);
            }
        }
        void RestoreColor(Player p)
        {
            TWData data = TryGet(p);
            // TODO: p.Socket.Disconnected check should be elsewhere
            if (data == null || p.Socket.Disconnected) return;
            p.UpdateColor(PlayerInfo.DefaultColor(p));
            TabList.Update(p, true);
        }
        void JoinTeam(Player p, TWTeam team)
        {
            team.Members.Add(p);
            Map.Message(p.ColoredName + " &Sjoined the " + team.ColoredName + " &Steam");
            p.UpdateColor(team.Color);
            p.Message("You are now on the " + team.ColoredName + " team!");
            TabList.Update(p, true);
        }
        TWTeam TeamOf(Player p)
        {
            if (Red.Members.Contains(p)) return Red;
            if (Blue.Members.Contains(p)) return Blue;
            return null;
        }
        public void ModeTDM()
        {
            Config.Mode = 1;
            MessageMap(100,
                       "&4Gamemode changed to &fTeam Deathmatch");
            Player[] players = allPlayers.Items;
            foreach (Player pl in players)
            {
                string msg = pl.ColoredName + " &Sis now on the ";
                AutoAssignTeam(pl);
                // assigning team changed colour of player
                msg += TeamOf(pl).ColoredName + " team";
                Map.Message(msg);
            }
            Config.Save();
        }
        public void ModeFFA()
        {
            Config.Mode = 0;
            MessageMap(100,
                       "&4Gamemode changed to &fFree For All");
            ResetTeams();
            Config.Save();
        }
        public void SetDifficulty(int diff)
        {
            Config.Difficulty = diff;
            MessageMap(100,
                       "&4Difficulty changed to &f" + diff);
            Config.Save();
            bool teamKill = diff >= 2;
            if (cfg.TeamKills != teamKill)
            {
                cfg.TeamKills = teamKill;
                // TODO rethink this
                if (Map != null) cfg.Save(Map.name);
            }
        }
        public class TWZone
        {
            public ushort MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
            public TWZone(Vec3U16 p1, Vec3U16 p2)
            {
                MinX = Math.Min(p1.X, p2.X);
                MinY = Math.Min(p1.Y, p2.Y);
                MinZ = Math.Min(p1.Z, p2.Z);
                MaxX = Math.Max(p1.X, p2.X);
                MaxY = Math.Max(p1.Y, p2.Y);
                MaxZ = Math.Max(p1.Z, p2.Z);
            }
            public string DescribeBounds() => " &b- (" + MinX + ", " + MinY + ", " + MinZ +
                    ") to (" + MaxX + ", " + MaxY + ", " + MaxZ + ")";
        }
        public bool InZone(ushort x, ushort y, ushort z, List<TWZone> zones)
        {
            foreach (TWZone Zn in zones)
            {
                if (x >= Zn.MinX && y >= Zn.MinY && z >= Zn.MinZ
                    && x <= Zn.MaxX && y <= Zn.MaxY && z <= Zn.MaxZ) return true;
            }
            return false;
        }
        void AutoAssignTeam(Player p)
        {
            if (Blue.Members.Count > Red.Members.Count)
            {
                JoinTeam(p, Red);
            }
            else if (Red.Members.Count > Blue.Members.Count)
            {
                JoinTeam(p, Blue);
            }
            else if (Red.Score > Blue.Score)
            {
                JoinTeam(p, Blue);
            }
            else if (Blue.Score > Red.Score)
            {
                JoinTeam(p, Blue);
            }
            else
            {
                JoinTeam(p, new Random().Next(2) == 0 ? Red : Blue);
            }
        }
        public PlayerAndScore[] SortedByScore()
        {
            Player[] all = allPlayers.Items;
            PlayerAndScore[] sorted = new PlayerAndScore[all.Length];
            for (int i = 0; i < all.Length; i++)
            {
                PlayerAndScore entry = new()
                {
                    p = all[i]
                };
                entry.Score = Get(entry.p).Score;
                sorted[i] = entry;
            }
            Array.Sort(sorted, (a, b) => b.Score.CompareTo(a.Score));
            return sorted;
        }
        public string FormatTopScore(PlayerAndScore[] top, int i)
        {
            string col = "&f";
            PlayerAndScore p = top[i];
            if (i == 0) col = "&6";
            if (i == 1) col = "&7";
            if (i == 2) col = "&4";
            return string.Format("{0}) {2} - {1}{3} points", i + 1, col,
                                 p.p.ColoredName, p.Score);
        }
        public void ChangeScore(Player p, int amount)
        {
            Get(p).Score += amount;
            UpdateStatus2(p);
            if (Config.Mode == 1)
            {
                TWTeam team = TeamOf(p);
                if (team != null)
                {
                    team.Score += amount;
                    UpdateAllStatus1();
                }
            }
        }
        bool TeamKill(Player p1, Player p2) => Config.Mode == 1 && TeamOf(p1) == TeamOf(p2);
        protected override string FormatStatus1(Player p)
        {
            if (Config.Mode != 1) return "";
            return Red.ColoredName + ": &f" + Red.Score + "/" + cfg.ScoreRequired + ", "
                + Blue.ColoredName + ": &f" + Blue.Score + "/" + cfg.ScoreRequired;
        }
        protected override string FormatStatus2(Player p) => "&aHealth: &f" + Get(p).Health + " HP, &eScore: &f"
                + Get(p).Score + "/" + cfg.ScoreRequired + " points";
    }
}
