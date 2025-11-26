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
    public class GifDecoder : ImageDecoder
    {
        static readonly byte[] gif87Sig = new byte[]
        {
            0x47, 0x49, 0x46, 0x38, 0x37, 0x61
        },
        gif89Sig = new byte[]
        {
            0x47, 0x49, 0x46, 0x38, 0x39, 0x61
        };
        Pixel[] globalPal;
        public static bool DetectHeader(byte[] data)
        {
            return MatchesSignature(data, gif87Sig)
                || MatchesSignature(data, gif89Sig);
        }
        public override SimpleBitmap Decode(byte[] src)
        {
            SetBuffer(src);
            if (!DetectHeader(src))
            {
                Fail("sig invalid");
            }
            AdvanceOffset(gif87Sig.Length);
            SimpleBitmap bmp = new();
            ReadGlobalHeader(src, bmp);
            ReadMarkers(src, bmp);
            return bmp;
        }
        byte curSubBlockLeft;
        bool subBlocksEnd;
        int subBlocksOffset;
        void ReadGlobalHeader(byte[] src, SimpleBitmap bmp)
        {
            int offset = AdvanceOffset(7);
            bmp.Width = MemUtils.ReadU16_LE(src, offset + 0);
            bmp.Height = MemUtils.ReadU16_LE(src, offset + 2);
            byte flags = src[offset + 4],
                bgIndex = src[offset + 5];
            bool hasGlobalPal = (flags & 0x80) != 0;
            if (hasGlobalPal)
            {
                globalPal = ReadPalette(src, flags);
            }
            if (hasGlobalPal && bgIndex < globalPal.Length)
            {
                _ = globalPal[bgIndex];
            }
            bmp.AllocatePixels();
        }
        Pixel[] ReadPalette(byte[] src, byte flags)
        {
            int size = 1 << ((flags & 0x7) + 1);
            Pixel[] pal = new Pixel[size];
            int offset = AdvanceOffset(3 * size);
            for (int i = 0; i < pal.Length; i++)
            {
                pal[i].R = src[offset++];
                pal[i].G = src[offset++];
                pal[i].B = src[offset++];
                pal[i].A = 255;
            }
            return pal;
        }
        void ReadMarkers(byte[] src, SimpleBitmap bmp)
        {
            for (; ; )
            {
                int offset = AdvanceOffset(1);
                byte marker = src[offset];
                switch (marker)
                {
                    case 0x21:
                        ReadExtension(src);
                        break;
                    case 0x2C:
                        ReadImage(src, bmp);
                        return;
                    case 0x3B:
                        return;
                    default:
                        Fail("unknown marker:" + marker.ToString("X2"));
                        break;
                }
            }
        }
        void ReadExtension(byte[] src)
        {
            int offset = AdvanceOffset(1);
            byte type = src[offset++];
            if (type == 0xF9)
            {
                ReadGraphicsControl(src);
            }
            else
            {
                SkipSubBlocks(src);
            }
        }
        void ReadGraphicsControl(byte[] src)
        {
            int offset = AdvanceOffset(1);
            byte length = src[offset];
            if (length < 4)
            {
                Fail("graphics control extension too short");
            }
            offset = AdvanceOffset(length);
            bool hasTrans = (src[offset + 0] & 0x01) != 0;
            byte tcIndex = src[offset + 3];
            Pixel[] pal = globalPal;
            if (hasTrans && pal != null && tcIndex < pal.Length)
            { 
                pal[tcIndex].A = 0;
            }
            offset = AdvanceOffset(1);
            length = src[offset];
            if (length != 0)
            {
                Fail("graphics control should be one sub block");
            }
        }
        void SkipSubBlocks(byte[] src)
        {
            for (; ; )
            {
                int offset = AdvanceOffset(1);
                byte length = src[offset++];
                if (length == 0)
                {
                    return;
                }
                AdvanceOffset(length);
            }
        }
        void ReadImage(byte[] src, SimpleBitmap bmp)
        {
            int offset = AdvanceOffset(2 + 2 + 2 + 2 + 1);
            ushort imageX = MemUtils.ReadU16_LE(src, offset + 0),
                imageY = MemUtils.ReadU16_LE(src, offset + 2),
                imageW = MemUtils.ReadU16_LE(src, offset + 4),
                imageH = MemUtils.ReadU16_LE(src, offset + 6);
            byte flags = src[offset + 8];
            if ((flags & 0x40) != 0)
            {
                Fail("Interlaced GIF unsupported");
            }
            if (imageX + imageW > bmp.Width)
            {
                Fail("Invalid X dimensions");
            }
            if (imageY + imageH > bmp.Height)
            {
                Fail("Invalid Y dimensions");
            }
            bool hasLocalPal = (flags & 0x80) != 0;
            Pixel[] localPal = null;
            if (hasLocalPal)
            {
                localPal = ReadPalette(src, flags);
            }
            Pixel[] pal = localPal ?? globalPal;
            int dst_index = 0;
            bool fastPath = imageX == 0 && imageY == 0 && imageW == bmp.Width && imageH == bmp.Height;
            offset = AdvanceOffset(1);
            byte minCodeSize = src[offset];
            if (minCodeSize >= 12)
            {
                Fail("codesize too long");
            }
            curSubBlockLeft = 0;
            subBlocksEnd = false;
            int codeLen = minCodeSize + 1,
                codeMask = (1 << codeLen) - 1,
                clearCode = (1 << minCodeSize) + 0,
                stopCode = (1 << minCodeSize) + 1,
                prevCode, availCode;
            DictEntry[] dict = new DictEntry[1 << codeLen];
            uint bufVal = 0;
            int bufLen = 0;
            for (availCode = 0; availCode < (1 << minCodeSize); availCode++)
            {
                dict[availCode].first = (byte)availCode;
                dict[availCode].value = (byte)availCode;
                dict[availCode].prev = -1;
                dict[availCode].len = 1;
            }
            availCode++; 
            prevCode = -1;
            for (; ; )
            {
                if (bufLen < codeLen)
                {
                    int read;
                    while (bufLen <= 24 && (read = ReadNextByte()) >= 0)
                    {
                        bufVal |= (uint)read << bufLen;
                        bufLen += 8;
                    }
                    if (bufLen < codeLen)
                    {
                        Fail("not enough bits for code");
                    }
                }
                int code = (int)(bufVal & codeMask);
                bufVal >>= codeLen;
                bufLen -= codeLen;
                if (code == clearCode)
                {
                    codeLen = minCodeSize + 1;
                    codeMask = (1 << codeLen) - 1;
                    for (availCode = 0; availCode < (1 << minCodeSize); availCode++)
                    {
                        dict[availCode].first = (byte)availCode;
                        dict[availCode].value = (byte)availCode;
                        dict[availCode].prev = -1;
                        dict[availCode].len = 1;
                    }
                    availCode++;
                    prevCode = -1;
                }
                else if (code == stopCode)
                {
                    break;
                }
                if (code > availCode)
                {
                    Fail("invalid code");
                }
                if (prevCode >= 0 && availCode < 1 << 12)
                {
                    int chainCode = code == availCode ? prevCode : code;
                    dict[availCode].first = dict[prevCode].first;
                    dict[availCode].value = dict[chainCode].first;
                    dict[availCode].prev = (short)prevCode;
                    dict[availCode].len = (short)(dict[prevCode].len + 1);
                    availCode++;
                    if ((availCode & codeMask) == 0 && availCode != 1 << 12)
                    {
                        codeLen++;
                        codeMask = (1 << codeLen) - 1;
                        Array.Resize(ref dict, 1 << codeLen);
                    }
                }
                prevCode = code;
                int chain_len = dict[code].len;
                if (fastPath)
                {
                    for (int i = chain_len - 1; i >= 0; i--)
                    {
                        byte palIndex = dict[code].value;
                        bmp.pixels[dst_index + i] = pal[palIndex];
                        code = dict[code].prev;
                    }
                }
                else
                {
                    for (int i = chain_len - 1; i >= 0; i--)
                    {
                        int index = dst_index + i;
                        byte palIndex = dict[code].value;
                        int globalX = imageX + (index % imageW),
                            globalY = imageY + (index / imageW);
                        bmp.pixels[globalY * bmp.Width + globalX] = pal[palIndex];
                        code = dict[code].prev;
                    }
                }
                dst_index += chain_len;
            }
        }
        struct DictEntry
        {
            public byte first, value;
            public short prev, len;
        }
        int ReadNextByte()
        {
            if (curSubBlockLeft == 0)
            {
                if (subBlocksEnd)
                {
                    return -1;
                }
                subBlocksOffset = AdvanceOffset(1);
                curSubBlockLeft = buf_data[subBlocksOffset++];
                if (curSubBlockLeft == 0)
                {
                    subBlocksEnd = true;
                    return -1;
                }
                AdvanceOffset(curSubBlockLeft);
            }
            curSubBlockLeft--;
            return buf_data[subBlocksOffset++];
        }
    }
}