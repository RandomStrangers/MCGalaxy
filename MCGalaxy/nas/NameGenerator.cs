#if NAS && TEN_BIT_BLOCKS
using System;
namespace NotAwesomeSurvival
{
    public class NameGenerator
    {
        public long[] numSyllables = new long[] { 1, 2, 3, 4, 5 },
            numSyllablesChance = new long[] { 150, 500, 80, 10, 1 },
            numConsonants = new long[] { 0, 1, 2, 3, 4 },
            numConsonantsChance = new long[] { 80, 350, 25, 5, 1 },
            numVowels = new long[] { 1, 2, 3 },
            numVowelsChance = new long[] { 180, 25, 1 },
            vowelChance = new long[] { 10, 12, 10, 10, 8, 2 },
            consonantChance = new long[] { 10, 10, 10, 10, 10, 10, 10, 10, 12, 12, 12, 10, 5, 12, 12, 12, 8, 8, 3, 4, 3 };
        public char[] vowel = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' },
            consonant = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
        public Random random;
        /// <summary>
        /// Create an instance.
        /// </summary>
        public NameGenerator()
        {
            random = new();
        }
        public long IndexSelect(long[] intArray)
        {
            long totalPossible = 0;
            for (long i = 0; i < intArray.LongLength; i++)
            {
                totalPossible += intArray[i];
            }
            long chosen = random.Next((int)totalPossible),
                chancesSoFar = 0;
            for (long j = 0; j < intArray.LongLength; j++)
            {
                chancesSoFar += intArray[j];
                if (chancesSoFar > chosen)
                {
                    return j;
                }
            }
            return 0;
        }
        public string MakeSyllable()
        {
            return MakeConsonantBlock() + MakeVowelBlock() + MakeConsonantBlock();
        }
        public string MakeConsonantBlock()
        {
            string newName = "";
            long numberConsonants = numConsonants[IndexSelect(numConsonantsChance)];
            for (long i = 0; i < numberConsonants; i++)
            {
                newName += consonant[IndexSelect(consonantChance)];
            }
            return newName;
        }
        public string MakeVowelBlock()
        {
            string newName = "";
            long numberVowels = numVowels[IndexSelect(numVowelsChance)];
            for (long i = 0; i < numberVowels; i++)
            {
                newName += vowel[IndexSelect(vowelChance)];
            }
            return newName;
        }
        /// <summary>
        /// Generates a name randomly using certain construction rules. The name
        /// will be different each time it is called.
        /// </summary>
        /// <returns>A name string.</returns>
        public string MakeName()
        {
            long numberSyllables = numSyllables[IndexSelect(numSyllablesChance)];
            string newName = "";
            for (long i = 0; i < numberSyllables; i++)
            {
                newName += MakeSyllable();
            }
            return char.ToUpper(newName[0]) + newName.Substring(1);
        }
    }
}
#endif