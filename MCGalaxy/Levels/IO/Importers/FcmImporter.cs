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
using System.Text;
namespace MCGalaxy.Levels.IO
{
    public sealed class FcmImporter : IMapImporter
    {
        public override string Extension => ".fcm";
        public override string Description => "fCraft/800Craft/ProCraft map";
        public override Vec3U16 ReadDimensions(Stream src) => ReadHeader(new BinaryReader(src));
        public override Level Read(Stream src, string name, bool metadata)
        {
            BinaryReader reader = new(src);
            Vec3U16 dims = ReadHeader(reader);
            Level lvl = new(name, dims.X, dims.Y, dims.Z)
            {
                spawnx = (ushort)(reader.ReadInt32() / 32),
                spawny = (ushort)(reader.ReadInt32() / 32),
                spawnz = (ushort)(reader.ReadInt32() / 32),
                rotx = reader.ReadByte(),
                roty = reader.ReadByte()
            };
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadBytes(16);
            reader.ReadBytes(26);
            int metaSize = reader.ReadInt32();
            using (DeflateStream ds = new(src, CompressionMode.Decompress))
            {
                reader = new BinaryReader(ds);
                for (int i = 0; i < metaSize; i++)
                {
                    string group = ReadString(reader),
                        key = ReadString(reader),
                        value = ReadString(reader);
                    if (group != "zones") continue;
                    try
                    {
                        ParseZone(lvl, value);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Error importing zone '" + key + "' from fCraft map", ex);
                    }
                }
                StreamUtils.ReadFully(ds, lvl.blocks, 0, lvl.blocks.Length);
            }
            ConvertCustom(lvl);
            return lvl;
        }
        static Vec3U16 ReadHeader(BinaryReader reader)
        {
            if (reader.ReadInt32() != 0x0FC2AF40 || reader.ReadByte() != 13)
            {
                throw new InvalidDataException("Unexpected constant in .fcm file");
            }
            Vec3U16 dims = new()
            {
                X = reader.ReadUInt16(),
                Y = reader.ReadUInt16(),
                Z = reader.ReadUInt16()
            };
            return dims;
        }
        static string ReadString(BinaryReader reader)
        {
            int length = reader.ReadUInt16();
            byte[] data = reader.ReadBytes(length);
            return Encoding.ASCII.GetString(data);
        }
        static readonly char[] comma = new char[] { ',' };
        static void ParseZone(Level lvl, string raw)
        {
            string[] parts = raw.Split(comma);
            string[] header = parts[0].SplitSpaces();
            Zone zone = new();
            zone.Config.Name = header[0];
            zone.MinX = ushort.Parse(header[1]);
            zone.MinZ = ushort.Parse(header[2]);
            zone.MinY = ushort.Parse(header[3]);
            zone.MaxX = ushort.Parse(header[4]);
            zone.MaxZ = ushort.Parse(header[5]);
            zone.MaxY = ushort.Parse(header[6]);
            string minRaw = header[7];
            int idStart = minRaw.IndexOf('#');
            if (idStart >= 0) minRaw = minRaw.Substring(0, idStart);
            Group minRank = Group.Find(minRaw);
            if (minRank != null) zone.Config.BuildMin = minRank.Permission;
            if (header.Length > 8)
            {
                zone.Config.ShowColor = header[9];
                zone.Config.ShowAlpha = byte.Parse(header[10]);
            }
            if (parts[1].Length > 0)
            {
                string[] whitelist = parts[1].SplitSpaces();
                zone.Config.BuildWhitelist.AddRange(whitelist);
            }
            if (parts[2].Length > 0)
            {
                string[] blacklist = parts[2].SplitSpaces();
                zone.Config.BuildBlacklist.AddRange(blacklist);
            }
            zone.AddTo(lvl);
        }
    }
}
