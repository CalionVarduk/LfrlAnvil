using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a generic asynchronous query lambda expression.
/// </summary>
/// <typeparam name="TDataReader">DB data reader type.</typeparam>
/// <typeparam name="TRow">Row type.</typeparam>
public sealed class SqlAsyncQueryLambdaExpression<TDataReader, TRow> : ISqlAsyncQueryLambdaExpression<TRow>
    where TDataReader : DbDataReader
    where TRow : notnull
{
    private readonly bool _populatesFieldTypes;

    private SqlAsyncQueryLambdaExpression(
        Expression<Func<TDataReader, SqlAsyncQueryReaderInitResult>> initExpression,
        LambdaExpression readRowExpression,
        bool populatesFieldTypes)
    {
        InitExpression = initExpression;
        ReadRowExpression = readRowExpression;
        _populatesFieldTypes = populatesFieldTypes;
    }

    /// <summary>
    /// Underlying expression that creates an <see cref="SqlAsyncQueryReaderInitResult"/> instance.
    /// </summary>
    public Expression<Func<TDataReader, SqlAsyncQueryReaderInitResult>> InitExpression { get; }

    /// <summary>
    /// Underlying expression that reads and returns a single row.
    /// </summary>
    public LambdaExpression ReadRowExpression { get; }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncQueryLambdaExpression{TDataReader,TRow}"/> instance that does not extract field types.
    /// </summary>
    /// <param name="initExpression">Underlying expression that creates an <see cref="SqlAsyncQueryReaderInitResult"/> instance.</param>
    /// <param name="readRowExpression">Underlying expression that reads and returns a single row.</param>
    /// <returns>New <see cref="SqlAsyncQueryLambdaExpression{TDataReader,TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryLambdaExpression<TDataReader, TRow> Create(
        Expression<Func<TDataReader, SqlAsyncQueryReaderInitResult>> initExpression,
        Expression<Func<TDataReader, int[], TRow>> readRowExpression)
    {
        return new SqlAsyncQueryLambdaExpression<TDataReader, TRow>( initExpression, readRowExpression, populatesFieldTypes: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncQueryLambdaExpression{TDataReader,TRow}"/> instance that extracts field types.
    /// </summary>
    /// <param name="initExpression">Underlying expression that creates an <see cref="SqlAsyncQueryReaderInitResult"/> instance.</param>
    /// <param name="readRowExpression">Underlying expression that reads and returns a single row.</param>
    /// <returns>New <see cref="SqlAsyncQueryLambdaExpression{TDataReader,TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryLambdaExpression<TDataReader, TRow> Create(
        Expression<Func<TDataReader, SqlAsyncQueryReaderInitResult>> initExpression,
        Expression<Func<TDataReader, int[], SqlResultSetField[], TRow>> readRowExpression)
    {
        return new SqlAsyncQueryLambdaExpression<TDataReader, TRow>( initExpression, readRowExpression, populatesFieldTypes: true );
    }

    /// <inheritdoc />
    [Pure]
    public Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<TRow>>> Compile()
    {
        var initDelegate = InitExpression.Compile();

        if ( _populatesFieldTypes )
        {
            var readRowDelegate = ReinterpretCast.To<Expression<Func<TDataReader, int[], SqlResultSetField[], TRow>>>( ReadRowExpression )
                .Compile();

            return async (reader, options, cancellationToken) =>
            {
                var concreteReader = ( TDataReader )reader;
                if ( ! await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                    return SqlQueryResult<TRow>.Empty;

                var (ordinals, resultSetFields) = initDelegate( concreteReader );
                Assume.IsNotNull( resultSetFields );
                var rows = options.CreateList<TRow>();

                do
                {
                    rows.Add( readRowDelegate( concreteReader, ordinals, resultSetFields ) );
                }
                while ( await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) );

                return new SqlQueryResult<TRow>( resultSetFields, rows );
            };
        }
        else
        {
            var readRowDelegate = ReinterpretCast.To<Expression<Func<TDataReader, int[], TRow>>>( ReadRowExpression )
                .Compile();

            return async (reader, options, cancellationToken) =>
            {
                var concreteReader = ( TDataReader )reader;
                if ( ! await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) )
                    return SqlQueryResult<TRow>.Empty;

                var (ordinals, resultSetFields) = initDelegate( concreteReader );
                var rows = options.CreateList<TRow>();

                do
                {
                    rows.Add( readRowDelegate( concreteReader, ordinals ) );
                }
                while ( await concreteReader.ReadAsync( cancellationToken ).ConfigureAwait( false ) );

                return new SqlQueryResult<TRow>( resultSetFields, rows );
            };
        }
    }

    [Pure]
    Delegate ISqlAsyncQueryLambdaExpression.Compile()
    {
        return Compile();
    }
}
