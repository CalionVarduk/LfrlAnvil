using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a factory of delegates used by query reader expression instances.
/// </summary>
public interface ISqlQueryReaderFactory
{
    /// <summary>
    /// Specifies whether or not generic asynchronous query readers can be constructed by this factory.
    /// </summary>
    bool SupportsAsync { get; }

    /// <summary>
    /// SQL dialect that this factory is associated with.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Creates a new <see cref="SqlQueryReader"/> instance.
    /// </summary>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlQueryReader"/> instance.</returns>
    [Pure]
    SqlQueryReader Create(SqlQueryReaderCreationOptions? options = null);

    /// <summary>
    /// Creates a new <see cref="SqlAsyncQueryReader"/> instance.
    /// </summary>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlAsyncQueryReader"/> instance.</returns>
    [Pure]
    SqlAsyncQueryReader CreateAsync(SqlQueryReaderCreationOptions? options = null);

    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderExpression"/> instance.
    /// </summary>
    /// <param name="rowType">Row type.</param>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlQueryReaderExpression"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <paramref name="rowType"/> is not a valid row type
    /// or does not contain a valid constructor or does not contain any valid members.
    /// </exception>
    [Pure]
    SqlQueryReaderExpression CreateExpression(Type rowType, SqlQueryReaderCreationOptions? options = null);

    /// <summary>
    /// Creates a new <see cref="SqlAsyncQueryReaderExpression"/> instance.
    /// </summary>
    /// <param name="rowType">Row type.</param>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlAsyncQueryReaderExpression"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <paramref name="rowType"/> is not a valid row type
    /// or does not contain a valid constructor or does not contain any valid members
    /// or this factory does not support asynchronous expressions.
    /// </exception>
    [Pure]
    SqlAsyncQueryReaderExpression CreateAsyncExpression(Type rowType, SqlQueryReaderCreationOptions? options = null);

    /// <summary>
    /// Creates a new <see cref="SqlScalarQueryReader"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlScalarQueryReader"/> instance.</returns>
    [Pure]
    SqlScalarQueryReader CreateScalar();

    /// <summary>
    /// Creates a new <see cref="SqlAsyncScalarQueryReader"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlAsyncScalarQueryReader"/> instance.</returns>
    [Pure]
    SqlAsyncScalarQueryReader CreateAsyncScalar();

    /// <summary>
    /// Creates a new <see cref="SqlScalarQueryReaderExpression"/> instance.
    /// </summary>
    /// <param name="resultType">Value type.</param>
    /// <param name="isNullable">Specifies whether or not the result is nullable. Equal to <b>false</b> by default.</param>
    /// <returns>New <see cref="SqlScalarQueryReaderExpression"/> instance.</returns>
    /// <exception cref="SqlCompilerException">When <paramref name="resultType"/> is not a valid result type.</exception>
    [Pure]
    SqlScalarQueryReaderExpression CreateScalarExpression(Type resultType, bool isNullable = false);

    /// <summary>
    /// Creates a new <see cref="SqlAsyncScalarQueryReaderExpression"/> instance.
    /// </summary>
    /// <param name="resultType">Value type.</param>
    /// <param name="isNullable">Specifies whether or not the result is nullable. Equal to <b>false</b> by default.</param>
    /// <returns>New <see cref="SqlAsyncScalarQueryReaderExpression"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <paramref name="resultType"/> is not a valid result type
    /// or this factory does not support asynchronous expressions.
    /// </exception>
    [Pure]
    SqlAsyncScalarQueryReaderExpression CreateAsyncScalarExpression(Type resultType, bool isNullable = false);
}
