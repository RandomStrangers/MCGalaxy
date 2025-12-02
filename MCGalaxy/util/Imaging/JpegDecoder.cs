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
            ComputeIDCTFactors();
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
                    table.values[valueIdx++] = src[offset++];
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
            if (!(numComps == 1 || numComps == 3))
            {
                Fail("num components");
            }
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
                comps[i].BlocksPerMcuX = comps[i].SamplingHor;
                comps[i].BlocksPerMcuY = comps[i].SamplingVer;
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
            //offset += 3;
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
        struct YCbCr 
        {
            public float Y, Cb, Cr; 
        };
        void DecodeMCUs(byte[] src, SimpleBitmap bmp)
        {
            int mcu_w = lowestHor * 8,
                mcu_h = lowestVer * 8,
                mcus_x = Utils.CeilDiv(bmp.Width, mcu_w),
                mcus_y = Utils.CeilDiv(bmp.Height, mcu_h);
            JpegComponent[] comps = this.comps;
            int[] block = new int[64];
            float* output = stackalloc float[64];
            YCbCr[] colors = new YCbCr[mcu_w * mcu_h];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].Cr = 128;
                colors[i].Cb = 128;
            }
            for (int mcuY = 0; mcuY < mcus_y; mcuY++)
            {
                for (int mcuX = 0; mcuX < mcus_x; mcuX++)
                {
                    if (hit_rst)
                    {
                        hit_rst = false;
                        ConsumeBits(bit_cnt & 0x07);
                        for (int i = 0; i < comps.Length; i++)
                        {
                            comps[i].PredDCValue = 0;
                        }
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
                                int samplesX = lowestHor / comp.SamplingHor,
                                    samplesY = lowestVer / comp.SamplingVer;
                                for (int y = 0; y < 8; y++)
                                {
                                    for (int x = 0; x < 8; x++)
                                    {
                                        float sample = output[y * 8 + x];
                                        for (int py = 0; py < samplesY; py++)
                                        {
                                            for (int px = 0; px < samplesX; px++)
                                            {
                                                int YY = (by * 8 + y) * samplesY + py,
                                                    XX = (bx * 8 + x) * samplesX + px,
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
                                float y = colors[j].Y,
                                    cr = colors[j].Cr,
                                    cb = colors[j].Cb,
                                    r = 1.40200f * (cr - 128) + y,
                                    g = -0.34414f * (cb - 128) - 0.71414f * (cr - 128) + y,
                                    b = 1.77200f * (cb - 128) + y;
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
        void DecodeBlock(JpegComponent comp, byte[] src, int[] block)
        {
            HuffmanTable table = dc_huff_tables[comp.DCHuffTable];
            int dc_code = ReadHuffman(table, src),
                dc_delta = ReadBiasedValue(src, dc_code),
                dc_value = comp.PredDCValue + dc_delta;
            comp.PredDCValue = dc_value;
            byte[] dequant = quant_tables[comp.QuantTable];
            for (int j = 0; j < block.Length; j++)
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
        float[] idct_factors;
        void ComputeIDCTFactors()
        {
            float[] factors = new float[64];
            for (int xy = 0; xy < 8; xy++)
            {
                for (int uv = 0; uv < 8; uv++)
                {
                    float cuv = uv == 0 ? 0.70710678f : 1.0f,
                        cosuv = (float)Math.Cos((2 * xy + 1) * uv * Math.PI / 16.0);
                    factors[(xy * 8) + uv] = cuv * cosuv;
                }
            }
            idct_factors = factors;
        }
        void IDCT(int[] block, float* output)
        {
            float[] factors = idct_factors;
            float* tmp = stackalloc float[64];
            for (int col = 0; col < 8; col++)
            {
                for (int y = 0; y < 8; y++)
                {
                    float sum = 0.0f;
                    for (int v = 0; v < 8; v++)
                    {
                        sum += block[v * 8 + col] * factors[(y * 8) + v];
                    }
                    tmp[y * 8 + col] = sum;
                }
            }
            for (int row = 0; row < 8; row++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float sum = 0.0f;
                    for (int u = 0; u < 8; u++)
                    {
                        sum += tmp[row * 8 + u] * factors[(x * 8) + u];
                    }
                    output[row * 8 + x] = (sum / 4.0f) + 128.0f;
                }
            }
        }
        uint bit_buf;
        int bit_cnt;
        bool hit_end, hit_rst;
        int ReadBits(byte[] src, int count)
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
            int read = bit_cnt - count,
                bits = (int)(bit_buf >> read);
            bit_buf &= (uint)((1 << read) - 1);
            bit_cnt -= count;
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
            int codeword = 0;
            for (int i = 0; i < 16; i++)
            {
                codeword <<= 1;
                codeword |= ReadBits(src, 1);
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
            int value = ReadBits(src, bits),
                midpoint = 1 << (bits - 1);
            if (value < midpoint)
            {
                value += (-1 << bits) + 1;
            }
            return value;
        }
    }
    class JpegComponent
    {
        public byte ID,
            QuantTable,
            SamplingHor,
            SamplingVer,
            BlocksPerMcuX,
            BlocksPerMcuY,
            ACHuffTable,
            DCHuffTable;
        public int PredDCValue;
    }
    class HuffmanTable
    {
        public ushort[] firstCodewords,
            endCodewords,
            firstOffsets;
        public byte[] values;
    }
}