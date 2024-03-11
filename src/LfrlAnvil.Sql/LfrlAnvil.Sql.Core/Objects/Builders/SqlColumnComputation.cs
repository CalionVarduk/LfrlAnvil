using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly record struct SqlColumnComputation(SqlExpressionNode Expression, SqlColumnComputationStorage Storage)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnComputation Virtual(SqlExpressionNode expression)
    {
        return new SqlColumnComputation( expression, SqlColumnComputationStorage.Virtual );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnComputation Stored(SqlExpressionNode expression)
    {
        return new SqlColumnComputation( expression, SqlColumnComputationStorage.Stored );
    }
}
