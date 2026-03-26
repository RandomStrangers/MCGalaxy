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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace MCGalaxy
{
    public sealed class ZipReaderStream : Stream
    {
        public long CompressedLen;
        public Stream stream;
        public ZipReaderStream(Stream stream) => this.stream = stream;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public static readonly Exception ex = new NotSupportedException();
        public override void Flush() => stream.Flush();
        public override long Length => throw ex;
        public override long Position { get { throw ex; } set { throw ex; } }
        public override long Seek(long offset, SeekOrigin origin) => throw ex;
        public override void SetLength(long length) => throw ex;
        public override void Write(byte[] buffer, int offset, int count) => throw ex;
        public override void WriteByte(byte value) => throw ex;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CompressedLen <= 0)
                return 0;
            if (count >= CompressedLen)
                count = (int)CompressedLen;
            count = stream.Read(buffer, offset, count);
            CompressedLen -= count;
            return count;
        }
        public override int ReadByte()
        {
            if (CompressedLen <= 0)
                return -1;
            CompressedLen--;
            return stream.ReadByte();
        }
        public override void Close() => stream = null;
    }
    /// <summary> Reads entries from a ZIP archive. </summary>
    public sealed class ZipReader
    {
        public readonly BinaryReader reader;
        public readonly Stream stream;
        public readonly List<ZipEntry> entries = new();
        public int numEntries;
        public long centralDirOffset, zip64EndOffset;
        public ZipReader(Stream stream)
        {
            this.stream = stream;
            reader = new(stream);
        }
        public Stream GetEntry(int i, out string file)
        {
            ZipEntry entry = entries[i];
            stream.Seek(entry.LocalHeaderOffset, SeekOrigin.Begin);
            file = null;
            uint sig = reader.ReadUInt32();
            if (sig != 0x04034b50)
            {
                Logger.Log(LogType.Warning, "&WFailed to find local file entry {0}", i);
                return null;
            }
            entry = ReadLocalFileRecord();
            file = Encoding.UTF8.GetString(entry.Filename);
            ZipReaderStream part = new(stream)
            {
                CompressedLen = entry.CompressedSize
            };
            return entry.CompressionMethod == 0 ? part : new DeflateStream(part, CompressionMode.Decompress);
        }
        public int FindEntries()
        {
            stream.Seek(centralDirOffset, SeekOrigin.Begin);
            for (int i = 0; i < numEntries; i++)
            {
                uint sig = reader.ReadUInt32();
                if (sig != 0x02014b50)
                {
                    Logger.Log(LogType.Warning, "&WFailed to find central dir entry {0}", i); 
                    return i;
                }
                ZipEntry entry = ReadCentralDirectoryRecord();
                entries.Add(entry);
            }
            return numEntries;
        }
        public void FindFooter()
        {
            BinaryReader r = reader;
            uint sig = 0;
            int i, len = Math.Min(257, (int)stream.Length);
            for (i = 22; i < len; i++)
            {
                stream.Seek(-i, SeekOrigin.End);
                sig = r.ReadUInt32();
                if (sig == 0x06054b50)
                    break;
            }
            if (sig != 0x06054b50)
            {
                Logger.Log(LogType.Warning, "&WFailed to find end of central directory");
                return;
            }
            ReadEndOfCentralDirectoryRecord();
            if (centralDirOffset != uint.MaxValue)
                return;
            Logger.Log(LogType.SystemActivity, "Backup .zip is using ZIP64 format");
            stream.Seek(-i - 20, SeekOrigin.End);
            sig = r.ReadUInt32();
            if (sig != 0x07064b50)
            {
                Logger.Log(LogType.Warning, "&WFailed to find ZIP64 locator");
                return;
            }
            ReadZip64EndOfCentralDirectoryLocator();
            stream.Seek(zip64EndOffset, SeekOrigin.Begin);
            sig = r.ReadUInt32();
            if (sig != 0x06064b50)
            {
                Logger.Log(LogType.Warning, "&WFailed to find ZIP64 end");
                return;
            }
            ReadZip64EndOfCentralDirectoryRecord();
        }
        public ZipEntry ReadLocalFileRecord()
        {
            BinaryReader r = reader;
            ZipEntry entry = default;
            r.ReadUInt16();
            r.ReadUInt16();
            entry.CompressionMethod = r.ReadUInt16();
            r.ReadUInt32();
            r.ReadUInt32();
            entry.CompressedSize = r.ReadUInt32();
            entry.UncompressedSize = r.ReadUInt32();
            int filenameLen = r.ReadUInt16(),
                extraLen = r.ReadUInt16();
            entry.Filename = r.ReadBytes(filenameLen);
            if (extraLen == 0)
                return entry;
            long extraEnd = stream.Position + extraLen;
            if (r.ReadUInt16() == 1)
            {
                r.ReadUInt16();
                if (entry.UncompressedSize == uint.MaxValue)
                    entry.UncompressedSize = r.ReadInt64();
                if (entry.CompressedSize == uint.MaxValue)
                    entry.CompressedSize = r.ReadInt64();
            }
            stream.Seek(extraEnd, SeekOrigin.Begin);
            return entry;
        }
        public ZipEntry ReadCentralDirectoryRecord()
        {
            BinaryReader r = reader;
            ZipEntry entry = default;
            r.ReadUInt16();
            r.ReadUInt16();
            r.ReadUInt16();
            entry.CompressionMethod = r.ReadUInt16();
            r.ReadUInt32();
            r.ReadUInt32();
            entry.CompressedSize = r.ReadUInt32();
            entry.UncompressedSize = r.ReadUInt32();
            int filenameLen = r.ReadUInt16(),
                extraLen = r.ReadUInt16();
            r.ReadUInt16();
            r.ReadUInt16();
            r.ReadUInt16();
            r.ReadUInt32();
            entry.LocalHeaderOffset = r.ReadUInt32();
            entry.Filename = r.ReadBytes(filenameLen);
            if (extraLen == 0)
                return entry;
            long extraEnd = stream.Position + extraLen;
            if (r.ReadUInt16() == 1)
            {
                r.ReadUInt16();
                if (entry.UncompressedSize == uint.MaxValue)
                    entry.UncompressedSize = r.ReadInt64();
                if (entry.CompressedSize == uint.MaxValue)
                    entry.CompressedSize = r.ReadInt64();
                if (entry.LocalHeaderOffset == uint.MaxValue)
                    entry.LocalHeaderOffset = r.ReadInt64();
            }
            stream.Seek(extraEnd, SeekOrigin.Begin);
            return entry;
        }
        public void ReadZip64EndOfCentralDirectoryRecord()
        {
            BinaryReader r = reader;
            r.ReadInt64();
            r.ReadUInt16();
            r.ReadUInt16();
            r.ReadUInt32();
            r.ReadUInt32();
            numEntries = (int)r.ReadInt64();
            r.ReadInt64();
            r.ReadInt64();
            centralDirOffset = r.ReadInt64();
        }
        public void ReadZip64EndOfCentralDirectoryLocator()
        {
            BinaryReader r = reader;
            r.ReadUInt32();
            zip64EndOffset = reader.ReadInt64();
            r.ReadUInt32();
        }
        public void ReadEndOfCentralDirectoryRecord()
        {
            BinaryReader r = reader;
            r.ReadUInt16();
            r.ReadUInt16();
            numEntries = r.ReadUInt16();
            r.ReadUInt16();
            r.ReadUInt32();
            centralDirOffset = r.ReadUInt32();
            r.ReadUInt16();
        }
    }
}
