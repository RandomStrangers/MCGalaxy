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
        public static bool Undefined(ushort block) => IsPhysicsType(block) && BlockNames.coreNames[block].CaselessEq("unknown");
        public static bool ExistsFor(Player p, ushort b) => b < 256 ? !Undefined(b) : !p.IsSuper ? p.Level.GetBlockDef(b) != null : BlockDefinition.GlobalDefs[b] != null;
        public static string GetName(Player p, ushort block)
        {
            if (IsPhysicsType(block)) return BlockNames.coreNames[block];
            BlockDefinition def = !p.IsSuper ? p.Level.GetBlockDef(block) : BlockDefinition.GlobalDefs[block];
            return def != null ? def.Name.Replace(" ", "") : block < 66 ? BlockNames.coreNames[block] : ToRaw(block).ToString();
        }
        public static ushort Parse(Player p, string input)
        {
            BlockDefinition[] defs = p.IsSuper ? BlockDefinition.GlobalDefs : p.Level.CustomBlockDefs;
            if (ushort.TryParse(input, out ushort block) && (block < 66 || (block <= 767 && defs[FromRaw(block)] != null)))
                return FromRaw(block);
            BlockDefinition def = BlockDefinition.ParseName(input, defs);
            if (def != null) return def.GetBlock();
            bool success = BlockNames.Aliases.TryGetValue(input.ToLower(), out byte coreID);
            return success ? coreID : Invalid;
        }
        public static string GetColoredName(Player p, ushort block) => Group.GetColor(BlockPerms.GetPlace(block).MinRank) + GetName(p, block);
        public static byte ConvertClassic(byte block, byte protocolVersion) => protocolVersion switch
        {
            >= 7 => block <= Obsidian ? block : v7_fallback[block - CobblestoneSlab],
            >= 6 => block <= Gold ? block : v6_fallback[block - Iron],
            >= 5 => block <= Glass ? block : v5_fallback[block - Red],
            _ => block <= Leaves ? block : v4_fallback[block - Sponge]
        };
        static readonly byte[] v7_fallback = {
               Slab,      Mushroom, Sand,     Air, Lava, Pink,     Green,      Dirt,
               Blue,    Cyan,     Glass, Iron,       Obsidian, White, Wood, Stone
        };
        static readonly byte[] v6_fallback = {
               Stone, Gray,      Gray, Red,  Red, Wood,     Cobblestone, Black,
               Cobblestone, Mushroom, Sand,     Air, Lava, Pink,     Green,      Dirt,
               Blue,    Cyan,     Glass, Gold,       Cobblestone, White, Wood, Stone
        };
        static readonly byte[] v5_fallback = {
               Sand, Sand,  Sand,  Sand, Sand, Sand, Sand, Sand,
               Sand, Sand,  Sand,  Sand,   Sand, Stone, Stone, Sand,
               Sapling,  Sapling, Sapling,    Sapling,  Sponge,
               Stone, Stone,     Stone, Cobblestone, Sand, Wood,     Cobblestone, Cobblestone,
               Cobblestone, Sapling, Sand,     Air, Lava, Sand,     Sand,       Dirt,
               Sand,    Sand,     Glass, Stone,      Cobblestone, Stone, Wood, Stone
        };
        static readonly byte[] v4_fallback = {
               GoldOre, Leaves,
               Sand, Sand,  Sand,  Sand, Sand, Sand, Sand, Sand,
               Sand, Sand,  Sand,  Sand,   Sand, Stone, Stone, Sand,
               Sapling,  Sapling, Sapling,    Sapling,  GoldOre,
               Stone, Stone,     Stone, Cobblestone, Sand, Wood,     Cobblestone, Cobblestone,
               Cobblestone, Sapling, Sand,     Air, Lava, Sand,     Sand,       Dirt,
               Sand,    Sand,     Leaves, Stone,      Cobblestone, Stone, Wood, Stone
        };
        public static ushort Convert(ushort block) => block switch
        {
            70 => 39,
            100 => 20,
            101 => 49,
            102 => 45,
            103 => 1,
            104 => 4,
            106 => 9,
            107 => 11,
            108 => 4,
            109 => 19,
            110 => 5,
            112 => 10,
            71 or 72 => 36,
            111 => 17,
            113 => 49,
            114 => 20,
            115 => 1,
            116 => 18,
            117 => 12,
            118 => 5,
            119 => 25,
            120 => 46,
            121 => 44,
            220 => 42,
            221 => 3,
            222 => 2,
            223 => 29,
            224 => 47,
            253 => 41,
            80 => 4,
            83 => 21,
            85 => 22,
            86 => 23,
            87 => 24,
            89 => 26,
            90 => 27,
            91 => 28,
            92 => 30,
            93 => 31,
            94 => 32,
            95 => 33,
            96 => 34,
            97 => 35,
            98 => 36,
            122 => 17,
            123 => 49,
            124 => 20,
            125 => 1,
            126 => 18,
            127 => 12,
            128 => 5,
            129 => 25,
            135 => 46,
            136 => 44,
            138 => 9,
            139 => 11,
            148 => 17,
            149 => 49,
            150 => 20,
            151 => 1,
            152 => 18,
            153 => 12,
            154 => 5,
            155 => 25,
            156 => 46,
            157 => 44,
            158 => 11,
            159 => 9,
            130 => 36,
            131 => 34,
            133 => 9,
            134 => 11,
            140 or 193 or 196 or 237 or 145 => 8,
            141 => 10,
            143 => 27,
            144 => 22,
            146 => 10,
            147 => 28,
            161 => 9,
            162 => 11,
            166 => 9,
            167 => 11,
            175 => 28,
            176 => 22,
            74 => 46,
            75 => 21,
            182 => 46,
            183 => 46,
            184 => 10,
            185 => 10,
            186 => 46,
            187 => 20,
            188 => 41,
            189 => 42,
            191 => 9,
            190 => 11,
            194 => 10,
            73 => 10,
            195 => 10,
            197 or 200 or 201 or 202 or 203 or 204
                or 205 or 206 or 207 or 208 or 209
                or 210 or 213 or 214 or 215 or 216
                or 217 or 225 or 254 or 81 or 226
                or 227 or 228 or 229 or 84 or 66
                or 67 or 68 or 69 or 137 or 105
                or 132 or 160 or 165 or 164 or 192
                or 168 or 169 or 170 or 171 or 172
                or 173 or 174 or 179 or 180 or 181 => 0,
            211 => 21,
            212 => 10,
            177 => 21,
            178 => 11,
            230 => 27,
            251 => 34,
            252 => 16,
            231 => 46,
            232 => 48,
            233 => 24,
            235 => 36,
            236 => 34,
            238 => 10,
            239 => 21,
            240 => 29,
            242 => 10,
            249 => 29,
            245 => 41,
            248 => 21,
            247 => 35,
            246 => 19,
            250 => 49,
            _ => block,
        };
    }
}