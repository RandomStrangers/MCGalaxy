using MCGalaxy.Maths;
using System.IO;
namespace MCGalaxy.Levels.IO
{
    public class UclImporter : IMapImporter
    {
        public override string Extension { get { return ".ucl"; } }
        public override string Description { get { return "Uncompressed level"; } }
        readonly byte[] header = new byte[16];
        public override Vec3U16 ReadDimensions(Stream src)
        {
            Read(header, 2);
            ReadU16(header, 0);
            Read(header, 16);
            ushort x = ReadU16(header, 0),
                z = ReadU16(header, 2),
                y = ReadU16(header, 4);
            return new(x, y, z);
        }
        Stream stream;
        public override Level Read(Stream gs, string name, bool metadata)
        {
            stream = gs;
            return ReadLevel(name);
        }
        Level ReadLevel(string name)
        {
            Read(header, 2);
            ReadU16(header, 0);
            Read(header, 16);
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
            stream.Read(blocks, 0, blocks.Length);
            for (long i = 0; i < blocks.LongLength; ++i)
            {
                lvl.blocks[i] = blocks[i];
            }
            return lvl;
        }
        void Read(byte[] data, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(data, 0, count);
                if (read == 0)
                {
                    throw new EndOfStreamException("End of stream reading data");
                }
                count -= read;
            }
        }
        ushort ReadU16(byte[] array, byte offset)
        {
            return (ushort)(array[offset] | array[offset + 1] << 8);
        }
    }
}