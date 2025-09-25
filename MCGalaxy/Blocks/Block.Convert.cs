/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    
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
using MCGalaxy.Blocks;

namespace MCGalaxy
{
    public static partial class Block
    {
        static readonly string[] coreNames = new string[CORE_COUNT];
        public static bool Undefined(ushort block) { return IsPhysicsType(block) && coreNames[block].CaselessEq("unknown"); }

        public static bool ExistsGlobal(ushort b) { return ExistsFor(Player.Console, b); }

        public static bool ExistsFor(Player p, ushort b)
        {
            if (b < CORE_COUNT) return !Undefined(b);

            if (!p.IsSuper) return p.level.GetBlockDef(b) != null;
            return BlockDefinition.GlobalDefs[b] != null;
        }

        /// <summary> Gets the name for the block with the given block ID </summary>
        /// <remarks> Block names can differ depending on the player's level </remarks>
        public static string GetName(Player p, ushort block)
        {
            if (IsPhysicsType(block)) return coreNames[block];

            BlockDefinition def;
            if (!p.IsSuper)
            {
                def = p.level.GetBlockDef(block);
            }
            else
            {
                def = BlockDefinition.GlobalDefs[block];
            }
            if (def != null) return def.Name.Replace(" ", "");

            return block < CPE_COUNT ? coreNames[block] : ToRaw(block).ToString();
        }

        public static ushort Parse(Player p, string input)
        {
            BlockDefinition[] defs = p.IsSuper ? BlockDefinition.GlobalDefs : p.level.CustomBlockDefs;
            // raw ID is treated specially, before names
            if (ushort.TryParse(input, out ushort block))
            {
                if (block < CPE_COUNT || (block <= MaxRaw && defs[FromRaw(block)] != null))
                {
                    return FromRaw(block);
                } // TODO redo to use ExistsFor?
            }

            BlockDefinition def = BlockDefinition.ParseName(input, defs);
            if (def != null) return def.GetBlock();

            bool success = Aliases.TryGetValue(input.ToLower(), out byte coreID);
            return success ? coreID : Invalid;
        }

        public static string GetColoredName(Player p, ushort block)
        {
            BlockPerms perms = BlockPerms.GetPlace(block); // TODO check Delete perms too?
            return Group.GetColor(perms.MinRank) + GetName(p, block);
        }


        /// <summary> Converts a block &lt;= CPE_MAX_BLOCK into a suitable
        /// block compatible for the given classic protocol version </summary>
        public static byte ConvertClassic(byte block, byte protocolVersion)
        {
            // protocol version 7 only supports up to Obsidian block
            if (protocolVersion >= Server.VERSION_0030)
            {
                return block <= Obsidian ? block : v7_fallback[block - CobblestoneSlab];
            }

            // protocol version 6 only supports up to Gold block
            if (protocolVersion >= Server.VERSION_0020)
            {
                return block <= Gold ? block : v6_fallback[block - Iron];
            }

            // protocol version 5 only supports up to Glass block
            if (protocolVersion >= Server.VERSION_0019)
            {
                return block <= Glass ? block : v5_fallback[block - Red];
            }

            // protocol version 4 only supports up to Leaves block
            //  protocol version 3 seems to have same support
            //  TODO what even changed between 3 and 4?
            return block <= Leaves ? block : v4_fallback[block - Sponge];
        }

        static readonly byte[] v7_fallback = {
            // CobbleSlab Rope      Sandstone Snow Fire  LightPink ForestGreen Brown
               Slab,      Mushroom, Sand,     Air, Lava, Pink,     Green,      Dirt,
            // DeepBlue Turquoise Ice    CeramicTile Magma     Pillar Crate StoneBrick
               Blue,    Cyan,     Glass, Iron,       Obsidian, White, Wood, Stone
        };
        static readonly byte[] v6_fallback = {
            // Iron   DoubleSlab Slab  Brick TNT  Bookshelf MossyRocks   Obsidian
               Stone, Gray,      Gray, Red,  Red, Wood,     Cobblestone, Black,
            // CobbleSlab   Rope      Sandstone Snow Fire  LightPink ForestGreen Brown
               Cobblestone, Mushroom, Sand,     Air, Lava, Pink,     Green,      Dirt,
            // DeepBlue Turquoise Ice    CeramicTile Magma        Pillar Crate StoneBrick
               Blue,    Cyan,     Glass, Gold,       Cobblestone, White, Wood, Stone
        };
        static readonly byte[] v5_fallback = {
            // Red   Orange Yellow Lime  Green Teal  Aqua  Cyan
               Sand, Sand,  Sand,  Sand, Sand, Sand, Sand, Sand,
            // Blue  Indigo Violet Magenta Pink  Black  Gray   White
               Sand, Sand,  Sand,  Sand,   Sand, Stone, Stone, Sand,
            // Dandelion Rose     BrownShroom RedShroom Gold
               Sapling,  Sapling, Sapling,    Sapling,  Sponge,
            // Iron   DoubleSlab Slab   Brick        TNT   Bookshelf MossyRocks   Obsidian
               Stone, Stone,     Stone, Cobblestone, Sand, Wood,     Cobblestone, Cobblestone,
            // CobbleSlab   Rope     Sandstone Snow Fire  LightPink ForestGreen Brown
               Cobblestone, Sapling, Sand,     Air, Lava, Sand,     Sand,       Dirt,
            // DeepBlue Turquoise Ice    CeramicTile Magma        Pillar Crate StoneBrick
               Sand,    Sand,     Glass, Stone,      Cobblestone, Stone, Wood, Stone
        };
        static readonly byte[] v4_fallback = {
            // Sponge   Glass
               GoldOre, Leaves,
            // Red   Orange Yellow Lime  Green Teal  Aqua  Cyan
               Sand, Sand,  Sand,  Sand, Sand, Sand, Sand, Sand,
            // Blue  Indigo Violet Magenta Pink  Black  Gray   White
               Sand, Sand,  Sand,  Sand,   Sand, Stone, Stone, Sand,
            // Dandelion Rose     BrownShroom RedShroom Gold
               Sapling,  Sapling, Sapling,    Sapling,  GoldOre,
            // Iron   DoubleSlab Slab   Brick        TNT   Bookshelf MossyRocks   Obsidian
               Stone, Stone,     Stone, Cobblestone, Sand, Wood,     Cobblestone, Cobblestone,
            // CobbleSlab   Rope     Sandstone Snow Fire  LightPink ForestGreen Brown
               Cobblestone, Sapling, Sand,     Air, Lava, Sand,     Sand,       Dirt,
            // DeepBlue Turquoise Ice     CeramicTile Magma        Pillar Crate StoneBrick
               Sand,    Sand,     Leaves, Stone,      Cobblestone, Stone, Wood, Stone
        };


        /// <summary> Converts physics block IDs to their visual block IDs </summary>
        /// <remarks> If block ID is not converted, returns input block ID </remarks>
        /// <example> Op_Glass becomes Glass, Door_Log becomes Log </example>
        public static ushort Convert(ushort block)
        {
            return block switch
            {
                FlagBase => Mushroom,
                Op_Glass => Glass,
                Op_Obsidian => Obsidian,
                Op_Brick => Brick,
                Op_Stone => Stone,
                Op_Cobblestone => Cobblestone,
                Op_Air => Air,//Must be cuboided / replaced
                Op_Water => StillWater,
                Op_Lava => StillLava,
                108 => Cobblestone,
                LavaSponge => Sponge,
                FloatWood => Wood,
                FastLava => Lava,
                71 or 72 => White,
                Door_Log => Log,
                Door_Obsidian => Obsidian,
                Door_Glass => Glass,
                Door_Stone => Stone,
                Door_Leaves => Leaves,
                Door_Sand => Sand,
                Door_Wood => Wood,
                Door_Green => Green,
                Door_TNT => TNT,
                Door_Slab => Slab,
                Door_Iron => Iron,
                Door_Dirt => Dirt,
                Door_Grass => Grass,
                Door_Blue => Blue,
                Door_Bookshelf => Bookshelf,
                Door_Gold => Gold,
                Door_Cobblestone => Cobblestone,
                Door_Red => Red,
                Door_Orange => Orange,
                Door_Yellow => Yellow,
                Door_Lime => Lime,
                Door_Teal => Teal,
                Door_Aqua => Aqua,
                Door_Cyan => Cyan,
                Door_Indigo => Indigo,
                Door_Purple => Violet,
                Door_Magenta => Magenta,
                Door_Pink => Pink,
                Door_Black => Black,
                Door_Gray => Gray,
                Door_White => White,
                tDoor_Log => Log,
                tDoor_Obsidian => Obsidian,
                tDoor_Glass => Glass,
                tDoor_Stone => Stone,
                tDoor_Leaves => Leaves,
                tDoor_Sand => Sand,
                tDoor_Wood => Wood,
                tDoor_Green => Green,
                tDoor_TNT => TNT,
                tDoor_Slab => Slab,
                tDoor_Air => Air,
                tDoor_Water => StillWater,
                tDoor_Lava => StillLava,
                oDoor_Log => Log,
                oDoor_Obsidian => Obsidian,
                oDoor_Glass => Glass,
                oDoor_Stone => Stone,
                oDoor_Leaves => Leaves,
                oDoor_Sand => Sand,
                oDoor_Wood => Wood,
                oDoor_Green => Green,
                oDoor_TNT => TNT,
                oDoor_Slab => Slab,
                oDoor_Lava => StillLava,
                oDoor_Water => StillWater,
                MB_White => White,
                MB_Black => Black,
                MB_Air => Air,
                MB_Water => StillWater,
                MB_Lava => StillLava,
                WaterDown => Water,
                LavaDown => Lava,
                WaterFaucet => Aqua,
                LavaFaucet => Orange,
                FiniteWater => Water,
                FiniteLava => Lava,
                FiniteFaucet => Cyan,
                Portal_Air => Air,
                Portal_Water => StillWater,
                Portal_Lava => StillLava,
                Door_Air => Air,
                Door_AirActivatable => Air,
                Door_Water => StillWater,
                Door_Lava => StillLava,
                Portal_Blue => Cyan,
                Portal_Orange => Orange,
                C4 => TNT,
                C4Detonator => Red,
                TNT_Small => TNT,
                TNT_Big => TNT,
                TNT_Explosion => Lava,
                LavaFire => Lava,
                TNT_Nuke => TNT,
                RocketStart => Glass,
                RocketHead => Gold,
                Fireworks => Iron,
                Deadly_Water => StillWater,
                Deadly_Lava => StillLava,
                Deadly_Air => Air,
                Deadly_ActiveWater => Water,
                Deadly_ActiveLava => Lava,
                Deadly_FastLava => Lava,
                Magma => Lava,
                Geyser => Water,
                Checkpoint => Air,
                Air_Flood or Door_Log_air or Air_FloodLayer or Air_FloodDown or Air_FloodUp or 205 or 206 or 207 or 208 or 209 or 210 or 213 or 214 or 215 or 216 or Door_Air_air or 225 or 254 or 81 or 226 or 227 or 228 or 229 or 84 or 66 or 67 or 68 or 69 => Air,
                Door_Green_air => Red,
                Door_TNT_air => Lava,
                oDoor_Log_air or oDoor_Obsidian_air or oDoor_Glass_air or oDoor_Stone_air or oDoor_Leaves_air or oDoor_Sand_air or oDoor_Wood_air or oDoor_Slab_air or oDoor_Lava_air or oDoor_Water_air => Air,
                oDoor_Green_air => Red,
                oDoor_TNT_air => StillLava,
                Train => Aqua,
                Snake => Black,
                SnakeTail => CoalOre,
                Creeper => TNT,
                ZombieBody => MossyRocks,
                ZombieHead => Lime,
                Bird_White => White,
                Bird_Black => Black,
                Bird_Lava => Lava,
                Bird_Red => Red,
                Bird_Water => Water,
                Bird_Blue => Blue,
                Bird_Killer => Lava,
                Fish_Betta => Blue,
                Fish_Gold => Gold,
                Fish_Salmon => Red,
                Fish_Shark => Gray,
                Fish_Sponge => Sponge,
                Fish_LavaShark => Obsidian,
                _ => block,
            };
        }
    }
}
