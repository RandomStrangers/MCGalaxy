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
///------|        them in the sidebar!!     |------\\\
//-------|__________________________________|-------\\
using MCGalaxy.Events.EntityEvents;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;
using MCGalaxy.Maths;
namespace MCGalaxy.Modules.Games.TW
{
    public partial class TWGame : RoundsGame
    {
        protected override void HookEventHandlers()
        {
            OnPlayerChatEvent.Register(HandlePlayerChat, 2);
            OnPlayerSpawningEvent.Register(HandlePlayerSpawning, 2);
            OnSentMapEvent.Register(HandleSentMap, 2);
            OnJoinedLevelEvent.Register(HandleJoinedLevel, 2);
            OnTabListEntryAddedEvent.Register(HandleTabListEntryAdded, 2);
            OnSettingColorEvent.Register(HandleSettingColor, 2);
            OnBlockHandlersUpdatedEvent.Register(HandleBlockHandlersUpdated, 2);
            base.HookEventHandlers();
        }
        protected override void UnhookEventHandlers()
        {
            OnPlayerChatEvent.Unregister(HandlePlayerChat);
            OnPlayerSpawningEvent.Unregister(HandlePlayerSpawning);
            OnSentMapEvent.Unregister(HandleSentMap);
            OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
            OnTabListEntryAddedEvent.Unregister(HandleTabListEntryAdded);
            OnSettingColorEvent.Unregister(HandleSettingColor);
            OnBlockHandlersUpdatedEvent.Unregister(HandleBlockHandlersUpdated);
            base.UnhookEventHandlers();
        }
        void HandlePlayerChat(Player p, string message)
        {
            if (p.Level != Map || message.Length == 0 || message[0] != ':') return;
            TWTeam team = TeamOf(p);
            if (team == null || Config.Mode != 1) return;
            message = message.Substring(1);
            Chat.MessageChat(2, p, team.Color + " - to " + team.Name + " - λNICK: &f" + message,
                             Map, (pl, arg) => pl.Game.Referee || TeamOf(pl) == team);
            p.cancelchat = true;
        }
        void HandlePlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning)
        {
            if (p.Level == Map)
            {
                TWData data = Get(p);
                if (respawning)
                {
                    data.Health = 2;
                    data.KillStreak = 0;
                    data.ScoreMultiplier = 1f;
                    data.LastKillStreakAnnounced = 0;
                }
                TWTeam team = TeamOf(p);
                if (team == null || Config.Mode != 1) return;
                Vec3U16 coords = team.SpawnPos;
                pos = Position.FromFeetBlockCoords(coords.X, coords.Y, coords.Z);
            }
        }
        void HandleTabListEntryAdded(Entity entity, ref string tabName, ref string tabGroup, Player dst)
        {
            if (entity is not Player p || p.Level != Map) return;
            TWTeam team = TeamOf(p);
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
        void HandleSettingColor(Player p, ref string color)
        {
            if (p.Level == Map)
            {
                TWTeam team = TeamOf(p);
                if (team != null) color = team.Color;
            }
        }
        void HandleSentMap(Player p, Level prevLevel, Level level)
        {
            if (level == Map)
            {
                OutputMapSummary(p, Map.name, Map.Config);
                if (TeamOf(p) == null) AutoAssignTeam(p);
            }
        }
        void HandleJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce)
        {
            HandleJoinedCommon(p, prevLevel, level, ref announce);
            if (level == Map) allPlayers.Add(p);
        }
    }
}
