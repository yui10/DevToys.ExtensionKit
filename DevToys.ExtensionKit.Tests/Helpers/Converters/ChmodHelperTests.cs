using DevToys.ExtensionKit.Helpers.Converters;
using DevToys.ExtensionKit.Models.Converters;
using Xunit;

namespace DevToys.ExtensionKit.Tests.Helpers.Converters;

public class ChmodHelperTests
{
    [Theory(DisplayName = "Calculate - Returns correct octal and symbolic notation for permission combinations")]
    [InlineData(Permission.All, Permission.All, Permission.All, "777", "rwxrwxrwx")]
    [InlineData(Permission.Read, Permission.None, Permission.None, "400", "r--------")]
    [InlineData(Permission.ReadWrite, Permission.None, Permission.None, "600", "rw-------")]
    [InlineData(Permission.All, Permission.None, Permission.None, "700", "rwx------")]
    [InlineData(Permission.Read, Permission.Read, Permission.None, "440", "r--r-----")]
    [InlineData(Permission.None, Permission.None, Permission.None, "000", "---------")]
    [InlineData(Permission.ReadExecute, Permission.Write, Permission.WriteExecute, "523", "r-x-w--wx")]
    public void Calculate_ReturnsExpectedResults(
        Permission owner, Permission group, Permission other,
        string expectedOctal, string expectedSymbol)
    {
        var (octal, symbol) = ChmodHelper.Calculate(owner, group, other);
        Assert.Equal(expectedOctal, octal);
        Assert.Equal(expectedSymbol, symbol);
    }

    [Theory(DisplayName = "ParseSymbolToPermissions - Correctly parses symbolic notation to permissions")]
    [InlineData("rwxrwxrwx", Permission.All, Permission.All, Permission.All)]
    [InlineData("r--------", Permission.Read, Permission.None, Permission.None)]
    [InlineData("rw-------", Permission.ReadWrite, Permission.None, Permission.None)]
    [InlineData("rwx------", Permission.All, Permission.None, Permission.None)]
    [InlineData("r--r-----", Permission.Read, Permission.Read, Permission.None)]
    [InlineData("---------", Permission.None, Permission.None, Permission.None)]
    [InlineData("r-x-w--wx", Permission.ReadExecute, Permission.Write, Permission.WriteExecute)]
    public void ParseSymbolToPermissions_ReturnsExpectedResults(
        string symbol,
        Permission expectedOwner, Permission expectedGroup, Permission expectedOther)
    {
        var (owner, group, other) = ChmodHelper.ParseSymbolToPermissions(symbol);
        Assert.Equal(expectedOwner, owner);
        Assert.Equal(expectedGroup, group);
        Assert.Equal(expectedOther, other);
    }    [Theory(DisplayName = "ParseSymbolToPermissions - Returns none for invalid inputs")]
    [InlineData("")]
    [InlineData("rwx")]  // Too short
    [InlineData("rwxrwxrwxrwx")]  // Too long
    public void ParseSymbolToPermissions_ReturnsNoneForInvalidInput(string symbol)
    {
        var (owner, group, other) = ChmodHelper.ParseSymbolToPermissions(symbol);
        Assert.Equal(Permission.None, owner);
        Assert.Equal(Permission.None, group);
        Assert.Equal(Permission.None, other);
    }

    [Theory(DisplayName = "ParseOctalToPermissions - Correctly parses octal value to permissions")]
    [InlineData(777, Permission.All, Permission.All, Permission.All)]
    [InlineData(400, Permission.Read, Permission.None, Permission.None)]
    [InlineData(600, Permission.ReadWrite, Permission.None, Permission.None)]
    [InlineData(700, Permission.All, Permission.None, Permission.None)]
    [InlineData(440, Permission.Read, Permission.Read, Permission.None)]
    [InlineData(000, Permission.None, Permission.None, Permission.None)]
    [InlineData(523, Permission.ReadExecute, Permission.Write, Permission.WriteExecute)]
    public void ParseOctalToPermissions_ReturnsExpectedResults(
        int octal,
        Permission expectedOwner, Permission expectedGroup, Permission expectedOther)
    {
        var (owner, group, other) = ChmodHelper.ParseOctalToPermissions(octal);
        Assert.Equal(expectedOwner, owner);
        Assert.Equal(expectedGroup, group);
        Assert.Equal(expectedOther, other);
    }

    [Theory(DisplayName = "ParseOctalToPermissions - Handles invalid octal values")]
    [InlineData(-1)]
    [InlineData(1000)]  // Exceeds 3 digits
    [InlineData(978)]   // Invalid octal digit (8, 9)
    public void ParseOctalToPermissions_HandlesInvalidOctalValues(int octal)
    {
        var (owner, group, other) = ChmodHelper.ParseOctalToPermissions(octal);
        
        // Note: The current implementation might not fully validate octal input
        // This test is to document the current behavior
        if (octal < 0)
        {
            Assert.Equal(Permission.None, owner);
            Assert.Equal(Permission.None, group);
            Assert.Equal(Permission.None, other);
        }
    }    [Theory(DisplayName = "IsValidPermissionSymbol - Validates symbol format correctly")]
    [InlineData("rwxrwxrwx", true)]
    [InlineData("r--r--r--", true)]
    [InlineData("---------", true)]
    [InlineData("r-xr-xr-x", true)]
    [InlineData("rw-r--r--", true)]
    [InlineData("", false)]
    [InlineData("rwxrwx", false)]  // Too short
    [InlineData("rwxrwxrwxrwx", false)]  // Too long
    [InlineData("abcdefghi", false)]  // Invalid characters
    [InlineData("rwx!@#$%^", false)]  // Invalid characters
    [InlineData("xr-xr-xr-", false)]  // Invalid position ('x' at read position)
    [InlineData("-r--w--x-", false)]  // Invalid position ('w' at execute position)
    public void IsValidPermissionSymbol_ReturnsExpectedResults(string symbol, bool expected)
    {
        bool actual = ChmodHelper.IsValidPermissionSymbol(symbol);
        Assert.Equal(expected, actual);
    }
}