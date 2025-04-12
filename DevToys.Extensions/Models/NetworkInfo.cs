using System;
using System.Net;

namespace DevToys.Extensions.Models;

/// <summary>
/// 統合されたネットワーク情報を表すクラス（CIDRとサブネット情報の両方を含む）
/// </summary>
public class NetworkInfo
{
    /// <summary>
    /// IPアドレス
    /// </summary>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// サブネットマスク
    /// </summary>
    public IPAddress SubnetMask { get; }

    /// <summary>
    /// プレフィックス長（CIDR表記）
    /// </summary>
    public int PrefixLength { get; }

    /// <summary>
    /// ネットワークアドレス
    /// </summary>
    public IPAddress NetworkAddress { get; }

    /// <summary>
    /// ブロードキャストアドレス
    /// </summary>
    public IPAddress BroadcastAddress { get; }

    /// <summary>
    /// 最初の使用可能ホスト
    /// </summary>
    public IPAddress FirstUsableHost { get; }

    /// <summary>
    /// 最後の使用可能ホスト
    /// </summary>
    public IPAddress LastUsableHost { get; }

    /// <summary>
    /// ワイルドカードマスク
    /// </summary>
    public IPAddress WildcardMask { get; }

    /// <summary>
    /// IPアドレスとプレフィックス長から新しいNetworkInfoを作成
    /// </summary>
    /// <param name="ipAddress">IPアドレス</param>
    /// <param name="prefixLength">プレフィックス長（0-32）</param>
    public NetworkInfo(IPAddress ipAddress, int prefixLength)
    {
        if (ipAddress == null)
            throw new ArgumentNullException(nameof(ipAddress));

        if (prefixLength < 0 || prefixLength > 32)
            throw new ArgumentException("プレフィックス長は0から32の間である必要があります", nameof(prefixLength));

        IPAddress = ipAddress;
        PrefixLength = prefixLength;
        SubnetMask = CreateSubnetMask(prefixLength);
        WildcardMask = CreateWildcardMask(SubnetMask);

        var ipBytes = ipAddress.GetAddressBytes();
        var maskBytes = SubnetMask.GetAddressBytes();

        // ネットワークアドレスを計算
        var networkBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
        }
        NetworkAddress = new IPAddress(networkBytes);

        // ブロードキャストアドレスを計算
        var wildcardBytes = WildcardMask.GetAddressBytes();
        var broadcastBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            broadcastBytes[i] = (byte)(networkBytes[i] | wildcardBytes[i]);
        }
        BroadcastAddress = new IPAddress(broadcastBytes);

        // 最初と最後のホストアドレスを計算
        var firstHostBytes = (byte[])networkBytes.Clone();
        var lastHostBytes = (byte[])broadcastBytes.Clone();

        // サブネットが/31または/32の場合の特別な処理
        if (prefixLength < 31)
        {
            firstHostBytes[3] = (byte)(firstHostBytes[3] + 1);
            lastHostBytes[3] = (byte)(lastHostBytes[3] - 1);
        }

        FirstUsableHost = new IPAddress(firstHostBytes);
        LastUsableHost = new IPAddress(lastHostBytes);
    }

    /// <summary>
    /// IPアドレスとサブネットマスクから新しいNetworkInfoを作成
    /// </summary>
    /// <param name="ipAddress">IPアドレス</param>
    /// <param name="subnetMask">サブネットマスク</param>
    public NetworkInfo(IPAddress ipAddress, IPAddress subnetMask)
        : this(ipAddress, CalculatePrefixLength(subnetMask))
    {
    }

    /// <summary>
    /// CIDR表記文字列（例："192.168.1.0/24"）からNetworkInfoを作成
    /// </summary>
    /// <param name="cidrNotation">CIDR表記文字列</param>
    /// <returns>NetworkInfoインスタンス</returns>
    public static NetworkInfo FromCidr(string cidrNotation)
    {
        if (string.IsNullOrEmpty(cidrNotation))
            throw new ArgumentNullException(nameof(cidrNotation));

        var parts = cidrNotation.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid CIDR notation format", nameof(cidrNotation));

        if (!IPAddress.TryParse(parts[0], out IPAddress? ipAddress) || ipAddress == null)
            throw new ArgumentException("Invalid IP address in CIDR notation", nameof(cidrNotation));

        if (!int.TryParse(parts[1], out int prefixLength) || prefixLength < 0 || prefixLength > 32)
            throw new ArgumentException("Invalid prefix length in CIDR notation", nameof(cidrNotation));

        return new NetworkInfo(ipAddress, prefixLength);
    }

    /// <summary>
    /// CIDR表記を文字列として返す
    /// </summary>
    /// <returns>CIDR表記文字列（例："192.168.1.0/24"）</returns>
    public string ToCidrString()
    {
        return $"{NetworkAddress}/{PrefixLength}";
    }

    /// <summary>
    /// サブネットサイズ（使用可能なIPアドレスの数）を取得
    /// </summary>
    /// <returns>サブネット内のIPアドレス総数</returns>
    public long GetSubnetSize()
    {
        return (long)Math.Pow(2, 32 - PrefixLength);
    }

    /// <summary>
    /// 使用可能なホストアドレスの数を取得
    /// </summary>
    /// <returns>使用可能なホストアドレスの数</returns>
    public long GetUsableHostsCount()
    {
        if (PrefixLength >= 31) // /31と/32は特殊ケース
            return PrefixLength == 31 ? 2 : 1;

        return GetSubnetSize() - 2; // ネットワークアドレスとブロードキャストアドレスを除く
    }

    /// <summary>
    /// プレフィックス長からサブネットマスクを作成
    /// </summary>
    private static IPAddress CreateSubnetMask(int prefixLength)
    {
        var maskBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            if (prefixLength >= 8)
            {
                maskBytes[i] = 255;
                prefixLength -= 8;
            }
            else
            {
                maskBytes[i] = (byte)(255 << (8 - prefixLength) & 255);
                prefixLength = 0;
            }
        }

        return new IPAddress(maskBytes);
    }

    /// <summary>
    /// サブネットマスクからワイルドカードマスクを作成
    /// </summary>
    private static IPAddress CreateWildcardMask(IPAddress subnetMask)
    {
        byte[] maskBytes = subnetMask.GetAddressBytes();
        byte[] wildcardBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            wildcardBytes[i] = (byte)~maskBytes[i];
        }

        return new IPAddress(wildcardBytes);
    }

    /// <summary>
    /// サブネットマスクからプレフィックス長を計算
    /// </summary>
    private static int CalculatePrefixLength(IPAddress subnetMask)
    {
        byte[] maskBytes = subnetMask.GetAddressBytes();
        int prefixLength = 0;

        foreach (byte b in maskBytes)
        {
            prefixLength += CountBits(b);
        }

        return prefixLength;
    }

    /// <summary>
    /// バイト内の1のビット数をカウント
    /// </summary>
    private static int CountBits(byte b)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            if ((b & (1 << (7 - i))) != 0)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// オブジェクトの文字列表現を返す
    /// </summary>
    /// <returns>文字列形式のネットワーク情報 (例：192.168.1.0/24 (255.255.255.0))</returns>
    public override string ToString()
    {
        return $"{NetworkAddress}/{PrefixLength} ({SubnetMask})";
    }
}
