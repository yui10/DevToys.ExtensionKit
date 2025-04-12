using DevToys.Extensions.Models;
using System;
using System.Net;
using Xunit;

namespace DevToys.Extensions.Tests;

public class NetworkInfoTests
{    
    [Theory(DisplayName = "コンストラクタ - 有効なIPとサブネットマスクオブジェクトで正しい結果を返す")]
    [InlineData("192.168.1.10", "255.255.255.0", 24, "192.168.1.0", "192.168.1.255")]
    [InlineData("10.0.0.1", "255.0.0.0", 8, "10.0.0.0", "10.255.255.255")]
    [InlineData("172.16.0.1", "255.240.0.0", 12, "172.16.0.0", "172.31.255.255")]
    public void Constructor_ValidIpAndSubnetMaskObject_ReturnsCorrectResults(
        string ipAddressStr, string subnetMaskStr, int expectedPrefixLength,
        string expectedNetwork, string expectedBroadcast)
    {
        // Arrange
        var ipAddress = IPAddress.Parse(ipAddressStr);
        var subnetMask = IPAddress.Parse(subnetMaskStr);

        // Act
        var result = new NetworkInfo(ipAddress, subnetMask);

        // Assert
        Assert.Equal(ipAddressStr, result.IPAddress.ToString());
        Assert.Equal(subnetMaskStr, result.SubnetMask.ToString());
        Assert.Equal(expectedPrefixLength, result.PrefixLength);
        Assert.Equal(expectedNetwork, result.NetworkAddress.ToString());
        Assert.Equal(expectedBroadcast, result.BroadcastAddress.ToString());
    }
    
    [Theory(DisplayName = "コンストラクタ - 無効なIPまたはプレフィックス長で例外をスロー")]
    [InlineData(-1)]
    [InlineData(33)]
    public void Constructor_InvalidPrefixLength_ThrowsException(int prefixLength)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new NetworkInfo(IPAddress.Parse("192.168.1.1"), prefixLength));
    }

    [Fact(DisplayName = "コンストラクタ - nullのIPアドレスで例外をスロー")]
    public void Constructor_NullIpAddress_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NetworkInfo((IPAddress)null, 24));
    }

    [Theory(DisplayName = "FromCidr - 有効なCIDR表記で正しい結果を返す")]
    [InlineData("192.168.1.0/24", "192.168.1.0", "255.255.255.0", 24, "192.168.1.255")]
    [InlineData("10.0.0.0/8", "10.0.0.0", "255.0.0.0", 8, "10.255.255.255")]
    public void FromCidr_ValidInput_ReturnsCorrectResults(
        string cidr, string expectedIP, string expectedMask, 
        int expectedPrefixLength, string expectedBroadcast)
    {
        // Act
        var result = NetworkInfo.FromCidr(cidr);

        // Assert
        Assert.Equal(expectedIP, result.IPAddress.ToString());
        Assert.Equal(expectedMask, result.SubnetMask.ToString());
        Assert.Equal(expectedPrefixLength, result.PrefixLength);
        Assert.Equal(expectedBroadcast, result.BroadcastAddress.ToString());
    }

    [Theory(DisplayName = "FromCidr - 無効な入力で例外をスロー")]
    [InlineData("192.168.1.0")]
    [InlineData("192.168.1.0/")]
    [InlineData("192.168.1.256/24")]
    [InlineData("192.168.1.0/33")]
    [InlineData("invalid/24")]
    public void FromCidr_InvalidInput_ThrowsException(string cidr)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => NetworkInfo.FromCidr(cidr));
    }

    [Theory(DisplayName = "FromCidr - 無効なCIDRで例外をスロー")]
    [InlineData(null)]
    [InlineData("")]
    public void FromCidr_NullOrEmpty_ThrowsException(string cidr)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NetworkInfo.FromCidr(cidr));
    }

    [Theory(DisplayName = "GetSubnetSize - プレフィックス長に基づいて正しいサブネットサイズを返す")]
    [InlineData("192.168.1.0/24", 256)]
    [InlineData("192.168.1.0/25", 128)]
    [InlineData("192.168.1.0/30", 4)]
    [InlineData("192.168.1.0/31", 2)]
    [InlineData("192.168.1.0/32", 1)]
    public void GetSubnetSize_ReturnsCorrectSize(string cidr, long expectedSize)
    {
        // Arrange
        var networkInfo = NetworkInfo.FromCidr(cidr);

        // Act
        var result = networkInfo.GetSubnetSize();

        // Assert
        Assert.Equal(expectedSize, result);
    }

    [Theory(DisplayName = "GetUsableHostsCount - 正しい使用可能ホスト数を返す")]
    [InlineData("192.168.1.0/24", 254)]  // 256 - 2
    [InlineData("192.168.1.0/25", 126)]  // 128 - 2
    [InlineData("192.168.1.0/30", 2)]    // 4 - 2
    [InlineData("192.168.1.0/31", 2)]    // 特殊ケース
    [InlineData("192.168.1.0/32", 1)]    // 特殊ケース
    public void GetUsableHostsCount_ReturnsCorrectCount(string cidr, long expectedCount)
    {
        // Arrange
        var networkInfo = NetworkInfo.FromCidr(cidr);

        // Act
        var result = networkInfo.GetUsableHostsCount();

        // Assert
        Assert.Equal(expectedCount, result);
    }

    [Theory(DisplayName = "ToCidrString - 正しいCIDR表記を返す")]
    [InlineData("192.168.1.1", 24, "192.168.1.0/24")]
    [InlineData("10.0.0.0", 8, "10.0.0.0/8")]
    [InlineData("172.16.32.0", 20, "172.16.32.0/20")]
    public void ToCidrString_ReturnsCorrectNotation(string ipAddress, int prefixLength, string expectedCidr)
    {
        // Arrange
        var networkInfo = new NetworkInfo(IPAddress.Parse(ipAddress), prefixLength);

        // Act
        var result = networkInfo.ToCidrString();

        // Assert
        Assert.Equal(expectedCidr, result);
    }

    [Theory(DisplayName = "ToString - 正しい文字列表現を返す")]
    [InlineData("192.168.1.0", 24, "192.168.1.0/24 (255.255.255.0)")]
    [InlineData("10.0.0.0", 8, "10.0.0.0/8 (255.0.0.0)")]
    public void ToString_ReturnsCorrectRepresentation(string ipAddress, int prefixLength, string expected)
    {
        // Arrange
        var networkInfo = new NetworkInfo(IPAddress.Parse(ipAddress), prefixLength);

        // Act
        var result = networkInfo.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}