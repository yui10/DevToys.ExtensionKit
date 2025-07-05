using DevToys.ExtensionKit.Helpers.Generators;
using DevToys.ExtensionKit.Models.Generators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.ExtensionKit.Tests.Helpers.Generators;

public class WifiQrCodeHelperTests
{
    [Theory]
    [InlineData("TestSSID", "password123", WifiSecurityType.WPA, false, "WIFI:T:WPA;S:TestSSID;P:password123;;")]
    [InlineData("OpenNetwork", "", WifiSecurityType.None, false, "WIFI:S:OpenNetwork;;")]
    [InlineData("HiddenNetwork", "secretpass", WifiSecurityType.WPA, true, "WIFI:T:WPA;S:HiddenNetwork;H:true;P:secretpass;;")]
    [InlineData("WEPNetwork", "12345", WifiSecurityType.WEP, false, "WIFI:T:WEP;S:WEPNetwork;P:12345;;")]
    public void GenerateWifiString_WithValidParameters_ReturnsCorrectFormat(
        string ssid, 
        string password, 
        WifiSecurityType securityType, 
        bool isHidden, 
        string expectedFormat)
    {
        // Act
        string result = WifiQrCodeHelper.GenerateWifiString(ssid, password, securityType, isHidden);

        // Assert
        Assert.Equal(expectedFormat, result);
    }

    [Theory]
    [InlineData("SSID;Test", "password123", WifiSecurityType.WPA, false, "WIFI:T:WPA;S:SSID%3BTest;P:password123;;")]
    [InlineData("Test", "pass;word123", WifiSecurityType.WPA, false, "WIFI:T:WPA;S:Test;P:pass%3Bword123;;")]
    [InlineData("Test", "パスワード123", WifiSecurityType.WPA, false, "WIFI:T:WPA;S:Test;P:%E3%83%91%E3%82%B9%E3%83%AF%E3%83%BC%E3%83%89123;;")]
    public void GenerateWifiString_WithSpecialCharacters_EscapesCorrectly(
        string ssid, 
        string password, 
        WifiSecurityType securityType, 
        bool isHidden, 
        string expectedFormat)
    {
        // Act
        string result = WifiQrCodeHelper.GenerateWifiString(ssid, password, securityType, isHidden);

        // Assert
        Assert.Equal(expectedFormat, result);
    }

    [Theory]
    [InlineData("TestSSID", "password123", WifiSecurityType.WPA)]
    [InlineData("OpenNetwork", "", WifiSecurityType.None)]
    [InlineData("WEPNetwork", "12345", WifiSecurityType.WEP)]
    public void ValidateWifiConfiguration_WithValidInputs_ReturnsValid(
        string ssid, 
        string password, 
        WifiSecurityType securityType)
    {
        // Act
        var (isValid, errorMessage) = WifiQrCodeHelper.ValidateWifiConfiguration(ssid, password, securityType);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errorMessage);
    }

    [Theory]
    [InlineData("", "password", WifiSecurityType.WPA, "SSID is required")]
    [InlineData("  ", "password", WifiSecurityType.WPA, "SSID is required")]
    [InlineData("ThisSSIDIsWayTooLongForWiFiStandardsAndWillFail", "password", WifiSecurityType.WPA, "SSID must be 32 characters or less")]
    [InlineData("TestSSID", "", WifiSecurityType.WPA, "Password is required for secured networks")]
    [InlineData("TestSSID", "", WifiSecurityType.WEP, "Password is required for secured networks")]
    public void ValidateWifiConfiguration_WithInvalidInputs_ReturnsInvalid(
        string ssid, 
        string password, 
        WifiSecurityType securityType, 
        string expectedErrorMessage)
    {
        // Act
        var (isValid, errorMessage) = WifiQrCodeHelper.ValidateWifiConfiguration(ssid, password, securityType);

        // Assert
        Assert.False(isValid);
        Assert.Equal(expectedErrorMessage, errorMessage);
    }

    [Theory]
    [InlineData("12345", true)]        // 5 chars ASCII
    [InlineData("1234567890123", true)] // 13 chars ASCII
    [InlineData("1234567890", true)]   // 10 chars hex
    [InlineData("12345678901234567890123456", true)] // 26 chars hex
    [InlineData("ABCDEF1234", true)]   // 10 chars hex uppercase
    [InlineData("abcdef1234", true)]   // 10 chars hex lowercase
    [InlineData("1234", false)]       // 4 chars - too short
    [InlineData("123456", false)]     // 6 chars - invalid length
    [InlineData("1234567890123456789012345678", false)] // 28 chars - too long
    [InlineData("test@key", false)]   // invalid character @
    [InlineData("12345G", false)]     // invalid hex character G
    public void ValidateWifiConfiguration_WithWEPPasswords_ValidatesCorrectly(
        string password, 
        bool expectedValid)
    {
        // Act
        var (isValid, errorMessage) = WifiQrCodeHelper.ValidateWifiConfiguration("TestSSID", password, WifiSecurityType.WEP);

        // Assert
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.NotEmpty(errorMessage);
        }
    }

    [Theory]
    [InlineData("12345678", true)]   // 8 chars - minimum
    [InlineData("123456789012345678901234567890123456789012345678901234567890123", true)]  // 63 chars - maximum
    [InlineData("password123", true)] // valid password
    [InlineData("1234567", false)]   // 7 chars - too short
    [InlineData("1234567890123456789012345678901234567890123456789012345678901234", false)]  // 64 chars - too long
    public void ValidateWifiConfiguration_WithWPAPasswords_ValidatesCorrectly(
        string password, 
        bool expectedValid)
    {
        // Act
        var (isValid, errorMessage) = WifiQrCodeHelper.ValidateWifiConfiguration("TestSSID", password, WifiSecurityType.WPA);

        // Assert
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.NotEmpty(errorMessage);
        }
    }

    [Fact]
    public void GenerateWifiQrCode_WithValidParameters_ReturnsImage()
    {
        // Arrange
        string ssid = "TestSSID";
        string password = "password123";
        WifiSecurityType securityType = WifiSecurityType.WPA;
        int size = 256;

        // Act
        using Image<Rgba32> image = WifiQrCodeHelper.GenerateWifiQrCode(ssid, password, securityType, size: size);

        // Assert
        Assert.NotNull(image);
        Assert.Equal(size, image.Width);
        Assert.Equal(size, image.Height);
    }

    [Fact]
    public void GenerateWifiQrCodeSvg_WithValidParameters_ReturnsSvgContent()
    {
        // Arrange
        string ssid = "TestSSID";
        string password = "password123";
        WifiSecurityType securityType = WifiSecurityType.WPA;

        // Act
        string svgContent = WifiQrCodeHelper.GenerateWifiQrCodeSvg(ssid, password, securityType);

        // Assert
        Assert.NotNull(svgContent);
        Assert.NotEmpty(svgContent);
        Assert.Contains("<svg", svgContent);
        Assert.Contains("</svg>", svgContent);
    }

    [Fact]
    public void GenerateWifiQrCode_WithInvalidSSID_ThrowsArgumentException()
    {
        // Arrange
        string ssid = ""; // Invalid empty SSID
        string password = "password123";
        WifiSecurityType securityType = WifiSecurityType.WPA;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            WifiQrCodeHelper.GenerateWifiQrCode(ssid, password, securityType));
        Assert.Equal("ssid", exception.ParamName);
        Assert.Contains("SSID is required", exception.Message);
    }

    [Fact]
    public void GenerateWifiQrCodeSvg_WithInvalidPassword_ThrowsArgumentException()
    {
        // Arrange
        string ssid = "TestSSID";
        string password = "123"; // Invalid WPA password (too short)
        WifiSecurityType securityType = WifiSecurityType.WPA;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            WifiQrCodeHelper.GenerateWifiQrCodeSvg(ssid, password, securityType));
        Assert.Equal("ssid", exception.ParamName);
        Assert.Contains("WPA password must be between 8 and 63 characters", exception.Message);
    }

    [Theory]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(1024)]
    public void GenerateWifiQrCode_WithDifferentSizes_ReturnsCorrectSize(int size)
    {
        // Arrange
        string ssid = "TestSSID";
        string password = "password123";
        WifiSecurityType securityType = WifiSecurityType.WPA;

        // Act
        using Image<Rgba32> image = WifiQrCodeHelper.GenerateWifiQrCode(ssid, password, securityType, size: size);

        // Assert
        Assert.Equal(size, image.Width);
        Assert.Equal(size, image.Height);
    }

    [Fact]
    public void GenerateWifiString_WithDefaultParameters_ReturnsCorrectFormat()
    {
        // Arrange
        string ssid = "TestSSID";
        string password = "password123"; // デフォルトはWPAなので有効なパスワードが必要

        // Act
        string result = WifiQrCodeHelper.GenerateWifiString(ssid, password);

        // Assert
        Assert.Equal("WIFI:T:WPA;S:TestSSID;P:password123;;", result);
    }

    [Fact]
    public void GenerateWifiString_WithEmptyPassword_HandlesCorrectly()
    {
        // Arrange
        string ssid = "TestSSID";
        string password = "";

        // Act
        string result = WifiQrCodeHelper.GenerateWifiString(ssid, password, WifiSecurityType.None);

        // Assert
        Assert.Equal("WIFI:S:TestSSID;;", result);
    }

    [Theory]
    [InlineData("NetworkWith32CharacterLongSSIDs!", true)]  // Exactly 32 characters
    [InlineData("VeryLongSSIDNameThatExceeds32CharsLimit", false)]  // More than 32 characters
    public void ValidateWifiConfiguration_WithSSIDLengthLimits_ValidatesCorrectly(
        string ssid, 
        bool expectedValid)
    {
        // Act
        var (isValid, errorMessage) = WifiQrCodeHelper.ValidateWifiConfiguration(ssid, "password123", WifiSecurityType.WPA);

        // Assert
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.Contains("SSID must be 32 characters or less", errorMessage);
        }
    }

    [Fact]
    public void GenerateWifiQrCode_WithHiddenNetwork_GeneratesCorrectly()
    {
        // Arrange
        string ssid = "HiddenSSID";
        string password = "hiddenpass";
        WifiSecurityType securityType = WifiSecurityType.WPA;
        bool isHidden = true;

        // Act
        using Image<Rgba32> image = WifiQrCodeHelper.GenerateWifiQrCode(ssid, password, securityType, isHidden);

        // Assert
        Assert.NotNull(image);
        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }

    [Fact]
    public void GenerateWifiString_WithOpenNetworkAndPassword_IgnoresPassword()
    {
        // Arrange
        string ssid = "OpenNetwork";
        string password = "thisPasswordWillBeIgnored";
        WifiSecurityType securityType = WifiSecurityType.None;

        // Act
        string result = WifiQrCodeHelper.GenerateWifiString(ssid, password, securityType);

        // Assert
        Assert.Equal("WIFI:S:OpenNetwork;;", result);
        Assert.DoesNotContain("P:", result);
    }
}
