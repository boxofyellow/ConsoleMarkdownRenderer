using System.Collections.Generic;
using System.Linq;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="Utility"/>
    /// </summary>
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void Utilities_LetterBaseTest()
        {
            // Some specific values to check
            Dictionary<int, string> knownValue = new() {
                {0, string.Empty},
                {                 1,    "A"},
                {                 2,    "B"},
                {                25,    "Y"},
                {                26,    "Z"},
                {           26 +  1,   "AA"},
                {           26 +  2,   "AB"},
                {           26 + 25,   "AY"},
                {           26 + 26,   "AZ"},
                {      26 + 26 +  1,   "BA"},
                {      26 + 26 +  2,   "BB"},
                {      26 + 26 + 25,   "BY"},
                {      26 + 26 + 26,   "BZ"},
                { 26 + 26 + 26 +  1,   "CA"},
                {(26 * 26)     +  1,   "ZA"},
                {(26 * 26)     + 26,   "ZZ"},
                {(26 * 27)     +  1,  "AAA"}, 
                {(26 * 27)     + 26,  "AAZ"},
                {(26 * 28)     +  1,  "ABA"},
                {(26 * 28)     + 26,  "ABZ"},
                {(26 * 29)     +  1,  "ACA"},
                {(26 * 52)     + 26,  "AZZ"},
                {(26 * 53)     +  1,  "BAA"}, // 27[AAA] + 26 = 53
                {(26 * 79)     +  1,  "CAA"}, //         + 26 = 79
                {(26 * 651)    +  1,  "YAA"}, //
                {(26 * 677)    +  1,  "ZAA"},
                {(26 * 677)+26 +  1,  "ZBA"},
                {(26 * 702)    +  1,  "ZZA"},
                {(26 * 702)    + 26,  "ZZZ"},
                {(26 * 703)    +  1, "AAAA"},
            };

            var max = knownValue.Keys.Max();

            // we will fill this array of chars from right to left (leaving padding on left)
            var chars = new string(' ', 5).ToCharArray();

            int length = 0;
            int startIndex = chars.Length - 1 - length;

            for (int count = 0; count <= max; count++)
            {
                var raw = $"[{new string(chars)}]{count}";

                //
                // Get the current value
                var current = new string(chars, startIndex, length);

                // spot check to make sure our test is valid 😀
                var known = knownValue.GetValueOrDefault(count);
                if (known != default)
                {
                    Assert.AreEqual(known, current, $"{raw} current did not match known value, {known} != {current}");
                }

                //
                // get the computed value
                var computed = Utilities.LetterBase(count, lower: false);

                //
                // assert that they match
                Assert.AreEqual(current, computed, $"{raw} '{current}' != '{computed}'");

                var lower = Utilities.LetterBase(count, lower: true);
                Assert.AreEqual(lower, computed.ToLower(), $"{raw} lower did not match {lower}");

                //
                // update the current value for the next iteration
                if (length == 0)
                {
                    // special case when we get started and the length of the current is zero
                    length = 1;
                }

                // starting from the right side
                int pos = chars.Length - 1;
                while (true)
                {
                    // check if we are now using a "new" index of the array, we will need to adjust how much of chars will be consider part of the answer
                    if (pos < startIndex)
                    {
                        length++;
                        startIndex--;
                    }

                    // if the value at this index is "empty", then set it to 'A', we already rolled all the other ones to the right over
                    if (chars[pos] == ' ')
                    {
                        chars[pos] = 'A';
                        break;
                    }

                    // get the next value that should be here
                    var next = (char)(chars[pos] + 1);

                    if (next > 'Z')
                    {
                        // if we rolled passed 'Z', then set this, and all the items to right to 'A'
                        for (int i = pos; i < chars.Length; i++)
                        {
                            chars[i] = 'A';
                        }

                        // get ready to flip th next one to the left
                        pos--;
                    }
                    else
                    {
                        // no carry, then just set it and we are done.
                        chars[pos] = next;
                        break;
                    }
                }
            }
        }
    }
}
