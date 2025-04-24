using System.Net;

namespace DevToys.ExtensionKit.Models;

/// <summary>
/// A class representing integrated network information (including both CIDR and subnet information)
/// </summary>
public class NetworkInfo
{
    /// <summary>
    /// IP Address
    /// </summary>
    public IPAddress IPAddress { get; }

    /// <summary>
    /// Subnet Mask
    /// </summary>
    public IPAddress SubnetMask { get; }

    /// <summary>
    /// Prefix Length (CIDR notation)
    /// </summary>
    public int PrefixLength { get; }

    /// <summary>
    /// Network Address
    /// </summary>
    public IPAddress NetworkAddress { get; }

    /// <summary>
    /// Broadcast Address
    /// </summary>
    public IPAddress BroadcastAddress { get; }

    /// <summary>
    /// First Usable Host
    /// </summary>
    public IPAddress FirstUsableHost { get; }

    /// <summary>
    /// Last Usable Host
    /// </summary>
    public IPAddress LastUsableHost { get; }

    /// <summary>
    /// Wildcard Mask
    /// </summary>
    public IPAddress WildcardMask { get; }

    /// <summary>
    /// Create a new NetworkInfo from an IP Address and prefix length
    /// </summary>
    /// <param name="ipAddress">IP Address</param>
    /// <param name="prefixLength">Prefix Length (0-32)</param>
    public NetworkInfo(IPAddress ipAddress, int prefixLength)
    {
        if (ipAddress == null)
            throw new ArgumentNullException(nameof(ipAddress));

        if (prefixLength < 0 || prefixLength > 32)
            throw new ArgumentException("Prefix length must be between 0 and 32", nameof(prefixLength));

        IPAddress = ipAddress;
        PrefixLength = prefixLength;
        SubnetMask = CreateSubnetMask(prefixLength);
        WildcardMask = CreateWildcardMask(SubnetMask);

        var ipBytes = ipAddress.GetAddressBytes();
        var maskBytes = SubnetMask.GetAddressBytes();

        // Calculate network address
        var networkBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
        }
        NetworkAddress = new IPAddress(networkBytes);

        // Calculate broadcast address
        var wildcardBytes = WildcardMask.GetAddressBytes();
        var broadcastBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            broadcastBytes[i] = (byte)(networkBytes[i] | wildcardBytes[i]);
        }
        BroadcastAddress = new IPAddress(broadcastBytes);

        // Calculate first and last host addresses
        var firstHostBytes = (byte[])networkBytes.Clone();
        var lastHostBytes = (byte[])broadcastBytes.Clone();

        // Special handling for /31 and /32 subnets
        if (prefixLength < 31)
        {
            firstHostBytes[3] = (byte)(firstHostBytes[3] + 1);
            lastHostBytes[3] = (byte)(lastHostBytes[3] - 1);
        }

        FirstUsableHost = new IPAddress(firstHostBytes);
        LastUsableHost = new IPAddress(lastHostBytes);
    }

    /// <summary>
    /// Create a new NetworkInfo from an IP Address and Subnet Mask
    /// </summary>
    /// <param name="ipAddress">IP Address</param>
    /// <param name="subnetMask">Subnet Mask</param>
    public NetworkInfo(IPAddress ipAddress, IPAddress subnetMask)
        : this(ipAddress, CalculatePrefixLength(subnetMask))
    {
    }

    /// <summary>
    /// Create a NetworkInfo from CIDR notation (e.g. "192.168.1.0/24")
    /// </summary>
    /// <param name="cidrNotation">CIDR notation string</param>
    /// <returns>NetworkInfo object</returns>
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
    /// Get the CIDR notation string (e.g. "192.168.1.0/24")
    /// </summary>
    /// <returns>CIDR notation string</returns>
    public string ToCidrString()
    {
        return $"{NetworkAddress}/{PrefixLength}";
    }

    /// <summary>
    /// Get the total number of addresses in this subnet (including network and broadcast addresses)
    /// </summary>
    /// <returns>Size of subnet</returns>
    public long GetSubnetSize()
    {
        return (long)Math.Pow(2, 32 - PrefixLength);
    }

    /// <summary>
    /// Get the number of usable host addresses in this subnet
    /// </summary>
    /// <returns>Number of usable host addresses</returns>
    public long GetUsableHostsCount()
    {
        if (PrefixLength >= 31) // /31 and /32 are special cases
            return PrefixLength == 31 ? 2 : 1;

        return GetSubnetSize() - 2; // Exclude network and broadcast addresses
    }

    /// <summary>
    /// Get a list of subnets based on the subdivision count
    /// </summary>
    /// <param name="subdivisionCount">The number of subdivisions</param>
    /// <returns>A list of NetworkInfo objects representing the subnets</returns>
    public List<NetworkInfo> GetSubnet(int subdivisionCount)
    {
        if (subdivisionCount <= 0 || PrefixLength + Math.Log2(subdivisionCount) > 32)
        {
            throw new ArgumentException("Invalid subdivision count", nameof(subdivisionCount));
        }

        int newPrefixLength = PrefixLength + (int)Math.Log2(subdivisionCount);
        long subnetSize = (long)Math.Pow(2, 32 - newPrefixLength);

        var subnets = new List<NetworkInfo>();
        byte[] networkBytes = NetworkAddress.GetAddressBytes();
        long networkBase = BitConverter.ToUInt32(networkBytes.Reverse().ToArray(), 0);

        for (int i = 0; i < subdivisionCount; i++)
        {
            long subnetBase = networkBase + (i * subnetSize);

            if (subnetBase > 0xFFFFFFFF)
            {
                throw new ArgumentException("Calculated subnet base is out of valid IP address range.", nameof(subdivisionCount));
            }

            byte[] subnetBytes = BitConverter.GetBytes((uint)subnetBase).Reverse().ToArray();
            IPAddress subnetAddress = new IPAddress(subnetBytes);

            subnets.Add(new NetworkInfo(subnetAddress, newPrefixLength));
        }

        return subnets;
    }

    /// <summary>
    /// Create a subnet mask from prefix length
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
    /// Create a wildcard mask from subnet mask
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
    /// Calculate prefix length from subnet mask
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
    /// Count the number of 1 bits in a byte
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
    /// Get a string representation of this network information
    /// </summary>
    /// <returns>String representation (ex: "192.168.1.0/24 (255.255.255.0)")</returns>
    public override string ToString()
    {
        return $"{NetworkAddress}/{PrefixLength} ({SubnetMask})";
    }
}
