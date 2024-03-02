﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncQueryReaderExpression(SqlDialect Dialect, Type RowType, ISqlAsyncQueryLambdaExpression Expression);

public readonly struct SqlAsyncQueryReaderExpression<TRow>
    where TRow : notnull
{
    internal SqlAsyncQueryReaderExpression(SqlAsyncQueryReaderExpression @base)
    {
        Assume.Equals( @base.RowType, typeof( TRow ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<ISqlAsyncQueryLambdaExpression<TRow>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public ISqlAsyncQueryLambdaExpression<TRow> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlAsyncQueryReader<TRow> Compile()
    {
        return new SqlAsyncQueryReader<TRow>( Dialect, Expression.Compile() );
    }
}
