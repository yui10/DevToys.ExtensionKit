using DevToys.ExtensionKit.Models.Converters;
using System;
using System.Net;
using Xunit;

namespace DevToys.ExtensionKit.Tests.Models.Converters;

public class NetworkInfoTests
{    
    [Theory(DisplayName = "Constructor - Returns correct results with valid IP and subnet mask objects")]
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
    
    [Theory(DisplayName = "Constructor - Throws exception for invalid IP or prefix length")]
    [InlineData(-1)]
    [InlineData(33)]
    public void Constructor_InvalidPrefixLength_ThrowsException(int prefixLength)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new NetworkInfo(IPAddress.Parse("192.168.1.1"), prefixLength));
    }

    [Fact(DisplayName = "Constructor - Throws exception for null IP address")]
    public void Constructor_NullIpAddress_ThrowsException()
    {
        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.Throws<ArgumentNullException>(() => new NetworkInfo(null, 24));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [Theory(DisplayName = "FromCidr - Returns correct results for valid CIDR notation")]
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

    [Theory(DisplayName = "FromCidr - Throws exception for invalid input")]
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

    [Theory(DisplayName = "FromCidr - Throws exception for null or empty CIDR")]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    [InlineData("")]
    public void FromCidr_NullOrEmpty_ThrowsException(string cidr)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NetworkInfo.FromCidr(cidr));
    }

    [Theory(DisplayName = "GetSubnetSize - Returns correct subnet size based on prefix length")]
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

    [Theory(DisplayName = "GetUsableHostsCount - Returns correct usable host count")]
    [InlineData("192.168.1.0/24", 254)]  // 256 - 2
    [InlineData("192.168.1.0/25", 126)]  // 128 - 2
    [InlineData("192.168.1.0/30", 2)]    // 4 - 2
    [InlineData("192.168.1.0/31", 2)]    // Special case
    [InlineData("192.168.1.0/32", 1)]    // Special case
    public void GetUsableHostsCount_ReturnsCorrectCount(string cidr, long expectedCount)
    {
        // Arrange
        var networkInfo = NetworkInfo.FromCidr(cidr);

        // Act
        var result = networkInfo.GetUsableHostsCount();

        // Assert
        Assert.Equal(expectedCount, result);
    }

    [Theory(DisplayName = "ToCidrString - Returns correct CIDR notation")]
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

    [Theory(DisplayName = "ToString - Returns correct string representation")]
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