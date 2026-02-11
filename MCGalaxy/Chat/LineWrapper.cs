/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
using System.Collections.Generic;
namespace MCGalaxy
{
    public static class LineWrapper
    {
        static bool EndsInEmote(char[] line, int length, int lineLength)
        {
            length = Math.Min(length, lineLength);
            for (; length > 0 & line[length - 1] == ' '; length--) 
            { 
            }
            if (length == 0) return false;
            char last = line[length - 1];
            return last.UnicodeToCp437() != last;
        }
        static char LastColor(char[] line, int length)
        {
            for (int i = length - 2; i >= 0; i--)
            {
                if (line[i] != '&') continue;
                char col = Colors.Lookup(line[i + 1]);
                if (col != '\0') return col;
            }
            return 'f';
        }
        static string MakeLine(char[] line, int length, bool emotePad)
        {
            length = TrimTrailingInvisible(line, length);
            if (emotePad) line[length++] = '\'';
            return new(line, 0, length);
        }
        static bool IsWrapper(char[] line, int i)
        {
            char c = line[i];
            if (c == ' ') return true;
            return (c == '-' || c == '\\') && line[i - 1] != ' ';
        }
        static bool StartsWithColor(char[] message, int messageLen, int offset) => message[offset] == '&'
                && (offset + 1) < messageLen
                && Colors.Lookup(message[offset + 1]) != '\0';
        public static List<string> Wordwrap(char[] message, int messageLen, bool supportsEmotes)
        {
            List<string> lines = new();
            char[] line = new char[65];
            bool firstLine = true;
            char lastColor = 'f';
            for (int offset = 0; offset < messageLen;)
            {
                int length = 0;
                if (!firstLine)
                {
                    line[0] = '>'; line[1] = ' ';
                    length += 2;
                    if (lastColor != 'f' && !StartsWithColor(message, messageLen, offset))
                    {
                        line[2] = '&'; line[3] = lastColor;
                        length += 2;
                    }
                }
                else if (!supportsEmotes)
                {
                    char first = message[0];
                    if (first <= ' ' || first > '~')
                    {
                        line[0] = '&'; line[1] = 'f';
                        length += 2;
                    }
                }
                bool foundStart = firstLine;
                for (; length < 65 && offset < messageLen;)
                {
                    char c = message[offset++];
                    if (c != ' ' || foundStart)
                    {
                        line[length++] = c;
                        foundStart = true;
                    }
                }
                int lineLength = 64;
                bool emotePad = false;
                if (!supportsEmotes && EndsInEmote(line, length, lineLength))
                {
                    lineLength--;
                    emotePad = EndsInEmote(line, length, lineLength);
                }
                if (length <= lineLength)
                {
                    lines.Add(MakeLine(line, length, emotePad));
                    break;
                }
                firstLine = false;
                for (int i = lineLength - 1; i > 64 - 20; i--)
                {
                    if (!IsWrapper(line, i)) continue;
                    i++;
                    offset -= length - i;
                    length = i;
                    break;
                }
                if (length > lineLength)
                {
                    offset -= length - lineLength;
                    length = lineLength;
                }
                if (line[length - 1] == '&')
                { 
                    length--; 
                    offset--;
                }
                lastColor = LastColor(line, length);
                lines.Add(MakeLine(line, length, emotePad));
            }
            return lines;
        }
        /// <summary> Removes redundant colour codes and fixes some colour codes to behave correctly for older clients </summary>
        /// <param name="fullAmpersands"> if false, ampersands not followed by valid colour code are converted to percents </param>
        /// <param name="customColors"> if false, converts custom colour codes into fallback colour codes </param>
        public static string CleanupColors(string value, bool fullAmpersands, bool customColors)
        {
            if (value.IndexOf('&') == -1) return value;
            char[] chars = CleanupColors(value, out int len, fullAmpersands, customColors);
            return new(chars, 0, len);
        }
        public static char[] CleanupColors(string value, out int bufferLen,
                                           bool fullAmpersands, bool customColors)
        {
            char[] chars = new char[value.Length];
            int lastIdx = -1, len = 0;
            char lastColor = 'f', col;
            bool combinable = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c != '&')
                {
                    if (c != ' ') combinable = false;
                    chars[len++] = c;
                    continue;
                }
                if (i == value.Length - 1 || (col = Colors.Lookup(value[i + 1])) == '\0')
                {
                    combinable = false;
                    chars[len++] = fullAmpersands ? '&' : '%';
                    continue;
                }
                if (!customColors) col = Colors.Get(col).Fallback;
                if (lastColor != col)
                {
                    if (combinable)
                    {
                        chars[lastIdx + 1] = col;
                    }
                    else
                    {
                        lastIdx = len;
                        chars[len++] = '&';
                        chars[len++] = col;
                    }
                    lastColor = col;
                    combinable = true;
                }
                i++;
            }
            bufferLen = TrimTrailingInvisible(chars, len);
            return chars;
        }
        static int TrimTrailingInvisible(char[] chars, int len)
        {
            while (len >= 2)
            {
                char c = chars[len - 1];
                if (c == ' ') 
                { 
                    len--; 
                    continue;
                }
                if (chars[len - 2] != '&') break;
                if (Colors.Lookup(c) == '\0') break;
                len -= 2;
            }
            return len;
        }
    }
}
