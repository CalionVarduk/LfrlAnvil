using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncScalarReaderExpression(
    SqlDialect Dialect,
    Type ResultType,
    ISqlAsyncScalarLambdaExpression Expression);

public readonly struct SqlAsyncScalarReaderExpression<T>
{
    internal SqlAsyncScalarReaderExpression(SqlAsyncScalarReaderExpression @base)
    {
        Assume.Equals( @base.ResultType, typeof( T ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<ISqlAsyncScalarLambdaExpression<T>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public ISqlAsyncScalarLambdaExpression<T> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlAsyncScalarReader<T> Compile()
    {
        return new SqlAsyncScalarReader<T>( Dialect, Expression.Compile() );
    }
}
