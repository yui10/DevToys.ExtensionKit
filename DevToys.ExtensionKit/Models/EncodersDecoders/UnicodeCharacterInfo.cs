using System.Globalization;

namespace DevToys.ExtensionKit.Models.EncodersDecoders;

public sealed class UnicodeCharacterInfo
{
    /// <summary>
    /// The character (high surrogate in case of surrogate pairs).
    /// Stores a character that can be represented in a single char.
    /// </summary>
    public required char Character { get; init; }

    /// <summary>
    /// Display text (complete character for surrogate pairs).
    /// The actual string to be displayed on screen.
    /// </summary>
    public required string DisplayText { get; init; }

    /// <summary>
    /// Unicode value (UTF-32 code point).
    /// Unique code point for the character.
    /// </summary>
    public required int UnicodeValue { get; init; }

    /// <summary>
    /// Hexadecimal representation (in \uXXXX format).
    /// Used for escape sequence visualization.
    /// </summary>
    public required string HexValue { get; init; }

    /// <summary>
    /// Code point's hexadecimal representation (in U+XXXX format).
    /// Standard Unicode notation.
    /// </summary>
    public required string CodePointHex { get; init; }

    /// <summary>
    /// Unicode category.
    /// Classification of character type (symbol/kanji/emoji etc.).
    /// </summary>
    public required UnicodeCategory Category { get; init; }

    /// <summary>
    /// Whether the character is a surrogate pair.
    /// Determines if it's a non-BMP character represented by 2 chars.
    /// </summary>
    public required bool IsSurrogatePair { get; init; }

    /// <summary>
    /// UTF-8 byte array.
    /// For byte sequence visualization and encoding verification.
    /// </summary>
    public required byte[] Utf8Bytes { get; init; }

    /// <summary>
    /// UTF-16 byte array.
    /// For byte sequence visualization and encoding verification.
    /// </summary>
    public required byte[] Utf16Bytes { get; init; }

    /// <summary>
    /// UTF-16 byte array in big-endian format.
    /// Byte sequence in BigEndianUnicode encoding.
    /// </summary>
    public required byte[] Utf16BEBytes { get; init; }
}
