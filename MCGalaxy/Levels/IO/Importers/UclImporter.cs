using MCGalaxy.Maths;
using System.IO;
namespace MCGalaxy.Levels.IO
{
    public class UclImporter : IMapImporter
    {
        public override string Extension => ".ucl";
        public override string Description => "Uncompressed level";
        readonly byte[] header = new byte[16];
        public override Vec3U16 ReadDimensions(Stream src)
        {
            Read(src, header, 2);
            ReadU16(header, 0);
            Read(src, header, 16);
            ushort x = ReadU16(header, 0),
                z = ReadU16(header, 2),
                y = ReadU16(header, 4);
            return new(x, y, z);
        }
        public override Level Read(Stream gs, string name, bool metadata)
        {
            Read(gs, header, 2);
            ReadU16(header, 0);
            Read(gs, header, 16);
            ushort x = ReadU16(header, 0),
                z = ReadU16(header, 2),
                y = ReadU16(header, 4);
            Level lvl = new(name, x, y, z)
            {
                spawnx = ReadU16(header, 6),
                spawnz = ReadU16(header, 8),
                spawny = ReadU16(header, 10),
                rotx = header[12],
                roty = header[13]
            };
            byte[] blocks = new byte[lvl.blocks.Length];
            gs.Read(blocks, 0, blocks.Length);
            for (long i = 0; i < blocks.LongLength; ++i)
            {
                lvl.blocks[i] = blocks[i];
            }
            return lvl;
        }
        void Read(Stream gs, byte[] data, int count)
        {
            while (count > 0)
            {
                int read = gs.Read(data, 0, count);
                if (read == 0)
                {
                    throw new EndOfStreamException("End of stream reading data");
                }
                count -= read;
            }
        }
        ushort ReadU16(byte[] array, byte offset) => (ushort)(array[offset] | array[offset + 1] << 8);
    }
}
