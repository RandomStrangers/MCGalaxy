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
    public sealed class CmdDeleteLvl : Command
    {
        public override string Name => "DeleteLvl";
        public override string Type => CommandTypes.World;
        public override LevelPermission DefaultRank => LevelPermission.Admin;
        public override CommandAlias[] Aliases => new CommandAlias[] {
                    new ("WDelete"), new("WorldDelete"), new("WRemove"),
                    new("DeleteBackup", "*backup")
                };
        public override CommandPerm[] ExtraPerms => new[] { new CommandPerm(LevelPermission.Owner, "can delete backups of levels") };
        public override bool MessageBlockRestricted => true;
        public override void Use(Player p, string message)
        {
            if (message.Length == 0) 
            { 
                Help(p);
                return; 
            }
            string[] words = message.SplitSpaces(2);
            if (words[0].CaselessEq("*backup"))
            {
                if (!CheckExtraPerm(p, 1)) return;
                UseBackup(p, words.Length >= 2 ? words[1] : "", false);
                return;
            }
            if (words.Length > 1) 
            { 
                Help(p);
                return;
            }
            string map = Matcher.FindMaps(p, message);
            if (map == null) return;
            if (!LevelInfo.Check(p, p.Rank, map, "delete this map", out LevelConfig cfg)) return;
            if (!LevelActions.Delete(p, map)) return;
            Chat.MessageGlobal("Level {0} &Swas deleted", cfg.Color + map);
        }
        /// <summary>
        /// os changes which confirmation text is displayed
        /// </summary>
        public static void UseBackup(Player p, string message, bool os)
        {
            if (message.Length == 0) 
            { 
                HelpBackup(p); 
                return; 
            }
            string[] words = message.SplitSpaces();
            if (words.Length < 2)
            {
                p.Message("You must provide a level name and the backup to delete.");
                p.Message("A backup is usually a number, but may also be named.");
                p.Message("See &T/help restore &7to display backups.");
                return;
            }
            bool confirmed = words.Length == 3 && words[2].CaselessEq("confirm");
            string map = words[0].ToLower(), backup = words[1].ToLower();
            if (!confirmed)
            {
                p.Message("You are about to &Wpermanently delete&S backup \"{0}\" from level \"{1}\"",
                    backup, map);
                if (os)
                {
                    p.Message("If you are sure, type &T/os delete *backup {0} confirm", backup);
                }
                else
                {
                    p.Message("If you are sure, type &T/deletebackup {0} {1} confirm", map, backup);
                }
                return;
            }
            LevelActions.DeleteBackup(p, map, backup);
        }
        public override void Help(Player p)
        {
            p.Message("&T/DeleteLvl [level]");
            p.Message("&HCompletely deletes [level] (portals, MBs, everything)");
            p.Message("&HA backup of the level is made in the levels/deleted folder");
            HelpBackup(p);
        }
        public static void HelpBackup(Player p)
        {
            p.Message("&T/DeleteLvl *backup [level] [backup]");
            p.Message("&H-Permanently- deletes [backup] of [level].");
        }
    }
}
