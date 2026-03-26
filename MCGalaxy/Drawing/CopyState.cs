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
namespace MCGalaxy.Drawing
{
    /// <summary> Represents a copied region/area of blocks plus some additional data </summary>
    public sealed class CopyState
    {
        public byte[] blocks;
        public byte[][] extBlocks;
        public int X, Y, Z, OriginX, OriginY, OriginZ, 
            Width, Height, Length, UsedBlocks;
        public bool PasteAir;
        public Vec3S32 Offset;
        /// <summary> Point at time at which this copy was created </summary>
        public DateTime CopyTime;
        /// <summary> Origin of this copy/where this copy came from </summary>
        /// <example> "level example1", "file example2" </example>
        public string CopySource;
        internal int OppositeOriginX => OriginX == X ? X + Width - 1 : X;
        internal int OppositeOriginY => OriginY == Y ? Y + Height - 1 : Y;
        internal int OppositeOriginZ => OriginZ == Z ? Z + Length - 1 : Z;
        public int Volume => Width * Height * Length;
        public int ExtChunks => (Volume + (0x1000 - 1)) / 0x1000;
        public string Summary => Volume + " blocks from " + CopySource + ", " + (DateTime.UtcNow - CopyTime).Shorten(true) + " ago";
        public CopyState(int x, int y, int z, int width, int height, int length)
        {
            Init(x, y, z, width, height, length);
            CopyTime = DateTime.UtcNow;
        }
        public void Init(int x, int y, int z, int width, int height, int length)
        {
            X = x;
            Y = y; 
            Z = z;
            Width = width; 
            Height = height;
            Length = length;
            blocks = new byte[Volume];
            extBlocks = new byte[ExtChunks][];
            UsedBlocks = Volume;
        }
        public void Clear()
        {
            blocks = null;
            extBlocks = null;
        }
        public void GetCoords(int index, out ushort x, out ushort y, out ushort z)
        {
            y = (ushort)(index / Width / Length);
            index -= y * Width * Length;
            z = (ushort)(index / Width);
            index -= z * Width;
            x = (ushort)index;
        }
        public int GetIndex(int x, int y, int z) => (y * Length + z) * Width + x;
        public ushort Get(int index)
        {
            byte raw = blocks[index];
            ushort extended = Block.ExtendedBase[raw];
            if (extended == 0) return raw;
            byte[] chunk = extBlocks[index >> 12];
            return chunk == null ? Block.Air : (ushort)(extended | chunk[index & 0xFFF]);
        }
        public void Set(ushort block, int index)
        {
            if (block >= 256)
            {
                blocks[index] = Block.ExtendedClass[block >> 8];
                byte[] chunk = extBlocks[index >> 12];
                if (chunk == null)
                {
                    chunk = new byte[0x1000];
                    extBlocks[index >> 12] = chunk;
                }
                chunk[index & 0xFFF] = (byte)block;
            }
            else
                blocks[index] = (byte)block;
        }
        public void Set(ushort block, int x, int y, int z) => Set(block, (y * Length + z) * Width + x);
        /// <summary> Saves this copy state to the given stream. </summary>
        public void SaveTo(Stream stream)
        {
            BinaryWriter w = new(stream);
            w.Write(0x434F5053);
            w.Write(X);
            w.Write(Y); 
            w.Write(Z);
            w.Write(Width);
            w.Write(Height); 
            w.Write(Length);
            byte[] data = blocks.GZip();
            w.Write(data.Length);
            w.Write(data);
            for (int i = 0; i < extBlocks.Length; i++)
            {
                if (extBlocks[i] == null)
                {
                    w.Write((byte)0);
                    continue;
                }
                w.Write((byte)1);
                data = extBlocks[i].GZip();
                w.Write((ushort)data.Length);
                w.Write(data);
            }
            w.Write(OriginX); 
            w.Write(OriginY); 
            w.Write(OriginZ);
            w.Write((byte)0x0f);
            w.Write(Offset.X); 
            w.Write(Offset.Y);
            w.Write(Offset.Z);
            w.Write((byte)(PasteAir ? 1 : 0));
        }
        /// <summary> Loads this copy state from the given stream. </summary>
        public void LoadFrom(Stream stream)
        {
            BinaryReader r = new(stream);
            int id = r.ReadInt32();
            if (!(id == 0x434F5059 || id == 0x434F5043 || id == 0x434F504F || id == 0x434F5053))
                throw new InvalidDataException("invalid identifier");
            X = r.ReadInt32();
            Y = r.ReadInt32();
            Z = r.ReadInt32();
            Width = r.ReadInt32();
            Height = r.ReadInt32(); 
            Length = r.ReadInt32();
            LoadBlocks(r, id);
            UsedBlocks = Volume;
            if (id == 0x434F5059) return;
            OriginX = r.ReadInt32();
            OriginY = r.ReadInt32(); 
            OriginZ = r.ReadInt32();
            if (stream.ReadByte() != 0x0f) return;
            Offset.X = r.ReadInt32(); 
            Offset.Y = r.ReadInt32(); 
            Offset.Z = r.ReadInt32();
            PasteAir = stream.ReadByte() == 1;
        }
        public void LoadBlocks(BinaryReader r, int id)
        {
            byte[] allExtBlocks;
            int dataLen;
            extBlocks = new byte[(Volume + (0x1000 - 1)) / 0x1000][];
            if (id == 0x434F5059)
            {
                blocks = r.ReadBytes(Volume);
                allExtBlocks = r.ReadBytes(Volume);
                UnpackExtBlocks(allExtBlocks);
            }
            else
            {
                dataLen = r.ReadInt32();
                blocks = r.ReadBytes(dataLen).Decompress(Volume);
                if (id == 0x434F5043)
                {
                    dataLen = r.ReadInt32();
                    allExtBlocks = r.ReadBytes(dataLen).Decompress(Volume);
                    UnpackExtBlocks(allExtBlocks);
                }
                else if (id == 0x434F504F)
                {
                    dataLen = r.ReadInt32();
                    allExtBlocks = r.ReadBytes(dataLen).Decompress((Volume + 7) / 8);
                    UnpackPackedExtBlocks(allExtBlocks);
                }
                else
                    for (int i = 0; i < extBlocks.Length; i++)
                    {
                        if (r.ReadByte() == 0) continue;
                        dataLen = r.ReadUInt16();
                        extBlocks[i] = r.ReadBytes(dataLen).Decompress(0x1000);
                    }
            }
        }
        public void UnpackExtBlocks(byte[] allExtBlocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i] != Block.custom_block) continue;
                Set((ushort)(256 | allExtBlocks[i]), i);
            }
        }
        public void UnpackPackedExtBlocks(byte[] allExtBlocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                bool isExt = (allExtBlocks[i >> 3] & (1 << (i & 0x7))) != 0;
                if (isExt)
                    Set((ushort)(256 | blocks[i]), i);
            }
        }
    }
}
