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
using System.IO;
namespace MCGalaxy.Blocks
{
    /// <summary> Type of animal this block behaves as. </summary>
    public enum AnimalAI : byte
    {
        None, Fly, FleeAir, KillerAir, FleeWater, KillerWater, FleeLava, KillerLava,
    }
    /// <summary> Extended and physics properties of a block. </summary>
    public struct BlockProps
    {
        public string DeathMessage;
        public bool KillerBlock, IsTDoor, IsDoor,
            IsMessageBlock, IsPortal, WaterKills, 
            LavaKills, OPBlock, IsRails, Drownable;
        public ushort oDoorBlock, StackBlock, GrassBlock, DirtBlock;
        public AnimalAI AnimalAI;
        public byte ChangedScope;
        public static BlockProps MakeEmpty()
        {
            BlockProps props = default;
            props.oDoorBlock = 0xff;
            props.GrassBlock = 0xff;
            props.DirtBlock = 0xff;
            return props;
        }
        public static void Save(string group, BlockProps[] list, byte scope)
        {
            lock (list)
            {
                if (!Directory.Exists("blockprops"))
                    Directory.CreateDirectory("blockprops");
                SaveCore(group, list, scope);
            }
        }
        static void SaveCore(string group, BlockProps[] list, byte scope)
        {
            using StreamWriter w = FileIO.CreateGuarded("blockprops/" + group + ".txt");
            w.WriteLine("# This represents the physics properties for blocks, in the format of:");
            w.WriteLine("# id : Is rails : Is tdoor : Is door : Is message block : Is portal : " +
                        "Killed by water : Killed by lava : Kills players : death message : " +
                        "Animal AI type : Stack block : Is OP block : oDoor block : Drownable : " +
                        "Grass block : Dirt block");
            for (int b = 0; b < list.Length; b++)
            {
                if ((list[b].ChangedScope & scope) == 0) continue;
                BlockProps props = list[b];
                string deathMsg = props.DeathMessage == null ? "" : props.DeathMessage.Replace(":", "\\;");
                w.WriteLine(b + ":" + props.IsRails + ":" + props.IsTDoor + ":" + props.IsDoor + ":"
                            + props.IsMessageBlock + ":" + props.IsPortal + ":" + props.WaterKills + ":"
                            + props.LavaKills + ":" + props.KillerBlock + ":" + deathMsg + ":"
                            + (byte)props.AnimalAI + ":" + props.StackBlock + ":" + props.OPBlock + ":"
                            + props.oDoorBlock + ":" + props.Drownable + ":" + props.GrassBlock + ":"
                            + props.DirtBlock);
            }
        }
        public static void Load(string group, BlockProps[] list, byte scope, bool mapOld)
        {
            lock (list)
            {
                if (!Directory.Exists("blockprops")) return;
                string path = Paths.BlockPropsPath(group);
                if (File.Exists(path)) LoadCore(path, list, scope, mapOld);
            }
        }
        static void LoadCore(string path, BlockProps[] list, byte scope, bool mapOld)
        {
            string[] lines = FileIO.TryReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.IsCommentLine()) continue;
                string[] parts = line.Split(':');
                if (parts.Length < 10)
                {
                    Logger.Log(LogType.Warning, "Invalid line \"{0}\" in {1}", line, path);
                    continue;
                }
                if (!ushort.TryParse(parts[0], out ushort b))
                {
                    Logger.Log(LogType.Warning, "Invalid line \"{0}\" in {1}", line, path);
                    continue;
                }
                if (mapOld) b = Block.MapOldRaw(b);
                if (b >= list.Length)
                {
                    Logger.Log(LogType.Warning, "Invalid block ID: " + b);
                    continue;
                }
                bool.TryParse(parts[1], out list[b].IsRails);
                bool.TryParse(parts[2], out list[b].IsTDoor);
                bool.TryParse(parts[3], out list[b].IsDoor);
                bool.TryParse(parts[4], out list[b].IsMessageBlock);
                bool.TryParse(parts[5], out list[b].IsPortal);
                bool.TryParse(parts[6], out list[b].WaterKills);
                bool.TryParse(parts[7], out list[b].LavaKills);
                bool.TryParse(parts[8], out list[b].KillerBlock);
                list[b].ChangedScope = scope;
                list[b].DeathMessage = parts[9].Replace("\\;", ":");
                if (list[b].DeathMessage.Length == 0)
                    list[b].DeathMessage = null;
                if (parts.Length > 10)
                {
                    byte.TryParse(parts[10], out byte ai);
                    list[b].AnimalAI = (AnimalAI)ai;
                }
                if (parts.Length > 11)
                {
                    ushort.TryParse(parts[11], out list[b].StackBlock);
                    list[b].StackBlock = Block.MapOldRaw(list[b].StackBlock);
                }
                if (parts.Length > 12)
                    bool.TryParse(parts[12], out list[b].OPBlock);
                if (parts.Length > 13)
                    ushort.TryParse(parts[13], out list[b].oDoorBlock);
                if (parts.Length > 14)
                    bool.TryParse(parts[14], out list[b].Drownable);
                if (parts.Length > 15)
                    ushort.TryParse(parts[15], out list[b].GrassBlock);
                if (parts.Length > 16)
                    ushort.TryParse(parts[16], out list[b].DirtBlock);
            }
        }
        public static BlockProps MakeDefault(BlockProps[] scope, Level lvl, ushort block) => scope == Block.Props ? Block.MakeDefaultProps(block) : IsDefaultBlock(lvl, block) ? Block.Props[block] : MakeEmpty();
        static bool IsDefaultBlock(Level lvl, ushort b) => Block.IsPhysicsType(b) || lvl.CustomBlockDefs[b] == BlockDefinition.GlobalDefs[b];
        public static void ApplyChanges(BlockProps[] scope, Level lvl_, ushort block, bool save)
        {
            byte scopeId = ScopeId(scope);
            string path;
            if (scope == Block.Props)
            {
                path = "default";
                Level[] loaded = LevelInfo.Loaded.Items;
                foreach (Level lvl in loaded)
                {
                    if ((lvl.Props[block].ChangedScope & 0x02) != 0 || !IsDefaultBlock(lvl, block)) continue;
                    lvl.Props[block] = scope[block];
                    lvl.UpdateBlockHandlers(block);
                }
            }
            else
            {
                path = "_" + lvl_.name;
                lvl_.UpdateBlockHandlers(block);
            }
            if (save) Save(path, scope, scopeId);
        }
        internal static byte ScopeId(BlockProps[] scope) => scope == Block.Props ? (byte)1 : (byte)2;
        public static string ScopedName(BlockProps[] scope, Player p, ushort block) => scope == Block.Props ? Block.GetName(Player.NASConsole, block) : Block.GetName(p, block);
    }
}
