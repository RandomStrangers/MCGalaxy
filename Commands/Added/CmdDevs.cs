/*
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCGalaxy)
 
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
namespace MCGalaxy.Commands {
	public class CmdDevs : Command {
		public override string name { get { return "devs"; } }
		public override string shortcut { get { return  "dev"; } }
		public override string type { get { return "information"; } }
		public override bool museumUsable { get { return true; } }
		public override LevelPermission defaultRank { get { return LevelPermission.Banned; } }
		public CmdDevs() { }

		public override void Use(Player p, string message) {
			if ( message != "" ) { Help(p); return; }
			string devlist = "";
			string temp;
			foreach ( string dev in Server.Devs ) {
				temp = dev.Substring(0, 1);
				temp = temp.ToLower() + dev.Remove(0, 1);
				devlist += temp + ", ";
			}
			devlist = devlist.Remove(devlist.Length - 2);
			Player.SendMessage(p, "New &9MCForge" +Server.DefaultColor + " Development Team (Continuing the legacy!): " + Server.DefaultColor + devlist + Server.DefaultColor + ".");
            Player.SendMessage(p, "&5Special thanks to the original &9MCForge &5developers!");
            Player.SendMessage(p, "Use &a/olddevs"+ Server.DefaultColor + "!");

        }

        public override void Help(Player p) {
			Player.SendMessage(p, "/devs - Displays the list of the New MCGalaxy developers.");
		}
	}
}