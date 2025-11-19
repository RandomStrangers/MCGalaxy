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
    public class JpegDecoder : ImageDecoder
    {
        static readonly byte[] jfifSig = new byte[] 
        { 
            0xFF, 0xD8, 0xFF, 0xE0 
        }, // "SOI", "APP0"
        exifSig = new byte[] 
        { 
            0xFF, 0xD8, 0xFF, 0xE1 
        }, // "SOI", "APP1"
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
        const ushort MARKER_IMAGE_BEG = 0xFFD8, MARKER_IMAGE_END = 0xFFD9,
            MARKER_APP0 = 0xFFE0, MARKER_APP15 = 0xFFEF,
            MARKER_TBL_QUANT = 0xFFDB, MARKER_TBL_HUFF = 0xFFC4,
            MARKER_FRAME_BEG = 0xFFC0, MARKER_SCAN_BEG = 0xFFDA, 
            MARKER_COMMENT = 0xFFFE;
        void ReadMarkers(byte[] src, SimpleBitmap bmp)
        {
            for (; ; )
            {
                int offset = AdvanceOffset(2);
                ushort marker = MemUtils.ReadU16_BE(src, offset);
                if (marker == MARKER_IMAGE_BEG)
                {
                }
                else if (marker == MARKER_IMAGE_END)
                {
                    return;
                }
                else if (marker >= MARKER_APP0 && marker <= MARKER_APP15)
                {
                    SkipMarker(src);
                }
                else if (marker == MARKER_COMMENT)
                {
                    SkipMarker(src);
                }
                else if (marker == MARKER_TBL_HUFF)
                {
                    ReadHuffmanTable(src);
                }
                else if (marker == MARKER_TBL_QUANT)
                {
                    ReadQuantisationTables(src);
                }
                else if (marker == MARKER_FRAME_BEG)
                {
                    ReadFrameStart(src, bmp);
                }
                else if (marker == MARKER_SCAN_BEG)
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
            offset = AdvanceOffset(length - 2);
            length -= 2;
            while (length != 0)
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
        const int HUFF_MAX_BITS = 16,
            HUFF_MAX_VALS = 256;
        void ReadHuffmanTable(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            offset = AdvanceOffset(length - 2);
            byte flags = src[offset++];
            HuffmanTable table = new();
            HuffmanTable[] tables = (flags >> 4) != 0 ? ac_huff_tables : dc_huff_tables;
            tables[flags & 0x03] = table;
            table.firstCodewords = new ushort[HUFF_MAX_BITS];
            table.endCodewords = new ushort[HUFF_MAX_BITS];
            table.firstOffsets = new ushort[HUFF_MAX_BITS];
            table.values = new byte[HUFF_MAX_VALS];
            int code = 0, total = 0;
            byte[] counts = new byte[HUFF_MAX_BITS];
            for (int i = 0; i < HUFF_MAX_BITS; i++)
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
            if (total > HUFF_MAX_VALS)
            {
                Fail("too many values");
            }
            total = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                for (int j = 0; j < counts[i]; j++)
                {
                    table.values[total++] = src[offset++];
                }
            }
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
                JpegComponent comp = default;
                comp.ID = src[offset++];
                byte sampling = src[offset++];
                comp.SamplingHor = (byte)(sampling >> 4);
                comp.SamplingVer = (byte)(sampling & 0x0F);
                comp.QuantTable = src[offset++];
                comps[i] = comp;
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
            byte spec_lo = src[offset++],
                spec_hi = src[offset++],
                succ_apr = src[offset++];
            if (spec_lo != 0)
            {
                Fail("spectral range start");
            }
            if (spec_hi != 0 && spec_hi != 63)
            {
                Fail("spectral range end");
            }
            if (succ_apr != 0)
            {
                Fail("successive approximation");
            }
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
            int mcus_x = (bmp.Width + 7) / 8,
                mcus_y = (bmp.Height + 7) / 8;
            JpegComponent[] comps = this.comps;
            int[] block = new int[64];
            for (int y = 0; y < mcus_y; y++)
            {
                for (int x = 0; x < mcus_x; x++)
                {
                    for (int i = 0; i < comps.Length; i++)
                    {
                        HuffmanTable table = dc_huff_tables[comps[i].DCHuffTable];
                        int dc_code = ReadHuffman(table, src),
                            dc_delta = ReadBiasedValue(src, dc_code),
                            dc_value = comps[i].PredDCValue + dc_delta;
                        comps[i].PredDCValue = dc_value;
                        byte[] dequant = quant_tables[comps[i].QuantTable];
                        for (int j = 0; j < block.Length; j++)
                        {
                            block[j] = 0;
                        }
                        block[0] = dc_value * dequant[0];
                        table = ac_huff_tables[comps[i].ACHuffTable];
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
                        float[] output = new float[64];
                        IDCT(block, output);
                        for (int YY = 0; YY < 8; YY++)
                        {
                            for (int XX = 0; XX < 8; XX++)
                            {
                                int globalX = x * 8 + XX,
                                    globalY = y * 8 + YY;
                                if (globalX < bmp.Width && globalY < bmp.Height)
                                {
                                    byte rgb = (byte)output[YY * 8 + XX];
                                    Pixel p = new(rgb, rgb, rgb, 255);
                                    bmp.pixels[globalY * bmp.Width + globalX] = p;
                                }
                            }
                        }
                    }
                }
            }
        }
        void IDCT(int[] block, float[] output)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    float sum = 0.0f;
                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {
                            float cu = u == 0 ? 0.70710678f : 1.0f,
                                cv = v == 0 ? 0.70710678f : 1.0f,
                                suv = block[v * 8 + u],
                                cosu = (float)Math.Cos((2 * x + 1) * u * Math.PI / 16.0),
                                cosv = (float)Math.Cos((2 * y + 1) * v * Math.PI / 16.0);
                            sum += cu * cv * suv * cosu * cosv;
                        }
                    }
                    output[y * 8 + x] = (sum / 4.0f) + 128.0f;
                }
            }
        }
        uint bit_buf;
        int bit_cnt;
        bool end;
        int ReadBits(byte[] src, int count)
        {
            while (bit_cnt <= 24 && !end)
            {
                byte next = src[buf_offset++];
                if (next == 0xFF)
                {
                    byte type = src[buf_offset++];
                    if (type == 0xD9)
                    {
                        end = true;
                        buf_offset -= 2;
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
        byte ReadHuffman(HuffmanTable table, byte[] src)
        {
            int codeword = 0;
            for (int i = 0; i < HUFF_MAX_BITS; i++)
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
    struct JpegComponent
    {
        public byte ID, SamplingHor, SamplingVer, 
            QuantTable, ACHuffTable, DCHuffTable;
        public int PredDCValue;
    }
    class HuffmanTable
    {
        public ushort[] firstCodewords, endCodewords, firstOffsets;
        public byte[] values;
    }
}