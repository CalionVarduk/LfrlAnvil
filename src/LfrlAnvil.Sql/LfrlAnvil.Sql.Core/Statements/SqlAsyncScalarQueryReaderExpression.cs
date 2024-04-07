using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncScalarQueryReaderExpression(
    SqlDialect Dialect,
    Type ResultType,
    ISqlAsyncScalarQueryLambdaExpression Expression
);

public readonly struct SqlAsyncScalarQueryReaderExpression<T>
{
    internal SqlAsyncScalarQueryReaderExpression(SqlAsyncScalarQueryReaderExpression @base)
    {
        Assume.Equals( @base.ResultType, typeof( T ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<ISqlAsyncScalarQueryLambdaExpression<T>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public ISqlAsyncScalarQueryLambdaExpression<T> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlAsyncScalarQueryReader<T> Compile()
    {
        return new SqlAsyncScalarQueryReader<T>( Dialect, Expression.Compile() );
    }
}
