using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL column computation.
/// </summary>
/// <param name="Expression">Computation's expression.</param>
/// <param name="Storage">Computation's storage type.</param>
public readonly record struct SqlColumnComputation(SqlExpressionNode Expression, SqlColumnComputationStorage Storage)
{
    /// <summary>
    /// Creates a new <see cref="SqlColumnComputation"/> instance with <see cref="SqlColumnComputationStorage.Virtual"/> storage type.
    /// </summary>
    /// <param name="expression">Computation's expression.</param>
    /// <returns>New <see cref="SqlColumnComputation"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnComputation Virtual(SqlExpressionNode expression)
    {
        return new SqlColumnComputation( expression, SqlColumnComputationStorage.Virtual );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnComputation"/> instance with <see cref="SqlColumnComputationStorage.Stored"/> storage type.
    /// </summary>
    /// <param name="expression">Computation's expression.</param>
    /// <returns>New <see cref="SqlColumnComputation"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnComputation Stored(SqlExpressionNode expression)
    {
        return new SqlColumnComputation( expression, SqlColumnComputationStorage.Stored );
    }
}
