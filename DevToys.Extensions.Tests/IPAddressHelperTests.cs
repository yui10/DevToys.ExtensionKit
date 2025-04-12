using DevToys.Extensions.Helpers;

namespace DevToys.Extensions.Tests;

public class IPAddressHelperTests
{
    // [Theory(DisplayName = "CalculateNetworkInfo - Returns correct results for valid input")]
    // [InlineData("192.168.1.10", "255.255.255.0", "192.168.1.0", "192.168.1.255", "192.168.1.1", "192.168.1.254")]
    // [InlineData("10.0.0.1", "255.0.0.0", "10.0.0.0", "10.255.255.255", "10.0.0.1", "10.255.255.254")]
    // [InlineData("172.16.32.1", "255.255.240.0", "172.16.32.0", "172.16.47.255", "172.16.32.1", "172.16.47.254")]
    // public void CalculateNetworkInfo_ValidInput_ReturnsCorrectResults(string ipAddress, string subnetMask,
    //     string expectedNetwork, string expectedBroadcast, string expectedFirstHost, string expectedLastHost)
    // {
    //     // Act
    //     var result = IPAddressHelper.CalculateNetworkInfo(ipAddress, subnetMask);

    //     // Assert
    //     Assert.Equal(expectedNetwork, result.NetworkAddress.ToString());
    //     Assert.Equal(expectedBroadcast, result.BroadcastAddress.ToString());
    //     Assert.Equal(expectedFirstHost, result.FirstUsableHost.ToString());
    //     Assert.Equal(expectedLastHost, result.LastUsableHost.ToString());
    // }

    // [Theory(DisplayName = "CalculateNetworkInfo - Throws exception for invalid input")]
    // [InlineData("192.168.1.10", "255.255.0.256")]
    // [InlineData("invalid_ip", "255.255.255.0")]
    // public void CalculateNetworkInfo_InvalidInput_ThrowsException(string ipAddress, string subnetMask)
    // {
    //     // Act & Assert
    //     Assert.Throws<ArgumentException>(() => IPAddressHelper.CalculateNetworkInfo(ipAddress, subnetMask));
    // }

    // [Theory(DisplayName = "ParseCidrNotation - Returns correct results for valid input")]
    // [InlineData("192.168.1.0/24", "192.168.1.0", "255.255.255.0", 24)]
    // [InlineData("10.0.0.0/8", "10.0.0.0", "255.0.0.0", 8)]
    // public void ParseCidrNotation_ValidInput_ReturnsCorrectResults(string cidr, string expectedIP, string expectedMask, int expectedPrefixLength)
    // {
    //     // Act
    //     var result = IPAddressHelper.ParseCidrNotation(cidr);

    //     // Assert
    //     Assert.Equal(expectedIP, result.IPAddress.ToString());
    //     Assert.Equal(expectedMask, result.SubnetMask.ToString());
    //     Assert.Equal(expectedPrefixLength, result.PrefixLength);
    // }

    // [Theory(DisplayName = "ParseCidrNotation - Throws exception for invalid input")]
    // [InlineData("192.168.1.0/33")]
    // [InlineData("invalid_cidr")]
    // public void ParseCidrNotation_InvalidInput_ThrowsException(string cidr)
    // {
    //     // Act & Assert
    //     Assert.Throws<ArgumentException>(() => IPAddressHelper.ParseCidrNotation(cidr));
    // }

    [Theory(DisplayName = "GetSubnetMaskFromPrefixLength - Returns correct mask for valid prefix length")]
    [InlineData(24, "255.255.255.0")]
    [InlineData(16, "255.255.0.0")]
    [InlineData(8, "255.0.0.0")]
    public void GetSubnetMaskFromPrefixLength_ValidPrefixLength_ReturnsCorrectMask(int prefixLength, string expectedMask)
    {
        // Act
        string result = IPAddressHelper.GetSubnetMaskFromPrefixLength(prefixLength);

        // Assert
        Assert.Equal(expectedMask, result);
    }

    [Theory(DisplayName = "GetSubnetMaskFromPrefixLength - Throws exception for invalid prefix length")]
    [InlineData(-1)]
    [InlineData(33)]
    public void GetSubnetMaskFromPrefixLength_InvalidPrefixLength_ThrowsException(int prefixLength)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => IPAddressHelper.GetSubnetMaskFromPrefixLength(prefixLength));
    }
}