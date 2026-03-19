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
                    for (int i = 0; i < copy; i++)
                        data[index + i + 3] = buffer[offset + i];
                else
                    Buffer.BlockCopy(buffer, offset, data, index + 3, copy);
                offset += copy; 
                index += copy; 
                count -= copy;
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
                CompressMap(level, stream, dst);
            else
                CompressMapSimple(level, stream, dst);
        }
        Stream CompressMapHeader(int volume)
        {
            if (session.Supports(CpeExt.FastMap))
                return new DeflateStream(this, CompressionMode.Compress, true);
            Stream stream = new GZipStream(this, CompressionMode.Compress, true);
            byte[] buffer = new byte[4];
            NetUtils.WriteI32(volume, buffer, 0);
            stream.Write(buffer, 0, 4);
            return stream;
        }
        static unsafe void CompressMapSimple(Level lvl, Stream stream, LevelChunkStream dst)
        {
            int bufferSize = 64 * 1024;
            byte[] buffer = new byte[bufferSize];
            int bIndex = 0;
            ClassicProtocol s = dst.session;
            byte[] blocks = lvl.blocks;
            float progScale = 100.0f / blocks.Length;
            byte* conv = stackalloc byte[256];
            for (int i = 0; i < 256; i++)
                conv[i] = (byte)s.ConvertBlock((ushort)i);
            for (int i = 0; i < blocks.Length; ++i)
            {
                buffer[bIndex] = conv[blocks[i]];
                bIndex++;
                if (bIndex == bufferSize)
                {
                    dst.chunkValue = s.hasExtBlocks ? (byte)0 : (byte)(i * progScale);
                    stream.Write(buffer, 0, bufferSize);
                    bIndex = 0;
                }
            }
            if (bIndex > 0) stream.Write(buffer, 0, bIndex);
        }
        static unsafe void CompressMap(Level lvl, Stream stream, LevelChunkStream dst)
        {
            int bufferSize = 64 * 1024;
            byte[] buffer = new byte[bufferSize];
            int bIndex = 0;
            ClassicProtocol s = dst.session;
            byte[] blocks = lvl.blocks;
            float progScale = 100.0f / blocks.Length;
            byte* conv = stackalloc byte[1024],
                convExt = conv + 256,
                convExt2 = conv + 256 * 2,
                convExt3 = conv + 256 * 3;
            for (int j = 0; j < 1024; j++)
                conv[j] = (byte)s.ConvertBlock((ushort)j);
            if (s.hasExtBlocks)
            {
                int i;
                for (i = 0; i < blocks.Length; i++)
                {
                    byte block = blocks[i];
                    if (block == 163)
                        buffer[bIndex] = lvl.GetExtTile(i);
                    else if (block == 198 || block == 199)
                        break;
                    else
                        buffer[bIndex] = conv[block];
                    bIndex++;
                    if (bIndex == bufferSize)
                    {
                        stream.Write(buffer, 0, bufferSize);
                        bIndex = 0;
                    }
                }
                if (bIndex > 0) stream.Write(buffer, 0, bIndex);
                if (i == blocks.Length) return;
                bIndex = 0;
                using LevelChunkStream dst2 = new(s);
                using Stream stream2 = dst2.CompressMapHeader(blocks.Length);
                dst2.chunkValue = 1;
                byte[] buffer2 = new byte[bufferSize];
                for (int j = 0; j < i; j += bufferSize)
                {
                    int len = Math.Min(bufferSize, i - j);
                    stream2.Write(buffer2, 0, len);
                }
                for (; i < blocks.Length; i++)
                {
                    byte block = blocks[i];
                    switch (block)
                    {
                        case 163:
                            buffer[bIndex] = lvl.GetExtTile(i);
                            buffer2[bIndex] = 0;
                            break;
                        case 198:
                            buffer[bIndex] = lvl.GetExtTile(i);
                            buffer2[bIndex] = 1;
                            break;
                        case 199:
                            buffer[bIndex] = lvl.GetExtTile(i);
                            buffer2[bIndex] = 2;
                            break;
                        default:
                            buffer[bIndex] = conv[block];
                            buffer2[bIndex] = 0;
                            break;
                    }
                    bIndex++;
                    if (bIndex == bufferSize)
                    {
                        stream.Write(buffer, 0, bufferSize);
                        stream2.Write(buffer2, 0, bufferSize);
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
                    buffer[bIndex] = block switch
                    {
                        163 => convExt[lvl.GetExtTile(i)],
                        _ => block == 198 ? convExt2[lvl.GetExtTile(i)] : block == 199 ? convExt3[lvl.GetExtTile(i)] : conv[block],
                    };
                    bIndex++;
                    if (bIndex == bufferSize)
                    {
                        dst.chunkValue = (byte)(i * progScale);
                        stream.Write(buffer, 0, bufferSize); 
                        bIndex = 0;
                    }
                }
            }
            if (bIndex > 0) stream.Write(buffer, 0, bIndex);
        }
    }
}
