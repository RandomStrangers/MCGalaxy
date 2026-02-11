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
        public override string ItemName => ID.ToString();
        static readonly BlockPerms[] PlaceList = new BlockPerms[1024],
            DeleteList = new BlockPerms[1024];
        public BlockPerms(ushort id, LevelPermission min) : base(min) => ID = id;
        public static BlockPerms GetPlace(ushort b) => PlaceList[b];
        public static BlockPerms GetDelete(ushort b) => DeleteList[b];
        public static void ResendAllBlockPermissions()
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) 
            { 
                pl.SendCurrentBlockPermissions(); 
            }
        }
        public void MessageCannotUse(Player p, string action) => p.Message("Only {0} can {1} {2}",
                      Describe(), action, Block.GetName(p, ID));
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
            bool placeExists = File.Exists(Paths.PlacePermsFile),
                deleteExists = File.Exists(Paths.DeletePermsFile);
            if (placeExists) LoadFile(Paths.PlacePermsFile, PlaceList);
            if (deleteExists) LoadFile(Paths.DeletePermsFile, DeleteList);
            if (placeExists || deleteExists) return;
            if (File.Exists(Paths.BlockPermsFile))
            {
                LoadFile(Paths.BlockPermsFile, PlaceList);
                for (int i = 0; i < 1024; i++)
                    PlaceList[i].CopyPermissionsTo(DeleteList[i]);
                SetDefaultSpecialDeletePerms();
                try
                {
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
                line.Replace(" ", "").FixedSplit(args, ':');
                if (!ushort.TryParse(args[0], out ushort block))
                {
                    block = Block.Parse(Player.Console, args[0]);
                }
                if (block == 0xff) continue;
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
                perms = new(b, min);
                list[b] = perms;
            }
            perms.Init(min, allowed, disallowed);
        }
        static void SetDefaultPerms()
        {
            for (ushort block = 0; block < 1024; block++)
            {
                BlockProps props = Block.Props[block];
                LevelPermission min;
                if (block == 0xff)
                {
                    min = LevelPermission.Admin;
                }
                else if (props.OPBlock)
                {
                    min = LevelPermission.Operator;
                }
                else if (props.IsDoor || props.IsTDoor || props.oDoorBlock != 0xff)
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
            for (ushort b = 8; b <= 11; b++)
                DeleteList[b].MinRank = LevelPermission.Guest;
        }
        static LevelPermission DefaultPerm(ushort block) => block switch
        {
            7 or 200 or 203 or 202 or 204 or 183 or 186
            or 187 or 188 or 231 or 232 or 233 or 239 
            or 242 or 240 or 245 or 246 or 247 or 248 
            or 249 or 250 or 251 or 252 or 70 => LevelPermission.Operator,
            110 or 109 or 201 or 211 or 212 or 8 or 10 
            or 112 or 140 or 141 or 143 or 144 or 145 
            or 146 or 147 or 195 or 196 or 190 or 191 
            or 192 or 193 or 194 or 73 or 185 or 74 
            or 75 or 182 or 184 or 189 or 197 or 230 
            or 235 or 236 or 237 or 238 => LevelPermission.AdvBuilder,
            _ => LevelPermission.Guest,
        };
    }
}
