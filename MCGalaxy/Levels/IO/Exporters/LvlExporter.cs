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
using System.IO;
using System.IO.Compression;
using System.Text;
namespace MCGalaxy.Levels.IO
{
    public sealed unsafe class LvlExporter : IMapExporter
    {
        public override string Extension => ".lvl";
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
        static void WriteU16(byte[] dst, int idx, ushort value)
        {
            dst[idx] = (byte)value;
            dst[idx + 1] = (byte)(value >> 8);
        }
        static void WriteHeader(Level lvl, Stream gs, byte[] header)
        {
            WriteU16(header, 0, 1874);
            WriteU16(header, 2, lvl.Width);
            WriteU16(header, 4, lvl.Length);
            WriteU16(header, 6, lvl.Height);
            WriteU16(header, 8, lvl.spawnx);
            WriteU16(header, 10, lvl.spawnz);
            WriteU16(header, 12, lvl.spawny);
            header[14] = lvl.rotx;
            header[15] = lvl.roty;
            header[16] = (byte)lvl.VisitAccess.Min;
            header[17] = (byte)lvl.BuildAccess.Min;
            gs.Write(header, 0, 18);
        }
        static void WriteBlocksSection(Level lvl, Stream gs, byte[] buffer)
        {
            byte[] blocks = lvl.blocks;
            for (int i = 0; i < blocks.Length; i += 65536)
            {
                int len = Math.Min(65536, blocks.Length - i);
                Buffer.BlockCopy(blocks, i, buffer, 0, len);
                gs.Write(buffer, 0, len);
            }
        }
        static void WriteBlockDefsSection(Level lvl, Stream gs, byte[] buffer)
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
        static void WritePhysicsSection(Level lvl, Stream gs, byte[] buffer)
        {
            int count = lvl.ListCheck.Count;
            Check[] checks = lvl.ListCheck.Items;
            if (count == 0) return;
            gs.WriteByte(0xFC);
            NetUtils.WriteI32(count, buffer, 0);
            gs.Write(buffer, 0, sizeof(int));
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
                    if (entries != 8192) continue;
                    ptrInt = (int*)ptr;
                    gs.Write(buffer, 0, entries * 8);
                    entries = 0;
                }
                if (entries == 0) return;
                gs.Write(buffer, 0, entries * 8);
            }
        }
        static void WriteZonesSection(Level lvl, Stream gs, byte[] buffer)
        {
            Zone[] zones = lvl.Zones.Items;
            if (zones.Length == 0) return;
            gs.WriteByte(0x51);
            NetUtils.WriteI32(zones.Length, buffer, 0);
            gs.Write(buffer, 0, sizeof(int));
            foreach (Zone z in zones)
            {
                NetUtils.WriteU16(z.MinX, buffer, 0 * 2);
                NetUtils.WriteU16(z.MaxX, buffer, 1 * 2);
                NetUtils.WriteU16(z.MinY, buffer, 2 * 2);
                NetUtils.WriteU16(z.MaxY, buffer, 3 * 2);
                NetUtils.WriteU16(z.MinZ, buffer, 4 * 2);
                NetUtils.WriteU16(z.MaxZ, buffer, 5 * 2);
                gs.Write(buffer, 0, 6 * 2);
                ConfigElement[] elem = Server.zoneConfig;
                NetUtils.WriteI32(elem.Length, buffer, 0);
                gs.Write(buffer, 0, sizeof(int));
                for (int i = 0; i < elem.Length; i++)
                {
                    string value = elem[i].Format(z.Config);
                    int count = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 2);
                    NetUtils.WriteU16((ushort)count, buffer, 0);
                    gs.Write(buffer, 0, count + 2);
                }
            }
        }
    }
}
