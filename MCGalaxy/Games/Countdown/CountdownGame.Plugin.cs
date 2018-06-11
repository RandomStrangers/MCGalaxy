﻿/*
    Copyright 2011 MCForge
        
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;

namespace MCGalaxy.Games {
    
    public sealed partial class CountdownGame : RoundsGame {
        
        void HookEventHandlers() {
            OnPlayerMoveEvent.Register(HandlePlayerMove, Priority.High);
            OnPlayerDisconnectEvent.Register(HandlePlayerDisconnect, Priority.High);
            OnLevelUnloadEvent.Register(HandleLevelUnload, Priority.High);
            OnPlayerSpawningEvent.Register(HandlePlayerSpawning, Priority.High);
            OnJoinedLevelEvent.Register(HandleOnJoinedLevel, Priority.High);
        }
        
        void UnhookEventHandlers() {
            OnPlayerMoveEvent.Unregister(HandlePlayerMove);
            OnPlayerDisconnectEvent.Unregister(HandlePlayerDisconnect);
            OnLevelUnloadEvent.Unregister(HandleLevelUnload);
            OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
            OnJoinedLevelEvent.Unregister(HandleOnJoinedLevel);
        }
        
        
        void HandlePlayerMove(Player p, Position next, byte yaw, byte pitch) {
            if (!RoundInProgress || !FreezeMode) return;
            if (!Remaining.Contains(p)) return;
            
            int freezeX = p.Extras.GetInt("MCG_CD_X");
            int freezeZ = p.Extras.GetInt("MCG_CD_Z");
            if (next.X != freezeX || next.Z != freezeZ) {
                next.X = freezeX; next.Z = freezeZ;
                p.SendPos(Entities.SelfID, next, new Orientation(yaw, pitch));
            }
            
            p.Pos = next;
            p.SetYawPitch(yaw, pitch);
            p.cancelmove = true;
        }
        
        void HandlePlayerDisconnect(Player p, string reason) {
            if (!Players.Contains(p)) return;           
            PlayerLeftGame(p);
        }
        
        void HandlePlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning) {
            if (!respawning || !Remaining.Contains(p)) return;
            Map.Message(p.ColoredName + " %Sis out of countdown!");
            Remaining.Remove(p);
            UpdatePlayersLeft();
        }
        
        void HandleOnJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce) {
            if (prevLevel == Map && level != Map) { PlayerLeftGame(p); }
        }
    }
}