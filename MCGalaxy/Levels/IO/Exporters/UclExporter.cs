using System;
using System.IO;
namespace MCGalaxy.Levels.IO
{
    //UCL -> UnCompressedLevel
    public class UclExporter : IMapExporter
    {
        public override string Extension { get { return ".ucl"; } }
        public override void Write(Stream dst, Level lvl)
        {
            byte[] header = new byte[16];
            BitConverter.GetBytes(0000).CopyTo(header, 0);
            dst.Write(header, 0, 2);
            BitConverter.GetBytes(lvl.Width).CopyTo(header, 0);
            BitConverter.GetBytes(lvl.Height).CopyTo(header, 2);
            BitConverter.GetBytes(lvl.Length).CopyTo(header, 4);
            BitConverter.GetBytes(lvl.spawnx).CopyTo(header, 6);
            BitConverter.GetBytes(lvl.spawnz).CopyTo(header, 8);
            BitConverter.GetBytes(lvl.spawny).CopyTo(header, 10);
            header[12] = lvl.rotx;
            header[13] = lvl.roty;
            header[14] = (byte)lvl.VisitAccess.Min;
            header[15] = (byte)lvl.BuildAccess.Min;
            dst.Write(header, 0, header.Length);
            byte[] levelBlocks = new byte[lvl.blocks.Length];
            for (int i = 0; i < lvl.blocks.Length; ++i)
            {
                if (lvl.blocks[i] < 66)
                {
                    levelBlocks[i] = lvl.blocks[i];
                }
                else
                {
                    levelBlocks[i] = lvl.blocks[i];
                }
            }
            dst.Write(levelBlocks, 0, levelBlocks.Length);
            dst.Close();
        }
    }
}