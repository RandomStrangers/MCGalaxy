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
using System;
using System.Collections.Generic;
using System.Threading;
namespace MCGalaxy.Generator
{
    public delegate bool MapGenFunc(Player p, Level lvl, MapGenArgs args);
    public class MapGenArgs
    {
        public string Args;
        public int Seed;
        public bool RandomDefault = true;
        public bool ParseArgs()
        {
            bool gotSeed = false;
            foreach (string arg in Args.SplitSpaces())
            {
                if (arg.Length == 0) continue;
                else if (NumberUtils.TryParseInt32(arg, out Seed))
                    gotSeed = true;
            }
            if (!gotSeed) Seed = RandomDefault ? new Random().Next() : -1;
            return true;
        }
    }
    /// <summary> Map generators initialise the blocks in a level. </summary>
    /// <remarks> e.g. flatgrass generator, mountains theme generator, etc </remarks>
    public sealed class MapGen
    {
        public string Theme, Desc;
        public MapGenFunc GenFunc;
        /// <summary> Applies this map generator to the given level. </summary>
        /// <returns> Whether generation was actually successful. </returns>
        public bool Generate(Player p, Level lvl, string seed)
        {
            lvl.Config.Theme = Theme;
            lvl.Config.Seed = seed;
            MapGenArgs args = new()
            {
                Args = seed
            };
            return GenFunc(p, lvl, args);
        }
        public static List<MapGen> Generators = new();
        public static MapGen Find(string theme)
        {
            foreach (MapGen gen in Generators)
                if (gen.Theme.CaselessEq(theme)) return gen;
            return null;
        }
        /// <summary> Adds a new map generator to the list of generators. </summary>
        public static void Register(string theme, MapGenFunc func, string desc) => Generators.Add(new()
        {
            Theme = theme,
            GenFunc = func,
            Desc = desc,
        });
        static MapGen()
        {
            Register("Flat", GenFlat, "&HSeed specifies grass height (default half of level height)");
            Register("Empty", GenEmpty, "&HSeed does nothing");
        }
        public static unsafe bool GenFlat(Player _, Level lvl, MapGenArgs args)
        {
            args.RandomDefault = false;
            if (!args.ParseArgs()) return false;
            int grassHeight = lvl.Height / 2;
            if (args.Seed >= 0 && args.Seed <= lvl.Height) grassHeight = args.Seed;
            lvl.Config.EdgeLevel = grassHeight;
            int grassY = grassHeight - 1;
            fixed (byte* ptr = lvl.blocks)
            {
                if (grassY > 0)
                    MapSet(lvl.Width, lvl.Length, ptr, 0, grassY - 1, Block.Dirt);
                if (grassY >= 0 && grassY < lvl.Height)
                    MapSet(lvl.Width, lvl.Length, ptr, grassY, grassY, Block.Grass);
            }
            return true;
        }
        public static unsafe void MapSet(int width, int length, byte* ptr,
                                  int yBeg, int yEnd, byte block)
        {
            int beg = yBeg * length * width,
                end = (yEnd * length + (length - 1)) * width + (width - 1);
            MemUtils.Memset((IntPtr)ptr, block, beg, end - beg + 1);
        }
        public static bool GenEmpty(Player _, Level lvl, MapGenArgs args)
        {
            if (!args.ParseArgs()) return false;
            int maxX = lvl.Width - 1, maxZ = lvl.Length - 1;
            int width = lvl.Width, length = lvl.Length;
            byte[] blocks = lvl.blocks;
            for (int y = 0; y <= Math.Min(0, lvl.MaxY); y++)
                for (int z = 0; z <= maxZ; z++)
                    for (int x = 0; x <= maxX; x++)
                        blocks[x + width * (z + y * length)] = Block.Bedrock;
            lvl.Config.EdgeLevel = 1;
            return true;
        }
        public static Level Generate(Player p, MapGen gen, string name,
                                     ushort x, ushort y, ushort z, string seed)
        {
            name = name.ToLower();
            if (gen == null) 
            {
                p.Message("Themes: &f " + Generators.Join(g => g.Theme));
                return null; 
            }
            if (!Formatter.ValidMapName(p, name)) return null;
            if (LevelInfo.MapExists(name))
            {
                p.Message("&WLevel \"{0}\" already exists", name);
                return null;
            }
            if (Interlocked.CompareExchange(ref p.GeneratingMap, 1, 0) == 1)
            {
                p.Message("You are already generating a map, please wait until that map has finished generating first.");
                return null;
            }
            Level lvl;
            try
            {
                p.Message("Generating map \"{0}\"..", name);
                lvl = new(name, x, y, z);
                DateTime start = DateTime.UtcNow;
                if (!gen.Generate(p, lvl, seed)) 
                { 
                    lvl.Dispose();
                    return null; 
                }
                Logger.Log(LogType.SystemActivity, "Generation completed in {0:F3} seconds",
                           (DateTime.UtcNow - start).TotalSeconds);
                string msg = seed.Length > 0 ? "λNICK&S created level {0}&S with seed \"{1}\"" : "λNICK&S created level {0}";
                Chat.MessageFrom(p, string.Format(msg, lvl.ColoredName, seed));
            }
            finally
            {
                Interlocked.Exchange(ref p.GeneratingMap, 0);
                Server.DoGC();
            }
            return lvl;
        }
        public static bool GetDimensions(Player p, string[] args, int i,
                                         ref ushort x, ref ushort y, ref ushort z, bool checkVolume = true) => CheckMapAxis(p, args[i], "Width", ref x) &&
                CheckMapAxis(p, args[i + 1], "Height", ref y) &&
                CheckMapAxis(p, args[i + 2], "Length", ref z) &&
                (!checkVolume || CheckMapVolume(p, x, y, z));
        public static bool CheckMapAxis(Player p, string input, string type, ref ushort len) => CommandParser.GetUShort(p, input, type, ref len, 1, 16384);
        public static bool CheckMapVolume(Player p, int x, int y, int z)
        {
            int limit = p.group.GenVolume;
            if ((long)x * y * z <= limit) return true;
            string text = "&WYou cannot create a map with over ";
            if (limit > 1000 * 1000) text += (limit / (1000 * 1000)) + " million blocks";
            else if (limit > 1000) text += (limit / 1000) + " thousand blocks";
            else text += limit + " blocks";
            p.Message(text);
            return false;
        }
        /// <summary> Sets default permissions for a newly generated realm map. </summary>
        internal static void SetRealmPerms(Player p, Level lvl)
        {
            lvl.Config.RealmOwner = p.name;
            lvl.BuildAccess.Whitelist(Player.NASConsole, LevelPermission.NASConsole, lvl, p.name);
            lvl.VisitAccess.Whitelist(Player.NASConsole, LevelPermission.NASConsole, lvl, p.name);
            Group grp = Group.Find(Server.Config.OSPerbuildDefault);
            if (grp == null) return;
            lvl.BuildAccess.SetMin(Player.NASConsole, LevelPermission.NASConsole, lvl, grp);
        }
    }
}
