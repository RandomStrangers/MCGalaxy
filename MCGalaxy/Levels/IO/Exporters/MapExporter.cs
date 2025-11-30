using System;
using System.IO;
using System.IO.Compression;
using System.Text;
namespace MCGalaxy.Levels.IO
{
    public unsafe class MapExporter : IMapExporter
    {
        public override string Extension { get { return ".map"; } }
        public override void Write(Stream dst, Level lvl)
        {
            using Stream gs = new GZipStream(dst, CompressionMode.Compress);
            byte[] buffer = new byte[65536];
            WriteHeader(lvl, gs, buffer);
            lock (lvl.physTickLock)
            {
                WriteBlocksSection(lvl, gs, buffer);
                WriteBlockDefsSection(lvl, gs, buffer);
                WritePhysicsSection(lvl, gs, buffer);
            }
            WriteZonesSection(lvl, gs, buffer);
        }
        public static void WriteU8(byte[] dst, int idx, byte value)
        {
            dst[idx] = value;
            dst[idx + 1] = (byte)(value >> 8);
        }
        public static void WriteHeader(Level lvl, Stream gs, byte[] header)
        {
            WriteU8(header, 0, 255);
            WriteU8(header, 2, (byte)lvl.Width);
            WriteU8(header, 4, (byte)lvl.Length);
            WriteU8(header, 6, (byte)lvl.Height);
            WriteU8(header, 8, (byte)lvl.spawnx);
            WriteU8(header, 10, (byte)lvl.spawnz);
            WriteU8(header, 12, (byte)lvl.spawny);
            header[14] = lvl.rotx;
            header[15] = lvl.roty;
            header[16] = (byte)lvl.VisitAccess.Min;
            header[17] = (byte)lvl.BuildAccess.Min;
            gs.Write(header, 0, 18);
        }
        public static void WriteBlocksSection(Level lvl, Stream gs, byte[] buffer)
        {
            byte[] blocks = lvl.blocks;
            for (int i = 0; i < blocks.Length; i += 65536)
            {
                int len = Math.Min(65536, blocks.Length - i);
                Buffer.BlockCopy(blocks, i, buffer, 0, len);
                gs.Write(buffer, 0, len);
            }
        }
        public static void WriteBlockDefsSection(Level lvl, Stream gs, byte[] buffer)
        {
            gs.WriteByte(0xBD);
            int index = 0;
            for (int y = 0; y < lvl.ChunksY; y++)
            {
                for (int z = 0; z < lvl.ChunksZ; z++)
                {
                    for (int x = 0; x < lvl.ChunksX; x++)
                    {
                        byte[] chunk = lvl.CustomBlocks[index];
                        if (chunk == null)
                        {
                            gs.WriteByte(0);
                        }
                        else
                        {
                            gs.WriteByte(1);
                            Buffer.BlockCopy(chunk, 0, buffer, 0, chunk.Length);
                            gs.Write(buffer, 0, chunk.Length);
                        }
                        index++;
                    }
                }
            }
        }
        public static void WritePhysicsSection(Level lvl, Stream gs, byte[] buffer)
        {
            short count = (short)lvl.ListCheck.Count;
            Check[] checks = lvl.ListCheck.Items;
            if (count == 0)
            {
                return;
            }
            gs.WriteByte(0xFC);
            NetUtils.WriteI16(count, buffer, 0);
            gs.Write(buffer, 0, sizeof(short));
            fixed (byte* ptr = buffer)
            {
                int entries = 0;
                int* ptrInt = (int*)ptr;
                for (int i = 0; i < count; i++)
                {
                    Check C = checks[i];
                    *ptrInt = C.Index;
                    ptrInt++;
                    *ptrInt = (int)C.data.Raw;
                    ptrInt++;
                    entries++;
                    if (entries != 8192)
                    {
                        continue;
                    }
                    ptrInt = (int*)ptr;
                    gs.Write(buffer, 0, entries * 8);
                    entries = 0;
                }
                if (entries == 0)
                {
                    return;
                }
                gs.Write(buffer, 0, entries * 8);
            }
        }
        public static void WriteU8(byte value, byte[] array, int index)
        {
            array[index++] = value;
        }
        public static void WriteZonesSection(Level lvl, Stream gs, byte[] buffer)
        {
            Zone[] zones = lvl.Zones.Items;
            if (zones.Length == 0)
            {
                return;
            }
            gs.WriteByte(0x51);
            NetUtils.WriteI16((short)zones.Length, buffer, 0);
            gs.Write(buffer, 0, sizeof(short));
            foreach (Zone z in zones)
            {
                WriteU8((byte)z.MinX, buffer, 0 * 2);
                WriteU8((byte)z.MaxX, buffer, 1 * 2);
                WriteU8((byte)z.MinY, buffer, 2 * 2);
                WriteU8((byte)z.MaxY, buffer, 3 * 2);
                WriteU8((byte)z.MinZ, buffer, 4 * 2);
                WriteU8((byte)z.MaxZ, buffer, 5 * 2);
                gs.Write(buffer, 0, 6 * 2);
                ConfigElement[] elem = Server.zoneConfig;
                NetUtils.WriteI16((short)elem.Length, buffer, 0);
                gs.Write(buffer, 0, sizeof(short));
                for (int i = 0; i < elem.Length; i++)
                {
                    string value = elem[i].Format(z.Config);
                    int count = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 2);
                    WriteU8((byte)count, buffer, 0);
                    gs.Write(buffer, 0, count + 2);
                }
            }
        }
    }
}