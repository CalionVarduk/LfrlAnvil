using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

public sealed class SqlAsyncScalarQueryLambdaExpression<TDataReader, T> : ISqlAsyncScalarQueryLambdaExpression<T>
    where TDataReader : DbDataReader
{
    private SqlAsyncScalarQueryLambdaExpression(Expression<Func<TDataReader, SqlScalarQueryResult<T>>> readResultExpression)
    {
        ReadResultExpression = readResultExpression;
    }

    public Expression<Func<TDataReader, SqlScalarQueryResult<T>>> ReadResultExpression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarQueryLambdaExpression<TDataReader, T> Create(
        Expression<Func<TDataReader, SqlScalarQueryResult<T>>> readRowExpression)
    {
        return new SqlAsyncScalarQueryLambdaExpression<TDataReader, T>( readRowExpression );
    }

    [Pure]
    public Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<T>>> Compile()
    {
        var readRowDelegate = ReadResultExpression.Compile();

        return async (reader, cancellationToken) =>
        {
            var concreteReader = (TDataReader)reader;
            return await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false )
                ? readRowDelegate( concreteReader )
                : SqlScalarQueryResult<T>.Empty;
        };
    }

    [Pure]
    Delegate ISqlAsyncScalarQueryLambdaExpression.Compile()
    {
        return Compile();
    }
}
