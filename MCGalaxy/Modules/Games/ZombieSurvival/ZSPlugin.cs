﻿/*
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
using MCGalaxy.Events.ServerEvents;

namespace MCGalaxy.Modules.Games.ZS
{
    public sealed class ZSPlugin : Plugin 
    {
        public override string name { get { return "ZS"; } }
        static Command cmdZS = new CmdZombieSurvival();
        
        public override void Load(bool startup) {
            Command.Register(cmdZS);
            
            ZSGame game      = ZSGame.Instance;
            game.Config.Path = "properties/zombiesurvival.properties";
            game.ReloadConfig();
            game.AutoStart();
            
            OnConfigUpdatedEvent.Register(game.ReloadConfig, Priority.Low);
        }
        
        public override void Unload(bool shutdown) {
            ZSGame game = ZSGame.Instance;
            OnConfigUpdatedEvent.Unregister(game.ReloadConfig);
            Command.Unregister(cmdZS);
        }
    }
}
