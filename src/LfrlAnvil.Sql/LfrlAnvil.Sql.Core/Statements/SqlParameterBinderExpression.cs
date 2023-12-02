using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlParameterBinderExpression(SqlDialect Dialect, Type SourceType, LambdaExpression Expression);

public readonly struct SqlParameterBinderExpression<TSource>
    where TSource : notnull
{
    internal SqlParameterBinderExpression(SqlParameterBinderExpression @base)
    {
        Assume.Equals( typeof( TSource ), @base.SourceType );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Action<IDbCommand, TSource>>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public Expression<Action<IDbCommand, TSource>> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlParameterBinder<TSource> Compile()
    {
        return new SqlParameterBinder<TSource>( Dialect, Expression.Compile() );
    }
}
