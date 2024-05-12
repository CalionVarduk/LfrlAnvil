using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents a boundary of an <see cref="SqlWindowFrameNode"/>.
/// </summary>
public readonly struct SqlWindowFrameBoundary
{
    /// <summary>
    /// Represents a <see cref="SqlWindowFrameBoundaryDirection.CurrentRow"/> boundary.
    /// </summary>
    public static readonly SqlWindowFrameBoundary CurrentRow = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.CurrentRow,
        null );

    /// <summary>
    /// Represents an unlimited <see cref="SqlWindowFrameBoundaryDirection.Preceding"/> boundary.
    /// </summary>
    public static readonly SqlWindowFrameBoundary UnboundedPreceding = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.Preceding,
        null );

    /// <summary>
    /// Represents an unlimited <see cref="SqlWindowFrameBoundaryDirection.Following"/> boundary.
    /// </summary>
    public static readonly SqlWindowFrameBoundary UnboundedFollowing = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.Following,
        null );

    private SqlWindowFrameBoundary(SqlWindowFrameBoundaryDirection direction, SqlExpressionNode? expression)
    {
        Direction = direction;
        Expression = expression;
    }

    /// <summary>
    /// <see cref="SqlWindowFrameBoundaryDirection"/> of this boundary.
    /// </summary>
    public SqlWindowFrameBoundaryDirection Direction { get; }

    /// <summary>
    /// Optional offset expression.
    /// </summary>
    public SqlExpressionNode? Expression { get; }

    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameBoundary"/> instance with <see cref="SqlWindowFrameBoundaryDirection.Preceding"/> direction
    /// and a custom offset <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">Offset expression to use.</param>
    /// <returns>New <see cref="SqlWindowFrameBoundary"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlWindowFrameBoundary Preceding(SqlExpressionNode expression)
    {
        return new SqlWindowFrameBoundary( SqlWindowFrameBoundaryDirection.Preceding, expression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameBoundary"/> instance with <see cref="SqlWindowFrameBoundaryDirection.Following"/> direction
    /// and a custom offset <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">Offset expression to use.</param>
    /// <returns>New <see cref="SqlWindowFrameBoundary"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlWindowFrameBoundary Following(SqlExpressionNode expression)
    {
        return new SqlWindowFrameBoundary( SqlWindowFrameBoundaryDirection.Following, expression );
    }
}
