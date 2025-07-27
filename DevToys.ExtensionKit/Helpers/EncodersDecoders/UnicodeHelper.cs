using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DevToys.ExtensionKit.Models.EncodersDecoders;

namespace DevToys.ExtensionKit.Helpers.EncodersDecoders;

/// <summary>
/// Helper class for Unicode encoding and decoding operations.
/// Provides methods for converting between text and Unicode escape sequences.
/// </summary>
public static partial class UnicodeHelper
{
    /// <summary>
    /// Regular expression pattern to detect Unicode escape sequences in \uXXXX format.
    /// </summary>
    private static readonly Regex UnicodeEscapePattern = UnicodeEscapeRegex();


    // Generates a regex to detect \uXXXX format Unicode escapes.
    // Using RegexOptions.Compiled for better performance.
    [GeneratedRegex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled)]
    private static partial Regex UnicodeEscapeRegex();

    /// <summary>
    /// Converts a string to Unicode escape sequences (\uXXXX format).<br />
    /// ASCII characters are preserved as-is for better readability, except for backslash.
    /// </summary>
    /// <param name="input">String to convert</param>
    /// <returns>String with Unicode escape sequences</returns>
    public static string EncodeToUnicode(string input)
    {
        // Return empty string for null/empty input
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        // Buffer for storing surrogate pairs (2 chars)
        Span<char> charBuffer = stackalloc char[2];

        foreach (var rune in input.EnumerateRunes())
        {
            // Keep printable ASCII characters (0x20-0x7F) as-is for readability,
            // except for backslash which must be escaped to prevent misinterpretation
            if (rune.IsAscii && rune.Value != '\\')
            {
                result.Append(rune);
            }
            else
            {
                // Non-BMP characters (surrogate pairs) require 2 chars in UTF-16,
                // so output each as \uXXXX for accurate reconstruction
                if (rune.Utf16SequenceLength > 1)
                {
                    int length = rune.EncodeToUtf16(charBuffer);
                    for (int i = 0; i < length; i++)
                    {
                        result.Append($"\\u{(int)charBuffer[i]:X4}");
                    }
                }
                else
                {
                    // BMP characters can be represented with a single \uXXXX
                    result.Append($"\\u{rune.Value:X4}");
                }
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts Unicode escape sequences (\uXXXX format) back to their original string representation.
    /// Handles both single characters and surrogate pairs correctly.
    /// </summary>
    /// <param name="input">String containing Unicode escape sequences</param>
    /// <returns>Decoded string with actual characters</returns>
    public static string DecodeFromUnicode(string input)
    {
        // Return empty string for null/empty input
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        // Extract all \uXXXX patterns from the input
        var matches = UnicodeEscapePattern.Matches(input);
        int lastIndex = 0;

        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];

            // Add the normal text portion before the match as-is.
            if (match.Index > lastIndex)
            {
                result.Append(input.AsSpan(lastIndex, match.Index - lastIndex));
            }

            string hexValue = match.Groups[1].Value;
            // Convert hex to integer Unicode value
            if (int.TryParse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int unicodeValue))
            {
                // Check for surrogate pairs (high surrogate + low surrogate)
                // If found, combine two consecutive \uXXXX sequences into one character
                if (char.IsHighSurrogate((char)unicodeValue) && i + 1 < matches.Count)
                {
                    var nextMatch = matches[i + 1];
                    // Only treat as pair if low surrogate follows immediately
                    if (nextMatch.Index == match.Index + match.Length)
                    {
                        string nextHexValue = nextMatch.Groups[1].Value;
                        if (int.TryParse(nextHexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int lowSurrogate) &&
                            char.IsLowSurrogate((char)lowSurrogate))
                        {
                            // Convert surrogate pair to UTF-32 code point and add as single character
                            result.Append(char.ConvertFromUtf32(char.ConvertToUtf32((char)unicodeValue, (char)lowSurrogate)));
                            i++; // Skip next match
                            lastIndex = nextMatch.Index + nextMatch.Length;
                            continue;
                        }
                    }
                }

                // For non-surrogate pairs, safely convert to character using Rune
                if (Rune.IsValid(unicodeValue))
                {
                    var rune = new Rune(unicodeValue);
                    result.Append(rune.ToString());
                }
                else
                {
                    // Keep invalid values as escaped sequences
                    result.Append(match.Value);
                }
            }
            else
            {
                // Keep original escape sequence if hex parsing fails
                result.Append(match.Value);
            }

            lastIndex = match.Index + match.Length;
        }

        // Don't forget to append any remaining text after the last match
        if (lastIndex < input.Length)
        {
            result.Append(input.AsSpan(lastIndex));
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts all characters in a string to Unicode escape sequences (\uXXXX format),
    /// including ASCII characters.
    /// </summary>
    /// <param name="input">String to convert</param>
    /// <returns>String with all characters as Unicode escape sequences</returns>
    public static string EncodeToUnicodeAll(string input)
    {
        // Return empty string for null/empty input
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        // Buffer for storing surrogate pairs (2 chars)
        char[] charBuffer = new char[2];

        foreach (var rune in input.EnumerateRunes())
        {
            // For surrogate pairs (requiring 2 chars in UTF-16),
            // output each part as \uXXXX
            if (rune.Utf16SequenceLength > 1)
            {
                int length = rune.EncodeToUtf16(charBuffer);
                for (int i = 0; i < length; i++)
                {
                    result.Append($"\\u{(int)charBuffer[i]:X4}");
                }
            }
            else
            {
                // BMP characters can be represented with a single \uXXXX
                result.Append($"\\u{rune.Value:X4}");
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Analyzes a string to provide detailed Unicode information for each character.
    /// Includes encoding details, code points, and character properties.
    /// </summary>
    /// <param name="input">String to analyze</param>
    /// <returns>Detailed Unicode information for each character</returns>
    public static IEnumerable<UnicodeCharacterInfo> AnalyzeUnicode(string input)
    {
        // Return empty enumerable for null/empty input
        if (string.IsNullOrEmpty(input))
        {
            yield break;
        }

        // Buffer for storing surrogate pairs (2 chars)
        char[] charBuffer = new char[2];
        foreach (var rune in input.EnumerateRunes())
        {
            // Process each character using Rune, handling surrogate pairs as single units
            string displayText = rune.ToString();
            string hexValue;
            string codePointHex = rune.Value <= 0xFFFF ? $"U+{rune.Value:X4}" : $"U+{rune.Value:X}";

            // Check if the character is represented by a surrogate pair
            bool isSurrogatePair = rune.Utf16SequenceLength > 1;

            if (isSurrogatePair)
            {
                // For surrogate pairs, represent both UTF-16 chars as \uXXXX\uXXXX
                int length = rune.EncodeToUtf16(charBuffer);
                var hexParts = new string[length];
                for (int i = 0; i < length; i++)
                {
                    hexParts[i] = ((int)charBuffer[i]).ToString("X4");
                }
                hexValue = $"\\u{string.Join("\\u", hexParts)}";
            }
            else
            {
                // BMP characters can be represented with a single \uXXXX
                hexValue = $"\\u{rune.Value:X4}";
            }

            // Get byte arrays for various encodings for visualization
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(displayText);
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(displayText);
            byte[] utf16BEBytes = Encoding.BigEndianUnicode.GetBytes(displayText);

            yield return new UnicodeCharacterInfo
            {
                Character = displayText.Length > 0 ? displayText[0] : '\0',
                DisplayText = displayText,
                UnicodeValue = rune.Value,
                HexValue = hexValue,
                CodePointHex = codePointHex,
                Category = Rune.GetUnicodeCategory(rune),
                IsSurrogatePair = isSurrogatePair,
                Utf8Bytes = utf8Bytes,
                Utf16Bytes = utf16Bytes,
                Utf16BEBytes = utf16BEBytes
            };
        }
    }
}
