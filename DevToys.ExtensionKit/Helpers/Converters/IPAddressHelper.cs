using DevToys.ExtensionKit.Models.Converters;
using netIPAddress = System.Net.IPAddress;

namespace DevToys.ExtensionKit.Helpers.Converters;

internal static class IPAddressHelper
{
    /// <summary>
    /// IPアドレスとプレフィックス長から新しいNetworkInfoを計算します
    /// </summary>
    public static NetworkInfo CalculateNetworkInfo(string ipAddress, int prefixLength)
    {
        if (prefixLength < 0 || prefixLength > 32)
        {
            throw new ArgumentException("Invalid prefix length in CIDR notation.");
        }

        if (!netIPAddress.TryParse(ipAddress, out netIPAddress? _ipAddress) || _ipAddress == null)
        {
            throw new ArgumentException("Invalid IP address.");
        }

        return new NetworkInfo(_ipAddress, prefixLength);
    }

    /// <summary>
    /// IPアドレスとサブネットマスクから新しいNetworkInfoを計算します
    /// </summary>
    public static NetworkInfo CalculateNetworkInfo(string ipAddress, string subnetMask)
    {
        if (!netIPAddress.TryParse(ipAddress, out netIPAddress? _ipAddress) || _ipAddress == null)
        {
            throw new ArgumentException("Invalid IP address.");
        }

        if (!netIPAddress.TryParse(subnetMask, out netIPAddress? _subnetMask) || _subnetMask == null)
        {
            throw new ArgumentException("Invalid subnet mask.");
        }

        return new NetworkInfo(_ipAddress, _subnetMask);
    }

    /// <summary>
    /// CIDR表記（例："192.168.1.0/24"）からNetworkInfoを解析します
    /// </summary>
    public static NetworkInfo ParseCidrNotation(string cidr)
    {
        return NetworkInfo.FromCidr(cidr);
    }

    /// <summary>
    /// プレフィックス長からサブネットマスクの文字列表現を取得します
    /// </summary>
    public static string GetSubnetMaskFromPrefixLength(int prefixLength)
    {
        if (prefixLength < 0 || prefixLength > 32)
        {
            throw new ArgumentException("Invalid prefix length in CIDR notation.");
        }

        // 一時的にNetworkInfoを作成してそのSubnetMaskを文字列として返す
        var dummyIp = netIPAddress.Parse("0.0.0.0");
        var network = new NetworkInfo(dummyIp, prefixLength);
        return network.SubnetMask.ToString();
    }
}

