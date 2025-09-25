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
using MCGalaxy.Maths;
using System.IO;


namespace MCGalaxy
{
    public static partial class Block
    {
        public static bool Walkthrough(ushort block)
        {
            return block == Air || block == Sapling || block == Snow
                || block == Fire || block == Rope
                || (block >= Water && block <= StillLava)
                || (block >= Dandelion && block <= RedMushroom);
        }

        public static bool AllowBreak(ushort block)
        {
            return block switch
            {
                Portal_Blue or Portal_Orange or MB_White or MB_Black or Door_Log or Door_Obsidian or Door_Glass or Door_Stone or Door_Leaves or Door_Sand or Door_Wood or Door_Green or Door_TNT or Door_Slab or Door_Iron or Door_Gold or Door_Dirt or Door_Grass or Door_Blue or Door_Bookshelf or Door_Cobblestone or Door_Red or Door_Orange or Door_Yellow or Door_Lime or Door_Teal or Door_Aqua or Door_Cyan or Door_Indigo or Door_Purple or Door_Magenta or Door_Pink or Door_Black or Door_Gray or Door_White or C4 or TNT_Small or TNT_Big or TNT_Nuke or RocketStart or Fireworks or ZombieBody or Creeper or ZombieHead => true,
                _ => false,
            };
        }

        public static bool LightPass(ushort block)
        {
            return Convert(block) switch
            {
                Air or Glass or Leaves or Rose or Dandelion or Mushroom or RedMushroom or Sapling or Rope => true,
                _ => false,
            };
        }

        public static bool NeedRestart(ushort block)
        {
            return block switch
            {
                Train or Snake or SnakeTail or Air_Flood or Air_FloodDown or Air_FloodUp or Air_FloodLayer or LavaFire or RocketHead or Fireworks or Creeper or ZombieBody or ZombieHead or Bird_Black or Bird_Blue or Bird_Killer or Bird_Lava or Bird_Red or Bird_Water or Bird_White or Fish_Betta or Fish_Gold or Fish_Salmon or Fish_Shark or Fish_LavaShark or Fish_Sponge or TNT_Explosion => true,
                _ => false,
            };
        }

        public static AABB BlockAABB(ushort block, Level lvl)
        {
            BlockDefinition def = lvl.GetBlockDef(block);
            if (def != null)
            {
                return new AABB(def.MinX * 2, def.MinZ * 2, def.MinY * 2,
                                def.MaxX * 2, def.MaxZ * 2, def.MaxY * 2);
            }

            if (block >= Extended) return new AABB(0, 0, 0, 32, 32, 32);
            ushort core = Convert(block);
            return new AABB(0, 0, 0, 32, DefaultSet.Height(core) * 2, 32);
        }

        public static void SetBlocks()
        {
            BlockProps[] props = Props;
            for (int b = 0; b < props.Length; b++)
            {
                props[b] = MakeDefaultProps((ushort)b);
            }

            SetDefaultNames();
            string propsPath = Paths.BlockPropsPath("default");

            // backwards compatibility with older versions
            if (!File.Exists(propsPath))
            {
                BlockProps.Load("core", Props, 1, false);
                BlockProps.Load("global", Props, 1, true);
            }
            else
            {
                BlockProps.Load("default", Props, 1, false);
            }

            UpdateLoadedLevels();
        }

        public static void UpdateLoadedLevels()
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                lvl.UpdateBlockProps();
                lvl.UpdateAllBlockHandlers();
            }
        }

        /// <summary> Converts a raw/client block ID to a server block ID </summary>
        public static ushort FromRaw(ushort raw)
        {
            return raw < CPE_COUNT ? raw : (ushort)(raw + Extended);
        }

        /// <summary> Converts a server block ID to a raw/client block ID </summary>
        /// <remarks> Undefined behaviour for physics block IDs </remarks>
        public static ushort ToRaw(ushort raw)
        {
            return raw < CPE_COUNT ? raw : (ushort)(raw - Extended);
        }

        public static ushort MapOldRaw(ushort raw)
        {
            // old raw form was: 0 - 65 core block ids, 66 - 255 custom block ids
            // 256+ remain unchanged
            return IsPhysicsType(raw) ? ((ushort)(raw + Extended)) : raw;
        }

        public static bool IsPhysicsType(ushort block)
        {
            return block >= CPE_COUNT && block < Extended;
        }

        public static bool VisuallyEquals(ushort a, ushort b)
        {
            return Convert(a) == Convert(b);
        }
    }
}
