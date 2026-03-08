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
using System;
using System.IO;
using System.IO.Compression;
namespace MCGalaxy.Levels.IO
{
    public sealed class DatImporter : IMapImporter
    {
        public override string Extension => ".dat";
        public override string Description => "Minecraft Classic map";
        public override Vec3U16 ReadDimensions(Stream src) => throw new NotSupportedException();
        public override Level Read(Stream src, string name, bool metadata)
        {
            using GZipStream s = new(src, CompressionMode.Decompress);
            Level lvl = new(name, 0, 0, 0);
            JavaReader r = new()
            {
                src = new BinaryReader(s)
            };
            int signature = r.ReadInt32();
            return signature == 0x01010101
                ? ReadFormat0(lvl, s)
                : signature != 0x271BB788
                ? throw new InvalidDataException("Invalid .dat map signature")
                : r.ReadUInt8() switch
            {
                0x01 => ReadFormat1(lvl, r),
                0x02 => ReadFormat2(lvl, r),
                _ => throw new InvalidDataException("Invalid .dat map version"),
            };
        }
        static Level ReadFormat0(Level lvl, Stream s)
        {
            lvl.Width = 256;
            lvl.Height = 64;
            lvl.Length = 256;
            byte[] blocks = new byte[256 * 64 * 256];
            blocks[0] = 1;
            blocks[1] = 1;
            blocks[2] = 1;
            blocks[3] = 1;
            s.Read(blocks, 4, blocks.Length - 4);
            lvl.blocks = blocks;
            SetupClassic013(lvl);
            lvl.Config.EdgeBlock = Block.Air;
            lvl.Config.HorizonBlock = Block.Air;
            return lvl;
        }
        static Level ReadFormat1(Level lvl, JavaReader r)
        {
            r.ReadUtf8();
            r.ReadUtf8();
            r.ReadInt64();
            lvl.Width = r.ReadUInt16();
            lvl.Length = r.ReadUInt16();
            lvl.Height = r.ReadUInt16();
            lvl.blocks = r.ReadBytes(lvl.Width * lvl.Height * lvl.Length);
            SetupClassic013(lvl);
            return lvl;
        }
        static void SetupClassic013(Level lvl)
        {
            lvl.spawnx = (ushort)(lvl.Width / 2);
            lvl.spawny = lvl.Height;
            lvl.spawnz = (ushort)(lvl.Length / 2);
            lvl.Config.CloudsHeight = -30000;
            lvl.Config.SkyColor = "#7FCCFF";
            lvl.Config.FogColor = "#7FCCFF";
        }
        static Level ReadFormat2(Level lvl, JavaReader r)
        {
            if (r.ReadUInt16() != 0xACED)
                throw new InvalidDataException("Invalid stream magic");
            if (r.ReadUInt16() != 0x0005)
                throw new InvalidDataException("Invalid stream version");
            JObject obj = (JObject)r.ReadObject();
            ParseRootObject(lvl, obj);
            return lvl;
        }
        static ushort U16(object o) => (ushort)(int)o;
        static void ParseRootObject(Level lvl, JObject obj)
        {
            JFieldDesc[] fields = obj.Desc.Fields;
            object[] values = obj.ClassData[0].Values;
            for (int i = 0; i < fields.Length; i++)
            {
                JFieldDesc f = fields[i];
                object value = values[i];
                if (f.Name == "width")
                    lvl.Width = U16(value);
                if (f.Name == "height")
                    lvl.Length = U16(value);
                if (f.Name == "depth")
                    lvl.Height = U16(value);
                if (f.Name == "blocks")
                    lvl.blocks = (byte[])((JArray)value).Values;
                if (f.Name == "xSpawn")
                    lvl.spawnx = U16(value);
                if (f.Name == "ySpawn")
                    lvl.spawny = U16(value);
                if (f.Name == "zSpawn")
                    lvl.spawnz = U16(value);
            }
        }
    }
}
