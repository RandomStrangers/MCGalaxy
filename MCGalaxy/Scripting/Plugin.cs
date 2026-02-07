/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
using MCGalaxy.Core;
using MCGalaxy.Scripting;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    /// <summary> This class provides for more advanced modification to MCGalaxy </summary>
    public abstract class Plugin
    {
        /// <summary> Hooks into events and initalises states/resources etc </summary>
        /// <param name="auto"> True if plugin is being automatically loaded (e.g. on server startup), false if manually. </param>
        public abstract void Load(bool auto);
        /// <summary> Unhooks from events and disposes of state/resources etc </summary>
        /// <param name="auto"> True if plugin is being auto unloaded (e.g. on server shutdown), false if manually. </param>
        public abstract void Unload(bool auto);
        /// <summary> Called when a player does /Help on the plugin. Typically tells the player what this plugin is about. </summary>
        /// <param name="p"> Player who is doing /Help. </param>
        public virtual void Help(Player p) => p.Message("No help is available for this plugin.");
        /// <summary> Name of the plugin. </summary>
        public abstract string Name { get; }
        /// <summary> The oldest version of MCGalaxy this plugin is compatible with. </summary>
        public virtual string MCGalaxy_Version => null;
        /// <summary> Whether or not to auto load this plugin on server startup. </summary>
        public virtual bool LoadAtStartup => true;
        /// <summary> List of plugins/modules included in the server software </summary>
        public static List<Plugin> core = new(), custom = new();
        public static Plugin FindCustom(string name)
        {
            foreach (Plugin pl in custom)
            {
                if (pl.Name.CaselessEq(name))
                {
                    return pl;
                }
            }
            return null;
        }
        public static void Load(Plugin pl, bool auto)
        {
            string ver = pl.MCGalaxy_Version;
            if (!string.IsNullOrEmpty(ver) && new Version(ver) > new Version(Server.InternalVersion))
            {
                string msg = string.Format("Plugin '{0}' requires a more recent version of {1}!", pl.Name, Server.SoftwareName);
                throw new InvalidOperationException(msg);
            }
            try
            {
                custom.Add(pl);
                if (pl.LoadAtStartup || !auto)
                {
                    pl.Load(auto);
                    Logger.Log(1, "Plugin {0} loaded", pl.Name);
                }
                else
                {
                    Logger.Log(1, "Plugin {0} was not loaded, you can load it with /pload", pl.Name);
                }
            }
            catch
            {
                if (custom.Contains(pl))
                {
                    custom.Remove(pl);
                }
                throw;
            }
        }
        public static bool Unload(Plugin pl)
        {
            bool success = UnloadPlugin(pl, false);
            if (success)
            {
                custom.Remove(pl);
                core.Remove(pl);
            }
            return success;
        }
        static bool UnloadPlugin(Plugin pl, bool auto)
        {
            try
            {
                pl.Unload(auto);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error unloading plugin " + pl.Name, ex);
                return false;
            }
        }
        public static void UnloadAll()
        {
            for (int i = 0; i < custom.Count; i++)
            {
                UnloadPlugin(custom[i], true);
            }
            custom.Clear();
            for (int i = 0; i < core.Count; i++)
            {
                UnloadPlugin(core[i], true);
            }
        }
        public static void LoadAll()
        {
            LoadCorePlugin(new CorePlugin());
            LoadCorePlugin(new Modules.Moderation.Review.ReviewPlugin());
            LoadCorePlugin(new Modules.Moderation.Notes.NotesPlugin());
            LoadCorePlugin(new Modules.Relay.Discord.DiscordPlugin());
            LoadCorePlugin(new Modules.Relay.IRC.IRCPlugin());
            LoadCorePlugin(new Modules.Security.IPThrottler());
            LoadCorePlugin(new Modules.Warps.WarpsPlugin());
            LoadCorePlugin(new Modules.Compiling.CompilerPlugin());
            LoadCorePlugin(new Modules.Games.Countdown.CountdownPlugin());
            LoadCorePlugin(new Modules.Games.CTF.CTFPlugin());
            LoadCorePlugin(new Modules.Games.LS.LSPlugin());
            LoadCorePlugin(new Modules.Games.TW.TWPlugin());
            LoadCorePlugin(new Modules.Games.ZS.ZSPlugin());
            LoadCorePlugin(new NASPlugin());
            IScripting.AutoloadPlugins();
        }
        static void LoadCorePlugin(Plugin plugin)
        {
            if (!Server.Config.DisabledModules.CaselessContains(plugin.Name))
            {
                plugin.Load(true);
                core.Add(plugin);
            }
        }
    }
}
