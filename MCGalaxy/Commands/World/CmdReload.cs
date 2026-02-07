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
namespace MCGalaxy.Commands.World
{
    public sealed class CmdReload : Command
    {
        public override string Name => "Reload";
        public override string Shortcut => "Reveal";
        public override string Type => CommandTypes.World;
        public override bool MuseumUsable => false;
        public override CommandAlias[] Aliases => new[] { new CommandAlias("ReJoin"), new CommandAlias("rd"),
                    new CommandAlias("WFlush"), new CommandAlias("WorldFlush") };
        public override CommandPerm[] ExtraPerms => new[] { new CommandPerm(LevelPermission.Operator, "can reload for all players") };
        public override void Use(Player p, string message)
        {
            if (CheckSuper(p, message, "level name")) return;
            if (message.Length == 0)
            {
                if (!Hacks.CanUseNoclip(p))
                {
                    p.Message("You cannot use &T/Reload &Son this level");
                }
                else
                {
                    PlayerActions.ReloadMap(p);
                    p.Message("&bMap reloaded");
                }
                return;
            }
            if (!CheckExtraPerm(p, 1)) return;
            Level lvl = p.Level;
            if (!message.CaselessEq("all"))
            {
                lvl = Matcher.FindLevels(p, message);
                if (lvl == null) return;
            }
            LevelActions.ReloadAll(lvl, p, true);
        }
        public override void Help(Player p)
        {
            p.Message("&T/Reload &H- Reloads the level you are in, just for you");
            p.Message("&T/Reload all &H- Reloads for all players in level you are in");
            p.Message("&T/Reload [level] &H- Reloads for all players in [level]");
        }
    }
}
