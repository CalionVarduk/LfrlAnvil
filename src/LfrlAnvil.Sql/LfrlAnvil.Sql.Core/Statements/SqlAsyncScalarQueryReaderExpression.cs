using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased prepared asynchronous scalar query reader expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="ResultType">Value type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlAsyncScalarQueryReaderExpression(
    SqlDialect Dialect,
    Type ResultType,
    ISqlAsyncScalarQueryLambdaExpression Expression
);

/// <summary>
/// Represents a generic prepared asynchronous scalar query reader expression.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct SqlAsyncScalarQueryReaderExpression<T>
{
    internal SqlAsyncScalarQueryReaderExpression(SqlAsyncScalarQueryReaderExpression @base)
    {
        Assume.Equals( @base.ResultType, typeof( T ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<ISqlAsyncScalarQueryLambdaExpression<T>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public ISqlAsyncScalarQueryLambdaExpression<T> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlAsyncScalarQueryReader{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlAsyncScalarQueryReader<T> Compile()
    {
        return new SqlAsyncScalarQueryReader<T>( Dialect, Expression.Compile() );
    }
}
