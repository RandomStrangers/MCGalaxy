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
using System;
namespace MCGalaxy.Commands.Maintenance
{
    public class CmdUpdate : Command
    {
        public override string Name => "Update";
        public override string Type => CommandTypes.Moderation;
        public override sbyte DefaultRank => 120;
        public override void Use(Player p, string message)
        {
            if (!Updater.SetupDone)
            {
                Updater.Setup();
            }
            if (message.CaselessEq("check"))
            {
                p.Message("Checking for updates..");
                p.Message("Server {0}", Updater.NeedsUpdating() ? "&cneeds updating" : "&ais up to date");
                if (Updater.NeedsUpdating())
                {
                    if (!string.IsNullOrEmpty(Updater.Latest))
                    {
                        p.Message("Current version: {0}.", Server.Version);
                        p.Message("Latest version: {0}.", Updater.Latest);
                    }
                }
            }
            else if (message.CaselessEq("latest"))
            {
                if (Environment.Version.Major == 4)
                {
                    Updater.PerformUpdate();
                }
            }
            else
            {
                Help(p);
            }
        }
        public override void Help(Player p)
        {
            p.Message("&T/Update check");
            p.Message("&HChecks whether the server needs updating");
            p.Message("&T/Update latest");
            p.Message("&HUpdates the server to the latest build");
        }
    }
}