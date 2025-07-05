using DevToys.ExtensionKit.Models.Generators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace DevToys.ExtensionKit.Helpers.Generators;

/// <summary>
/// Helper class for generating WiFi QR codes that can be scanned by mobile devices to automatically connect to WiFi networks.
/// Supports open networks, WEP, and WPA/WPA2-PSK security types with proper validation.
/// </summary>
internal static class WifiQrCodeHelper
{
    /// <summary>
    /// Standard QR code size for optimal scanning balance between size and readability.
    /// </summary>
    private const int DefaultQrCodeSize = 512;

    /// <summary>
    /// QR code margin size for better scanning reliability.
    /// </summary>
    private const int DefaultMargin = 2;

    /// <summary>
    /// Generate WiFi QR code image for cross-platform compatibility.
    /// </summary>
    /// <param name="ssid">WiFi network name (SSID) - must be 32 characters or less</param>
    /// <param name="password">WiFi password - requirements depend on security type</param>
    /// <param name="securityType">Security type (None, WEP, WPA/WPA2-PSK)</param>
    /// <param name="isHidden">Whether the network is hidden</param>
    /// <param name="size">QR code size in pixels</param>
    /// <returns>Generated QR code image</returns>
    internal static Image<Rgba32> GenerateWifiQrCode(
        string ssid,
        string password = "",
        WifiSecurityType securityType = WifiSecurityType.WPA,
        bool isHidden = false,
        int size = DefaultQrCodeSize)
    {
        // Generate WiFi configuration string according to specification
        string wifiString = GenerateWifiString(ssid, password, securityType, isHidden);

        // Configure QR code generator with optimal settings
        var barcodeWriter = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Height = size,
                Width = size,
                Margin = DefaultMargin
            }
        };

        // Set error correction level to Medium for better reliability in real-world scanning conditions
        barcodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.M);

        Image<Rgba32> image = barcodeWriter.Write(wifiString);
        return image;
    }

    /// <summary>
    /// Generate WiFi QR code as SVG format for scalable vector graphics.
    /// </summary>
    /// <param name="ssid">WiFi network name (SSID) - must be 32 characters or less</param>
    /// <param name="password">WiFi password - requirements depend on security type</param>
    /// <param name="securityType">Security type (None, WEP, WPA/WPA2-PSK)</param>
    /// <param name="isHidden">Whether the network is hidden</param>
    /// <param name="size">QR code size in pixels</param>
    /// <returns>SVG content as string</returns>
    internal static string GenerateWifiQrCodeSvg(
        string ssid,
        string password = "",
        WifiSecurityType securityType = WifiSecurityType.WPA,
        bool isHidden = false,
        int size = DefaultQrCodeSize)
    {
        // Generate WiFi configuration string according to specification
        string wifiString = GenerateWifiString(ssid, password, securityType, isHidden);

        // Configure SVG QR code generator
        var barcodeWriter = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.QR_CODE,
            Renderer = new SvgRenderer()
        };

        var encodingOptions = new EncodingOptions
        {
            Width = size,
            Height = size,
            Margin = DefaultMargin,
        };

        // Use Medium error correction for optimal balance of size and reliability
        encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.M);
        barcodeWriter.Options = encodingOptions;

        var svg = barcodeWriter.Write(wifiString);
        return svg.Content;
    }

    /// <summary>
    /// Generate WiFi configuration string in WIFI: format.
    /// Special characters are properly encoded for QR code compatibility.
    /// </summary>
    /// <param name="ssid">WiFi network name</param>
    /// <param name="password">WiFi password</param>
    /// <param name="securityType">Security type</param>
    /// <param name="isHidden">Whether the network is hidden</param>
    /// <returns>WiFi configuration string</returns>
    internal static string GenerateWifiString(
        string ssid,
        string password = "",
        WifiSecurityType securityType = WifiSecurityType.WPA,
        bool isHidden = false)
    {
        // Validate input parameters before proceeding with generation
        var (IsValid, ErrorMessage) = ValidateWifiConfiguration(ssid, password, securityType);
        if (!IsValid)
        {
            throw new ArgumentException(ErrorMessage, nameof(ssid));
        }
        
        // Apply percent-encoding to handle special characters properly
        string escapedSsid = PercentEncodeString(ssid);
        string escapedPassword = PercentEncodeString(password ?? "");

        // Build WiFi string according to specification:
        // WIFI:[type ";"] [trdisable ";"] ssid ";" [hidden ";"] [id ";"] [password ";"] [publickey ";"] ";"
        var wifiString = "WIFI:";

        // Add security type if not open network (follows spec order)
        if (securityType != WifiSecurityType.None)
        {
            string securityTypeString = securityType switch
            {
                WifiSecurityType.WEP => "WEP",
                WifiSecurityType.WPA => "WPA",
                _ => "WPA" // Default to WPA for unknown types
            };
            wifiString += $"T:{securityTypeString};";
        }

        // Add SSID (required field in specification)
        wifiString += $"S:{escapedSsid};";

        // Add hidden network flag if specified
        if (isHidden)
        {
            wifiString += "H:true;";
        }

        // Add password if security type requires it
        if (securityType != WifiSecurityType.None && !string.IsNullOrEmpty(password))
        {
            wifiString += $"P:{escapedPassword};";
        }

        // End with mandatory semicolon
        wifiString += ";";

        return wifiString;
    }

    /// <summary>
    /// Encode special characters for WiFi QR code format.
    /// Semicolons and other special characters are percent-encoded.
    /// </summary>
    /// <param name="input">Input string to encode</param>
    /// <returns>Encoded string safe for WiFi QR codes</returns>
    private static string PercentEncodeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        foreach (char c in input)
        {
            // Characters in printable set: %x20-3a / %x3c-7e (semi-colon excluded)
            // Printable characters: space (0x20) to colon (0x3A), less-than (0x3C) to tilde (0x7E)
            // But semi-colon (0x3B) must be percent-encoded as it's a field separator
            if (c >= 0x20 && c <= 0x7E && c != ';')
            {
                result.Append(c);
            }
            else
            {
                // Percent-encode special characters including semi-colon
                byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
                foreach (byte b in bytes)
                {
                    result.Append($"%{b:X2}");
                }
            }
        }
        return result.ToString();
    }

    /// <summary>
    /// Validate WiFi configuration parameters.
    /// Checks SSID length and password requirements for each security type.
    /// 
    /// Password requirements:
    /// - None: No password required
    /// - WEP: 5 or 13 characters (ASCII) or 10 or 26 characters (hex)
    /// - WPA: 8-63 characters
    /// </summary>
    /// <param name="ssid">WiFi network name to validate</param>
    /// <param name="password">WiFi password to validate</param>
    /// <param name="securityType">Security type</param>
    /// <returns>Validation result and error message if invalid</returns>
    internal static (bool IsValid, string ErrorMessage) ValidateWifiConfiguration(
        string ssid,
        string password,
        WifiSecurityType securityType)
    {
        // SSID validation - required and length constrained
        if (string.IsNullOrWhiteSpace(ssid))
        {
            return (false, "SSID is required");
        }

        if (ssid.Length > 32)
        {
            return (false, "SSID must be 32 characters or less");
        }

        // Open networks (None) do not require a password
        if (securityType == WifiSecurityType.None)
        {
            return (true, string.Empty);
        }

        // Password is required for secured networks (WEP, WPA)
        if (string.IsNullOrEmpty(password))
        {
            return (false, "Password is required for secured networks");
        }

        // Security type specific validation
        if (securityType == WifiSecurityType.WEP)
        {
            // WEP key validation - supports ASCII and hex formats
            // 40bit: 5 chars (ASCII) or 10 chars (hex)
            // 104bit: 13 chars (ASCII) or 26 chars (hex)
            int length = password.Length;

            if (length == 5 || length == 13)
            {
                // ASCII mode: alphanumeric characters and underscore only
                if (!IsValidWepAsciiKey(password))
                {
                    return (false, "WEP ASCII key must contain only alphanumeric characters and underscore");
                }
            }
            else if (length == 10 || length == 26)
            {
                // Hex mode: 0-9 and a-f characters only (case insensitive)
                if (!IsValidWepHexKey(password))
                {
                    return (false, "WEP hex key must contain only hexadecimal characters (0-9, a-f, A-F)");
                }
            }
            else
            {
                return (false, "WEP key must be 5 or 13 characters (ASCII) or 10 or 26 characters (hex)");
            }
        }
        else if (securityType == WifiSecurityType.WPA)
        {
            // WPA password must be 8-63 characters
            if (password.Length < 8 || password.Length > 63)
            {
                return (false, "WPA password must be between 8 and 63 characters");
            }
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Validate WEP ASCII key format.
    /// Accepts alphanumeric characters and underscores only.
    /// </summary>
    /// <param name="key">WEP key to validate</param>
    /// <returns>True if valid ASCII key</returns>
    private static bool IsValidWepAsciiKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        // WEP ASCII key: alphanumeric characters and underscore only
        foreach (char c in key)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Validate WEP hex key format.
    /// Accepts hexadecimal characters (0-9, a-f, A-F) only.
    /// </summary>
    /// <param name="key">WEP key to validate</param>
    /// <returns>True if valid hex key</returns>
    private static bool IsValidWepHexKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        // WEP hex key: 0-9 and a-f characters only (case insensitive)
        foreach (char c in key)
        {
            if (!char.IsDigit(c) &&
                !((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
            {
                return false;
            }
        }
        return true;
    }
}
