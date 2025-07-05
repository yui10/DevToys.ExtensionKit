namespace DevToys.ExtensionKit.Models.Generators;

/// <summary>
/// WiFi security types
/// </summary>
[Flags]
public enum WifiSecurityType
{
    /// <summary>
    /// No password (open network)
    /// </summary>
    None,

    /// <summary>
    /// WEP encryption
    /// </summary>
    WEP,

    /// <summary>
    /// WPA/WPA2-PSK encryption
    /// </summary>
    WPA
}