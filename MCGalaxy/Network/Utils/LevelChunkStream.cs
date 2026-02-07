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
using System;
using System.IO;
using System.IO.Compression;
namespace MCGalaxy.Network
{
    /// <summary> Streams the compressed form of a map directly to a Minecraft Classic client </summary>
    public sealed class LevelChunkStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        static readonly Exception ex = new NotSupportedException();
        public override void Flush() { }
        public override long Length => throw ex;
        public override long Position { get { throw ex; } set { throw ex; } }
        public override int Read(byte[] buffer, int offset, int count) => throw ex;
        public override long Seek(long offset, SeekOrigin origin) => throw ex;
        public override void SetLength(long length) => throw ex;
        int index;
        byte chunkValue;
        ClassicProtocol session;
        byte[] data = new byte[1028];
        public LevelChunkStream(ClassicProtocol s) => session = s;
        public override void Close()
        {
            if (index > 0) WritePacket();
            session = null;
            base.Close();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int copy = Math.Min(1024 - index, count);
                if (copy <= 8)
                {
                    for (int i = 0; i < copy; i++)
                        data[index + i + 3] = buffer[offset + i];
                }
                else
                {
                    Buffer.BlockCopy(buffer, offset, data, index + 3, copy);
                }
                offset += copy; index += copy; count -= copy;
                if (index != 1024) continue;
                WritePacket();
                data = new byte[1028];
            }
        }
        public override void WriteByte(byte value)
        {
            data[index + 3] = value;
            index++;
            if (index != 1024) return;
            WritePacket();
            data = new byte[1028];
        }
        void WritePacket()
        {
            data[0] = 3;
            NetUtils.WriteU16((ushort)index, data, 1);
            data[1027] = chunkValue;
            session.Send(data);
            index = 0;
        }
        public static void SendLevel(ClassicProtocol session, Level level, int volume)
        {
            using LevelChunkStream dst = new(session);
            using Stream stream = dst.CompressMapHeader(volume);
            if (level.MightHaveCustomBlocks())
            {
                CompressMap(level, stream, dst);
            }
            else
            {
                CompressMapSimple(level, stream, dst);
            }
        }
        Stream CompressMapHeader(int volume)
        {
            // FastMap sends volume in LevelInit packet instead
            if (session.Supports(CpeExt.FastMap))
            {
                return new DeflateStream(this, CompressionMode.Compress, true);
            }
            Stream stream = new GZipStream(this, CompressionMode.Compress, true);
            byte[] buffer = new byte[4];
            NetUtils.WriteI32(volume, buffer, 0);
            stream.Write(buffer, 0, 4);
            return stream;
        }
        static unsafe void CompressMapSimple(Level lvl, Stream stream, LevelChunkStream dst)
        {
            byte[] buffer = new byte[64 * 1024];
            int bIndex = 0;
            ClassicProtocol s = dst.session;
            byte[] blocks = lvl.blocks;
            float progScale = 100.0f / blocks.Length;
            // Store on stack instead of performing function call for every block in map
            byte* conv = stackalloc byte[256];
            for (int i = 0; i < 256; i++)
            {
                conv[i] = (byte)s.ConvertBlock((ushort)i);
            }
            // compress the map data in 64 kb chunks
            for (int i = 0; i < blocks.Length; ++i)
            {
                buffer[bIndex] = conv[blocks[i]];
                bIndex++;
                if (bIndex == (64 * 1024))
                {
                    // '0' to indicate this chunk has lower 8 bits of block ids
                    dst.chunkValue = s.hasExtBlocks ? (byte)0 : (byte)(i * progScale);
                    stream.Write(buffer, 0, 64 * 1024); 
                    bIndex = 0;
                }
            }
            if (bIndex > 0) stream.Write(buffer, 0, bIndex);
        }
        static unsafe void CompressMap(Level lvl, Stream stream, LevelChunkStream dst)
        {
            byte[] buffer = new byte[64 * 1024];
            int bIndex = 0;
            ClassicProtocol s = dst.session;
            byte[] blocks = lvl.blocks;
            float progScale = 100.0f / blocks.Length;
            // Store on stack instead of performing function call for every block in map
            byte* conv = stackalloc byte[1024],
                convExt = conv + 256; // 256 blocks per group/class
            byte* convExt2 = conv + 256 * 2,
                convExt3 = conv + 256 * 3;
            for (int j = 0; j < 1024; j++)
            {
                conv[j] = (byte)s.ConvertBlock((ushort)j);
            }
            // compress the map data in 64 kb chunks
            if (s.hasExtBlocks)
            {
                // Initially assume all custom blocks are <= 255
                int i;
                for (i = 0; i < blocks.Length; i++)
                {
                    byte block = blocks[i];
                    if (block == 163)
                    {
                        buffer[bIndex] = lvl.GetExtTile(i);
                    }
                    else if (block == 198)
                    {
                        break;
                    }
                    else if (block == 199)
                    {
                        break;
                    }
                    else
                    {
                        buffer[bIndex] = conv[block];
                    }
                    bIndex++;
                    if (bIndex == (64 * 1024))
                    {
                        stream.Write(buffer, 0, 64 * 1024);
                        bIndex = 0;
                    }
                }
                // Check if map only used custom blocks <= 255
                if (bIndex > 0) stream.Write(buffer, 0, bIndex);
                if (i == blocks.Length) return;
                bIndex = 0;
                // Nope - have to go slower path now
                using LevelChunkStream dst2 = new(s);
                using Stream stream2 = dst2.CompressMapHeader(blocks.Length);
                dst2.chunkValue = 1; // 'extended' blocks
                byte[] buffer2 = new byte[64 * 1024];
                // Need to fill in all the upper 8 bits of blocks before this one with 0
                for (int j = 0; j < i; j += 64 * 1024)
                {
                    int len = Math.Min(64 * 1024, i - j);
                    stream2.Write(buffer2, 0, len);
                }
                for (; i < blocks.Length; i++)
                {
                    byte block = blocks[i];
                    if (block == 163)
                    {
                        buffer[bIndex] = lvl.GetExtTile(i);
                        buffer2[bIndex] = 0;
                    }
                    else if (block == 198)
                    {
                        buffer[bIndex] = lvl.GetExtTile(i);
                        buffer2[bIndex] = 1;
                    }
                    else if (block == 199)
                    {
                        buffer[bIndex] = lvl.GetExtTile(i);
                        buffer2[bIndex] = 2;
                    }
                    else
                    {
                        buffer[bIndex] = conv[block];
                        buffer2[bIndex] = 0;
                    }
                    bIndex++;
                    if (bIndex == (64 * 1024))
                    {
                        stream.Write(buffer, 0, 64 * 1024);
                        stream2.Write(buffer2, 0, 64 * 1024);
                        bIndex = 0;
                    }
                }
                if (bIndex > 0) stream2.Write(buffer2, 0, bIndex);
            }
            else
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    byte block = blocks[i];
                    if (block == 163)
                    {
                        buffer[bIndex] = convExt[lvl.GetExtTile(i)];
                    }
                    else if (block == 198)
                    {
                        buffer[bIndex] = convExt2[lvl.GetExtTile(i)];
                    }
                    else if (block == 199)
                    {
                        buffer[bIndex] = convExt3[lvl.GetExtTile(i)];
                    }
                    else
                    {
                        buffer[bIndex] = conv[block];
                    }
                    bIndex++;
                    if (bIndex == (64 * 1024))
                    {
                        dst.chunkValue = (byte)(i * progScale);
                        stream.Write(buffer, 0, 64 * 1024); 
                        bIndex = 0;
                    }
                }
            }
            if (bIndex > 0) stream.Write(buffer, 0, bIndex);
        }
    }
}
