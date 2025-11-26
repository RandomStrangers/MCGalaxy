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
using System.IO;
using System.IO.Compression;
namespace MCGalaxy.Util.Imaging
{
    public unsafe class PngDecoder : ImageDecoder
    {
        int bytesPerPixel, scanline_size;
        RowExpander rowExpander;
        static readonly byte[] pngSig = new byte[] 
        { 
            137, 80, 78, 71, 13, 10, 26, 10 
        },
        samplesPerPixel = new byte[] 
        {
            1, 0, 3, 1, 2, 0, 4 
        };
        public static bool DetectHeader(byte[] data)
        {
            return MatchesSignature(data, pngSig);
        }
        static Pixel ExpandRGB(byte bitsPerSample, int r, int g, int b)
        {
            switch (bitsPerSample)
            {
                case 1:
                    r *= 255; 
                    g *= 255; 
                    b *= 255; 
                    break;
                case 2:
                    r *= 85; 
                    g *= 85; 
                    b *= 85; 
                    break;
                case 4:
                    r *= 17; 
                    g *= 17; 
                    b *= 17; 
                    break;
            }
            return new((byte)r, (byte)g, (byte)b, 0);
        }
        public override SimpleBitmap Decode(byte[] src)
        {
            byte colorspace = 0xFF, bitsPerSample = 0;
            Pixel trnsColor = Pixel.BLACK;
            Pixel[] palette = null;
            SimpleBitmap bmp = new();
            MemoryStream all_idats = new();
            bool reachedEnd = false;
            SetBuffer(src);
            if (!DetectHeader(src))
            {
                Fail("sig invalid");
            }
            AdvanceOffset(pngSig.Length);
            while (!reachedEnd)
            {
                int offset = AdvanceOffset(4 + 4),
                    dataSize = MemUtils.ReadI32_BE(src, offset + 0),
                    fourCC = MemUtils.ReadI32_BE(src, offset + 4);
                switch (fourCC)
                {
                    case ('I' << 24) | ('H' << 16) | ('D' << 8) | 'R':
                        {
                            if (dataSize != 13)
                            {
                                Fail("Header size");
                            }
                            offset = AdvanceOffset(13);
                            bmp.Width = MemUtils.ReadI32_BE(src, offset + 0);
                            bmp.Height = MemUtils.ReadI32_BE(src, offset + 4);
                            if (bmp.Width < 0 || bmp.Width > 32768)
                            {
                                Fail("too wide");
                            }
                            if (bmp.Height < 0 || bmp.Height > 32768)
                            {
                                Fail("too tall");
                            }
                            bitsPerSample = src[offset + 8];
                            colorspace = src[offset + 9];
                            if (bitsPerSample == 16)
                            {
                                Fail("16 bpp");
                            }
                            rowExpander = GetRowExpander(colorspace, bitsPerSample);
                            if (rowExpander == null)
                            {
                                Fail("Colorspace/bpp combination");
                            }
                            if (src[offset + 10] != 0)
                            {
                                Fail("Compression method");
                            }
                            if (src[offset + 11] != 0)
                            {
                                Fail("Filter");
                            }
                            if (src[offset + 12] != 0)
                            {
                                Fail("Interlaced unsupported");
                            }
                            bytesPerPixel = ((samplesPerPixel[colorspace] * bitsPerSample) + 7) >> 3;
                            scanline_size = ((samplesPerPixel[colorspace] * bitsPerSample * bmp.Width) + 7) >> 3;
                            bmp.AllocatePixels();
                        }
                        break;
                    case ('P' << 24) | ('L' << 16) | ('T' << 8) | 'E':
                        {
                            if (dataSize > 256 * 3)
                            {
                                Fail("Palette size");
                            }
                            if ((dataSize % 3) != 0)
                            {
                                Fail("Palette align");
                            }
                            offset = AdvanceOffset(dataSize);
                            palette ??= CreatePalette();
                            for (int i = 0; i < dataSize; i += 3)
                            {
                                palette[i / 3].R = src[offset + i];
                                palette[i / 3].G = src[offset + i + 1];
                                palette[i / 3].B = src[offset + i + 2];
                            }
                        }
                        break;
                    case ('t' << 24) | ('R' << 16) | ('N' << 8) | 'S':
                        {
                            if (colorspace == 0)
                            {
                                if (dataSize != 2)
                                {
                                    Fail("tRNS size");
                                }
                                offset = AdvanceOffset(dataSize);
                                byte rgb = src[offset + 1];
                                trnsColor = ExpandRGB(bitsPerSample, rgb, rgb, rgb);
                            }
                            else if (colorspace == 3)
                            {
                                if (dataSize > 256)
                                {
                                    Fail("tRNS size");
                                }
                                offset = AdvanceOffset(dataSize);
                                palette ??= CreatePalette();
                                for (int i = 0; i < dataSize; i++)
                                {
                                    palette[i].A = src[offset + i];
                                }
                            }
                            else if (colorspace == 2)
                            {
                                if (dataSize != 6)
                                {
                                    Fail("tRNS size");
                                }
                                offset = AdvanceOffset(dataSize);
                                byte r = src[offset + 1],
                                    g = src[offset + 3],
                                    b = src[offset + 5];
                                trnsColor = ExpandRGB(bitsPerSample, r, g, b);
                            }
                            else
                            {
                                Fail("tRNS/colorspace combination");
                            }
                        }
                        break;
                    case ('I' << 24) | ('D' << 16) | ('A' << 8) | 'T':
                        {
                            if (!read_zlib_header)
                            {
                                SkipZLibHeader(src);
                                dataSize -= 2;
                            }
                            offset = AdvanceOffset(dataSize);
                            all_idats.Write(src, offset, dataSize);
                        }
                        break;
                    case ('I' << 24) | ('E' << 16) | ('N' << 8) | 'D':
                        reachedEnd = true;
                        break;
                    default:
                        AdvanceOffset(dataSize);
                        break;
                }
                AdvanceOffset(4);
            }
            all_idats.Position = 0;
            using (DeflateStream comp = new(all_idats, CompressionMode.Decompress))
            {
                DecompressImage(comp, bmp, palette, trnsColor);
            }
            return bmp;
        }
        static Pixel[] CreatePalette()
        {
            Pixel[] pal = new Pixel[256];
            for (int i = 0; i < pal.Length; i++)
            {
                pal[i] = Pixel.BLACK;
            }
            return pal;
        }
        void DecompressImage(Stream src, SimpleBitmap bmp, Pixel[] palette, Pixel trnsColor)
        {
            if (bmp.pixels == null)
            {
                Fail("no data");
            }
            byte[] line = new byte[scanline_size],
                tmp, prior = new byte[scanline_size],
                one = new byte[1];
            fixed (Pixel* dst = bmp.pixels)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int read = src.Read(one, 0, 1);
                    if (read == 0)
                    {
                        Fail("scanline");
                    }
                    byte method = one[0];
                    if (method > 4)
                    {
                        Fail("Scanline");
                    }
                    StreamUtils.ReadFully(src, line, 0, scanline_size);
                    ReconstructRow(method, bytesPerPixel, line, prior, scanline_size);
                    rowExpander(bmp.Width, palette, line, dst + y * bmp.Width);
                    tmp = line;
                    line = prior;
                    prior = tmp;
                }
            }
            if (trnsColor.A == 0)
            {
                MakeTransparent(bmp.pixels, trnsColor);
            }
            return;
        }
        static void MakeTransparent(Pixel[] img, Pixel color)
        {
            for (int i = 0; i < img.Length; i++)
            {
                if (img[i].R != color.R)
                {
                    continue;
                }
                if (img[i].G != color.G)
                {
                    continue;
                }
                if (img[i].B != color.B)
                {
                    continue;
                }
                img[i].A = 0;
            }
        }
        bool read_zlib_header;
        void SkipZLibHeader(byte[] src)
        {
            int offset = AdvanceOffset(2);
            byte method = src[offset + 0];
            if ((method & 0x0F) != 0x08)
            {
                Fail("Zlib method");
            }
            byte flags = src[offset + 1];
            if ((flags & 0x20) != 0)
            {
                Fail("Zlip flags");
            }
            read_zlib_header = true;
        }
        static void ReconstructRow(byte type, int bytesPerPixel, byte[] line, byte[] prior, int lineLen)
        {
            int i, j;
            switch (type)
            {
                case 1:
                    for (i = bytesPerPixel, j = 0; i < lineLen; i++, j++)
                    {
                        line[i] += line[j];
                    }
                    return;
                case 2:
                    for (i = 0; i < lineLen; i++)
                    {
                        line[i] += prior[i];
                    }
                    return;
                case 3:
                    for (i = 0; i < bytesPerPixel; i++)
                    {
                        line[i] += (byte)(prior[i] >> 1);
                    }
                    for (j = 0; i < lineLen; i++, j++)
                    {
                        line[i] += (byte)((prior[i] + line[j]) >> 1);
                    }
                    return;
                case 4:
                    for (i = 0; i < bytesPerPixel; i++)
                    {
                        line[i] += prior[i];
                    }
                    for (j = 0; i < lineLen; i++, j++)
                    {
                        byte a = line[j], b = prior[i], c = prior[j];
                        int p = a + b - c,
                            pa = Math.Abs(p - a),
                            pb = Math.Abs(p - b),
                            pc = Math.Abs(p - c);
                        if (pa <= pb && pa <= pc) 
                        {
                            line[i] += a; 
                        }
                        else if (pb <= pc)
                        { 
                            line[i] += b;
                        }
                        else 
                        { 
                            line[i] += c; 
                        }
                    }
                    return;
            }
        }
        delegate void RowExpander(int width, Pixel[] palette, byte[] src, Pixel* dst);
        static int Get_1BPP(byte[] src, int i)
        {
            int j = 7 - (i & 7);
            return (src[i >> 3] >> j) & 0x01;
        }
        static int Get_2BPP(byte[] src, int i)
        {
            int j = (3 - (i & 3)) * 2;
            return (src[i >> 2] >> j) & 0x03;
        }
        static int Get_4BPP(byte[] src, int i)
        {
            int j = (1 - (i & 1)) * 4;
            return (src[i >> 1] >> j) & 0x0F;
        }
        static void Expand_GRAYSCALE_1(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte rgb = (byte)(Get_1BPP(src, i) * 255);
                dst[i] = new(rgb, rgb, rgb, 255);
            }
        }
        static void Expand_GRAYSCALE_2(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte rgb = (byte)(Get_2BPP(src, i) * 85);
                dst[i] = new(rgb, rgb, rgb, 255);
            }
        }
        static void Expand_GRAYSCALE_4(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte rgb = (byte)(Get_4BPP(src, i) * 17);
                dst[i] = new(rgb, rgb, rgb, 255);
            }
        }
        static void Expand_GRAYSCALE_8(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte rgb = src[i];
                dst[i] = new(rgb, rgb, rgb, 255);
            }
        }
        static void Expand_RGB_8(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte r = src[i * 3 + 0],
                    g = src[i * 3 + 1],
                    b = src[i * 3 + 2];
                dst[i] = new(r, g, b, 255);
            }
        }
        static void Expand_INDEXED_1(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++) 
            { 
                dst[i] = palette[Get_1BPP(src, i)];
            }
        }
        static void Expand_INDEXED_2(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++) 
            { 
                dst[i] = palette[Get_2BPP(src, i)];
            }
        }
        static void Expand_INDEXED_4(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++) 
            {
                dst[i] = palette[Get_4BPP(src, i)]; 
            }
        }
        static void Expand_INDEXED_8(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++) 
            { 
                dst[i] = palette[src[i]]; 
            }
        }
        static void Expand_GRAYSCALE_A_8(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte rgb = src[i * 2 + 0],
                    a = src[i * 2 + 1];
                dst[i] = new(rgb, rgb, rgb, a);
            }
        }
        static void Expand_RGB_A_8(int width, Pixel[] palette, byte[] src, Pixel* dst)
        {
            for (int i = 0; i < width; i++)
            {
                byte r = src[i * 4 + 0],
                    g = src[i * 4 + 1],
                    b = src[i * 4 + 2],
                    a = src[i * 4 + 3];
                dst[i] = new(r, g, b, a);
            }
        }
        static RowExpander GetRowExpander(byte colorspace, byte bitsPerSample)
        {
            return colorspace switch
            {
                0 => bitsPerSample switch
                {
                    1 => Expand_GRAYSCALE_1,
                    2 => Expand_GRAYSCALE_2,
                    4 => Expand_GRAYSCALE_4,
                    8 => Expand_GRAYSCALE_8,
                    _ => null,
                },
                2 => bitsPerSample switch
                {
                    8 => Expand_RGB_8,
                    _ => null,
                },
                3 => bitsPerSample switch
                {
                    1 => Expand_INDEXED_1,
                    2 => Expand_INDEXED_2,
                    4 => Expand_INDEXED_4,
                    8 => Expand_INDEXED_8,
                    _ => null,
                },
                4 => bitsPerSample switch
                {
                    8 => Expand_GRAYSCALE_A_8,
                    _ => null,
                },
                6 => bitsPerSample switch
                {
                    8 => Expand_RGB_A_8,
                    _ => null,
                },
                _ => null,
            };
        }
    }
}