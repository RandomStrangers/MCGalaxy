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
namespace MCGalaxy
{
    /// <summary> Extension methods relating to strings. </summary>
    public static class StringExts
    {
        /// <summary> Sets the first character of the input string to uppercase. </summary>
        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            char[] a = str.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new(a);
        }
        /// <summary> Removes an ending + from a username. </summary>
        public static string RemoveLastPlus(this string str) => string.IsNullOrEmpty(str) ? str : str[str.Length - 1] != '+' ? str : str.Substring(0, str.Length - 1);
        /// <summary> Returns whether line is empty or starts with a #. </summary>
        public static bool IsCommentLine(this string line) => line.Length == 0 || line[0] == '#';
        /// <summary> Returns whether all characters in the given string are also in the given alphabet </summary>
        public static bool ContainsAllIn(this string str, string alphabet)
        {
            foreach (char c in str)
                if (alphabet.IndexOf(c) == -1)
                    return false;
            return true;
        }
        /// <summary> Converts a string consisting of code page 437 indices into unicode. </summary>
        public static string Cp437ToUnicode(this string str)
        {
            str = str.ReplaceCursive();
            if (!HasSpecial(str))
                return str;
            char[] c = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
                c[i] = Cp437ToUnicode(str[i]);
            return new(c);
        }
        /// <summary> Converts a unicode string into a string consisting of code page 437 indices. </summary>
        /// <remarks> Unicode characters not in code page 437 are converted to '?'. </remarks>
        public static string UnicodeToCp437(this string str)
        {
            str = str.ReplaceCursive();
            if (!HasSpecial(str))
                return str;
            char[] c = str.ToCharArray();
            for (int i = 0; i < str.Length; i++)
                c[i] = UnicodeToCp437(str[i]);
            return new(c);
        }
        /// <summary> Converts a code page 437 indice into unicode. </summary>
        public static char Cp437ToUnicode(this char c)
        {
            if (c < 0x20)
                return EmotesHandler.ControlCharReplacements[c];
            else if (c < 0x7F)
                return c;
            else if (c <= 0xFF)
                return EmotesHandler.ExtendedCharReplacements[c - 0x7F];
            return '?';
        }
        public static string ReplaceCursive(this string str) => str.Replace("𝓐", "A")
                .Replace("𝓑", "B")
                .Replace("𝓒", "C")
                .Replace("𝓓", "D")
                .Replace("𝓔", "E")
                .Replace("𝓕", "F")
                .Replace("𝓖", "G")
                .Replace("𝓗", "H")
                .Replace("𝓘", "I")
                .Replace("𝓙", "J")
                .Replace("𝓚", "K")
                .Replace("𝓛", "L")
                .Replace("𝓜", "M")
                .Replace("𝓝", "N")
                .Replace("𝓞", "O")
                .Replace("𝓟", "P")
                .Replace("𝓠", "Q")
                .Replace("𝓡", "R")
                .Replace("𝓢", "S")
                .Replace("𝓣", "T")
                .Replace("𝓤", "U")
                .Replace("𝓥", "V")
                .Replace("𝓦", "W")
                .Replace("𝓧", "X")
                .Replace("𝓨", "Y")
                .Replace("𝓩", "Z")
                .Replace("𝓪", "a")
                .Replace("𝓫", "b")
                .Replace("𝓬", "c")
                .Replace("𝓭", "d")
                .Replace("𝓮", "e")
                .Replace("𝓯", "f")
                .Replace("𝓰", "g")
                .Replace("𝓱", "h")
                .Replace("𝓲", "i")
                .Replace("𝓳", "j")
                .Replace("𝓴", "k")
                .Replace("𝓵", "l")
                .Replace("𝓶", "m")
                .Replace("𝓷", "n")
                .Replace("𝓸", "o")
                .Replace("𝓹", "p")
                .Replace("𝓺", "q")
                .Replace("𝓻", "r")
                .Replace("𝓼", "s")
                .Replace("𝓽", "t")
                .Replace("𝓾", "u")
                .Replace("𝓿", "v")
                .Replace("𝔀", "w")
                .Replace("𝔁", "x")
                .Replace("𝔂", "y")
                .Replace("𝔃", "z");
        /// <summary> Converts a unicode character into a code page 437 indice. </summary>
        public static char UnicodeToCp437(this char c)
        {
            int cpIndex;
            switch (c)
            {
                case >= ' ' and <= '~':
                    return c;
                default:
                    if ((cpIndex = EmotesHandler.ControlCharReplacements.IndexOf(c)) >= 0)
                        return (char)cpIndex;
                    else if ((cpIndex = EmotesHandler.ExtendedCharReplacements.IndexOf(c)) >= 0)
                        return (char)(cpIndex + 127);
                    break;
            }
            return '?';
        }
        public static bool HasSpecial(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            for (int i = 0; i < str.Length; i++)
                if (str[i] < ' ' || str[i] > '~')
                    return true;
            return false;
        }
        public static string[] SplitExact(this string str, int maxArgs)
        {
            string[] output = new string[maxArgs],
                input = string.IsNullOrEmpty(str) ? new string[0] : str.SplitSpaces(maxArgs);
            for (int i = 0; i < output.Length; i++)
                output[i] = i < input.Length ? input[i] : "";
            return output;
        }
    }
}
