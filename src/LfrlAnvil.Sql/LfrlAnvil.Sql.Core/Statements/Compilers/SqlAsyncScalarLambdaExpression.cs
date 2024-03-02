using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

public sealed class SqlAsyncScalarLambdaExpression<TDataReader, T> : ISqlAsyncScalarLambdaExpression<T>
    where TDataReader : DbDataReader
{
    private SqlAsyncScalarLambdaExpression(Expression<Func<TDataReader, SqlScalarResult<T>>> readResultExpression)
    {
        ReadResultExpression = readResultExpression;
    }

    public Expression<Func<TDataReader, SqlScalarResult<T>>> ReadResultExpression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarLambdaExpression<TDataReader, T> Create(Expression<Func<TDataReader, SqlScalarResult<T>>> readRowExpression)
    {
        return new SqlAsyncScalarLambdaExpression<TDataReader, T>( readRowExpression );
    }

    [Pure]
    public Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult<T>>> Compile()
    {
        var readRowDelegate = ReadResultExpression.Compile();

        return async (reader, cancellationToken) =>
        {
            var concreteReader = (TDataReader)reader;
            return await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false )
                ? readRowDelegate( concreteReader )
                : SqlScalarResult<T>.Empty;
        };
    }

    [Pure]
    Delegate ISqlAsyncScalarLambdaExpression.Compile()
    {
        return Compile();
    }
}
