﻿/*
    Copyright 2015 MCGalaxy
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Collections.Generic;

namespace MCGalaxy.Games {

    public class GameProps {
        
        /// <summary> Team the player is currently in. </summary>
        public Team Team;
        
        /// <summary> Last team the player was invited to. </summary>
        public string TeamInvite; 
        
        /// <summary> Whether the player has liked or disliked the map in this round. </summary>
        internal bool RatedMap = false; 
        
        /// <summary> Whether the player has pledged that they will survive/win in this round. </summary>
        internal bool PledgeSurvive = false;
        
        //CTF
        public CtfTeam team;
        public CtfTeam hasflag;

        //Zombie
        /// <summary> Whether this play is acting as a referee (spectator) in the game. </summary>
        public bool Referee = false;
        
        /// <summary> Remaining number of blocks the player can place this round. </summary>
        public int BlocksLeft = 50;
        
        /// <summary> Number of blocks the player has sequentially pillared up. </summary>
        internal int BlocksStacked = 0;
        internal int LastX, LastY, LastZ;
        
        /// <summary> Whether this player is currently infected/dead. </summary>
        public bool Infected = false;
        
        /// <summary> Point in time this player was infected at. </summary>
        public DateTime TimeInfected;
        
        /// <summary> Name of last player to infect this player, if any. </summary>
        public string LastInfecter;
        
        /// <summary> Whether the real names of zombies are always shown to the player. </summary>
        public bool Aka = false;
        
        /// <summary> Last name colour sent to other players from a call to GlobalSpawn. </summary>
        internal string lastSpawnColor = "";
        
        /// <summary> List of custom infect messages this player has. </summary>
        internal List<string> InfectMessages = null;
        
        /// <summary> Whether this player is currently using an invisibility potion. </summary>
        public bool Invisible;
        
        /// <summary> Point in time at which the invisibility potion expires. </summary>
        public DateTime InvisibilityEnd;
        
        /// <summary> Last 'invisible for X more seconds' time sent to the player. </summary>
        public int InvisibilityTime = -1;
        
        /// <summary> Number of invisibility potions bought this round. </summary>
        public int InvisibilityPotions;
        
        /// <summary> Number of successful revives this round. </summary>
        public int RevivesUsed;    
        
        /// <summary> Resets all the invisibility variables back to default. </summary>
        public void ResetInvisibility() {
            Invisible = false;
            InvisibilityEnd = DateTime.MinValue;
            InvisibilityTime = -1;
        }
        
        /// <summary> Resets all the zombie game round variables back to default. </summary>
        public void ResetZombieState() {
            BlocksLeft = 50;
            CurrentInfected = 0;
            Infected = false;
            RatedMap = false;
            PledgeSurvive = false;
            InvisibilityPotions = 0;
            RevivesUsed = 0;
            TimeInfected = DateTime.MinValue;
            LastInfecter = null;
        }
        
        
        /// <summary> The total number of rounds this player has survived. </summary>
        public int TotalRoundsSurvived;
        
        /// <summary> The maximum number of rounds this player has consecutively survived. </summary>
        public int MaxRoundsSurvived;
        
        /// <summary> The current number of rounds this player has consecutively survived. </summary>
        public int CurrentRoundsSurvived;
        
        /// <summary> The total number of other players this player has infected. </summary>
        public int TotalInfected;
        
        /// <summary> Maximum number of players this player infected in the current round. </summary>
        public int MaxInfected;
        
        /// <summary> The current number of other players this player infected in the current round. </summary>
        public int CurrentInfected;
    }
}
