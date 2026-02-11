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
using MCGalaxy.Maths;
using MCGalaxy.Util;
using System;
using System.IO;
namespace MCGalaxy.DB
{
    public unsafe class BlockDBFile
    {
        public static string FilePath(string map) => "blockdb/" + map + ".cbdb";
        public static void WriteHeader(Stream s, Vec3U16 dims)
        {
            byte[] header = new byte[16 * 4];
            NetUtils.Write("CBDB_MCG", header, 0, false);
            WriteU16(1, header, 8);
            WriteU16(dims.X, header, 10);
            WriteU16(dims.Y, header, 12);
            WriteU16(dims.Z, header, 14);
            s.Write(header, 0, 16);
        }
        public static BlockDBFile ReadHeader(Stream s, out Vec3U16 dims)
        {
            dims = default;
            byte[] header = new byte[16];
            StreamUtils.ReadFully(s, header, 0, header.Length);
            ushort fileVersion = ReadU16(header, 8);
            if (fileVersion != 1)
                throw new NotSupportedException("only version 1 is supported");
            dims.X = ReadU16(header, 10);
            dims.Y = ReadU16(header, 12);
            dims.Z = ReadU16(header, 14);
            return new();
        }
        public static void WriteEntries(Stream s, FastList<BlockDBEntry> entries)
        {
            byte[] bulk = new byte[4096 * 16];
            for (int i = 0; i < entries.Count; i += 4096)
            {
                int bulkCount = Math.Min(4096, entries.Count - i);
                for (int j = 0; j < bulkCount; j++)
                {
                    WriteEntry(entries.Items[i + j], bulk, j * 16);
                }
                s.Write(bulk, 0, bulkCount * 16);
            }
        }
        public void WriteEntries(Stream s, BlockDBCache cache)
        {
            byte[] bulk = new byte[4096 * 16];
            BlockDBCacheNode node = cache.Tail;
            while (node != null)
            {
                int count = node.Count;
                for (int i = 0; i < count; i += 4096)
                {
                    int bulkCount = Math.Min(4096, count - i);
                    for (int j = 0; j < bulkCount; j++)
                    {
                        BlockDBEntry entry = node.Unpack(node.Entries[i + j]);
                        WriteEntry(entry, bulk, j * 16);
                    }
                    s.Write(bulk, 0, bulkCount * 16);
                }
                lock (cache.Locker)
                    node = node.Next;
            }
        }
        public long CountEntries(Stream s) => (s.Length / 16) - 1;
        static void WriteEntry(BlockDBEntry entry, byte[] bulk, int index)
        {
            bulk[index + 0] = (byte)entry.PlayerID;
            bulk[index + 1] = (byte)(entry.PlayerID >> 8);
            bulk[index + 2] = (byte)(entry.PlayerID >> 16);
            bulk[index + 3] = (byte)(entry.PlayerID >> 24);
            bulk[index + 4] = (byte)entry.TimeDelta;
            bulk[index + 5] = (byte)(entry.TimeDelta >> 8);
            bulk[index + 6] = (byte)(entry.TimeDelta >> 16);
            bulk[index + 7] = (byte)(entry.TimeDelta >> 24);
            bulk[index + 8] = (byte)entry.Index;
            bulk[index + 9] = (byte)(entry.Index >> 8);
            bulk[index + 10] = (byte)(entry.Index >> 16);
            bulk[index + 11] = (byte)(entry.Index >> 24);
            bulk[index + 12] = entry.OldRaw;
            bulk[index + 13] = entry.NewRaw;
            bulk[index + 14] = (byte)entry.Flags;
            bulk[index + 15] = (byte)(entry.Flags >> 8);
        }
        public static unsafe int ReadForward(Stream s, byte[] bulk)
        {
            long remaining = (s.Length - s.Position) / 16;
            int count = (int)Math.Min(remaining, 4096);
            if (count > 0)
            {
                StreamUtils.ReadFully(s, bulk, 0, count * 16);
            }
            return count;
        }
        public unsafe int ReadBackward(Stream s, byte[] bulk)
        {
            long pos = s.Position,
                remaining = (pos / 16) - 1;
            int count = (int)Math.Min(remaining, 4096);
            if (count > 0)
            {
                pos -= count * 16;
                s.Position = pos;
                StreamUtils.ReadFully(s, bulk, 0, count * 16);
                s.Position = pos;
            }
            return count;
        }
        /// <summary> Deletes the backing file on disc if it exists. </summary>
        public static void DeleteBackingFile(string map) => FileIO.TryDelete(FilePath(map));
        /// <summary> Moves the backing file on disc if it exists. </summary>
        public static void MoveBackingFile(string srcMap, string dstMap)
        {
            string srcPath = FilePath(srcMap), dstPath = FilePath(dstMap);
            if (!File.Exists(srcPath)) return;
            FileIO.TryDelete(dstPath);
            FileIO.TryMove(srcPath, dstPath);
        }
        public static void ResizeBackingFile(BlockDB db, string path)
        {
            Logger.Log(LogType.BackgroundActivity, "Resizing BlockDB for " + db.MapName);
            string tempPath = "blockdb/" + db.MapName + ".temp";
            using (Stream src = FileIO.TryOpenRead(path), dst = File.Create(tempPath))
            {
                ReadHeader(src, out Vec3U16 dims);
                WriteHeader(dst, db.Dims);
                int width = db.Dims.X, length = db.Dims.Z;
                byte[] bulk = new byte[4096 * 16];
                fixed (byte* ptr = bulk)
                {
                    BlockDBEntry* entryPtr = (BlockDBEntry*)ptr;
                    while (true)
                    {
                        int count = ReadForward(src, bulk);
                        if (count == 0) break;
                        for (int i = 0; i < count; i++)
                        {
                            int index = entryPtr[i].Index,
                                x = index % dims.X,
                                y = index / dims.X / dims.Z,
                                z = index / dims.X % dims.Z;
                            entryPtr[i].Index = (y * length + z) * width + x;
                        }
                        dst.Write(bulk, 0, count * 16);
                    }
                }
            }
            FileIO.TryDelete(path);
            FileIO.TryMove(tempPath, path);
        }
        /// <summary> Returns number of entries in the backing file on disc if it exists. </summary>
        public static long CountEntries(string map)
        {
            string path = FilePath(map);
            if (!File.Exists(path)) return 0;
            using Stream src = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            BlockDBFile file = ReadHeader(src, out Vec3U16 dims);
            return file.CountEntries(src);
        }
        /// <summary> Iterates from the very oldest to newest entry in the BlockDB. </summary>
        public static void FindChangesAt(Stream s, int index, Action<BlockDBEntry> output)
        {
            byte[] bulk = new byte[4096 * 16];
            fixed (byte* ptr = bulk)
            {
                while (true)
                {
                    BlockDBEntry* entryPtr = (BlockDBEntry*)ptr;
                    int count = ReadForward(s, bulk);
                    if (count == 0) return;
                    for (int i = 0; i < count; i++)
                    {
                        if (entryPtr->Index == index) 
                        { 
                            output(*entryPtr); 
                        }
                        entryPtr++;
                    }
                }
            }
        }
        /// <summary> Iterates from the very newest to oldest entry in the BlockDB. </summary>
        /// <returns> whether an entry before start time was reached. </returns>
        public bool FindChangesBy(Stream s, int[] ids, int start, int end, Action<BlockDBEntry> output)
        {
            byte[] bulk = new byte[4096 * 16];
            s.Position = s.Length;
            fixed (byte* ptr = bulk)
            {
                while (true)
                {
                    BlockDBEntry* entryPtr = (BlockDBEntry*)ptr;
                    int count = ReadBackward(s, bulk);
                    if (count == 0) break;
                    entryPtr += count - 1;
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (entryPtr->TimeDelta < start) return true;
                        if (entryPtr->TimeDelta <= end)
                        {
                            for (int j = 0; j < ids.Length; j++)
                            {
                                if (entryPtr->PlayerID != ids[j]) continue;
                                output(*entryPtr);
                                break;
                            }
                        }
                        entryPtr--;
                    }
                }
            }
            return false;
        }
        static ushort ReadU16(byte[] array, int offset) => (ushort)(array[offset] | array[offset + 1] << 8);
        static void WriteU16(ushort value, byte[] array, int index)
        {
            array[index++] = (byte)value;
            array[index++] = (byte)(value >> 8);
        }
    }
}
