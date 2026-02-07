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
using System;
namespace MCGalaxy.Commands.Info
{
    public sealed class CmdTime : Command
    {
        public override string Name => "Time";
        public override string Shortcut => "ti";
        public override string Type => CommandTypes.Information;
        public override bool UseableWhenFrozen => true;
        public override void Use(Player p, string message) => p.Message("Server time: {0:HH:mm:ss} on {0:yyyy-MM-dd}", DateTime.Now);
        public override void Help(Player p) => p.Message("&T/Time - &HShows the server time.");
    }
}
