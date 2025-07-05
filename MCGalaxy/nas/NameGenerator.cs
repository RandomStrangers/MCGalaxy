#if NAS && TEN_BIT_BLOCKS
using System;
namespace NotAwesomeSurvival
{
    public class NameGenerator
    {
        public long[] numSyllables = new long[] { 1, 2, 3, 4, 5 };
        public long[] numSyllablesChance = new long[] { 150, 500, 80, 10, 1 };
        public long[] numConsonants = new long[] { 0, 1, 2, 3, 4 };
        public long[] numConsonantsChance = new long[] { 80, 350, 25, 5, 1 };
        public long[] numVowels = new long[] { 1, 2, 3 };
        public long[] numVowelsChance = new long[] { 180, 25, 1 };
        public char[] vowel = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        public long[] vowelChance = new long[] { 10, 12, 10, 10, 8, 2 };
        public char[] consonant = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
        public long[] consonantChance = new long[] { 10, 10, 10, 10, 10, 10, 10, 10, 12, 12, 12, 10, 5, 12, 12, 12, 8, 8, 3, 4, 3 };
        public Random random;
        /// <summary>
        /// Create an instance.
        /// </summary>
        public NameGenerator()
        {
            random = new Random();
        }
        public long IndexSelect(long[] intArray)
        {
            long totalPossible = 0;
            for (long i = 0; i < intArray.LongLength; i++)
            {
                totalPossible += intArray[i];
            }
            long chosen = random.Next((int)totalPossible);
            long chancesSoFar = 0;
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