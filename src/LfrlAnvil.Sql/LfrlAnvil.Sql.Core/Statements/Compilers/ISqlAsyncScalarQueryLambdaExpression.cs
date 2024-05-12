using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a type-erased asynchronous scalar query lambda expression.
/// </summary>
public interface ISqlAsyncScalarQueryLambdaExpression
{
    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>Compiled <see cref="Delegate"/>.</returns>
    [Pure]
    Delegate Compile();
}

/// <summary>
/// Represents a generic asynchronous scalar query lambda expression.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface ISqlAsyncScalarQueryLambdaExpression<T> : ISqlAsyncScalarQueryLambdaExpression
{
    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>Compiled <see cref="Delegate"/>.</returns>
    [Pure]
    new Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<T>>> Compile();
}
