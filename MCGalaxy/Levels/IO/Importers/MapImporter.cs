using MCGalaxy.Maths;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace MCGalaxy.Levels.IO
{
    public unsafe class MapImporter : IMapImporter
    {
        public override string Extension { get { return ".map"; } }
        public override string Description { get { return "Map"; } }
        public override Vec3U16 ReadDimensions(Stream src)
        {
            using Stream gs = new GZipStream(src, CompressionMode.Decompress, true);
            byte[] header = new byte[18];
            ReadHeader(gs, header, out byte X, out byte Y, out byte Z);
            return new()
            {
                X = X,
                Y = Y,
                Z = Z
            };
        }
        public override Level Read(Stream src, string name, bool metadata)
        {
            using Stream gs = new GZipStream(src, CompressionMode.Decompress, true);
            byte[] header = new byte[18];
            ReadHeader(gs, header, out byte X, out byte Y, out byte Z);
            Level lvl = new(name, X, Y, Z)
            {
                spawnx = (byte)BitConverter.ToUInt16(header, 8),
                spawnz = (byte)BitConverter.ToUInt16(header, 10),
                spawny = (byte)BitConverter.ToUInt16(header, 12),
                rotx = header[14],
                roty = header[15]
            };
            StreamUtils.ReadFully(gs, lvl.blocks, 0, lvl.blocks.Length);
            ReadCustomBlocksSection(lvl, gs);
            if (!metadata)
            {
                return lvl;
            }
            for (; ; )
            {
                int section = gs.ReadByte();
                if (section == 0xFC)
                {
                    ReadPhysicsSection(lvl, gs);
                    continue;
                }
                if (section == 0x51)
                {
                    ReadZonesSection(lvl, gs);
                    continue;
                }
                return lvl;
            }
        }
        public static void ReadHeader(Stream gs, byte[] header, out byte X, out byte Y, out byte Z)
        {
            StreamUtils.ReadFully(gs, header, 0, 18);
            int signature = BitConverter.ToUInt16(header, 0);
            if (signature != 255)
            {
                throw new InvalidDataException("Invalid .map map signature");
            }
            X = (byte)BitConverter.ToUInt16(header, 2);
            Z = (byte)BitConverter.ToUInt16(header, 4);
            Y = (byte)BitConverter.ToUInt16(header, 6);
        }
        public static void ReadCustomBlocksSection(Level lvl, Stream gs)
        {
            byte[] data = new byte[1];
            int read = gs.Read(data, 0, 1);
            if (read == 0 || data[0] != 0xBD)
            {
                return;
            }
            int index = 0;
            for (int y = 0; y < lvl.ChunksY; y++)
            {
                for (int z = 0; z < lvl.ChunksZ; z++)
                {
                    for (int x = 0; x < lvl.ChunksX; x++)
                    {
                        read = gs.Read(data, 0, 1);
                        if (read > 0 && data[0] == 1)
                        {
                            byte[] chunk = new byte[16 * 16 * 16];
                            StreamUtils.ReadFully(gs, chunk, 0, chunk.Length);
                            lvl.CustomBlocks[index] = chunk;
                        }
                        index++;
                    }
                }
            }
        }
        public static void ReadPhysicsSection(Level lvl, Stream gs)
        {
            byte[] buffer = new byte[sizeof(int)];
            int count = TryRead_I16(buffer, gs);
            if (count == 0)
            {
                return;
            }
            lvl.ListCheck.Count = count;
            lvl.ListCheck.Items = new Check[count];
            ReadPhysicsEntries(lvl, gs, count);
        }
        public static void ReadPhysicsEntries(Level lvl, Stream gs, int count)
        {
            byte[] buffer = new byte[Math.Min(count, 1024) * 8];
            Check C;
            fixed (byte* ptr = buffer)
            {
                for (int i = 0; i < count; i += 1024)
                {
                    int entries = Math.Min(1024, count - i),
                        read = gs.Read(buffer, 0, entries * 8);
                    if (read < entries * 8)
                    {
                        return;
                    }
                    int* ptrInt = (int*)ptr;
                    for (int j = 0; j < entries; j++)
                    {
                        C.Index = *ptrInt;
                        ptrInt++;
                        C.data.Raw = (uint)*ptrInt;
                        ptrInt++;
                        lvl.ListCheck.Items[i + j] = C;
                    }
                }
            }
        }
        public static void ReadZonesSection(Level lvl, Stream gs)
        {
            byte[] buffer = new byte[sizeof(int)];
            int count = TryRead_I16(buffer, gs);
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                try
                {
                    ParseZone(lvl, ref buffer, gs);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error importing zone #" + i + " from MCSharp map", ex);
                }
            }
        }
        public static void ParseZone(Level lvl, ref byte[] buffer, Stream gs)
        {
            Zone z = new()
            {
                MinX = Read_U8(buffer, gs),
                MaxX = Read_U8(buffer, gs),
                MinY = Read_U8(buffer, gs),
                MaxY = Read_U8(buffer, gs),
                MinZ = Read_U8(buffer, gs),
                MaxZ = Read_U8(buffer, gs)
            };
            int metaCount = TryRead_I16(buffer, gs);
            ConfigElement[] elems = Server.zoneConfig;
            for (int j = 0; j < metaCount; j++)
            {
                int size = Read_U8(buffer, gs);
                if (size > buffer.Length)
                {
                    buffer = new byte[size + 16];
                }
                StreamUtils.ReadFully(gs, buffer, 0, size);
                string line = Encoding.UTF8.GetString(buffer, 0, size);
                PropertiesFile.ParseLine(line, '=', out string key, out string value);
                if (key == null)
                {
                    continue;
                }
                value = value.Trim();
                ConfigElement.Parse(elems, z.Config, key, value);
            }
            z.AddTo(lvl);
        }
        public static short TryRead_I16(byte[] buffer, Stream gs)
        {
            int read = gs.Read(buffer, 0, sizeof(short));
            if (read < sizeof(short))
            {
                return 0;
            }
            return MemUtils.ReadI16_BE(buffer, 0);
        }
        public static byte Read_U8(byte[] buffer, Stream gs)
        {
            StreamUtils.ReadFully(gs, buffer, 0, sizeof(byte));
            return ReadU8_BE(buffer, 0);
        }
        public static byte ReadU8_BE(byte[] array, int offset)
        {
            return (byte)(array[offset] | array[offset + 1]);
        }
    }
}