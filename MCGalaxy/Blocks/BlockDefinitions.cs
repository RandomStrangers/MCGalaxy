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
using MCGalaxy.Config;
using MCGalaxy.Network;
using System;
using System.IO;
namespace MCGalaxy
{
    public sealed class BlockDefinition
    {
        [ConfigUShort("BlockID", null)]
        public ushort RawID;
        [ConfigString] public string Name;
        [ConfigFloat] public float Speed;
        [ConfigByte] public byte CollideType;
        [ConfigUShort] public ushort TopTex;
        [ConfigUShort] public ushort BottomTex;
        [ConfigBool] public bool BlocksLight;
        [ConfigByte] public byte WalkSound;
        [ConfigBool] public bool FullBright;
        [ConfigByte] public byte Shape;
        [ConfigByte] public byte BlockDraw;
        [ConfigByte] public byte FallBack;
        [ConfigByte] public byte FogDensity;
        [ConfigByte] public byte FogR;
        [ConfigByte] public byte FogG;
        [ConfigByte] public byte FogB;
        [ConfigByte] public byte MinX;
        [ConfigByte] public byte MinY;
        [ConfigByte] public byte MinZ;
        [ConfigByte] public byte MaxX;
        [ConfigByte] public byte MaxY;
        [ConfigByte] public byte MaxZ;
        [ConfigUShort] public ushort LeftTex;
        [ConfigUShort] public ushort RightTex;
        [ConfigUShort] public ushort FrontTex;
        [ConfigUShort] public ushort BackTex;
        [ConfigInt(null, null, -1, -1)]
        public int InventoryOrder = -1;
        [ConfigInt(null, null, -1, -1, 15)] public int Brightness = -1;
        [ConfigBool] public bool UseLampBrightness;
        public ushort GetBlock() => Block.FromRaw(RawID);
        public void SetBlock(ushort b) => RawID = Block.ToRaw(b);
        public static BlockDefinition[] GlobalDefs;
        public BlockDefinition Copy()
        {
            BlockDefinition def = new()
            {
                RawID = RawID,
                Name = Name,
                Speed = Speed,
                CollideType = CollideType,
                TopTex = TopTex,
                BottomTex = BottomTex,
                BlocksLight = BlocksLight,
                WalkSound = WalkSound,
                FullBright = FullBright,
                Shape = Shape,
                BlockDraw = BlockDraw,
                FallBack = FallBack,
                FogDensity = FogDensity,
                FogR = FogR,
                FogG = FogG,
                FogB = FogB,
                MinX = MinX,
                MinY = MinY,
                MinZ = MinZ,
                MaxX = MaxX,
                MaxY = MaxY,
                MaxZ = MaxZ,
                LeftTex = LeftTex,
                RightTex = RightTex,
                FrontTex = FrontTex,
                BackTex = BackTex,
                InventoryOrder = InventoryOrder,
                Brightness = Brightness,
                UseLampBrightness = UseLampBrightness
            };
            return def;
        }
        static ConfigElement[] elems;
        public static BlockDefinition[] Load(string path)
        {
            BlockDefinition[] defs = new BlockDefinition[1024];
            if (!File.Exists(path)) return defs;
            elems ??= ConfigElement.GetAll(typeof(BlockDefinition));
            try
            {
                string json = FileIO.TryReadAllText(path);
                JsonReader reader = new(json)
                {
                    OnMember = (obj, key, value) =>
                    {
                        obj.Meta ??= new BlockDefinition();
                        ConfigElement.Parse(elems, obj.Meta, key, (string)value);
                    }
                };
                JsonArray array = (JsonArray)reader.Parse();
                if (array == null) return defs;
                foreach (object raw in array)
                {
                    JsonObject obj = (JsonObject)raw;
                    if (obj == null || obj.Meta == null) continue;
                    BlockDefinition def = (BlockDefinition)obj.Meta;
                    if (string.IsNullOrEmpty(def.Name)) continue;
                    ushort block = def.GetBlock();
                    if (block >= defs.Length)
                        Logger.Log(LogType.Warning, "Invalid block ID: " + def.RawID);
                    else
                        defs[block] = def;
                    def.FallBack = Math.Min(def.FallBack, (byte)65);
                    if (def.Brightness == -1)
                        def.Brightness = def.FullBright ? 15 : 0;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Loading block defs from " + path, ex);
            }
            return defs;
        }
        public static void Save(bool global, Level lvl)
        {
            string path = global ? "blockdefs/global.json" : Paths.MapBlockDefs(lvl.MapName);
            BlockDefinition[] defs = global ? GlobalDefs : lvl.CustomBlockDefs;
            Save(global, defs, path);
        }
        public static void Save(bool global, BlockDefinition[] defs, string path)
        {
            elems ??= ConfigElement.GetAll(typeof(BlockDefinition));
            lock (defs) SaveCore(global, defs, path);
        }
        static void SaveCore(bool global, BlockDefinition[] defs, string path)
        {
            string separator = null;
            using StreamWriter w = FileIO.CreateGuarded(path);
            w.WriteLine("[");
            JsonConfigWriter ser = new(w, elems);
            for (int i = 0; i < defs.Length; i++)
            {
                BlockDefinition def = defs[i];
                if (!global && def == GlobalDefs[i]) def = null;
                if (def == null) continue;
                w.Write(separator);
                ser.WriteObject(def);
                separator = ",\r\n";
            }
            w.WriteLine("]");
        }
        public static void LoadGlobal()
        {
            BlockDefinition[] oldDefs = GlobalDefs;
            GlobalDefs = Load("blockdefs/global.json");
            GlobalDefs[Block.Air] = null;
            try
            {
                FileIO.TryCopy("blockdefs/global.json", "blockdefs/global.json.bak", true);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error backing up global block defs", ex);
            }
            if (oldDefs != null) UpdateLoadedLevels(oldDefs);
        }
        static void UpdateLoadedLevels(BlockDefinition[] oldGlobalDefs)
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                for (int b = 0; b < lvl.CustomBlockDefs.Length; b++)
                {
                    if (lvl.CustomBlockDefs[b] != oldGlobalDefs[b]) continue;
                    if ((lvl.Props[b].ChangedScope & 2) == 0)
                        lvl.Props[b] = Block.Props[b];
                    lvl.UpdateCustomBlock((ushort)b, GlobalDefs[b]);
                }
            }
        }
        public static void Add(BlockDefinition def, BlockDefinition[] defs, Level level)
        {
            ushort block = def.GetBlock();
            bool global = defs == GlobalDefs;
            if (global) UpdateGlobalCustom(block, def);
            defs[block] = def;
            if (global) BlockNames.UpdateCore();
            if (!global) level.UpdateCustomBlock(block, def);
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players)
            {
                if (!global && pl.Level != level) continue;
                if (global && pl.Level.CustomBlockDefs[block] != GlobalDefs[block]) continue;
                pl.Session.SendDefineBlock(def);
            }
        }
        public static void Remove(BlockDefinition def, BlockDefinition[] defs, Level level)
        {
            ushort block = def.GetBlock();
            bool global = defs == GlobalDefs;
            if (global) UpdateGlobalCustom(block, null);
            defs[block] = null;
            if (global) BlockNames.UpdateCore();
            if (!global) level.UpdateCustomBlock(block, null);
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players)
            {
                if (!global && pl.Level != level) continue;
                if (global && pl.Level.CustomBlockDefs[block] != null) continue;
                pl.Session.SendUndefineBlock(def);
            }
        }
        public static void UpdateOrder(BlockDefinition def, bool global, Level level)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players)
            {
                if (!global && pl.Level != level) continue;
                if (!pl.Supports(CpeExt.InventoryOrder) || def.RawID > pl.Session.MaxRawBlock) continue;
                SendLevelInventoryOrder(pl);
            }
        }
        static void UpdateGlobalCustom(ushort block, BlockDefinition def)
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            foreach (Level lvl in loaded)
            {
                if (lvl.CustomBlockDefs[block] != GlobalDefs[block]) continue;
                lvl.UpdateCustomBlock(block, def);
            }
        }
        public void SetAllTex(ushort id)
        {
            SetSideTex(id);
            TopTex = id; 
            BottomTex = id;
        }
        public void SetSideTex(ushort id)
        {
            LeftTex = id; 
            RightTex = id; 
            FrontTex = id; 
            BackTex = id;
        }
        public void SetFullBright(bool fullBright) => SetBrightness(fullBright ? 15 : 0, false);
        public void SetBrightness(int brightness, bool lamp)
        {
            Brightness = brightness;
            UseLampBrightness = lamp;
            FullBright = Brightness switch
            {
                > 0 => true,
                _ => false,
            };
        }
        internal static void SendLevelCustomBlocks(Player pl)
        {
            BlockDefinition[] defs = pl.Level.CustomBlockDefs;
            for (int i = 0; i < defs.Length; i++)
            {
                BlockDefinition def = defs[i];
                if (def == null) continue;
                pl.Session.SendDefineBlock(def);
            }
        }
        internal static unsafe void SendLevelInventoryOrder(Player pl)
        {
            BlockDefinition[] defs = pl.Level.CustomBlockDefs;
            int maxRaw = pl.Session.MaxRawBlock,
                count = maxRaw + 1;
            int* order_to_blocks = stackalloc int[1024],
                block_to_orders = stackalloc int[1024];
            for (int b = 0; b < 1024; b++)
            {
                order_to_blocks[b] = -1;
                block_to_orders[b] = -1;
            }
            for (int i = 0; i < defs.Length; i++)
            {
                BlockDefinition def = defs[i];
                if (def == null || def.RawID > maxRaw || def.InventoryOrder == -1) continue;
                if (def.InventoryOrder != 0)
                {
                    if (order_to_blocks[def.InventoryOrder] != -1) continue;
                    order_to_blocks[def.InventoryOrder] = def.RawID;
                }
                block_to_orders[def.RawID] = def.InventoryOrder;
            }
            for (int i = 0; i < defs.Length; i++)
            {
                BlockDefinition def = defs[i];
                int raw = def != null ? def.RawID : i;
                if (raw > maxRaw || (def == null && raw >= 66) || def != null && def.InventoryOrder >= 0) continue;
                if (order_to_blocks[raw] == -1)
                {
                    order_to_blocks[raw] = raw;
                    block_to_orders[raw] = raw;
                }
            }
            for (int i = defs.Length - 1; i >= 0; i--)
            {
                BlockDefinition def = defs[i];
                int raw = def != null ? def.RawID : i;
                if (raw > maxRaw || (def == null && raw >= 66) || block_to_orders[raw] != -1) continue;
                for (int slot = count - 1; slot >= 1; slot--)
                {
                    if (order_to_blocks[slot] != -1) continue;
                    block_to_orders[raw] = slot;
                    order_to_blocks[slot] = raw;
                    break;
                }
            }
            for (int raw = 0; raw < count; raw++)
            {
                int order = block_to_orders[raw];
                if (order == -1) order = 0;
                BlockDefinition def = defs[Block.FromRaw((ushort)raw)];
                if (def == null && raw >= 66 || raw == 255 && def.InventoryOrder == -1) continue;
                pl.Send(Packet.SetInventoryOrder((ushort)raw, (ushort)order, pl.Session.hasExtBlocks));
            }
        }
        public static void UpdateFallback(bool global, ushort block, Level level)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players)
            {
                if (!global && pl.Level != level || pl.Session.hasBlockDefs || block >= 66 && !pl.Level.MightHaveCustomBlocks()) continue;
                PlayerActions.ReloadMap(pl);
            }
        }
        public static BlockDefinition ParseName(string name, BlockDefinition[] defs)
        {
            for (int b = 1; b < defs.Length; b++)
            {
                BlockDefinition def = defs[b];
                if (def == null) continue;
                if (def.Name.Replace(" ", "").CaselessEq(name)) return def;
            }
            return null;
        }
    }
}