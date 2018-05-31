/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.osedu.org/licenses/ECL-2.0
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.Games;
using MCGalaxy.Maths;

namespace MCGalaxy.Commands.Misc {
    public sealed class CmdTp : Command {
        public override string name { get { return "TP"; } }
        public override string shortcut { get { return "Move"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool SuperUseable { get { return false; } }
        public override CommandAlias[] Aliases {
            get { return new [] { new CommandAlias("Teleport"), new CommandAlias("TPP", "-precise") }; }
        }
        
        const string precisePrefix = "-precise ";
        public override void Use(Player p, string message) {           
            if (message.Length == 0) { Help(p); return; }
            
            bool preciseTP = message.CaselessStarts(precisePrefix);        
            if (preciseTP) {
                message = message.Substring(precisePrefix.Length);
            }
            
            string[] args = message.SplitSpaces();            
            if (args.Length >= 3) { TeleportCoords(p, args, preciseTP); return; }
            
            Player target = null;
            PlayerBot bot = null;
            if (args.Length == 1) {
                target = PlayerInfo.FindMatches(p, args[0]);
                if (target == null) return;
                if (!CheckPlayer(p, target)) return;
            } else if (args[0].CaselessEq("bot")) {
                bot = Matcher.FindBots(p, args[1]);
                if (bot == null) return;
            } else {
                Help(p); return;
            }
            
            SavePreTeleportState(p);
            Level lvl = bot != null ? bot.level : target.level;

            if (p.level != lvl) PlayerActions.ChangeMap(p, lvl.name);
            if (target != null && target.Loading) {
                Player.Message(p, "Waiting for " + target.ColoredName + " %Sto spawn..");
                target.BlockUntilLoad(10);
            }
            
            // Player wasn't able to join target map, so don't move
            if (p.level != lvl) return;
            
            Position pos = bot != null ? bot.Pos : target.Pos;
            Orientation rot = bot != null ? bot.Rot : target.Rot;
            p.BlockUntilLoad(10);  //Wait for player to spawn in new map
            p.SendPos(Entities.SelfID, pos, rot);
        }
        
        static void TeleportCoords(Player p, string[] args, bool precise) {
            Vec3S32 P; Position pos;
            
            if (!precise) {
                // relative to feet block coordinates
                P = p.Pos.FeetBlockCoords;
                if (!CommandParser.GetCoords(p, args, 0, ref P)) return;
                pos = Position.FromFeetBlockCoords(P.X, P.Y, P.Z);
            } else {
                // relative to feet position exactly
                P = new Vec3S32(p.Pos.X, p.Pos.Y + Entities.CharacterHeight, p.Pos.Z);
                if (!CommandParser.GetCoords(p, args, 0, ref P)) return;
                pos = new Position(P.X, P.Y - Entities.CharacterHeight, P.Z);
            }
            
            byte yaw = p.Rot.RotY, pitch = p.Rot.HeadX;
            int angle = 0;
            
            if (args.Length > 3) {
                if (!CommandParser.GetInt(p, args[3], "Yaw angle", ref angle, -360, 360)) return;
                yaw = Orientation.DegreesToPacked(angle);
            }            
            if (args.Length > 4) {
                if (!CommandParser.GetInt(p, args[4], "Pitch angle", ref angle, -360, 360)) return;
                pitch = Orientation.DegreesToPacked(angle);
            }
            
            SavePreTeleportState(p);
            p.SendPos(Entities.SelfID, pos, new Orientation(yaw, pitch));
        }
        
        static void SavePreTeleportState(Player p) {
            p.PreTeleportMap = p.level.name;
            p.PreTeleportPos = p.Pos;
            p.PreTeleportRot = p.Rot;
        }
        
        static bool CheckPlayer(Player p, Player target) {
            if (target.level.IsMuseum) {
                Player.Message(p, target.ColoredName + " %Sis in a museum."); return false;
            }
            
            if (!ServerConfig.HigherRankTP && p.Rank < target.Rank) {
                MessageTooHighRank(p, "teleport to", true); return false;
            }
            
            IGame game = target.level.CurrentGame();
            if (!p.Game.Referee && game != null && !game.TeleportAllowed) {
                Player.Message(p, "You can only teleport to players who are " +
                               "playing a game when you are in referee mode."); return false;
            }
            return true;
        }
        
        public override void Help(Player p) {
            Player.Message(p, "%HUse ~ before a coordinate to move relative to current position");
            Player.Message(p, "%T/TP [x y z] <yaw> <pitch>");
            Player.Message(p, "%HTeleports yourself to the given block coordinates.");
            Player.Message(p, "%T/TP -precise [x y z] <yaw> <pitch>");
            Player.Message(p, "%HTeleports using precise units. (32 units = 1 block)");
            Player.Message(p, "%T/TP [player]");
            Player.Message(p, "%HTeleports yourself to that player.");
            Player.Message(p, "%T/TP bot [name]");
            Player.Message(p, "%HTeleports yourself to that bot.");
        }
    }
}
