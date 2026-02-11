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
using MCGalaxy.Blocks;
using MCGalaxy.Blocks.Physics;
using MCGalaxy.DB;
using MCGalaxy.Maths;
using System;
namespace MCGalaxy
{
    /// <summary> Result of attempting to change a block to another </summary>
    public enum ChangeResult
    {
        /// <summary> Block change was not performed </summary>
        Unchanged,
        /// <summary> Old block was same as new block visually (e.g. white to door_white) </summary>
        VisuallySame,
        /// <summary> Old block was different to new block visually </summary>
        Modified
    }
    public sealed partial class Level : IDisposable
    {
        public byte[] blocks;
        /// <summary> Lazily allocated 16x16x16 chunks that store extended tile IDs </summary>
        /// <remarks> Access should be done through GetBlock or GetExtTile </remarks>
        public byte[][] CustomBlocks;
        /// <summary> Number of 16x16x16 chunks that this level consists of </summary>
        public int ChunksX, ChunksY, ChunksZ;
        /// <summary> Makes a quick guess at whether this level might use custom blocks </summary>
        public bool MightHaveCustomBlocks()
        {
            byte[][] customBlocks = CustomBlocks;
            if (customBlocks == null) return false;
            for (int i = 0; i < customBlocks.Length; i++)
            {
                if (customBlocks[i] != null) return true;
            }
            return false;
        }
        /// <summary> Converts the given coordinates to a position index </summary>
        /// <remarks> Returns -1 if coordinates outside this level's boundaries are given </remarks>
        public int PosToInt(ushort x, ushort y, ushort z) => x >= Width || y >= Height || z >= Length ? -1 : x + Width * (z + y * Length);
        /// <summary> Converts the given position index to coordinates </summary>
        /// <remarks> Undefined coordinates if given position index is invalid </remarks>
        public void IntToPos(int pos, out ushort x, out ushort y, out ushort z)
        {
            y = (ushort)(pos / Width / Length);
            pos -= y * Width * Length;
            z = (ushort)(pos / Width);
            pos -= z * Width;
            x = (ushort)pos;
        }
        /// <summary> Offsets the given position index by the given relative coordinates </summary>
        /// <example> index = lvl.IntOffset(index, 0, -1, 0); </example>
        public int IntOffset(int pos, int x, int y, int z) => pos + x + z * Width + y * Width * Length;
        /// <summary> Returns whether the given coordinates lie within this level's boundaries </summary>
        public bool IsValidPos(Vec3U16 pos) => pos.X < Width && pos.Y < Height && pos.Z < Length;
        /// <summary> Returns whether the given coordinates lie within this level's boundaries </summary>
        public bool IsValidPos(int x, int y, int z) => x >= 0 && y >= 0 && z >= 0 && x < Width && y < Height && z < Length;
        /// <summary> Gets the block at the given coordinates </summary>
        /// <returns> Undefined behaviour if coordinates are invalid </returns>
        public ushort FastGetBlock(int index) => Block.ExtendedBase[blocks[index]] == 0 ? blocks[index] : (ushort)(Block.ExtendedBase[blocks[index]] | GetExtTile(index));
        /// <summary> Gets the block at the given coordinates </summary>
        /// <returns> Undefined behaviour if coordinates are invalid </returns>
        public ushort FastGetBlock(ushort x, ushort y, ushort z) => Block.ExtendedBase[blocks[x + Width * (z + y * Length)]] == 0 ? blocks[x + Width * (z + y * Length)] : (ushort)(Block.ExtendedBase[blocks[x + Width * (z + y * Length)]] | FastGetExtTile(x, y, z));
        /// <summary> Gets the block at the given coordinates </summary>
        /// <returns> Block.Invalid if coordinates outside level </returns>
        public ushort GetBlock(ushort x, ushort y, ushort z)
        {
            if (x >= Width || y >= Height || z >= Length || blocks == null) return 0xff;
            byte raw = blocks[x + Width * (z + y * Length)];
            ushort extended = Block.ExtendedBase[raw];
            return extended == 0 ? raw : (ushort)(extended | FastGetExtTile(x, y, z));
        }
        /// <summary> Gets the block at the given coordinates </summary>
        /// <returns> Block.Invalid if coordinates outside level </returns>
        public ushort GetBlock(ushort x, ushort y, ushort z, out int index)
        {
            if (x >= Width || y >= Height || z >= Length || blocks == null) { index = -1; return 0xff; }
            index = x + Width * (z + y * Length);
            byte raw = blocks[index];
            ushort extended = Block.ExtendedBase[raw];
            return extended == 0 ? raw : (ushort)(extended | FastGetExtTile(x, y, z));
        }
        /// <summary> Gets whether the block at the given coordinates is air </summary>
        public bool IsAirAt(ushort x, ushort y, ushort z) => x < Width && y < Height && z < Length && blocks != null && blocks[x + Width * (z + y * Length)] == 0;
        /// <summary> Gets whether the block at the given coordinates is air </summary>
        public bool IsAirAt(ushort x, ushort y, ushort z, out int index)
        {
            if (x >= Width || y >= Height || z >= Length || blocks == null) 
            { 
                index = -1; 
                return false; 
            }
            index = x + Width * (z + y * Length);
            return blocks[index] == 0;
        }
        /// <summary> Gets the extended tile at the given position index </summary>
        /// <remarks> GetBlock / FastGetBlock is preferred over calling this method </remarks>
        public byte GetExtTile(int index)
        {
            IntToPos(index, out ushort x, out ushort y, out ushort z);
            int cx = x >> 4, cy = y >> 4, cz = z >> 4;
            byte[] chunk = CustomBlocks[(cy * ChunksZ + cz) * ChunksX + cx];
            return chunk == null ? (byte)0 : chunk[(y & 0x0F) << 8 | (z & 0x0F) << 4 | (x & 0x0F)];
        }
        /// <summary> Gets the extended tile at the given coordinates </summary>
        /// <remarks> GetBlock / FastGetBlock is preferred over calling this method </remarks>
        /// <returns> Undefined behaviour if coordinates are invalid. </returns>
        public byte FastGetExtTile(ushort x, ushort y, ushort z)
        {
            int cx = x >> 4, cy = y >> 4, cz = z >> 4;
            byte[] chunk = CustomBlocks[(cy * ChunksZ + cz) * ChunksX + cx];
            return chunk == null ? (byte)0 : chunk[(y & 0x0F) << 8 | (z & 0x0F) << 4 | (x & 0x0F)];
        }
        public void SetTile(ushort x, ushort y, ushort z, byte block)
        {
            int index = PosToInt(x, y, z);
            if (blocks == null || index < 0) return;
            blocks[index] = block;
            Changed = true;
        }
        public void FastSetExtTile(ushort x, ushort y, ushort z, byte extBlock)
        {
            int cx = x >> 4, cy = y >> 4, cz = z >> 4,
                cIndex = (cy * ChunksZ + cz) * ChunksX + cx;
            byte[] chunk = CustomBlocks[cIndex];
            if (chunk == null)
            {
                chunk = new byte[16 * 16 * 16];
                CustomBlocks[cIndex] = chunk;
            }
            chunk[(y & 0x0F) << 8 | (z & 0x0F) << 4 | (x & 0x0F)] = extBlock;
        }
        public void FastRevertExtTile(ushort x, ushort y, ushort z)
        {
            int cx = x >> 4, cy = y >> 4, cz = z >> 4,
                cIndex = (cy * ChunksZ + cz) * ChunksX + cx;
            byte[] chunk = CustomBlocks[cIndex];
            if (chunk == null) return;
            chunk[(y & 0x0F) << 8 | (z & 0x0F) << 4 | (x & 0x0F)] = 0;
        }
        public void SetBlock(ushort x, ushort y, ushort z, ushort block)
        {
            int index = PosToInt(x, y, z);
            if (blocks == null || index < 0) return;
            Changed = true;
            if (block >= 256)
            {
                blocks[index] = Block.ExtendedClass[block >> 8];
                FastSetExtTile(x, y, z, (byte)block);
            }
            else
            {
                blocks[index] = (byte)block;
            }
        }
        /// <summary> Returns the AccessController denying the player from changing blocks at the given coordinates. </summary>
        /// <remarks> If no AccessController denies the player, returns null. </remarks>
        public AccessController CanAffect(Player p, ushort x, ushort y, ushort z)
        {
            Zone[] zones = Zones.Items;
            if (zones.Length == 0) goto checkRank;
            for (int i = 0; i < zones.Length; i++)
            {
                Zone zn = zones[i];
                if (x < zn.MinX || x > zn.MaxX || y < zn.MinY || y > zn.MaxY || z < zn.MinZ || z > zn.MaxZ) continue;
                ZoneConfig cfg = zn.Config;
                if (cfg.BuildBlacklist.Count > 0 && cfg.BuildBlacklist.CaselessContains(p.name)) break;
                if (p.group.Permission >= cfg.BuildMin) return null;
                if (cfg.BuildWhitelist.Count > 0 && cfg.BuildWhitelist.CaselessContains(p.name)) return null;
            }
            for (int i = 0; i < zones.Length; i++)
            {
                Zone zn = zones[i];
                if (x < zn.MinX || x > zn.MaxX || y < zn.MinY || y > zn.MaxY || z < zn.MinZ || z > zn.MaxZ) continue;
                AccessResult access = zn.Access.Check(p.name, p.Rank);
                if (access == AccessResult.Accepted || access == AccessResult.Whitelisted) continue;
                return zn.Access;
            }
        checkRank:
            return p.Level == this ? p.AllowBuild ? null : BuildAccess : BuildAccess.CheckAllowed(p) ? null : BuildAccess;
        }
        public bool CheckAffect(Player p, ushort x, ushort y, ushort z, ushort old, ushort block)
        {
            if (!p.group.CanDelete[old] || !p.group.CanPlace[block]) return false;
            AccessController denier = CanAffect(p, x, y, z);
            if (denier == null) return true;
            if (p.lastAccessStatus < DateTime.UtcNow)
            {
                denier.CheckDetailed(p);
                p.lastAccessStatus = DateTime.UtcNow.AddSeconds(2);
            }
            return false;
        }
        /// <summary> Sends a block update packet to all players in this level. </summary>
        public void BroadcastChange(ushort x, ushort y, ushort z, ushort block)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (p.Level == this) p.SendBlockchange(x, y, z, block);
            }
        }
        public void Blockchange(Player p, ushort x, ushort y, ushort z, ushort block)
        {
            if (TryChangeBlock(p, x, y, z, block) == ChangeResult.Modified) BroadcastChange(x, y, z, block);
        }
        /// <summary> Performs a user like block change, but **DOES NOT** update the BlockDB. </summary>
        /// <remarks> The return code can be used to avoid sending redundant block changes. </remarks>
        /// <remarks> Does NOT send the changed block to any players - use BroadcastChange. </remarks>
        public ChangeResult TryChangeBlock(Player p, ushort x, ushort y, ushort z, ushort block, bool drawn = false)
        {
            string errorLocation = "start";
            try
            {
                ushort old = GetBlock(x, y, z);
                if (old == 0xff) return ChangeResult.Unchanged;
                errorLocation = "Permission checking";
                if (!CheckAffect(p, x, y, z, old, block)) return ChangeResult.Unchanged;
                if (old == block) return ChangeResult.Unchanged;
                if (old == 19 && LevelPhysics > 0 && block != 19)
                {
                    OtherPhysics.DoSpongeRemoved(this, PosToInt(x, y, z), false);
                }
                if (old == 109 && LevelPhysics > 0 && block != 109)
                {
                    OtherPhysics.DoSpongeRemoved(this, PosToInt(x, y, z), true);
                }
                p.TotalModified++;
                if (drawn) p.TotalDrawn++;
                else if (block == 0) p.TotalDeleted++;
                else p.TotalPlaced++;
                errorLocation = "Setting tile";
                if (block >= 256)
                {
                    SetTile(x, y, z, Block.ExtendedClass[block >> 8]);
                    FastSetExtTile(x, y, z, (byte)block);
                }
                else
                {
                    SetTile(x, y, z, (byte)block);
                    if (old >= 256)
                    {
                        FastRevertExtTile(x, y, z);
                    }
                }
                errorLocation = "Adding physics";
                if (LevelPhysics > 0 && ActivatesPhysics(block)) AddCheck(PosToInt(x, y, z));
                Changed = true;
                ChangedSinceBackup = true;
                return Block.VisuallyEquals(old, block) ? ChangeResult.VisuallySame : ChangeResult.Modified;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                Chat.MessageOps(p.name + " triggered a non-fatal error on " + ColoredName + ", &Sat location: " + errorLocation);
                Logger.Log(LogType.Warning, "{0} triggered a non-fatal error on {1}, &Sat location: {2}",
                           p.name, ColoredName, errorLocation);
                return 0;
            }
        }
        public void Blockchange(int b, ushort block, bool overRide = false,
                                PhysicsArgs data = default, bool addUndo = true)
        {
            if (!DoPhysicsBlockchange(b, block, overRide, data, addUndo)) return;
            IntToPos(b, out ushort x, out ushort y, out ushort z);
            BroadcastChange(x, y, z, block);
        }
        public void Blockchange(ushort x, ushort y, ushort z, ushort block, bool overRide = false,
                                PhysicsArgs data = default, bool addUndo = true) => Blockchange(PosToInt(x, y, z), block, overRide, data, addUndo); //Block change made by physics
        public void Blockchange(ushort x, ushort y, ushort z, ushort block) => Blockchange(PosToInt(x, y, z), block, false, default); //Block change made by physics
        public bool DoPhysicsBlockchange(int b, ushort block, bool overRide = false,
                                         PhysicsArgs data = default, bool addUndo = true)
        {
            if (blocks == null || b < 0 || b >= blocks.Length) return false;
            ushort old = blocks[b], 
                extended = Block.ExtendedBase[old];
            if (extended > 0) old = (ushort)(extended | GetExtTile(b));
            try
            {
                if (!overRide)
                {
                    if (Props[old].OPBlock || (Props[block].OPBlock && data.Raw != 0)) return false;
                }
                if (old == 19 && LevelPhysics > 0 && block != 19)
                {
                    OtherPhysics.DoSpongeRemoved(this, b, false);
                }
                if (old == 109 && LevelPhysics > 0 && block != 109)
                {
                    OtherPhysics.DoSpongeRemoved(this, b, true);
                }
                if (addUndo)
                {
                    UndoPos uP = default;
                    uP.Index = b;
                    uP.SetData(old, block);
                    if (UndoBuffer.Count < Server.Config.PhysicsUndo)
                    {
                        UndoBuffer.Add(uP);
                    }
                    else
                    {
                        if (currentUndo >= Server.Config.PhysicsUndo)
                            currentUndo = 0;
                        UndoBuffer[currentUndo] = uP;
                    }
                    currentUndo++;
                }
                Changed = true;
                if (block >= 256)
                {
                    blocks[b] = Block.ExtendedClass[block >> 8];
                    IntToPos(b, out ushort x, out ushort y, out ushort z);
                    FastSetExtTile(x, y, z, (byte)block);
                }
                else
                {
                    blocks[b] = (byte)block;
                    if (old >= 256)
                    {
                        IntToPos(b, out ushort x, out ushort y, out ushort z);
                        FastRevertExtTile(x, y, z);
                    }
                }
                if (LevelPhysics > 0 && (ActivatesPhysics(block) || data.Raw != 0))
                {
                    AddCheck(b, false, data);
                }
                return !Block.VisuallyEquals(old, block);
            }
            catch
            {
                return false;
            }
        }
        public void UpdateBlock(Player p, ushort x, ushort y, ushort z, ushort block,
                                ushort flags = 1 << 0, bool buffered = false)
        {
            ushort old = GetBlock(x, y, z, out int index);
            bool drawn = (flags & (1 << 0)) == 0;
            ChangeResult result = TryChangeBlock(p, x, y, z, block, drawn);
            if (result == ChangeResult.Unchanged) return;
            BlockDB.Cache.Add(p, x, y, z, flags, old, block);
            if (result == ChangeResult.VisuallySame) return;
            if (buffered)
            {
                p.Level.blockqueue.Add(index, block);
            }
            else
            {
                BroadcastChange(x, y, z, block);
            }
        }
        public Vec3S32 ClampPos(Vec3S32 P)
        {
            P.X = Math.Max(0, Math.Min(P.X, Width - 1));
            P.Y = Math.Max(0, Math.Min(P.Y, Height - 1));
            P.Z = Math.Max(0, Math.Min(P.Z, Length - 1));
            return P;
        }
        public BlockDefinition GetBlockDef(ushort block) => block == 0 ? null : Block.IsPhysicsType(block) ? CustomBlockDefs[Block.Convert(block)] : CustomBlockDefs[block];
        public byte CollideType(ushort block)
        {
            BlockDefinition def = GetBlockDef(block);
            byte collide = def != null ? def.CollideType : (byte)2;
            return def == null && block < 256 ? DefaultSet.Collide(Block.Convert(block)) : collide;
        }
        public bool LightPasses(ushort block)
        {
            BlockDefinition def = GetBlockDef(block);
            return def != null ? !def.BlocksLight || def.BlockDraw == 2 || def.MinZ > 0 : Block.LightPass(block);
        }
        public byte GetFallback(ushort b)
        {
            BlockDefinition def = CustomBlockDefs[b];
            return def != null ? def.FallBack : b < 66 ? (byte)b : (byte)0;
        }
    }
}
