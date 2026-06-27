using System.Diagnostics.CodeAnalysis;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support
{
    [SpectreSourceFile]
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
        internal static string LetterBase(int val, bool lower)
        {
            // +1 to avoid fence-post-errors
            const int numOfDigits = 'Z' - 'A' + 1;

            var list = new List<char>();
            while (val > 0)
            {
                val--; // This helps us deal with the presence of the magic ' '
                list.Add((char)((lower ? 'a' : 'A') + (val % numOfDigits)));
                val /= numOfDigits;
            }
            var array = list.ToArray();
            Array.Reverse(array);
            return new string(array);
        }

        // This is making assumptions about Color.ToString() and Color.ToMarkup()
        internal static bool TryParseColor(string? colorString, [NotNullWhen(true)] out Color? color)
        {
            color = null;
            if (string.IsNullOrWhiteSpace(colorString))
            {
                return false;
            }

            if (colorString.Equals(nameof(Color.Default), StringComparison.OrdinalIgnoreCase))
            {
                color = Color.Default;
                return true;
            }

            if (Color.FromName(colorString) is Color namedColor)
            {
                color = namedColor;
                return true;
            }   

            if (Color.TryFromHex(colorString, out var result))
            {
                color = result;
                return true;
            }

            // This is not in Color, but it is in TextColor, so might as well
            if (TryParseRgb(colorString, out var r, out var g, out var b))
            {
                color = new Color(r, g, b);
                return true;
            }

            return false;
        }

        public static bool TryParseRgb(string? rgbString, out byte r, out byte g, out byte b)
        {
            r = 0;
            g = 0;
            b = 0;

            if (string.IsNullOrWhiteSpace(rgbString))
            {
                return false;
            }

            if (rgbString.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && rgbString.EndsWith(')'))
            {
                var inner = rgbString[4..^1];
                var parts = inner.Split(',');
                if (parts.Length == 3 &&
                    byte.TryParse(parts[0], out r) &&
                    byte.TryParse(parts[1], out g) &&
                    byte.TryParse(parts[2], out b))
                {
                    return true;
                }
            }

            return false;
        }
    }
}