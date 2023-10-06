/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
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
using System;
using System.Collections.Generic;
using MCGalaxy.Core;
using MCGalaxy.Scripting2;

namespace MCGalaxy 
{
    /// <summary> This class provides for more advanced modification to MCGalaxy </summary>
    public abstract class Plugin2 
    {
        /// <summary> Hooks into events and initalises states/resources etc </summary>
        /// <param name="auto"> True if plugin is being automatically loaded (e.g. on server startup), false if manually. </param>
        public abstract void Load(bool auto);
        
        /// <summary> Unhooks from events and disposes of state/resources etc </summary>
        /// <param name="auto"> True if plugin is being auto unloaded (e.g. on server shutdown), false if manually. </param>
        public abstract void Unload(bool auto);
        
        /// <summary> Called when a player does /Help on the plugin. Typically tells the player what this plugin is about. </summary>
        /// <param name="p"> Player who is doing /Help. </param>
        public virtual void Help(Player p) {
            p.SendMessage("No help is available for this plugin.");
        }
        
        /// <summary> Name of the plugin. </summary>
        public abstract string name { get; }
        /// <summary> Oldest version of MCGalaxy this plugin is compatible with. </summary>
        public abstract string MCGalaxy_Version { get; }
        /// <summary> Version of this plugin. </summary>
        public virtual int build { get { return 0; } }
        /// <summary> Message to display once this plugin is loaded. </summary>
        public virtual string welcome { get { return ""; } }
        /// <summary> The creator/author of this plugin. (Your name) </summary>
        public virtual string creator { get { return ""; } }
        /// <summary> Whether or not to auto load this plugin on server startup. </summary>
        public virtual bool LoadAtStartup { get { return true; } }
        
        
        internal static List<Plugin2> core = new List<Plugin2>();
        public static List<Plugin2> all = new List<Plugin2>();
        
        public static bool Load(Plugin2 p, bool auto) {
            try {
                string ver = p.MCGalaxy_Version;
                if (!string.IsNullOrEmpty(ver) && new Version(ver) > new Version(Server.Version1)) {
                    return false;
                }
                all.Add(p);
                
                if (p.LoadAtStartup || !auto) {
                    p.Load(auto);
                    Server.s.Log("Plugin " + p.name + " loaded...build: " + p.build);
                } else {
                }
                
                return true;
            } catch (Exception ex) {
                Server.s.Log("Error loading plugin " + p.name + " :" + ex);               
                return false;
            }
        }

        public static bool Unload(Plugin2 p, bool auto) {
            bool success = true;
            try {
                p.Unload(auto);
                Server.s.Log("Plugin " + p.name + " was unloaded.");
            } catch (Exception ex) {
                Server.s.Log("Error unloading plugin " + p.name + " : " + ex);
                success = false;
            }
            
            all.Remove(p);
            return success;
        }

        public static void UnloadAll() {
            for (int i = 0; i < all.Count; i++) {
                Unload(all[i], true); i--;
            }
        }

        public static void LoadAll() {

            IScripting.AutoloadPlugins();
        }
        
        static void LoadCorePlugin(Plugin2 plugin2) {
            plugin2.Load(true);
            all.Add(plugin2);
            core.Add(plugin2);
        }
    }
}

