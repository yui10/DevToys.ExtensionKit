using DevToys.ExtensionKit.Models.Converters;

namespace DevToys.ExtensionKit.Helpers.Converters;

/// <summary>
/// Unix chmod (permission) conversion utility
/// </summary>
public static class ChmodHelper
{
    /// <summary>
    /// Calculates chmod notation (octal and symbol) from owner, group, and other permissions
    /// </summary>
    /// <param name="owner">Owner permissions</param>
    /// <param name="group">Group permissions</param>
    /// <param name="other">Other permissions</param>
    /// <returns>Tuple containing octal and symbolic notation</returns>
    public static (string Octal, string Symbol) Calculate(Permission owner, Permission group, Permission other)
    {
        string octal = $"{ToOctal(owner)}{ToOctal(group)}{ToOctal(other)}";
        string symbol = ToSymbol(owner) + ToSymbol(group) + ToSymbol(other);
        return (octal, symbol);
    }


    /// <summary>
    /// Parses symbolic notation (e.g., rwxr-xr--) into permissions
    /// </summary>
    /// <remarks>
    /// The symbolic notation consists of 9 characters representing:
    /// - Characters 0-2: Owner permissions (rwx)
    /// - Characters 3-5: Group permissions (rwx)
    /// - Characters 6-8: Other permissions (rwx)
    ///
    /// Each character position represents:
    /// - 'r' or '-' for read permission
    /// - 'w' or '-' for write permission
    /// - 'x' or '-' for execute permission
    ///
    /// If the input is invalid (null, empty, or incorrect length), 
    /// the method returns None for all permission types.
    /// </remarks>
    /// <param name="symbol">Symbolic notation (9 characters)</param>
    /// <returns>Tuple containing owner, group, and other permissions</returns>
    public static (Permission owner, Permission group, Permission other) ParseSymbolToPermissions(string symbol)
    {
        static Permission ParseSegment(string segment)
        {
            if (string.IsNullOrEmpty(segment) || segment.Length != 3)
            {
                return Permission.None;
            }

            return (segment[0] == 'r' ? Permission.Read : Permission.None) |
                   (segment[1] == 'w' ? Permission.Write : Permission.None) |
                   (segment[2] == 'x' ? Permission.Execute : Permission.None);
        }

        if (string.IsNullOrEmpty(symbol) || symbol.Length != 9)
        {
            return (Permission.None, Permission.None, Permission.None);
        }

        return (
            ParseSegment(symbol.Substring(0, 3)),
            ParseSegment(symbol.Substring(3, 3)),
            ParseSegment(symbol.Substring(6, 3))
        );
    }


    /// <summary>
    /// Parses octal value (e.g., 755) into permissions
    /// </summary>
    /// <remarks>
    /// The octal value consists of 3 digits representing:
    /// - First digit (hundreds): Owner permissions (0-7)
    /// - Second digit (tens): Group permissions (0-7)
    /// - Third digit (ones): Other permissions (0-7)
    /// 
    /// Each digit is interpreted as a bit field:
    /// - Bit 2 (value 4): Read permission
    /// - Bit 1 (value 2): Write permission
    /// - Bit 0 (value 1): Execute permission
    /// 
    /// For example:
    /// - 7 (binary 111) = Read + Write + Execute
    /// - 5 (binary 101) = Read + Execute
    /// - 0 (binary 000) = No permissions
    /// 
    /// Invalid values (negative, or digits > 7) will result in None permissions.
    /// </remarks>
    /// <param name="octalValue">3-digit octal value</param>
    /// <returns>Tuple containing owner, group, and other permissions</returns>
    public static (Permission owner, Permission group, Permission other) ParseOctalToPermissions(int octalValue)
    {
        static Permission ParseSegment(int segment)
        {
            if (segment < 0 || segment > 7)
            {
                return Permission.None;
            }

            return ((segment & 4) != 0 ? Permission.Read : Permission.None) |
                   ((segment & 2) != 0 ? Permission.Write : Permission.None) |
                   ((segment & 1) != 0 ? Permission.Execute : Permission.None);
        }

        int owner = octalValue / 100 % 10;
        int group = octalValue / 10 % 10;
        int other = octalValue % 10;

        return (
            ParseSegment(owner),
            ParseSegment(group),
            ParseSegment(other)
        );
    }

    /// <summary>
    /// Converts Permission to a single octal digit (0-7)
    /// </summary>
    /// <remarks>
    /// Maps permission flags to their octal representation:
    /// - Read: 4
    /// - Write: 2
    /// - Execute: 1
    /// 
    /// Combinations add up, e.g.:
    /// - Read + Write = 6
    /// - Read + Execute = 5
    /// - Read + Write + Execute = 7
    /// </remarks>
    /// <param name="permission">Permission to convert</param>
    /// <returns>Single octal digit (0-7)</returns>
    private static int ToOctal(Permission permission)
    {
        // Read:4, Write:2, Execute:1
        int value = 0;
        if ((permission & Permission.Read) != 0) value += 4;
        if ((permission & Permission.Write) != 0) value += 2;
        if ((permission & Permission.Execute) != 0) value += 1;
        return value;
    }

    /// <summary>
    /// Converts Permission to symbolic notation (rwx, etc.)
    /// </summary>
    /// <remarks>
    /// Creates a 3-character string representation of permissions:
    /// - First character: 'r' if Read permission is set, '-' otherwise
    /// - Second character: 'w' if Write permission is set, '-' otherwise
    /// - Third character: 'x' if Execute permission is set, '-' otherwise
    /// 
    /// Examples:
    /// - Permission.All (Read | Write | Execute) → "rwx"
    /// - Permission.ReadWrite (Read | Write) → "rw-"
    /// - Permission.None → "---"
    /// </remarks>
    /// <param name="permission">Permission to convert</param>
    /// <returns>3-character symbolic notation</returns>
    private static string ToSymbol(Permission permission)
    {
        return $"{((permission & Permission.Read) != 0 ? "r" : "-")}" +
               $"{((permission & Permission.Write) != 0 ? "w" : "-")}" +
               $"{((permission & Permission.Execute) != 0 ? "x" : "-")}";
    }

    /// <summary>
    /// Validates if the given text is a valid symbolic permission notation
    /// </summary>
    /// <remarks>
    /// A valid symbolic notation must:
    /// - Be exactly 9 characters long
    /// - Follow the pattern of 3 sets of "rwx" or "-" characters
    /// - Position 0,3,6 can only be 'r' or '-' (read permission)
    /// - Position 1,4,7 can only be 'w' or '-' (write permission)
    /// - Position 2,5,8 can only be 'x' or '-' (execute permission)
    /// </remarks>
    /// <param name="text">The symbolic notation to validate</param>
    /// <returns>True if the text represents a valid permission symbol, false otherwise</returns>
    public static bool IsValidPermissionSymbol(string text)
    {
        // Check for null or empty string
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        // Check for exact length of 9 characters
        if (text.Length != 9)
        {
            return false;
        }

        // Validate each character based on its position
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // Calculate position within each permission triplet (0=read, 1=write, 2=execute)
            int pos = i % 3;

            // Validate character based on its position
            bool isValid = pos switch
            {
                0 => c == 'r' || c == '-', // Read position
                1 => c == 'w' || c == '-', // Write position
                2 => c == 'x' || c == '-', // Execute position
                _ => false                  // Should never happen
            };

            if (!isValid)
            {
                return false;
            }
        }

        return true;
    }
}
