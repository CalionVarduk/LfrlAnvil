using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlScalarReaderExpression(SqlDialect Dialect, Type ResultType, LambdaExpression Expression);

public readonly struct SqlScalarReaderExpression<T>
{
    internal SqlScalarReaderExpression(SqlScalarReaderExpression @base)
    {
        Assume.Equals( @base.ResultType, typeof( T ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Func<IDataReader, SqlScalarResult<T>>>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public Expression<Func<IDataReader, SqlScalarResult<T>>> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarReader<T> Compile()
    {
        return new SqlScalarReader<T>( Dialect, Expression.Compile() );
    }
}
