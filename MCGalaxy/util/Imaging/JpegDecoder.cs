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

namespace MCGalaxy.Util.Imaging
{
    public class JpegDecoder : ImageDecoder
    {
        static readonly byte[] jfifSig = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // "SOI", "APP0"
                exifSig = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }; // "SOI", "APP1"
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
            ReadMarkers(src);
            Fail("JPEG decoder unfinished");
            return null;
        }
        const ushort MARKER_IMAGE_BEG = 0xFFD8, MARKER_IMAGE_END = 0xFFD9,
            MARKER_APP0 = 0xFFE0, MARKER_APP1 = 0xFFE1,
            MARKER_TBL_QUANT = 0xFFDB, MARKER_TBL_HUFF = 0xFFC4,
            MARKER_FRAME_BEG = 0xFFC0, MARKER_SCAN_BEG = 0xFFDA;
        void ReadMarkers(byte[] src)
        {
            for (; ; )
            {
                int offset = AdvanceOffset(2);
                ushort marker = MemUtils.ReadU16_BE(src, offset);
                switch (marker)
                {
                    case MARKER_IMAGE_BEG:
                        break;
                    case MARKER_IMAGE_END:
                        return;
                    case MARKER_APP0:
                    case MARKER_APP1:
                        SkipMarker(src);
                        break;
                    case MARKER_TBL_HUFF:
                        ReadHuffmanTable(src);
                        break;
                    case MARKER_TBL_QUANT:
                        ReadQuantisationTables(src);
                        break;
                    case MARKER_FRAME_BEG:
                        ReadFrameStart(src);
                        break;
                    case MARKER_SCAN_BEG:
                        ReadScanStart(src);
                        DecodeMCUs();
                        break;
                    default:
                        Fail("unknown marker:" + marker.ToString("X4"));
                        break;
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
        const int HUFF_MAX_BITS = 16, HUFF_MAX_VALS = 256;
        void ReadHuffmanTable(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            offset = AdvanceOffset(length - 2);
            byte flags = src[offset++];
            HuffmanTable table;
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
                else
                {
                    table.endCodewords[i] = 0;
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
            HuffmanTable[] tables = (flags >> 4) != 0 ? ac_huff_tables : dc_huff_tables;
            tables[flags & 0x03] = table;
        }
        void ReadFrameStart(byte[] src)
        {
            int offset = AdvanceOffset(2),
                length = MemUtils.ReadU16_BE(src, offset);
            offset = AdvanceOffset(length - 2);
            byte bits = src[offset + 0];
            if (bits != 8)
            {
                Fail("bits per sample");
            }
            _ = MemUtils.ReadU16_BE(src, offset + 1);
            _ = MemUtils.ReadU16_BE(src, offset + 3);
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
                return;
            }
            Fail("unknown scan component");
        }
        void DecodeMCUs()
        {
            Fail("MCUs");
        }
    }
    struct JpegComponent
    {
        public byte ID, SamplingHor, SamplingVer, 
            QuantTable, ACHuffTable, DCHuffTable;
    }
    struct HuffmanTable
    {
        public ushort[] firstCodewords, endCodewords, firstOffsets;
        public byte[] values;
    }
}