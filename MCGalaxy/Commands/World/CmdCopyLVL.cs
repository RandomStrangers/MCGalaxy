/*
    Written by Jack1312
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
    public class CmdCopyLvl : Command
    {
        public override string Name => "CopyLvl";
        public override string Type => CommandTypes.World;
        public override sbyte DefaultRank => 100;
        public override CommandAlias[] Aliases => new[] { new CommandAlias("WCopy"), new CommandAlias("WorldCopy") };
        public override bool MessageBlockRestricted => true;
        public override void Use(Player p, string message)
        {
            if (message.Length == 0) { Help(p); return; }
            string[] args = message.ToLower().SplitSpaces();
            if (args.Length < 2)
            {
                p.Message("You did not specify the destination level name."); return;
            }
            string src = Matcher.FindMaps(p, args[0]);
            if (src == null) return;
            if (!LevelInfo.Check(p, p.Rank, src, "copy this map", out LevelConfig cfg)) return;
            string dst = args[1];
            if (!Formatter.ValidMapName(p, dst)) return;
            if (!LevelActions.Copy(p, src, dst)) return;
            Chat.MessageGlobal("Level {0} &Swas copied to {1}", cfg.Color + src, cfg.Color + dst);
        }
        public override void Help(Player p)
        {
            p.Message("&T/CopyLvl [level] [copied level]");
            p.Message("&HMakes a copy of [level] called [copied level].");
            p.Message("&HNote: The level's BlockDB is not copied.");
        }
    }
}
