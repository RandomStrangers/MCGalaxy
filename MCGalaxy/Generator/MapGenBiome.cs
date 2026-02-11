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
using MCGalaxy.Generator.Foliage;
using System.Collections.Generic;
namespace MCGalaxy.Generator
{
    /// <summary> Contains environment settings and the types of blocks that are used to generate a map </summary>
    public struct MapGenBiome
    {
        public byte Surface, Ground, Cliff, Horizon, Border,
            Water, Bedrock, BeachSandy, BeachRocky;
        public string CloudColor, SkyColor,
            FogColor, TreeType;
        public const string FOREST = "Forest",
            ARCTIC = "Arctic",
            DESERT = "Desert",
            HELL = "Hell",
            SWAMP = "Swamp",
            MINE = "Mine",
            SANDY = "Sandy",
            PLAINS = "Plains",
            SPACE = "Space";
        public readonly void ApplyEnv(EnvConfig env)
        {
            if (CloudColor != null) env.CloudColor = CloudColor;
            if (SkyColor != null) env.SkyColor = SkyColor;
            if (FogColor != null) env.FogColor = FogColor;
            if (Horizon != 0) env.HorizonBlock = Horizon;
            if (Border != 0) env.EdgeBlock = Border;
        }
        public readonly Tree GetTreeGen(string defaultType)
        {
            if (TreeType == null) return null;
            string type = TreeType;
            if (type.Length == 0) type = defaultType;
            return Tree.TreeTypes[type]();
        }
        static MapGenBiome forest = new()
        {
            Surface = Block.Grass,
            Ground = Block.Dirt,
            Cliff = Block.Stone,
            Water = Block.StillWater,
            Bedrock = Block.Stone,
            BeachSandy = Block.Sand,
            BeachRocky = Block.Gravel,
            TreeType = "", // "use default for generator"
        },
        arctic = new()
        {
            Surface = Block.White,
            Ground = Block.White,
            Cliff = Block.Stone,
            Water = Block.StillWater,
            Bedrock = Block.Stone,
            BeachSandy = Block.White,
            BeachRocky = Block.Stone,
            CloudColor = "#8E8E8E",
            SkyColor = "#8E8E8E",
            FogColor = "#AFAFAF",
        },
        desert = new()
        {
            Surface = Block.Sand,
            Ground = Block.Sand,
            Cliff = Block.Gravel,
            Water = Block.Air,
            Bedrock = Block.Stone,
            BeachSandy = Block.Sand,
            BeachRocky = Block.Gravel,
            CloudColor = "#FFEE88",
            SkyColor = "#FFEE88",
            FogColor = "#FFEE88",
            Horizon = Block.Sand,
            Border = Block.Sandstone,
            TreeType = "Cactus",
        },
        hell = new()
        {
            Surface = Block.Obsidian,
            Ground = Block.Stone,
            Cliff = Block.Stone,
            Water = Block.StillLava,
            Bedrock = Block.Stone,
            BeachSandy = Block.Obsidian,
            BeachRocky = Block.Obsidian,
            CloudColor = "#000000",
            SkyColor = "#FFCC00",
            FogColor = "#FF6600",
            Horizon = Block.StillLava,
        },
        swamp = new()
        {
            Surface = Block.Dirt,
            Ground = Block.Dirt,
            Cliff = Block.Stone,
            Water = Block.StillWater,
            Bedrock = Block.Stone,
            BeachSandy = Block.Leaves,
            BeachRocky = Block.Dirt,
        },
        mine = new()
        {
            Surface = Block.Gravel,
            Ground = Block.Cobblestone,
            Cliff = Block.Stone,
            Water = Block.StillWater,
            Bedrock = Block.Bedrock,
            BeachSandy = Block.Stone,
            BeachRocky = Block.Cobblestone,
            CloudColor = "#444444",
            SkyColor = "#444444",
            FogColor = "#777777",
        },
        sandy = new()
        {
            Surface = Block.Sand,
            Ground = Block.Sand,
            Cliff = Block.Gravel,
            Water = Block.StillWater,
            Bedrock = Block.Stone,
            BeachSandy = Block.Sand,
            BeachRocky = Block.Gravel,
            CloudColor = "#52C6F7",
            Border = Block.Sand,
            TreeType = "Palm",
        },
        plains = new()
        {
            Surface = Block.Grass,
            Ground = Block.Dirt,
            Cliff = Block.Stone,
            Water = Block.Air,
            Bedrock = Block.Stone,
            BeachSandy = Block.Grass,
            BeachRocky = Block.Grass,
            TreeType = "", // "use default for generator"
            Horizon = Block.Grass,
        },
        space = new()
        {
            Surface = Block.Obsidian,
            Ground = Block.Iron,
            Cliff = Block.Iron,
            Water = Block.Air,
            Bedrock = Block.Bedrock,
            BeachSandy = Block.Obsidian,
            BeachRocky = Block.Obsidian,
            SkyColor = "#000000",
            FogColor = "#000000",
            Horizon = Block.Obsidian,
            Border = Block.Obsidian,
        };
        public static MapGenBiome Get(string biome)
        {
            foreach (KeyValuePair<string, MapGenBiome> kvp in Biomes)
            {
                if (kvp.Key.CaselessEq(biome)) return kvp.Value;
            }
            return forest;
        }
        public static string FindMatch(Player p, string biome)
        {
            KeyValuePair<string, MapGenBiome> match = Matcher.Find(p, biome, out int matches, Biomes,
                                        null, b => b.Key, "biomes");
            if (match.Key == null && matches == 0) ListBiomes(p);
            return match.Key;
        }
        public static void ListBiomes(Player p) => p.Message("&HAvailable biomes: &f" + Biomes.Join(b => b.Key));
        public static Dictionary<string, MapGenBiome> Biomes = new()
        {
            { FOREST, forest }, { ARCTIC, arctic }, { DESERT, desert },
            { HELL,   hell },   { SWAMP,  swamp },  { MINE,   mine },
            { PLAINS, plains }, { SANDY,  sandy },  { SPACE,  space },
        };
    }
}
