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
namespace MCGalaxy.Util.Imaging
{
    public unsafe class JpegDecoder : ImageDecoder
    {
        static readonly byte[] jfifSig = new byte[] 
        { 
            0xFF, 0xD8, 0xFF, 0xE0 
        },
        exifSig = new byte[] 
        { 
            0xFF, 0xD8, 0xFF, 0xE1 
        }, 
        zigzag_to_linear = new byte[64]
        {
            0,  1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63,
        };
        readonly byte[][] quant_tables = new byte[4][];
        readonly HuffmanTable[] ac_huff_tables = new HuffmanTable[4],
            dc_huff_tables = new HuffmanTable[4];
        JpegComponent[] comps;
        byte lowestHor = 1, lowestVer = 1;
        public static bool DetectHeader(byte[] data)
        {
            return MatchesSignature(data, jfifSig)
                || MatchesSignature(data, exifSig);
        }
        public override SimpleBitmap Decode(byte[] src)
        {
            SetBuffer(src);
            SimpleBitmap bmp = new();
            ReadMarkers(src, bmp);
            return bmp;
        }
        void ReadMarkers(byte[] src, SimpleBitmap bmp)
        {
            for (; ; )
            {
                int offset = AdvanceOffset(2);
                ushort marker = MemUtils.ReadU16_BE(src, offset);
                if (marker == 0xFFD8)
                {
                }
                else if (marker == 0xFFD9)
                {
                    return;
                }
                else if (marker >= 0xFFE0 && marker <= 0xFFEF)
                {
                    SkipMarker(src);
                }
                else if (marker == 0xFFFE || marker == 0xFFDD)
                {
                    SkipMarker(src);
                }
                else if (marker == 0xFFC4)
                {
                    ReadHuffmanTable(src);
                }
                else if (marker == 0xFFDB)
                {
                    ReadQuantisationTables(src);
                }
                else if (marker == 0xFFC0)
                {
                    ReadFrameStart(src, bmp);
                }
                else if (marker == 0xFFDA)
                {
                    ReadScanStart(src);
                    DecodeMCUs(src, bmp);
                }
                else
                {
                    Fail("unknown marker:" + marker.ToString("X4"));
                }
            }
        }
        void SkipMarker(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            AdvanceOffset(length - 2);
        }
        void ReadQuantisationTables(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            length -= 2; 
            offset = AdvanceOffset(length);
            while (length > 0)
            {
                if (length < 65)
                {
                    Fail("quant table too short: " + length);
                }
                length -= 65;
                byte flags = src[offset++];
                if ((flags & 0xF0) != 0)
                {
                    Fail("16 bit quant table unsupported");
                }
                int idx = flags & 0x03;
                if (quant_tables[idx] == null)
                {
                    quant_tables[idx] = new byte[64];
                }
                byte[] table = quant_tables[idx];
                for (int i = 0; i < table.Length; i++)
                {
                    table[i] = src[offset++];
                }
            }
        }
        void ReadHuffmanTable(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            length -= 2;
            offset = AdvanceOffset(length);
            while (length > 0)
            {
                byte flags = src[offset++];
                HuffmanTable table = new();
                HuffmanTable[] tables = (flags >> 4) != 0 ? ac_huff_tables : dc_huff_tables;
                tables[flags & 0x03] = table;
                int read = DecodeHuffmanTable(src, table, ref offset);
                length -= 1 + read;
            }
        }
        int DecodeHuffmanTable(byte[] src, HuffmanTable table, ref int offset)
        {
            table.firstCodewords = new ushort[16];
            table.endCodewords = new ushort[16];
            table.firstOffsets = new ushort[16];
            table.values = new byte[256];
            table.fast = new ushort[1 << 8];
            int code = 0, total = 0;
            byte[] counts = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                byte count = src[offset++];
                if (count > (1 << (i + 1)))
                {
                    Fail("too many codewords for bit length");
                }
                counts[i] = count;
                table.firstCodewords[i] = (ushort)code;
                table.firstOffsets[i] = (ushort)total;
                total += count;
                if (count != 0)
                {
                    table.endCodewords[i] = (ushort)(code + count);
                }
                code = (code + count) << 1;
            }
            if (total > 256)
            {
                Fail("too many values");
            }
            int valueIdx = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                for (int j = 0; j < counts[i]; j++)
                {
                    byte value = src[offset++];
                    table.values[valueIdx++] = value;
                    int len = i + 1;
                    if (len > 8)
                    {
                        continue;
                    }
                    ushort packed = (ushort)((len << 8) | value);
                    int codeword = table.firstCodewords[i] + j;
                    codeword <<= 8 - len;
                    for (int k = 0; k < 1 << (8 - len); k++)
                    {
                        table.fast[codeword + k] = packed;
                    }
                }
            }
            return 16 + total;
        }
        void ReadFrameStart(byte[] src, SimpleBitmap bmp)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            offset = AdvanceOffset(length - 2);
            byte bits = src[offset + 0];
            if (bits != 8)
            {
                Fail("bits per sample");
            }
            bmp.Height = MemUtils.ReadU16_BE(src, offset + 1);
            bmp.Width = MemUtils.ReadU16_BE(src, offset + 3);
            bmp.AllocatePixels();
            byte numComps = src[offset + 5];
            if (!(numComps == 1 || numComps == 3)) Fail("num components");
            offset += 6;
            comps = new JpegComponent[numComps];
            for (int i = 0; i < numComps; i++)
            {
                JpegComponent comp = new()
                {
                    ID = src[offset++]
                };
                byte sampling = src[offset++];
                comp.SamplingHor = (byte)(sampling >> 4);
                comp.SamplingVer = (byte)(sampling & 0x0F);
                comp.QuantTable = src[offset++];
                lowestHor = Math.Max(lowestHor, comp.SamplingHor);
                lowestVer = Math.Max(lowestVer, comp.SamplingVer);
                comps[i] = comp;
            }
            for (int i = 0; i < numComps; i++)
            {
                JpegComponent comp = comps[i];
                comp.BlocksPerMcuX = comp.SamplingHor;
                comp.BlocksPerMcuY = comp.SamplingVer;
                comp.SamplesPerBlockX = (byte)(lowestHor / comp.BlocksPerMcuX);
                comp.SamplesPerBlockY = (byte)(lowestVer / comp.BlocksPerMcuY);
                comp.OutputBlock = GetBlockOutputFunction(comp);
            }
        }
        void ReadScanStart(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            offset = AdvanceOffset(length - 2);
            byte numComps = src[offset++];
            for (int i = 0; i < numComps; i++)
            {
                byte compID = src[offset++],
                    tables = src[offset++];
                SetHuffTables(compID, tables);
            }
            offset += 3;
        }
        void SetHuffTables(byte compID, byte tables)
        {
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i].ID != compID)
                {
                    continue;
                }
                comps[i].DCHuffTable = (byte)(tables >> 4);
                comps[i].ACHuffTable = (byte)(tables & 0x0F);
                comps[i].PredDCValue = 0;
                return;
            }
            Fail("unknown scan component");
        }
        void DecodeMCUs(byte[] src, SimpleBitmap bmp)
        {
            int mcu_w = lowestHor * 8,
                mcu_h = lowestVer * 8,
                mcus_x = Utils.CeilDiv(bmp.Width, mcu_w),
                mcus_y = Utils.CeilDiv(bmp.Height, mcu_h);
            JpegComponent[] comps = this.comps;
            int* block = stackalloc int[64 + 32];
            float* output = stackalloc float[64];
            YCbCr[] colors = new YCbCr[mcu_w * mcu_h];
            for (int mcuY = 0; mcuY < mcus_y; mcuY++)
            {
                for (int mcuX = 0; mcuX < mcus_x; mcuX++)
                {
                    if (hit_rst)
                    {
                        hit_rst = false;
                        ConsumeBits(bit_cnt & 0x07);
                        for (int i = 0; i < comps.Length; i++) comps[i].PredDCValue = 0;
                    }
                    for (int i = 0; i < comps.Length; i++)
                    {
                        JpegComponent comp = comps[i];
                        for (int by = 0; by < comp.BlocksPerMcuY; by++)
                        {
                            for (int bx = 0; bx < comp.BlocksPerMcuX; bx++)
                            {
                                DecodeBlock(comp, src, block);
                                IDCT(block, output);
                                comp.OutputBlock(comp, colors, mcu_w, i, output,
                                                 bx * 8, by * 8);
                            }
                        }
                    }
                    int baseX = mcuX * mcu_w,
                        baseY = mcuY * mcu_h,
                        j = 0;
                    for (int YY = 0; YY < mcu_w; YY++)
                    {
                        for (int XX = 0; XX < mcu_h; XX++, j++)
                        {
                            int globalX = baseX + XX,
                                globalY = baseY + YY;
                            if (globalX < bmp.Width && globalY < bmp.Height)
                            {
                                float y = colors[j].Y + 128.0f,
                                    cr = colors[j].Cr,
                                    cb = colors[j].Cb,
                                    r = 1.40200f * cr + y,
                                    g = -0.34414f * cb - 0.71414f * cr + y,
                                    b = 1.77200f * cb + y;
                                Pixel p = new(ByteClamp(r), ByteClamp(g), ByteClamp(b), 255);
                                bmp.pixels[globalY * bmp.Width + globalX] = p;
                            }
                        }
                    }
                }
            }
        }
        static byte ByteClamp(float v)
        {
            if (v < 0)
            {
                return 0;
            }
            if (v > 255)
            {
                return 255;
            }
            return (byte)v;
        }
        void DecodeBlock(JpegComponent comp, byte[] src, int* block)
        {
            HuffmanTable table = dc_huff_tables[comp.DCHuffTable];
            int dc_code = ReadHuffman(table, src),
                dc_delta = ReadBiasedValue(src, dc_code),
                dc_value = comp.PredDCValue + dc_delta;
            comp.PredDCValue = dc_value;
            byte[] dequant = quant_tables[comp.QuantTable];
            for (int j = 0; j < 64; j++)
            {
                block[j] = 0;
            }
            block[0] = dc_value * dequant[0];
            table = ac_huff_tables[comp.ACHuffTable];
            int idx = 1;
            do
            {
                int code = ReadHuffman(table, src);
                if (code == 0)
                {
                    break;
                }
                int bits = code & 0x0F,
                    num_zeros = code >> 4;
                if (bits == 0)
                {
                    if (code == 0)
                    {
                        break;
                    }
                    if (num_zeros != 15)
                    {
                        Fail("too many zeroes");
                    }
                    idx += 16;
                }
                else
                {
                    idx += num_zeros;
                    int lin = zigzag_to_linear[idx];
                    block[lin] = ReadBiasedValue(src, bits) * dequant[idx];
                    idx++;
                }
            } while (idx < 64);
        }
        static void IDCT(int* block, float* output)
        {
            float* tmp = stackalloc float[8 * 8];
            for (int col = 0; col < 8; col++)
            {
                float B0 = block[0 * 8 + col], 
                    B1 = block[1 * 8 + col],
                    B2 = block[2 * 8 + col], 
                    B3 = block[3 * 8 + col],
                    B4 = block[4 * 8 + col], 
                    B5 = block[5 * 8 + col],
                    B6 = block[6 * 8 + col], 
                    B7 = block[7 * 8 + col],
                    a4 = 0.70710678118f * B0,
                    e4 = 0.70710678118f * B4,
                    c2 = 0.92387953251f * B2, 
                    c6 = 0.38268343236f * B2,
                    g2 = 0.92387953251f * B6, 
                    g6 = 0.38268343236f * B6,
                    b1 = 0.98078528040f * B1, 
                    b3 = 0.83146961230f * B1, 
                    b5 = 0.55557023302f * B1,
                    b7 = 0.19509032201f * B1,
                    d1 = 0.98078528040f * B3, 
                    d3 = 0.83146961230f * B3, 
                    d5 = 0.55557023302f * B3, 
                    d7 = 0.19509032201f * B3,
                    f1 = 0.98078528040f * B5, 
                    f3 = 0.83146961230f * B5, 
                    f5 = 0.55557023302f * B5, 
                    f7 = 0.19509032201f * B5,
                    h1 = 0.98078528040f * B7, 
                    h3 = 0.83146961230f * B7,
                    h5 = 0.55557023302f * B7, 
                    h7 = 0.19509032201f * B7,
                    w1 = a4 + e4, 
                    w2 = a4 - e4,
                    x1 = c2 + g6, 
                    x2 = c6 - g2,
                    y1 = b1 + d3, 
                    y2 = b3 - d7, 
                    y3 = b5 - d1,
                    y4 = b7 - d5,
                    z1 = f5 + h7, 
                    z2 = f1 + h5, 
                    z3 = f7 + h3, 
                    z4 = f3 - h1,
                    u1 = w1 + x1, 
                    u2 = w2 + x2, 
                    u3 = w2 - x2, 
                    u4 = w1 - x1,
                    v1 = y1 + z1, 
                    v2 = y2 - z2, 
                    v3 = y3 + z3, 
                    v4 = y4 + z4;
                tmp[0 * 8 + col] = u1 + v1;
                tmp[1 * 8 + col] = u2 + v2;
                tmp[2 * 8 + col] = u3 + v3;
                tmp[3 * 8 + col] = u4 + v4;
                tmp[4 * 8 + col] = u4 - v4;
                tmp[5 * 8 + col] = u3 - v3;
                tmp[6 * 8 + col] = u2 - v2;
                tmp[7 * 8 + col] = u1 - v1;
            }
            for (int row = 0; row < 8; row++)
            {
                float B0 = tmp[row * 8 + 0], 
                    B1 = tmp[row * 8 + 1],
                    B2 = tmp[row * 8 + 2], 
                    B3 = tmp[row * 8 + 3],
                    B4 = tmp[row * 8 + 4], 
                    B5 = tmp[row * 8 + 5],
                    B6 = tmp[row * 8 + 6], 
                    B7 = tmp[row * 8 + 7],
                    a4 = 0.70710678118f / 4.0f * B0,
                    e4 = 0.70710678118f / 4.0f * B4,
                    c2 = 0.92387953251f / 4.0f * B2, 
                    c6 = 0.38268343236f / 4.0f * B2,
                    g2 = 0.92387953251f / 4.0f * B6, 
                    g6 = 0.38268343236f / 4.0f * B6,
                    b1 = 0.98078528040f / 4.0f * B1, 
                    b3 = 0.83146961230f / 4.0f * B1, 
                    b5 = 0.55557023302f / 4.0f * B1, 
                    b7 = 0.19509032201f / 4.0f * B1,
                    d1 = 0.98078528040f / 4.0f * B3, 
                    d3 = 0.83146961230f / 4.0f * B3, 
                    d5 = 0.55557023302f / 4.0f * B3, 
                    d7 = 0.19509032201f / 4.0f * B3,
                    f1 = 0.98078528040f / 4.0f * B5, 
                    f3 = 0.83146961230f / 4.0f * B5, 
                    f5 = 0.55557023302f / 4.0f * B5, 
                    f7 = 0.19509032201f / 4.0f * B5,
                    h1 = 0.98078528040f / 4.0f * B7, 
                    h3 = 0.83146961230f / 4.0f * B7, 
                    h5 = 0.55557023302f / 4.0f * B7, 
                    h7 = 0.19509032201f / 4.0f * B7,
                    w1 = a4 + e4, 
                    w2 = a4 - e4,
                    x1 = c2 + g6, 
                    x2 = c6 - g2,
                    y1 = b1 + d3, 
                    y2 = b3 - d7, 
                    y3 = b5 - d1, 
                    y4 = b7 - d5,
                    z1 = f5 + h7, 
                    z2 = f1 + h5, 
                    z3 = f7 + h3, 
                    z4 = f3 - h1,
                    u1 = w1 + x1, 
                    u2 = w2 + x2, 
                    u3 = w2 - x2,
                    u4 = w1 - x1,
                    v1 = y1 + z1, 
                    v2 = y2 - z2, 
                    v3 = y3 + z3, 
                    v4 = y4 + z4;
                output[row * 8 + 0] = u1 + v1;
                output[row * 8 + 1] = u2 + v2;
                output[row * 8 + 2] = u3 + v3;
                output[row * 8 + 3] = u4 + v4;
                output[row * 8 + 4] = u4 - v4;
                output[row * 8 + 5] = u3 - v3;
                output[row * 8 + 6] = u2 - v2;
                output[row * 8 + 7] = u1 - v1;
            }
        }
        uint bit_buf;
        int bit_cnt;
        bool hit_end, hit_rst;
        void RefillBits(byte[] src)
        {
            while (bit_cnt <= 24 && !hit_end)
            {
                byte next = src[buf_offset++];
                if (next == 0xFF)
                {
                    byte type = src[buf_offset++];
                    if (type == (0xFFD9 & 0xFF))
                    {
                        next = 0;
                        hit_end = true;
                        buf_offset -= 2;
                    }
                    else if (type >= (0xFFD0 & 0xFF) && type <= (0xFFD7 & 0xFF))
                    {
                        hit_rst = true;
                        continue;
                    }
                    else if (type != 0)
                    {
                        Fail("unexpected marker");
                    }
                }
                bit_buf <<= 8;
                bit_buf |= next;
                bit_cnt += 8;
            }
        }
        int ReadBits(int count)
        {
            int read = bit_cnt - count,
                bits = (int)(bit_buf >> read);
            bit_buf &= (uint)((1 << read) - 1);
            bit_cnt -= count;
            return bits;
        }
        int PeekBits(int count)
        {
            int read = bit_cnt - count,
                bits = (int)(bit_buf >> read);
            return bits;
        }
        void ConsumeBits(int count)
        {
            int read = bit_cnt - count;
            bit_buf &= (uint)((1 << read) - 1);
            bit_cnt -= count;
        }
        byte ReadHuffman(HuffmanTable table, byte[] src)
        {
            RefillBits(src);
            int codeword = PeekBits(8),
                packed = table.fast[codeword];
            if (packed != 0)
            {
                ConsumeBits(packed >> 8);
                return (byte)packed;
            }
            ConsumeBits(8);
            for (int i = 8; i < 16; i++)
            {
                codeword <<= 1;
                codeword |= ReadBits(1);
                if (codeword < table.endCodewords[i])
                {
                    int offset = table.firstOffsets[i] + (codeword - table.firstCodewords[i]);
                    byte value = table.values[offset];
                    return value;
                }
            }
            Fail("no huffman code");
            return 0;
        }
        int ReadBiasedValue(byte[] src, int bits)
        {
            if (bits == 0)
            {
                return 0;
            }
            RefillBits(src);
            int value = ReadBits(bits),
                midpoint = 1 << (bits - 1);
            if (value < midpoint)
            {
                value += (-1 << bits) + 1;
            }
            return value;
        }
        static JpegBlockOutput GetBlockOutputFunction(JpegComponent comp)
        {
            if (comp.SamplesPerBlockX == 1 && comp.SamplesPerBlockY == 1)
            {
                if (comp.ID == 1)
                {
                    return Y_1x1Output;
                }
                if (comp.ID == 2)
                {
                    return Cb_1x1Output;
                }
                if (comp.ID == 3)
                {
                    return Cr_1x1Output;
                }
            }
            if (comp.SamplesPerBlockX == 2 && comp.SamplesPerBlockY == 2)
            {
                if (comp.ID == 2)
                {
                    return Cb_2x2Output;
                }
                if (comp.ID == 3)
                {
                    return Cr_2x2Output;
                }
            }
            return Generic_BlockOutput;
        }
        static void Generic_BlockOutput(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                        int i, float* output, int baseX, int baseY)
        {
            int samplesX = comp.SamplesPerBlockX,
                samplesY = comp.SamplesPerBlockY;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float sample = output[y * 8 + x];
                    for (int py = 0; py < samplesY; py++)
                    {
                        for (int px = 0; px < samplesX; px++)
                        {
                            int YY = (baseY + y) * samplesY + py,
                                XX = (baseX + x) * samplesX + px,
                                idx = YY * mcu_w + XX;
                            if (i == 0)
                            {
                                colors[idx].Y = sample;
                            }
                            else if (i == 1)
                            {
                                colors[idx].Cb = sample;
                            }
                            else if (i == 2)
                            {
                                colors[idx].Cr = sample;
                            }
                        }
                    }
                }
            }
        }
        static void Y_1x1Output(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                int i, float* output, int baseX, int baseY)
        {
            for (int y = 0, src = 0; y < 8; y++)
            {
                int dst = (baseY + y) * mcu_w + baseX + 0;
                for (int x = 0; x < 8; x++)
                {
                    colors[dst++].Y = output[src++];
                }
            }
        }
        static void Cb_1x1Output(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                int i, float* output, int baseX, int baseY)
        {
            for (int y = 0, src = 0; y < 8; y++)
            {
                int dst = (baseY + y) * mcu_w + baseX + 0;
                for (int x = 0; x < 8; x++)
                {
                    colors[dst++].Cb = output[src++];
                }
            }
        }
        static void Cr_1x1Output(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                int i, float* output, int baseX, int baseY)
        {
            for (int y = 0, src = 0; y < 8; y++)
            {
                int dst = (baseY + y) * mcu_w + baseX + 0;
                for (int x = 0; x < 8; x++)
                {
                    colors[dst++].Cr = output[src++];
                }
            }
        }
        static void Cb_2x2Output(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                 int i, float* output, int baseX, int baseY)
        {
            for (int y = 0, src = 0; y < 8; y++)
            {
                int dst = (baseY + y) * 2 * mcu_w + (baseX + 0) * 2;
                for (int x = 0; x < 8; x++, dst += 2)
                {
                    float sample = output[src++];
                    colors[dst + 0].Cb = sample; 
                    colors[dst + 1].Cb = sample;
                    colors[dst + mcu_w + 0].Cb = sample;
                    colors[dst + mcu_w + 1].Cb = sample;
                }
            }
        }
        static void Cr_2x2Output(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                 int i, float* output, int baseX, int baseY)
        {
            for (int y = 0, src = 0; y < 8; y++)
            {
                int dst = (baseY + y) * 2 * mcu_w + (baseX + 0) * 2;
                for (int x = 0; x < 8; x++, dst += 2)
                {
                    float sample = output[src++];
                    colors[dst + 0].Cr = sample; 
                    colors[dst + 1].Cr = sample;
                    colors[dst + mcu_w + 0].Cr = sample;
                    colors[dst + mcu_w + 1].Cr = sample;
                }
            }
        }
    }
    struct YCbCr 
    { 
        public float Y, Cb, Cr;
    };
    unsafe delegate void JpegBlockOutput(JpegComponent comp, YCbCr[] colors, int mcu_w,
                                         int i, float* output, int baseX, int baseY);
    class JpegComponent
    {
        public byte ID,
            QuantTable,
            ACHuffTable,
            DCHuffTable,
            BlocksPerMcuX,
            BlocksPerMcuY,
            SamplesPerBlockX,
            SamplesPerBlockY,
            SamplingHor,
            SamplingVer;
        public int PredDCValue;
        public JpegBlockOutput OutputBlock;
    }
    class HuffmanTable
    {
        public ushort[] firstCodewords,
            endCodewords,
            firstOffsets,
            fast;
        public byte[] values;
    }
}