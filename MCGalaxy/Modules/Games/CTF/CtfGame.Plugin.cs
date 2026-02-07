/*
    Copyright 2011 MCForge
    Written by fenderrock87
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
using MCGalaxy.Events.EntityEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using System;
namespace MCGalaxy.Modules.Games.CTF
{
    public partial class CTFGame : RoundsGame
    {
        protected override void HookEventHandlers()
        {
            OnPlayerDiedEvent.Register(HandlePlayerDied, 2);
            OnPlayerChatEvent.Register(HandlePlayerChat, 2);
            OnPlayerCommandEvent.Register(HandlePlayerCommand, 2);
            OnBlockChangingEvent.Register(HandleBlockChanging, 2);
            OnPlayerSpawningEvent.Register(HandlePlayerSpawning, 2);
            OnTabListEntryAddedEvent.Register(HandleTabListEntryAdded, 2);
            OnSentMapEvent.Register(HandleSentMap, 2);
            OnJoinedLevelEvent.Register(HandleJoinedLevel, 2);
            base.HookEventHandlers();
        }
        protected override void UnhookEventHandlers()
        {
            OnPlayerDiedEvent.Unregister(HandlePlayerDied);
            OnPlayerChatEvent.Unregister(HandlePlayerChat);
            OnPlayerCommandEvent.Unregister(HandlePlayerCommand);
            OnBlockChangingEvent.Unregister(HandleBlockChanging);
            OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
            OnTabListEntryAddedEvent.Unregister(HandleTabListEntryAdded);
            OnSentMapEvent.Unregister(HandleSentMap);
            OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
            base.UnhookEventHandlers();
        }
        void HandlePlayerDied(Player p, ushort deathblock, ref TimeSpan cooldown)
        {
            if (p.Level != Map || !Get(p).HasFlag) return;
            CtfTeam team = TeamOf(p);
            if (team != null) DropFlag(p, team);
        }
        void HandlePlayerChat(Player p, string message)
        {
            if (p.Level != Map || !Get(p).TeamChatting) return;
            CtfTeam team = TeamOf(p);
            if (team != null)
            {
                Chat.MessageChat(2, p, team.Color + " - to " + team.Name + " - λNICK: &f" + message,
                                 Map, (pl, arg) => pl.Game.Referee || TeamOf(pl) == team);
                p.cancelchat = true;
            }
        }
        void HandlePlayerCommand(Player p, string cmd, string args, CommandData data)
        {
            if (p.Level != Map || cmd != "teamchat") return;
            CtfData data_ = Get(p);
            if (data_.TeamChatting)
            {
                p.Message("You are no longer chatting with your team!");
            }
            else
            {
                p.Message("You are now chatting with your team!");
            }
            data_.TeamChatting = !data_.TeamChatting;
            p.cancelcommand = true;
        }
        void HandleBlockChanging(Player p, ushort x, ushort y, ushort z, ushort block, bool placing, ref bool cancel)
        {
            if (p.Level == Map)
            {
                CtfTeam team = TeamOf(p);
                if (team == null)
                {
                    p.RevertBlock(x, y, z);
                    cancel = true;
                    p.Message("You are not on a team!");
                    return;
                }
                if (x == Opposing(team).FlagPos.X && y == Opposing(team).FlagPos.Y && z == Opposing(team).FlagPos.Z && !Map.IsAirAt(x, y, z))
                {
                    TakeFlag(p, team);
                }
                if (x == team.FlagPos.X && y == team.FlagPos.Y && z == team.FlagPos.Z && !Map.IsAirAt(x, y, z))
                {
                    ReturnFlag(p, team);
                    cancel = true;
                }
            }
        }
        void HandlePlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning)
        {
            if (p.Level == Map)
            {
                CtfTeam team = TeamOf(p);
                if (team != null)
                {
                    if (respawning) DropFlag(p, team);
                    Vec3U16 coords = team.SpawnPos;
                    pos = Position.FromFeetBlockCoords(coords.X, coords.Y, coords.Z);
                }
            }
        }
        void HandleTabListEntryAdded(Entity entity, ref string tabName, ref string tabGroup, Player dst)
        {
            if (entity is not Player p || p.Level != Map) return;
            CtfTeam team = TeamOf(p);
            if (p.Game.Referee)
            {
                tabGroup = "&2Referees";
            }
            else if (team != null)
            {
                tabGroup = team.ColoredName + " team";
            }
            else
            {
                tabGroup = "&7Spectators";
            }
        }
        void HandleSentMap(Player p, Level _, Level level)
        {
            if (level == Map)
            {
                OutputMapSummary(p, Map.name, Map.Config);
                if (TeamOf(p) == null) AutoAssignTeam(p);
            }
        }
        void HandleJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce) => HandleJoinedCommon(p, prevLevel, level, ref announce);
    }
}
