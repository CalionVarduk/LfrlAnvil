using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlScalarQueryReaderExpression(SqlDialect Dialect, Type ResultType, LambdaExpression Expression);

public readonly struct SqlScalarQueryReaderExpression<T>
{
    internal SqlScalarQueryReaderExpression(SqlScalarQueryReaderExpression @base)
    {
        Assume.Equals( @base.ResultType, typeof( T ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Func<IDataReader, SqlScalarQueryResult<T>>>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public Expression<Func<IDataReader, SqlScalarQueryResult<T>>> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryReader<T> Compile()
    {
        return new SqlScalarQueryReader<T>( Dialect, Expression.Compile() );
    }
}
