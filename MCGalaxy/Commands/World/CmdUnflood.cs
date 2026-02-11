/*
    Copyright 2011 MCForge
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
namespace MCGalaxy.Commands.World
{
    public sealed class CmdUnflood : Command2
    {
        public override string Name => "Unflood";
        public override string Type => CommandTypes.World;
        public override bool MuseumUsable => false;
        public override LevelPermission DefaultRank => LevelPermission.Operator;
        public override bool SuperUseable => false;
        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0) 
            { 
                Help(p); 
                return;
            }
            if (!message.CaselessEq("all") && !CommandParser.GetBlock(p, message, out _)) return;
            Level lvl = p.Level;
            if (!LevelInfo.Check(p, data.Rank, lvl, "unflood this level")) return;
            bool paused = lvl.PhysicsPaused;
            lvl.PhysicsPaused = true;
            try
            {
                Command cmd = Find("ReplaceAll");
                string args = !message.CaselessEq("all") ? message :
                    "8 10 lavafall waterfall lava_fast active_hot_lava active_cold_water fast_hot_lava magma geyser";
                cmd.Use(p, args + " air", data);
            }
            finally
            {
                lvl.PhysicsPaused = paused;
            }
            lvl.Message("Unflooded!");
        }
        public override void Help(Player p)
        {
            p.Message("&T/Unflood [liquid]");
            p.Message("&HUnfloods the map you are currently in of [liquid].");
            p.Message("&H  If [liquid] is \"all\", unfloods the map of all liquids.");
        }
    }
}
