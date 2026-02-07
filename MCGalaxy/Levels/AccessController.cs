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
using System.Collections.Generic;
using System.Text;
using MCGalaxy.Commands;
namespace MCGalaxy
{
    /// <summary> Encapuslates access permissions (visit or build) for a level/zone. </summary>
    public abstract class AccessController
    {
        public abstract sbyte Min { get; set; }
        public abstract sbyte Max { get; set; }
        /// <summary> List of players who are always allowed to access. </summary>
        public abstract List<string> Whitelisted { get; }
        /// <summary> List of players who are never allowd to access. </summary>
        public abstract List<string> Blacklisted { get; }
        protected abstract string ColoredName { get; }
        protected abstract string Action { get; }
        protected abstract string ActionIng { get; }
        protected abstract string Type { get; }
        protected abstract string MaxCmd { get; }
        /// <summary> Replaces this instance's access permissions
        /// with a copy of the source's access permissions </summary>
        public void CloneAccess(AccessController source)
        {
            Min = source.Min;
            Max = source.Max;
            Whitelisted.Clear(); 
            Whitelisted.AddRange(source.Whitelisted);
            Blacklisted.Clear();
            Blacklisted.AddRange(source.Blacklisted);
        }
        public bool CheckAllowed(Player p) => Check(p.name, p.Rank) == 2 || Check(p.name, p.Rank) == 0;
        public int Check(string name, sbyte rank)
        {
            if (Blacklisted.CaselessContains(name)) return 1;
            if (Whitelisted.CaselessContains(name)) return 0;
            if (rank < Min) return 3;
            if (rank > Max && MaxCmd != null && !CommandExtraPerms.Find(MaxCmd, 1).UsableBy(rank))
            {
                return 4;
            }
            return 2;
        }
        public bool CheckDetailed(Player p) => CheckDetailed(p, p.Rank);
        public bool CheckDetailed(Player p, sbyte plRank)
        {
            int access = Check(p.name, plRank);
            if (access == 2) return true;
            if (access == 0) return true;
            if (access == 1)
            {
                p.Message("You are blacklisted from {0} {1}", ActionIng, ColoredName);
                return false;
            }
            string whitelist = "";
            if (Whitelisted.Count > 0)
            {
                whitelist = "(and " + Whitelisted.Join(pl => p.FormatNick(pl)) + "&S) ";
            }
            if (access == 3)
            {
                p.Message("Only {2}&S+ {3}may {0} {1}",
                               Action, ColoredName, Group.GetColoredName(Min), whitelist);
            }
            else if (access == 4)
            {
                p.Message("Only {2} &Sand below {3}may {0} {1}",
                               Action, ColoredName, Group.GetColoredName(Max), whitelist);
            }
            return false;
        }
        /// <summary>
        /// Returns true if shortening occured (more than 5 in a list)
        /// </summary>
        public bool Describe(Player p, StringBuilder perms, bool shorten)
        {
            perms.Append(Group.GetColoredName(Min) + "&S+");
            if (Max < 120)
            {
                perms.Append(" up to " + Group.GetColoredName(Max));
            }
            bool shortened = false;
            DescribeList(p, perms, Whitelisted, ", {0}", shorten, ref shortened);
            if (Blacklisted.Count == 0) return shortened;
            perms.Append(" &S(except ");
            DescribeList(p, perms, Blacklisted, "{0}, ", shorten, ref shortened);
            //Remove the comma and space
            perms.Remove(perms.Length - 2, 2);
            perms.Append("&S)");
            return shortened;
        }
        static void DescribeList(Player p, StringBuilder perms, List<string> list, string format, bool shorten, ref bool shortened)
        {
            int displayCount = list.Count;
            if (shorten && list.Count > 5) displayCount = 5;
            for (int i = 0; i < displayCount; i++)
            {
                string name = list[i];
                perms.Append(string.Format(format, p.FormatNick(name)));
            }
            if (list.Count > displayCount)
            {
                shortened = true;
                //perms.Append(" &Sand " + (list.Count - displayCount) + " more");
                perms.Append(string.Format(format, "&Sand " + (list.Count - displayCount) + " more"));
            }
        }
        public bool SetMin(Player p, sbyte plRank, Level lvl, Group grp)
        {
            if (!CheckRank(p, plRank, grp.Permission, false)) return false;
            Min = grp.Permission;
            OnPermissionChanged(p, lvl, grp, "Min ");
            return true;
        }
        public bool SetMax(Player p, sbyte plRank, Level lvl, Group grp)
        {
            if (!CheckRank(p, plRank, grp.Permission, true)) return false;
            Max = grp.Permission;
            OnPermissionChanged(p, lvl, grp, "Max ");
            return true;
        }
        public bool Whitelist(Player p, sbyte plRank, Level lvl, string target)
        {
            if (!CheckList(p, plRank, target, true)) return false;
            if (Whitelisted.CaselessContains(target))
            {
                p.Message("{0} &Sis already whitelisted.", p.FormatNick(target));
                return true;
            }
            bool removed = true;
            if (!Blacklisted.CaselessRemove(target))
            {
                Whitelisted.Add(target);
                removed = false;
            }
            OnListChanged(p, lvl, target, true, removed);
            return true;
        }
        public bool Blacklist(Player p, sbyte plRank, Level lvl, string target)
        {
            if (!CheckList(p, plRank, target, false)) return false;
            if (Blacklisted.CaselessContains(target))
            {
                p.Message("{0} &Sis already blacklisted.", p.FormatNick(target));
                return true;
            }
            bool removed = true;
            if (!Whitelisted.CaselessRemove(target))
            {
                Blacklisted.Add(target);
                removed = false;
            }
            OnListChanged(p, lvl, target, false, removed);
            return true;
        }
        public void OnPermissionChanged(Player p, Level lvl, Group grp, string type) => ApplyChanges(p, lvl, type + Type + " rank changed to " + grp.ColoredName);
        public void OnListChanged(Player p, Level lvl, string name, bool whitelist, bool removedFromOpposite)
        {
            string msg = p.FormatNick(name);
            if (removedFromOpposite)
            {
                msg += " &Swas removed from the " + Type + (whitelist ? " blacklist" : " whitelist");
            }
            else
            {
                msg += " &Swas " + Type + (whitelist ? " whitelisted" : " blacklisted");
            }
            ApplyChanges(p, lvl, msg);
        }
        protected abstract void ApplyChanges(Player p, Level lvl, string msg);
        bool CheckRank(Player p, sbyte plRank, sbyte perm, bool max)
        {
            string mode = max ? "max" : "min";
            if (!CheckDetailed(p, plRank))
            {
                p.Message("&WHence you cannot change the {1} {0} rank.", Type, mode); return false;
            }
            if (perm <= plRank || max && perm == 120) return true;
            p.Message("&WYou cannot change the {1} {0} rank of {2} &Wto a rank higher than yours.",
                      Type, mode, ColoredName);
            return false;
        }
        bool CheckList(Player p, sbyte plRank, string name, bool whitelist)
        {
            if (!CheckDetailed(p, plRank))
            {
                string mode = whitelist ? "whitelist" : "blacklist";
                p.Message("&WHence you cannot modify the {0} {1}.", Type, mode); return false;
            }
            Group group = PlayerInfo.GetGroup(name);
            if (group.Permission <= plRank) return true;
            if (!whitelist)
            {
                p.Message("&WYou cannot blacklist players of a higher rank.");
                return false;
            }
            else if (Check(name, group.Permission) == 1)
            {
                p.Message("{0} &Sis blacklisted from {1} {2}&S.",
                          p.FormatNick(name), ActionIng, ColoredName);
                return false;
            }
            return true;
        }
    }
    /// <summary> Encapuslates access permissions (visit or build) for a level. </summary>
    public sealed class LevelAccessController : AccessController
    {
        readonly bool isVisit;
        readonly LevelConfig cfg;
        readonly string lvlName;
        public LevelAccessController(LevelConfig cfg, string levelName, bool isVisit)
        {
            this.cfg = cfg;
            lvlName = levelName;
            this.isVisit = isVisit;
        }
        public override sbyte Min
        {
            get { return isVisit ? cfg.VisitMin : cfg.BuildMin; }
            set
            {
                if (isVisit) cfg.VisitMin = value;
                else cfg.BuildMin = value;
            }
        }
        public override sbyte Max
        {
            get { return isVisit ? cfg.VisitMax : cfg.BuildMax; }
            set
            {
                if (isVisit) cfg.VisitMax = value;
                else cfg.BuildMax = value;
            }
        }
        public override List<string> Whitelisted => isVisit ? cfg.VisitWhitelist : cfg.BuildWhitelist;
        public override List<string> Blacklisted => isVisit ? cfg.VisitBlacklist : cfg.BuildBlacklist;
        protected override string ColoredName => cfg.Color + lvlName;
        protected override string Action => isVisit ? "go to" : "build in";
        protected override string ActionIng => isVisit ? "going to" : "building in";
        protected override string Type => isVisit ? "visit" : "build";
        protected override string MaxCmd => isVisit ? "PerVisit" : "PerBuild";
        protected override void ApplyChanges(Player p, Level lvl, string msg)
        {
            Update(lvl);
            Logger.Log(3, "{0} &Son {1}", msg, lvlName);
            lvl?.Message(Chat.LocalPrefix + msg);
            if (p != Player.Console && p.Level != lvl)
            {
                p.Message("{0} &Son {1} &Sby you.", msg, ColoredName);
            }
        }
        void Update(Level lvl)
        {
            cfg.SaveFor(lvlName);
            if (lvl != null)
            {
                if (isVisit && lvl == Server.mainLevel) return;
                Player[] players = PlayerInfo.Online.Items;
                foreach (Player p in players)
                {
                    if (p.Level != lvl) continue;
                    bool allowed = CheckAllowed(p);
                    if (!isVisit)
                    {
                        p.AllowBuild = allowed;
                    }
                    else if (!allowed)
                    {
                        p.Message("&WNo longer allowed to visit &S{0}", ColoredName);
                        PlayerActions.ChangeMap(p, Server.mainLevel);
                    }
                }
            }
        }
    }
}
