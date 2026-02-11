/*
Copyright 2011-2014 MCGalaxy
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
using MCGalaxy.SQL;
using System.Collections.Generic;
namespace MCGalaxy.Commands.Info
{
    public class CmdSearch : Command2
    {
        public override string Name => "Search";
        public override string Type => CommandTypes.Information;
        public override LevelPermission DefaultRank => LevelPermission.Builder;
        public override bool UseableWhenFrozen => true;
        public override void Use(Player p, string message, CommandData data)
        {
            string[] args = message.SplitSpaces(3);
            if (args.Length < 2) 
            { 
                Help(p);
                return;
            }
            string list = args[0].ToLower(),
                keyword = args[1],
                modifier = args.Length > 2 ? args[2] : "";
            if (list == "block" || list == "blocks")
            {
                SearchBlocks(p, keyword, modifier);
            }
            else if (list == "rank" || list == "ranks")
            {
                SearchRanks(p, keyword, modifier);
            }
            else if (list == "command" || list == "commands")
            {
                SearchCommands(p, keyword, modifier);
            }
            else if (list == "player" || list == "players")
            {
                SearchPlayers(p, keyword, modifier);
            }
            else if (list == "online")
            {
                SearchOnline(p, keyword, modifier);
            }
            else if (list == "loaded")
            {
                SearchLoaded(p, keyword, modifier);
            }
            else if (list == "level" || list == "levels" || list == "maps")
            {
                SearchMaps(p, keyword, modifier);
            }
            else
            {
                Help(p);
            }
        }
        static void SearchBlocks(Player p, string keyword, string modifier)
        {
            List<ushort> blocks = new();
            for (int b = 0; b < 1024; b++)
            {
                ushort block = (ushort)b;
                if (Block.ExistsFor(p, block)) blocks.Add(block);
            }
            List<string> blockNames = Paginator.Filter(blocks, keyword,
                                                      b => Block.GetName(p, b), null,
                                                      b => Block.GetColoredName(p, b));
            OutputList(p, keyword, "search blocks", "blocks", modifier, blockNames);
        }
        static void SearchCommands(Player p, string keyword, string modifier)
        {
            List<string> commands = Paginator.Filter(allCmds, keyword, cmd => cmd.Name,
                                                     null, GetColoredName);
            List<string> shortcuts = Paginator.Filter(allCmds, keyword, cmd => cmd.Shortcut,
                                                     cmd => !string.IsNullOrEmpty(cmd.Shortcut),
                                                     GetColoredName);
            foreach (string shortcutCmd in shortcuts)
            {
                if (commands.CaselessContains(shortcutCmd)) continue;
                commands.Add(shortcutCmd);
            }
            OutputList(p, keyword, "search commands", "commands", modifier, commands);
        }
        static void SearchRanks(Player p, string keyword, string modifier)
        {
            List<string> ranks = Paginator.Filter(Group.GroupList, keyword, grp => grp.Name,
                                                null, grp => grp.ColoredName);
            OutputList(p, keyword, "search ranks", "ranks", modifier, ranks);
        }
        static void SearchOnline(Player p, string keyword, string modifier)
        {
            Player[] online = PlayerInfo.Online.Items;
            List<string> players = Paginator.Filter(online, keyword, pl => pl.name,
                                                  pl => p.CanSee(pl), pl => pl.ColoredName);
            OutputList(p, keyword, "search online", "players", modifier, players);
        }
        static void SearchLoaded(Player p, string keyword, string modifier)
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            List<string> levels = Paginator.Filter(loaded, keyword, level => level.name);
            OutputList(p, keyword, "search loaded", "loaded levels", modifier, levels);
        }
        static void SearchMaps(Player p, string keyword, string modifier)
        {
            string[] allMaps = LevelInfo.AllMapNames();
            List<string> maps = Paginator.Filter(allMaps, keyword, map => map);
            maps.Sort(new AlphanumComparator());
            OutputList(p, keyword, "search levels", "maps", modifier, maps);
        }
        static void OutputList(Player p, string keyword, string cmd, string type, string modifier, List<string> items)
        {
            if (items.Count == 0)
            {
                p.Message("No {0} found containing \"{1}\"", type, keyword);
            }
            else
            {
                Paginator.Output(p, items, item => item, cmd + " " + keyword, type, modifier);
            }
        }
        static void SearchPlayers(Player p, string keyword, string modifier)
        {
            List<string> names = new();
            Database.ReadRows("Players", "Name", r => names.Add(r.GetText(0)),
                              "WHERE Name LIKE @0 ESCAPE '#' LIMIT 100 COLLATE NOCASE",
                              "%" + keyword.Replace("_", "#_").Replace('*', '%').Replace('?', '_') + "%");
            OutputList(p, keyword, "search players", "players", modifier, names);
        }
        public override void Help(Player p)
        {
            p.Message("&T/Search [list] [keyword]");
            p.Message("&HFinds entries in a list that match the given keyword");
            p.Message("&H  keyword can also include wildcard characters:");
            p.Message("&H    * - placeholder for zero or more characters");
            p.Message("&H    ? - placeholder for exactly one character");
            p.Message("&HLists: &fblocks/commands/ranks/players/online/loaded/maps");
        }
    }
}
