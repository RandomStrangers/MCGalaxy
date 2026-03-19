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
using MCGalaxy.Commands;
namespace MCGalaxy
{
    public abstract partial class Command
    {
        protected bool CheckSuper(Player p, string message, string type)
        {
            if (message.Length > 0 || !p.IsSuper) return false;
            SuperRequiresArgs(p, type);
            return true;
        }
        protected void SuperRequiresArgs(Player p, string type) => p.Message("When using /{0} from {2}, you must provide a {1}.", Name, type, p.SuperName);
        protected bool HasExtraPerm(string cmd, LevelPermission plRank, int num) => CommandExtraPerms.Find(cmd, num).UsableBy(plRank);
        public bool HasExtraPerm(LevelPermission plRank, int num) => HasExtraPerm(Name, plRank, num);
        public bool CheckExtraPerm(Player p, int num)
        {
            if (HasExtraPerm(p.Rank, num)) return true;
            CommandExtraPerms perms = CommandExtraPerms.Find(Name, num);
            perms.MessageCannotUse(p);
            return false;
        }
        protected internal static bool CheckRank(Player p, CommandData data, Player target,
                                                 string action, bool canAffectOwnRank) => CheckRank(p, data, target.name, target.Rank, action, canAffectOwnRank);
        protected internal static bool CheckRank(Player p, CommandData data,
                                                 string plName, LevelPermission plRank,
                                                 string action, bool canAffectOwnRank)
        {
            if (p.name.CaselessEq(plName) || p.IsNASConsole || plRank < data.Rank || canAffectOwnRank && plRank == data.Rank) return true;
            if (canAffectOwnRank)
                p.Message("Can only {0} players ranked {1} &Sor below", action, p.group.ColoredName);
            else
                p.Message("Can only {0} players ranked below {1}", action, p.group.ColoredName);
            return false;
        }
        public string CheckOwn(Player p, string name, string type)
        {
            if (name.CaselessEq("-own"))
            {
                if (p.IsSuper) 
                { 
                    SuperRequiresArgs(p, type);
                    return null;
                }
                return p.name;
            }
            return name;
        }
        public static bool IsListModifier(string str) => str.CaselessEq("all") || NumberUtils.TryParseInt32(str, out _);
        public static bool IsCreateAction(string str) => str.CaselessEq("create") || str.CaselessEq("add") || str.CaselessEq("new");
        public static bool IsDeleteAction(string str) => str.CaselessEq("del") || str.CaselessEq("delete") || str.CaselessEq("remove");
        public static bool IsEditAction(string str) => str.CaselessEq("edit") || str.CaselessEq("change") || str.CaselessEq("modify")
                || str.CaselessEq("move") || str.CaselessEq("update");
        public static bool IsInfoAction(string str) => str.CaselessEq("about") || str.CaselessEq("info") || str.CaselessEq("status")
                || str.CaselessEq("check");
        public static bool IsListAction(string str) => str.CaselessEq("list") || str.CaselessEq("view");
    }
    public sealed class CommandTypes
    {
        public const string Building = "Building";
        public const string Chat = "Chat";
        public const string Economy = "Economy";
        public const string Games = "Games";
        public const string Information = "Info";
        public const string Moderation = "Moderation";
        public const string Other = "Other";
        public const string World = "World";
    }
}
