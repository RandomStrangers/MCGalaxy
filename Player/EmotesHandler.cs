using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCGalaxy {
    
    public static class EmotesHandler {
        
        public static readonly Dictionary<string, char> EmoteKeywords = new Dictionary<string, char> {
            { "darksmile", '\u0001' },

            { "smile", '\u0002' }, // ☻

            { "heart", '\u0003' }, // ♥
            { "hearts", '\u0003' },

            { "diamond", '\u0004' }, // ♦
            { "diamonds", '\u0004' },
            { "rhombus", '\u0004' },

            { "club", '\u0005' }, // ♣
            { "clubs", '\u0005' },
            { "clover", '\u0005' },
            { "shamrock", '\u0005' },

            { "spade", '\u0006' }, // ♠
            { "spades", '\u0006' },

            { "*", '\u0007' }, // •
            { "bullet", '\u0007' },
            { "dot", '\u0007' },
            { "point", '\u0007' },

            { "hole", '\u0008' }, // ◘

            { "circle", '\u0009' }, // ○
            { "o", '\u0009' },

            { "male", '\u000B' }, // ♂
            { "mars", '\u000B' },

            { "female", '\u000C' }, // ♀
            { "venus", '\u000C' },

            { "8", '\u000D' }, // ♪
            { "note", '\u000D' },
            { "quaver", '\u000D' },

            { "notes", '\u000E' }, // ♫
            { "music", '\u000E' },

            { "sun", '\u000F' }, // ☼
            { "celestia", '\u000F' },

            { ">>", '\u0010' }, // ►
            { "right", '\u0010' },

            { "<<", '\u0011' }, // ◄
            { "left", '\u0011' },

            { "updown", '\u0012' }, // ↕
            { "^v", '\u0012' },

            { "!!", '\u0013' }, // ‼

            { "p", '\u0014' }, // ¶
            { "para", '\u0014' },
            { "pilcrow", '\u0014' },
            { "paragraph", '\u0014' },

            { "s", '\u0015' }, // §
            { "sect", '\u0015' },
            { "section", '\u0015' },

            { "-", '\u0016' }, // ▬
            { "_", '\u0016' },
            { "bar", '\u0016' },
            { "half", '\u0016' },

            { "updown2", '\u0017' }, // ↨
            { "^v_", '\u0017' },

            { "^", '\u0018' }, // ↑
            { "uparrow", '\u0018' },

            { "v", '\u0019' }, // ↓
            { "downarrow", '\u0019' },

            { "->", '\u001A' }, // →
            { "rightarrow", '\u001A' },

            { "<-", '\u001B' }, // ←
            { "leftarrow", '\u001B' },

            { "l", '\u001C' }, // ∟
            { "angle", '\u001C' },
            { "corner", '\u001C' },

            { "<>", '\u001D' }, // ↔
            { "<->", '\u001D' },
            { "leftright", '\u001D' },

            { "^^", '\u001E' }, // ▲
            { "up", '\u001E' },

            { "vv", '\u001F' }, // ▼
            { "down", '\u001F' },

            { "house", '\u007F' } // ⌂
        };
        /// <summary> Conversion for code page 437 characters from index 0 to 31 to unicode. </summary>
        public const string ControlCharReplacements = "\0☺☻♥♦♣♠•◘○◙♂♀♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼";
        /// <summary> Conversion for code page 437 characters from index 127 to 255 to unicode. </summary>
        public const string ExtendedCharReplacements = "⌂ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜ¢£¥₧ƒáíóúñÑªº¿⌐¬½¼¡«»" +
            "░▒▓│┤╡╢╖╕╣║╗╝╜╛┐└┴┬├─┼╞╟╚╔╩╦╠═╬╧╨╤╥╙╘╒╓╫╪┘┌" +
            "█▄▌▐▀αßΓπΣσµτΦΘΩδ∞φε∩≡±≥≤⌠⌡÷≈°∙·√ⁿ²■\u00a0";
        public static string ReplaceEmoteKeywords(string message) {
            return Unescape(message, '(', ')', EmoteKeywords);
        }
        
        public static string Unescape(string message, char start, char end, 
                                      Dictionary<string, char> tokens)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            int startIndex = message.IndexOf(start);
            if (startIndex == -1)
                return message;

            StringBuilder output = new StringBuilder(message.Length);
            int lastAppendedIndex = 0;
            while (startIndex != -1) {
                int endIndex = message.IndexOf(end, startIndex + 1);
                if (endIndex == -1)
                    break;

                bool escaped = false;
                for (int i = startIndex - 1; i >= 0 && message[i] == '\\'; i--) {
                    escaped = !escaped;
                }

                string keyword = message.Substring(startIndex + 1, endIndex - startIndex - 1);
                char substitute;
                if (tokens.TryGetValue(keyword.ToLowerInvariant(), out substitute))
                {
                    if (escaped) {
                        startIndex++;
                        output.Append(message, lastAppendedIndex, startIndex - lastAppendedIndex - 2);
                        lastAppendedIndex = startIndex - 1;
                    } else {
                        output.Append(message, lastAppendedIndex, startIndex - lastAppendedIndex);
                        output.Append(substitute);
                        startIndex = endIndex + 1;
                        lastAppendedIndex = startIndex;
                    }
                } else {
                    startIndex++;
                }
                startIndex = message.IndexOf(start, startIndex);
            }
            output.Append(message, lastAppendedIndex, message.Length - lastAppendedIndex);
            return output.ToString();
        }
    }
}
