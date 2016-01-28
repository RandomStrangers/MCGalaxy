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
using MCGalaxy.SQL;
namespace MCGalaxy.Commands
{
    public sealed class CmdXTColor : Command
    {
        public override string name { get { return "xtcolor"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public CmdXTColor() { }

        public override void Use(Player p, string message)
        {
            if (message == "") 
            {
                p.titlecolor = "";
                Player.GlobalChat(p, p.color + p.DisplayName + Server.DefaultColor + " had their title color removed.", false);
                Database.AddParams("@Name", p.name);
                Database.executeQuery("UPDATE Players SET title_color = '' WHERE Name = @Name");
                p.SetPrefix();
                return; 
            }
            string color = Colors.Parse(message);
            if (color == "") { Player.SendMessage(p, "There is no color \"" + message + "\"."); return; }
            else if (color == p.titlecolor) { Player.SendMessage(p, "You already have that title color."); return; }
            else
            {
                Database.AddParams("@Color", Colors.Name(color));
                Database.AddParams("@Name", p.name);
                Database.executeQuery("UPDATE Players SET title_color = @Color WHERE Name = @Name");
                Player.GlobalChat(p, p.color + p.DisplayName + Server.DefaultColor + " had their title color changed to " + color + Colors.Name(color) + Server.DefaultColor + ".", false);
                p.titlecolor = color;
                p.SetPrefix();
            }
        }

        public override void Help(Player p)
        {
            Player.SendMessage(p, "/xtcolor [color] - Gives you the title color of [color].");
            Player.SendMessage(p, "If no [color] is specified, title color is removed.");
            Player.SendMessage(p, "&0black &1navy &2green &3teal &4maroon &5purple &6gold &7silver");
            Player.SendMessage(p, "&8gray &9blue &alime &baqua &cred &dpink &eyellow &fwhite");
        }
    }
}
