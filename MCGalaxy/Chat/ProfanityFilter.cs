/*
    Copyright 2011 MCForge.
    Author: fenderrock87
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
using MCGalaxy.Util;
using System.Collections.Generic;
namespace MCGalaxy
{
    public static class ProfanityFilter
    {
        static string[] reduceKeys, reduceValues;
        static List<string> goodWords, badWords;
        static bool hookedFilter;
        public static void Init()
        {
            InitReduceTable();
            LoadBadWords();
        }
        public static string Parse(string text)
        {
            string[] words = text.SplitSpaces(),
                reduced = Reduce(text).SplitSpaces();
            for (int i = 0; i < reduced.Length; i++)
            {
                if (IsGoodWord(words[i])) continue;
                FilterBadWord(i, words, reduced);
            }
            return string.Join(" ", words);
        }
        static bool IsGoodWord(string word)
        {
            foreach (string goodWord in goodWords)
                if (Colors.Strip(word).CaselessEq(goodWord))
                    return true;
            return false;
        }
        static void FilterBadWord(int i, string[] words, string[] reduced)
        {
            foreach (string badWord in badWords)
                if (reduced[i].Contains(badWord))
                {
                    words[i] = Censor(Colors.Strip(words[i]).Length);
                    return;
                }
        }
        static string Censor(int badWordLength) => Server.Config.ProfanityReplacement.Length == 1 ? new string(Server.Config.ProfanityReplacement[0], badWordLength) : Server.Config.ProfanityReplacement;
        static void InitReduceTable()
        {
            if (reduceKeys != null) return;
            reduceKeys = "@|i3|l3|(|3|ph|6|#|l|!|1|0|9|$|5|vv|2".Split('|');
            reduceValues = "a|b|b|c|e|f|g|h|i|i|i|o|q|s|s|w|z".Split('|');
        }
        static void LoadBadWords()
        {
            TextFile goodWordsFile = TextFile.Files["Profanity filter exceptions"],
                badWordsFile = TextFile.Files["Profanity filter"];
            goodWordsFile.EnsureExists();
            badWordsFile.EnsureExists();
            if (!hookedFilter)
            {
                hookedFilter = true;
                badWordsFile.OnTextChanged += LoadBadWords;
                goodWordsFile.OnTextChanged += LoadBadWords;
            }
            goodWords = goodWordsFile.GetTextWithoutComments();
            badWords = badWordsFile.GetTextWithoutComments();
            for (int i = 0; i < badWords.Count; i++)
                badWords[i] = Reduce(badWords[i]);
        }
        static string Reduce(string text)
        {
            text = text.ToLower();
            text = Colors.Strip(text);
            for (int i = 0; i < reduceKeys.Length; i++)
                text = text.Replace(reduceKeys[i], reduceValues[i]);
            return text;
        }
    }
}
