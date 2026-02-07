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
using MCGalaxy.Events.EconomyEvents;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;
using System;
namespace MCGalaxy.Modules.Games.LS
{
    public partial class LSGame : RoundsGame
    {
        protected override void HookEventHandlers()
        {
            OnJoinedLevelEvent.Register(HandleJoinedLevel, 2);
            OnPlayerDyingEvent.Register(HandlePlayerDying, 2);
            OnPlayerDiedEvent.Register(HandlePlayerDied, 2);
            OnBlockHandlersUpdatedEvent.Register(HandleBlockHandlersUpdated, 2);
            OnBlockChangingEvent.Register(HandleBlockChanging, 2);
            OnMoneyChangedEvent.Register(HandleMoneyChanged, 2);
            base.HookEventHandlers();
        }
        protected override void UnhookEventHandlers()
        {
            OnJoinedLevelEvent.Unregister(HandleJoinedLevel);
            OnPlayerDyingEvent.Unregister(HandlePlayerDying);
            OnPlayerDiedEvent.Unregister(HandlePlayerDied);
            OnBlockHandlersUpdatedEvent.Unregister(HandleBlockHandlersUpdated);
            OnBlockChangingEvent.Unregister(HandleBlockChanging);
            OnMoneyChangedEvent.Unregister(HandleMoneyChanged);
            base.UnhookEventHandlers();
        }
        void HandleMoneyChanged(Player p)
        {
            if (p.Level == Map)
            {
                UpdateStatus1(p);
            }
        }
        void HandleJoinedLevel(Player p, Level prevLevel, Level level, ref bool announce)
        {
            HandleJoinedCommon(p, prevLevel, level, ref announce);
            if (Map == level)
            {
                ResetRoundState(p, Get(p)); // TODO: Check for /reload case?
                OutputMapSummary(p, Map.name, Map.Config);
                if (RoundInProgress) OutputStatus(p);
            }
        }
        void HandlePlayerDying(Player p, ushort block, ref bool cancel)
        {
            if (p.Level == Map && IsPlayerDead(p)) cancel = true;
        }
        void HandlePlayerDied(Player p, ushort block, ref TimeSpan cooldown)
        {
            if (p.Level != Map || IsPlayerDead(p)) return;
            cooldown = TimeSpan.FromSeconds(30);
            AddLives(p, -1, false);
        }
        void HandleBlockChanging(Player p, ushort x, ushort y, ushort z, ushort block, bool placing, ref bool cancel)
        {
            if (p.Level != Map || !(placing || p.painting)) return;
            if (Config.SpawnProtection && NearLavaSpawn(x, y, z))
            {
                p.Message("You can't place blocks so close to the {0} spawn", FloodBlockName());
                p.RevertBlock(x, y, z);
                cancel = true; 
                return;
            }
        }
        bool NearLavaSpawn(ushort x, ushort y, ushort z) => Math.Abs(x - (layerMode ? CurrentLayerPos() : cfg.FloodPos).X) <= Config.SpawnProtectionRadius && Math.Abs(y - (layerMode ? CurrentLayerPos() : cfg.FloodPos).Y) <= Config.SpawnProtectionRadius && Math.Abs(z - (layerMode ? CurrentLayerPos() : cfg.FloodPos).Z) <= Config.SpawnProtectionRadius;
        bool TryPlaceBlock(Player p, ref int blocksLeft, string type,
                           ushort block, ushort x, ushort y, ushort z)
        {
            if (!p.Game.Referee && blocksLeft <= 0)
            {
                p.Message("You have no {0} left", type);
                p.RevertBlock(x, y, z);
                return false;
            }
            if (p.ChangeBlock(x, y, z, block) == 0)
                return false;
            if (p.Game.Referee) return true;
            blocksLeft--;
            if ((blocksLeft % 10) == 0 || blocksLeft <= 10)
            {
                p.Message("{0} left: &4{1}", type, blocksLeft);
            }
            return true;
        }
    }
}
