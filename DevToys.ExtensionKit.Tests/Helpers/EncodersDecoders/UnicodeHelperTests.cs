using DevToys.ExtensionKit.Helpers.EncodersDecoders;
using System.Globalization;

namespace DevToys.ExtensionKit.Tests.Helpers.EncodersDecoders;

public class UnicodeHelperTests
{
    #region Test Data

    public static IEnumerable<object[]> EncodeToUnicodeTestData =>
    [
        ["", ""],
        ["Hello", "Hello"],
        ["こんにちは", "\\u3053\\u3093\\u306B\\u3061\\u306F"],
        ["Hello 世界!", "Hello \\u4E16\\u754C!"],
        ["𝓤𝓷𝓲𝓬𝓸𝓭𝓮", "\\uD835\\uDCE4\\uD835\\uDCF7\\uD835\\uDCF2\\uD835\\uDCEC\\uD835\\uDCF8\\uD835\\uDCED\\uD835\\uDCEE"]
    ];

    public static IEnumerable<object[]> EncodeToUnicodeAllTestData =>
    [
        ["", ""],
        ["Hello", "\\u0048\\u0065\\u006C\\u006C\\u006F"],
        ["こんにちは", "\\u3053\\u3093\\u306B\\u3061\\u306F"],
        ["Hello 世界!", "\\u0048\\u0065\\u006C\\u006C\\u006F\\u0020\\u4E16\\u754C\\u0021"],
        ["😀", "\\uD83D\\uDE00"]
    ];

    public static IEnumerable<object[]> DecodeFromUnicodeTestData =>
    [
        ["", ""],
        ["Hello", "Hello"],
        ["\\u3053\\u3093\\u306B\\u3061\\u306F", "こんにちは"],
        ["Hello \\u4E16\\u754C!", "Hello 世界!"],
        ["\\u0048\\u0065\\u006C\\u006C\\u006F", "Hello"]
    ];

    public static IEnumerable<object[]> RoundTripTestData =>
    [
        ["Hello"],
        ["こんにちは"],
        ["你好世界"],
        ["안녕하세요"],
        ["مرحبا"],
        ["Привет"],
        ["Hello 世界!"],
        ["😀😃😄"]
    ];

    public static IEnumerable<object[]> AnalyzeUnicodeTestData =>
    [
        // Input, Character, Unicode Value, Category, Surrogate Pair
        ["A", 'A', 65, UnicodeCategory.UppercaseLetter, false],
        ["1", '1', 49, UnicodeCategory.DecimalDigitNumber, false],
        ["あ", 'あ', 12354, UnicodeCategory.OtherLetter, false],
        ["\t", '\t', 9, UnicodeCategory.Control, false],
        ["!", '!', 33, UnicodeCategory.OtherPunctuation, false],
        ["$", '$', 36, UnicodeCategory.CurrencySymbol, false]
    ];

    public static IEnumerable<object[]> AnalyzeUnicodeEmojiTestData =>
    [
        // Input, Unicode Value, Surrogate Pair
        ["😀", 0x1F600, true],
        ["🎉", 0x1F389, true],
        ["👍", 0x1F44D, true]
    ];

    #endregion

    [Theory]
    [MemberData(nameof(EncodeToUnicodeTestData))]
    public void EncodeToUnicode_VariousText_ReturnsCorrectEncoding(string input, string expected)
    {
        // Act
        string result = UnicodeHelper.EncodeToUnicode(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(EncodeToUnicodeAllTestData))]
    public void EncodeToUnicodeAll_VariousText_ReturnsAllCharactersEncoded(string input, string expected)
    {
        // Act
        string result = UnicodeHelper.EncodeToUnicodeAll(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(DecodeFromUnicodeTestData))]
    public void DecodeFromUnicode_VariousInput_ReturnsCorrectString(string input, string expected)
    {
        // Act
        string result = UnicodeHelper.DecodeFromUnicode(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void EncodeToUnicodeAll_DecodeFromUnicode_RoundTrip_ReturnsOriginalString(string input)
    {
        // Act
        string encoded = UnicodeHelper.EncodeToUnicodeAll(input);
        string decoded = UnicodeHelper.DecodeFromUnicode(encoded);

        // Assert
        Assert.Equal(input, decoded);
    }

    [Theory]
    [MemberData(nameof(AnalyzeUnicodeTestData))]
    public void AnalyzeUnicode_SingleCharacter_ReturnsCorrectAnalysis(string input, char expectedChar, int expectedUnicodeValue, UnicodeCategory expectedCategory, bool expectedIsSurrogatePair)
    {
        // Act
        var results = UnicodeHelper.AnalyzeUnicode(input).ToList();
        
        // Assert
        Assert.Single(results);
        
        var result = results[0];
        Assert.Equal(expectedChar, result.Character);
        Assert.Equal(expectedUnicodeValue, result.UnicodeValue);
        Assert.Equal(expectedCategory, result.Category);
        Assert.Equal(expectedIsSurrogatePair, result.IsSurrogatePair);
        
        // Basic format check
        Assert.StartsWith("\\u", result.HexValue);
        Assert.StartsWith("U+", result.CodePointHex);
        Assert.NotNull(result.Utf8Bytes);
        Assert.NotNull(result.Utf16Bytes);
    }

    [Theory]
    [MemberData(nameof(AnalyzeUnicodeEmojiTestData))]
    public void AnalyzeUnicode_EmojiCharacter_ReturnsCorrectAnalysis(string input, int expectedUnicodeValue, bool expectedIsSurrogatePair)
    {
        // Act
        var results = UnicodeHelper.AnalyzeUnicode(input).ToList();
        
        // Assert
        Assert.Single(results);
        
        var result = results[0];
        Assert.Equal(expectedUnicodeValue, result.UnicodeValue);
        Assert.Equal(UnicodeCategory.OtherSymbol, result.Category);
        Assert.Equal(expectedIsSurrogatePair, result.IsSurrogatePair);
        Assert.Equal($"U+{expectedUnicodeValue:X}", result.CodePointHex);
    }

    [Theory]
    [InlineData("A1", 2)] // Multiple characters test
    [InlineData("\t\n\r", 3)] // Control characters test
    [InlineData("!@#$%", 5)] // Symbols and punctuation test
    public void AnalyzeUnicode_MultipleCharacters_ReturnsCorrectCount(string input, int expectedCount)
    {
        // Act
        var results = UnicodeHelper.AnalyzeUnicode(input).ToList();
        
        // Assert
        Assert.Equal(expectedCount, results.Count);

        // Verify that all results have the necessary properties set
        foreach (var result in results)
        {
            Assert.True(result.UnicodeValue >= 0);
            Assert.NotNull(result.HexValue);
            Assert.NotNull(result.CodePointHex);
            Assert.NotNull(result.Utf8Bytes);
            Assert.NotNull(result.Utf16Bytes);
        }
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Hello", "Hello")]
    [InlineData("Hello\\u0020World", "Hello World")]
    [InlineData("\\u3042\\u3044\\u3046\\u3048\\u304A", "あいうえお")]
    [InlineData("\\u0048\\u0065\\u006C\\u006C\\u006F", "Hello")]
    public void DecodeFromUnicode_MixedContent_ReturnsCorrectString(string input, string expected)
    {
        // Act
        string result = UnicodeHelper.DecodeFromUnicode(input);
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    public void AnalyzeUnicode_EmptyInput_ReturnsEmptyEnumerable(string input)
    {
        // Act
        var results = UnicodeHelper.AnalyzeUnicode(input);
        
        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void AnalyzeUnicode_NullInput_ReturnsEmptyEnumerable()
    {
        // Act
        var results = UnicodeHelper.AnalyzeUnicode(null!);
        
        // Assert
        Assert.Empty(results);
    }
}
