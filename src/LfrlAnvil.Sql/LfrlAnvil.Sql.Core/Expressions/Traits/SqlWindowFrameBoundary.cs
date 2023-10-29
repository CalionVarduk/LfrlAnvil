using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Expressions.Traits;

public readonly struct SqlWindowFrameBoundary
{
    public static readonly SqlWindowFrameBoundary CurrentRow = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.CurrentRow,
        null );

    public static readonly SqlWindowFrameBoundary UnboundedPreceding = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.Preceding,
        null );

    public static readonly SqlWindowFrameBoundary UnboundedFollowing = new SqlWindowFrameBoundary(
        SqlWindowFrameBoundaryDirection.Following,
        null );

    private SqlWindowFrameBoundary(SqlWindowFrameBoundaryDirection direction, SqlExpressionNode? expression)
    {
        Direction = direction;
        Expression = expression;
    }

    public SqlWindowFrameBoundaryDirection Direction { get; }
    public SqlExpressionNode? Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlWindowFrameBoundary Preceding(SqlExpressionNode expression)
    {
        return new SqlWindowFrameBoundary( SqlWindowFrameBoundaryDirection.Preceding, expression );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlWindowFrameBoundary Following(SqlExpressionNode expression)
    {
        return new SqlWindowFrameBoundary( SqlWindowFrameBoundaryDirection.Following, expression );
    }
}
