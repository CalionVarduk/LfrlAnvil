using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a type-erased asynchronous query lambda expression.
/// </summary>
public interface ISqlAsyncQueryLambdaExpression
{
    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>Compiled <see cref="Delegate"/>.</returns>
    [Pure]
    Delegate Compile();
}

/// <summary>
/// Represents a generic asynchronous query lambda expression.
/// </summary>
/// <typeparam name="TRow">Row type.</typeparam>
public interface ISqlAsyncQueryLambdaExpression<TRow> : ISqlAsyncQueryLambdaExpression
    where TRow : notnull
{
    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>Compiled <see cref="Delegate"/>.</returns>
    [Pure]
    new Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<TRow>>> Compile();
}
