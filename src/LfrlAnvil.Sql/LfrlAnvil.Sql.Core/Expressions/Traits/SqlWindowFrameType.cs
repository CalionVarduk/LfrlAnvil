namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents the type of an <see cref="SqlWindowFrameNode"/>.
/// </summary>
public enum SqlWindowFrameType : byte
{
    /// <summary>
    /// Specifies a custom window frame type.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Specifies that the frame's boundaries are determined by positions of rows relative to the current row.
    /// </summary>
    Rows = 1,

    /// <summary>
    /// Specifies that the frame's boundaries are determined by values of rows within range of the value of the current row.
    /// </summary>
    Range = 2
}
