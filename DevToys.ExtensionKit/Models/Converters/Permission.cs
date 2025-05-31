namespace DevToys.ExtensionKit.Models.Converters;

/// <summary>
/// Enumeration representing Unix file permissions
/// </summary>
[Flags]
public enum Permission
{
    /// <summary>No permissions</summary>
    None = 0,
    
    /// <summary>Read permission (r)</summary>
    Read = 1 << 2, // 4
    
    /// <summary>Write permission (w)</summary>
    Write = 1 << 1, // 2
    
    /// <summary>Execute permission (x)</summary>
    Execute = 1 << 0, // 1
    
    /// <summary>Read and write permissions (rw-)</summary>
    ReadWrite = Read | Write, // 6
    
    /// <summary>Read and execute permissions (r-x)</summary>
    ReadExecute = Read | Execute, // 5
    
    /// <summary>Write and execute permissions (-wx)</summary>
    WriteExecute = Write | Execute, // 3
    
    /// <summary>All permissions (rwx)</summary>
    All = Read | Write | Execute // 7
}
