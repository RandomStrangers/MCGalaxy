/*
    Copyright 2015-2024 MCGalaxy
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
using MCGalaxy.Events;
using MCGalaxy.Events.EconomyEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
namespace MCGalaxy.Core
{
    public sealed class CorePlugin : Plugin
    {
        public override string Name => "CorePlugin";
        public override void Load(bool startup)
        {
            OnPlayerConnectEvent.Register(ConnectHandler.HandleConnect, 3);
            OnPlayerCommandEvent.Register(ChatHandler.HandleCommand, 3);
            OnChatEvent.Register(ChatHandler.HandleOnChat, 3);
            OnPlayerStartConnectingEvent.Register(ConnectingHandler.HandleConnecting, 3);
            OnSentMapEvent.Register(MiscHandlers.HandleSentMap, 3);
            OnPlayerMoveEvent.Register(MiscHandlers.HandlePlayerMove, 3);
            OnPlayerClickEvent.Register(MiscHandlers.HandlePlayerClick, 3);
            OnChangedZoneEvent.Register(MiscHandlers.HandleChangedZone, 3);
            OnEcoTransactionEvent.Register(EcoHandlers.HandleEcoTransaction, 3);
            OnModActionEvent.Register(ModActionHandler.HandleModAction, 3);
        }
        public override void Unload(bool shutdown)
        {
            OnPlayerConnectEvent.Unregister(ConnectHandler.HandleConnect);
            OnPlayerCommandEvent.Unregister(ChatHandler.HandleCommand);
            OnChatEvent.Unregister(ChatHandler.HandleOnChat);
            OnPlayerStartConnectingEvent.Unregister(ConnectingHandler.HandleConnecting);
            OnSentMapEvent.Unregister(MiscHandlers.HandleSentMap);
            OnPlayerMoveEvent.Unregister(MiscHandlers.HandlePlayerMove);
            OnPlayerClickEvent.Unregister(MiscHandlers.HandlePlayerClick);
            OnChangedZoneEvent.Unregister(MiscHandlers.HandleChangedZone);
            OnEcoTransactionEvent.Unregister(EcoHandlers.HandleEcoTransaction);
            OnModActionEvent.Unregister(ModActionHandler.HandleModAction);
        }
    }
}