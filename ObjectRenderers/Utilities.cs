using System;
using System.Collections.Generic;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public static class Utilities
    {
        /// <summary>
        /// Given a number this will convert it to a string using the characters (a-z) or (A-Z)
        /// It sounds like this would just be a base26 conversion, however that is not the case
        /// We basically have 27 symbols (' ' is the 27th).  But this character is not like the others, it can't be included in the number
        /// Here is a short list
        ///    0 -> ""   This could be " ", but that is never used, it does not really matter
        ///    1 -> "A"
        ///    2 -> "B"
        ///    ...
        ///    25 -> "Y"
        ///    26 -> "Z"
        ///    27 -> "AA" NOT ("A ")
        ///    28 -> "AB"
        /// </summary>
        /// <param name="val">The value to convert</param>
        /// <param name="lower">flag to control if the output should be lower or upper case</param>
        /// <returns>A string representing the number</returns>
        public static string LetterBase(int val, bool lower)
        {
            // +1 to avoid fence-post-errors
            const int numOfDigits = 'Z' - 'A' + 1;

            var list = new List<char>();
            while (val > 0)
            {
                val--; // This helps us deal that presence of the magic ' '
                list.Add((char)((lower ? 'a' : 'A') + (val % numOfDigits)));
                val /= numOfDigits;
            }
            var array = list.ToArray();
            Array.Reverse(array);
            return new string(array);
        }
    }
}