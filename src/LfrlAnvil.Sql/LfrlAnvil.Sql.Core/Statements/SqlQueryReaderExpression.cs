using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased prepared query reader expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="RowType">Row type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlQueryReaderExpression(SqlDialect Dialect, Type RowType, LambdaExpression Expression);

/// <summary>
/// Represents a generic prepared query reader expression.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly struct SqlQueryReaderExpression<TRow>
    where TRow : notnull
{
    internal SqlQueryReaderExpression(SqlQueryReaderExpression @base)
    {
        Assume.Equals( @base.RowType, typeof( TRow ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>>>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public Expression<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>>> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlQueryReader{TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReader<TRow> Compile()
    {
        return new SqlQueryReader<TRow>( Dialect, Expression.Compile() );
    }
}
