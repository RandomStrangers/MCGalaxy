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
    public sealed class CmdOverseer : Command2
    {
        public override string Name => "Overseer";
        public override string Shortcut => Overseer.commandShortcut;
        public override string Type => CommandTypes.Moderation;
        public override LevelPermission DefaultRank => LevelPermission.Builder;
        public override bool SuperUseable => false;
        public override CommandAlias[] Aliases => new[] { new CommandAlias("Realm"), new CommandAlias("MyRealm") };
        public override CommandParallelism Parallelism => CommandParallelism.NoAndWarn;
        public override void Use(Player p, string message, CommandData data)
        {
            if (message.Length == 0) 
            {
                Help(p); 
                return;
            }
            if (Overseer.deprecatedSubCommandGroup.Use(p, message, false) != SubCommandGroup.UsageResult.NoneFound) return;
            Overseer.subCommandGroup.Use(p, message);
        }
        public override void Help(Player p, string message) => Overseer.subCommandGroup.DisplayHelpFor(p, message);
        public override void Help(Player p)
        {
            p.Message("&T/os [command] [args]");
            p.Message("&HAllows you to modify and manage your personal realms.");
            Overseer.subCommandGroup.DisplayAvailable(p);
        }
    }
}
