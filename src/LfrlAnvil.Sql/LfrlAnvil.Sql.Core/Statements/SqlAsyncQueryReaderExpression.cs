using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased prepared asynchronous query reader expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="RowType">Row type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlAsyncQueryReaderExpression(SqlDialect Dialect, Type RowType, ISqlAsyncQueryLambdaExpression Expression);

/// <summary>
/// Represents a generic prepared asynchronous query reader expression.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly struct SqlAsyncQueryReaderExpression<TRow>
    where TRow : notnull
{
    internal SqlAsyncQueryReaderExpression(SqlAsyncQueryReaderExpression @base)
    {
        Assume.Equals( @base.RowType, typeof( TRow ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<ISqlAsyncQueryLambdaExpression<TRow>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public ISqlAsyncQueryLambdaExpression<TRow> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlAsyncQueryReader{TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlAsyncQueryReader<TRow> Compile()
    {
        return new SqlAsyncQueryReader<TRow>( Dialect, Expression.Compile() );
    }
}
