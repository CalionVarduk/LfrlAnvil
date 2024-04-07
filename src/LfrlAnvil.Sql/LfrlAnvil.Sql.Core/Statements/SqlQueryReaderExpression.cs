using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlQueryReaderExpression(SqlDialect Dialect, Type RowType, LambdaExpression Expression);

public readonly struct SqlQueryReaderExpression<TRow>
    where TRow : notnull
{
    internal SqlQueryReaderExpression(SqlQueryReaderExpression @base)
    {
        Assume.Equals( @base.RowType, typeof( TRow ) );
        Dialect = @base.Dialect;
        Expression = ReinterpretCast.To<Expression<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>>>>( @base.Expression );
    }

    public SqlDialect Dialect { get; }
    public Expression<Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>>> Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReader<TRow> Compile()
    {
        return new SqlQueryReader<TRow>( Dialect, Expression.Compile() );
    }
}
