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
    struct ZipEntry
    {
        public byte[] Filename;
        public long CompressedSize, UncompressedSize, LocalHeaderOffset;
        public uint Crc32;
        public ushort BitFlags, CompressionMethod;
        public DateTime ModifiedDate;
        public void MakeZip64Placeholder()
        {
            CompressedSize = uint.MaxValue;
            UncompressedSize = uint.MaxValue;
            LocalHeaderOffset = uint.MaxValue;
        }
    }
    sealed class ZipWriterStream : Stream
    {
        public uint Crc32 = uint.MaxValue;
        public long CompressedLen;
        public Stream stream;
        public ZipWriterStream(Stream stream) => this.stream = stream;
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        static readonly Exception ex = new NotSupportedException();
        public override void Flush() => stream.Flush();
        public override long Length => throw ex;
        public override long Position { get { throw ex; } set { throw ex; } }
        public override int Read(byte[] buffer, int offset, int count) => throw ex;
        public override long Seek(long offset, SeekOrigin origin) => throw ex;
        public override void SetLength(long length) => throw ex;
        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
            CompressedLen += count;
        }
        public override void WriteByte(byte value)
        {
            stream.WriteByte(value);
            CompressedLen++;
        }
        public override void Close() => stream = null;
        public long WriteStream(Stream src, byte[] buffer, bool compress)
        {
            if (compress)
            {
                using DeflateStream ds = new(this, CompressionMode.Compress);
                return WriteData(ds, src, buffer);
            }
            return WriteData(this, src, buffer);
        }
        long WriteData(Stream dst, Stream src, byte[] buffer)
        {
            long totalLen = 0;
            int count;
            while ((count = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                dst.Write(buffer, 0, count);
                totalLen += count;
                for (int i = 0; i < count; i++)
                    Crc32 = crc32Table[(Crc32 ^ buffer[i]) & 0xFF] ^ (Crc32 >> 8);
            }
            return totalLen;
        }
        static readonly uint[] crc32Table;
        static ZipWriterStream()
        {
            crc32Table = new uint[256];
            for (int i = 0; i < crc32Table.Length; i++)
            {
                uint c = (uint)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((c & 1) != 0)
                        c = 0xEDB88320 ^ (c >> 1);
                    else
                        c >>= 1;
                }
                crc32Table[i] = c;
            }
        }
    }
    /// <summary> Writes entries into a ZIP archive. </summary>
    public sealed class ZipWriter
    {
        readonly BinaryWriter writer;
        readonly Stream stream;
        readonly byte[] buffer = new byte[81920];
        bool zip64;
        readonly List<ZipEntry> entries = new();
        int numEntries;
        long centralDirOffset, centralDirSize, zip64EndOffset;
        static readonly byte[] emptyZip64Local = new byte[20];
        public ZipWriter(Stream stream)
        {
            this.stream = stream;
            writer = new(stream);
        }
        public void WriteEntry(Stream src, string file, bool compress)
        {
            ZipEntry entry = default;
            entry.Filename = Encoding.UTF8.GetBytes(file);
            entry.LocalHeaderOffset = stream.Position;
            try
            {
                entry.ModifiedDate = File.GetLastWriteTime(file);
            }
            catch
            {
                entry.ModifiedDate = DateTime.Now;
            }
            int headerSize = 30 + entry.Filename.Length + 20;
            stream.Write(buffer, 0, headerSize);
            foreach (char c in file)
                if (c < ' ' || c > '~')
                    entry.BitFlags |= 1 << 11;
            ZipWriterStream dst = new(stream);
            entry.UncompressedSize = dst.WriteStream(src, buffer, compress);
            dst.stream = null;
            if (compress && entry.UncompressedSize > 0)
                entry.CompressionMethod = 8;
            entry.CompressedSize = dst.CompressedLen;
            entry.Crc32 = dst.Crc32 ^ uint.MaxValue;
            entries.Add(entry);
            numEntries++;
        }
        public void FinishEntries()
        {
            zip64 = numEntries >= ushort.MaxValue || stream.Length >= (int.MaxValue - 4 * 1000 * 1000);
            long pos = stream.Position;
            for (int i = 0; i < numEntries; i++)
            {
                ZipEntry entry = entries[i];
                stream.Seek(entry.LocalHeaderOffset, SeekOrigin.Begin);
                WriteLocalFileRecord(entry);
                entries[i] = entry;
            }
            stream.Seek(pos, SeekOrigin.Begin);
        }
        public void WriteFooter()
        {
            centralDirOffset = stream.Position;
            for (int i = 0; i < numEntries; i++)
                WriteCentralDirectoryRecord(entries[i]);
            centralDirSize = stream.Position - centralDirOffset;
            if (zip64)
                WriteZip64EndOfCentralDirectory();
            WriteEndOfCentralDirectoryRecord();
        }
        void WriteZip64EndOfCentralDirectory()
        {
            zip64EndOffset = stream.Position;
            WriteZip64EndOfCentralDirectoryRecord();
            WriteZip64EndOfCentralDirectoryLocator();
            numEntries = ushort.MaxValue;
            centralDirOffset = uint.MaxValue;
            centralDirSize = uint.MaxValue;
        }
        void WriteLocalFileRecord(ZipEntry entry)
        {
            ushort version = zip64 ? (ushort)45 : (ushort)20;
            BinaryWriter w = writer;
            ZipEntry copy = entry;
            if (zip64)
                entry.MakeZip64Placeholder();
            w.Write(0x04034b50);
            w.Write(version);
            w.Write(entry.BitFlags);
            w.Write(entry.CompressionMethod);
            WriteLastModified(entry.ModifiedDate);
            w.Write(entry.Crc32);
            w.Write((uint)entry.CompressedSize);
            w.Write((uint)entry.UncompressedSize);
            w.Write((ushort)entry.Filename.Length);
            w.Write(20);
            w.Write(entry.Filename);
            if (!zip64)
            {
                w.Write(emptyZip64Local);
                return;
            }
            w.Write(0x0001);
            w.Write((ushort)16);
            w.Write(copy.UncompressedSize);
            w.Write(copy.CompressedSize);
        }
        void WriteCentralDirectoryRecord(ZipEntry entry)
        {
            ushort extraLen = (ushort)(zip64 ? 28 : 0),
                version = zip64 ? (ushort)45 : (ushort)20;
            BinaryWriter w = writer;
            ZipEntry copy = entry;
            if (zip64)
                entry.MakeZip64Placeholder();
            w.Write(0x02014b50);
            w.Write(version);
            w.Write(version);
            w.Write(entry.BitFlags);
            w.Write(entry.CompressionMethod);
            WriteLastModified(entry.ModifiedDate);
            w.Write(entry.Crc32);
            w.Write((uint)entry.CompressedSize);
            w.Write((uint)entry.UncompressedSize);
            w.Write((ushort)entry.Filename.Length);
            w.Write(extraLen);
            w.Write((ushort)0);
            w.Write((ushort)0);
            w.Write((ushort)0);
            w.Write(0);
            w.Write((uint)entry.LocalHeaderOffset);
            w.Write(entry.Filename);
            if (!zip64)
                return;
            w.Write((ushort)1);
            w.Write((ushort)24);
            w.Write(copy.UncompressedSize);
            w.Write(copy.CompressedSize);
            w.Write(copy.LocalHeaderOffset);
        }
        void WriteLastModified(DateTime date)
        {
            int modTime = (date.Second / 2) | (date.Minute << 5) | (date.Hour << 11),
                modDate = (date.Day) | (date.Month << 5) | ((date.Year - 1980) << 9);
            writer.Write((ushort)modTime);
            writer.Write((ushort)modDate);
        }
        void WriteZip64EndOfCentralDirectoryRecord()
        {
            BinaryWriter w = writer;
            w.Write(0x06064b50);
            w.Write((2 * 2) + (2 * 4) + (4 * 8));
            w.Write(45);
            w.Write(45);
            w.Write(0);
            w.Write(0);
            w.Write((long)numEntries);
            w.Write((long)numEntries);
            w.Write(centralDirSize);
            w.Write(centralDirOffset);
        }
        void WriteZip64EndOfCentralDirectoryLocator()
        {
            BinaryWriter w = writer;
            w.Write(0x07064b50);
            w.Write(0);
            w.Write(zip64EndOffset);
            w.Write(1);
        }
        void WriteEndOfCentralDirectoryRecord()
        {
            BinaryWriter w = writer;
            w.Write(0x06054b50);
            w.Write((ushort)0);
            w.Write((ushort)0);
            w.Write((ushort)numEntries);
            w.Write((ushort)numEntries);
            w.Write((uint)centralDirSize);
            w.Write((uint)centralDirOffset);
            w.Write((ushort)0);
        }
    }
}
