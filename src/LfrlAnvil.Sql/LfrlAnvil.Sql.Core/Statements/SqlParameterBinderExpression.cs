using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased prepared parameter binder expression.
/// </summary>
/// <param name="Dialect">SQL dialect that this expression is associated with.</param>
/// <param name="SourceType">Parameter source type.</param>
/// <param name="Expression">Underlying compilable expression.</param>
public readonly record struct SqlParameterBinderExpression(SqlDialect Dialect, Type SourceType, LambdaExpression Expression);

/// <summary>
/// Represents a generic prepared parameter binder expression.
/// </summary>
/// <typeparam name="TSource">Parameter source type.</typeparam>
public readonly struct SqlParameterBinderExpression<TSource>
    where TSource : notnull
{
    internal SqlParameterBinderExpression(SqlParameterBinderExpression @base)
    {
        Assume.Equals( typeof( TSource ), @base.SourceType );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Action<IDbCommand, TSource>>>( @base.Expression );
    }

    /// <summary>
    /// SQL dialect that this expression is associated with.
    /// </summary>
    public SqlDialect Dialect { get; }

    /// <summary>
    /// Underlying compilable expression.
    /// </summary>
    public Expression<Action<IDbCommand, TSource>> Expression { get; }

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="SqlParameterBinder{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlParameterBinder<TSource> Compile()
    {
        return new SqlParameterBinder<TSource>( Dialect, Expression.Compile() );
    }
}
