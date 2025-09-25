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
using System;
using System.Collections.Generic;
using System.IO;


namespace MCGalaxy.Blocks
{
    /// <summary> Represents which ranks are allowed (and which are disallowed) to use a block. </summary>
    public sealed class BlockPerms : ItemPerms
    {
        public ushort ID;
        public override string ItemName { get { return ID.ToString(); } }

        static readonly BlockPerms[] PlaceList = new BlockPerms[Block.SUPPORTED_COUNT];
        static readonly BlockPerms[] DeleteList = new BlockPerms[Block.SUPPORTED_COUNT];


        public BlockPerms(ushort id, LevelPermission min) : base(min)
        {
            ID = id;
        }

        public BlockPerms Copy()
        {
            BlockPerms copy = new(ID, 0);
            CopyPermissionsTo(copy); return copy;
        }


        public static BlockPerms GetPlace(ushort b) { return PlaceList[b]; }
        public static BlockPerms GetDelete(ushort b) { return DeleteList[b]; }


        public static void ResendAllBlockPermissions()
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) { pl.SendCurrentBlockPermissions(); }
        }

        public void MessageCannotUse(Player p, string action)
        {
            p.Message("Only {0} can {1} {2}",
                      Describe(), action, Block.GetName(p, ID));
        }


        static readonly object ioLock = new();
        /// <summary> Saves list of block permissions to disc. </summary>
        public static void Save()
        {
            try
            {
                lock (ioLock) SaveCore();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving block perms", ex);
            }
        }

        static void SaveCore()
        {
            SaveList(Paths.PlacePermsFile, PlaceList, "use");
            SaveList(Paths.DeletePermsFile, DeleteList, "delete");
        }

        static void SaveList(string path, BlockPerms[] list, string action)
        {
            using StreamWriter w = FileIO.CreateGuarded(path);
            WriteHeader(w, "block", "each block", "Block ID", "lava", action);

            foreach (BlockPerms perms in list)
            {
                if (Block.Undefined(perms.ID)) continue;
                w.WriteLine(perms.Serialise());
            }
        }


        /// <summary> Applies new block permissions to server state. </summary>
        public static void ApplyChanges()
        {
            foreach (Group grp in Group.AllRanks)
            {
                SetUsable(grp);
            }
        }

        public static void SetUsable(Group grp)
        {
            SetUsableList(PlaceList, grp.CanPlace, grp);
            SetUsableList(DeleteList, grp.CanDelete, grp);
        }

        static void SetUsableList(BlockPerms[] list, bool[] permsList, Group grp)
        {
            foreach (BlockPerms perms in list)
            {
                permsList[perms.ID] = perms.UsableBy(grp.Permission);
            }
        }


        /// <summary> Loads list of block permissions from disc. </summary>
        public static void Load()
        {
            lock (ioLock) LoadCore();
            ApplyChanges();
        }

        static void LoadCore()
        {
            SetDefaultPerms();
            bool placeExists = File.Exists(Paths.PlacePermsFile);
            bool deleteExists = File.Exists(Paths.DeletePermsFile);

            if (placeExists) LoadFile(Paths.PlacePermsFile, PlaceList);
            if (deleteExists) LoadFile(Paths.DeletePermsFile, DeleteList);
            if (placeExists || deleteExists) return;

            if (File.Exists(Paths.BlockPermsFile))
            {
                LoadFile(Paths.BlockPermsFile, PlaceList);

                for (int i = 0; i < Block.SUPPORTED_COUNT; i++)
                    PlaceList[i].CopyPermissionsTo(DeleteList[i]);
                SetDefaultSpecialDeletePerms();

                try
                {
                    //File.Move(Paths.BlockPermsFile, Paths.BlockPermsFile + ".bak"); 
                    FileIO.TryMove(Paths.BlockPermsFile, Paths.BlockPermsFile + ".bak");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Moving old block.properties file", ex);
                }
            }
            Save();
        }

        static void LoadFile(string path, BlockPerms[] list)
        {
            using StreamReader r = new(path);
            ProcessLines(r, list);
        }

        static void ProcessLines(StreamReader r, BlockPerms[] list)
        {
            string[] args = new string[4];
            string line;

            while ((line = r.ReadLine()) != null)
            {
                if (line.IsCommentLine()) continue;
                // Format - ID : Lowest : Disallow : Allow
                line.Replace(" ", "").FixedSplit(args, ':');

                if (!ushort.TryParse(args[0], out ushort block))
                {
                    // Old format - Name : Lowest : Disallow : Allow
                    block = Block.Parse(Player.Console, args[0]);
                }
                if (block == Block.Invalid) continue;

                try
                {

                    Deserialise(args, 1, out LevelPermission min, out List<LevelPermission> allowed, out List<LevelPermission> disallowed);
                    Set(block, min, list, allowed, disallowed);
                }
                catch
                {
                    Logger.Log(LogType.Warning, "Hit an error on the block " + line);
                    continue;
                }
            }
        }

        static void Set(ushort b, LevelPermission min, BlockPerms[] list,
                        List<LevelPermission> allowed, List<LevelPermission> disallowed)
        {
            BlockPerms perms = list[b];
            if (perms == null)
            {
                perms = new BlockPerms(b, min);
                list[b] = perms;
            }
            perms.Init(min, allowed, disallowed);
        }


        static void SetDefaultPerms()
        {
            for (ushort block = 0; block < Block.SUPPORTED_COUNT; block++)
            {
                BlockProps props = Block.Props[block];
                LevelPermission min;

                if (block == Block.Invalid)
                {
                    min = LevelPermission.Admin;
                }
                else if (props.OPBlock)
                {
                    min = LevelPermission.Operator;
                }
                else if (props.IsDoor || props.IsTDoor || props.oDoorBlock != Block.Invalid)
                {
                    min = LevelPermission.Builder;
                }
                else if (props.IsPortal || props.IsMessageBlock)
                {
                    min = LevelPermission.AdvBuilder;
                }
                else
                {
                    min = DefaultPerm(block);
                }

                Set(block, min, PlaceList, null, null);
                Set(block, min, DeleteList, null, null);
            }
            SetDefaultSpecialDeletePerms();
        }

        static void SetDefaultSpecialDeletePerms()
        {
            for (ushort b = Block.Water; b <= Block.StillLava; b++)
                DeleteList[b].MinRank = LevelPermission.Guest;
        }

        static LevelPermission DefaultPerm(ushort block)
        {
            return block switch
            {
                Block.Bedrock or Block.Air_Flood or Block.Air_FloodDown or Block.Air_FloodLayer or Block.Air_FloodUp or Block.TNT_Big or Block.TNT_Nuke or Block.RocketStart or Block.RocketHead or Block.Creeper or Block.ZombieBody or Block.ZombieHead or Block.Bird_Red or Block.Bird_Killer or Block.Bird_Blue or Block.Fish_Gold or Block.Fish_Sponge or Block.Fish_Shark or Block.Fish_Salmon or Block.Fish_Betta or Block.Fish_LavaShark or Block.Snake or Block.SnakeTail or Block.FlagBase => LevelPermission.Operator,
                Block.FloatWood or Block.LavaSponge or Block.Door_Log_air or Block.Door_Green_air or Block.Door_TNT_air or Block.Water or Block.Lava or Block.FastLava or Block.WaterDown or Block.LavaDown or Block.WaterFaucet or Block.LavaFaucet or Block.FiniteWater or Block.FiniteLava or Block.FiniteFaucet or Block.Magma or Block.Geyser or Block.Deadly_Lava or Block.Deadly_Water or Block.Deadly_Air or Block.Deadly_ActiveWater or Block.Deadly_ActiveLava or Block.Deadly_FastLava or Block.LavaFire or Block.C4 or Block.C4Detonator or Block.TNT_Small or Block.TNT_Explosion or Block.Fireworks or Block.Checkpoint or Block.Train or Block.Bird_White or Block.Bird_Black or Block.Bird_Water or Block.Bird_Lava => LevelPermission.AdvBuilder,
                _ => LevelPermission.Guest,
            };
        }
    }
}
