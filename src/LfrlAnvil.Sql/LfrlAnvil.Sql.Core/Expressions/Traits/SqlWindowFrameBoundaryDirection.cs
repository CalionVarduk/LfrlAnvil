namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents a direction of an <see cref="SqlWindowFrameBoundary"/>.
/// </summary>
public enum SqlWindowFrameBoundaryDirection : byte
{
    /// <summary>
    /// Specifies the current row.
    /// </summary>
    CurrentRow = 0,

    /// <summary>
    /// Specifies rows preceding the current row.
    /// </summary>
    Preceding = 1,

    /// <summary>
    /// Specifies rows following the current row.
    /// </summary>
    Following = 2
}
