using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Represents a factory of delegates used by <see cref="SqlParameterBinderExpression"/> instances.
/// </summary>
public interface ISqlParameterBinderFactory
{
    /// <summary>
    /// SQL dialect that this factory is associated with.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Specifies whether or not this factory supports positional parameters.
    /// </summary>
    bool SupportsPositionalParameters { get; }

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinder"/> instance.
    /// </summary>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlParameterBinder"/> instance.</returns>
    [Pure]
    SqlParameterBinder Create(SqlParameterBinderCreationOptions? options = null);

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderExpression"/> instance.
    /// </summary>
    /// <param name="sourceType">Parameter source type.</param>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <returns>New <see cref="SqlParameterBinderExpression"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <paramref name="sourceType"/> is not a valid parameter source type or does not contain any valid members.
    /// </exception>
    [Pure]
    SqlParameterBinderExpression CreateExpression(Type sourceType, SqlParameterBinderCreationOptions? options = null);
}
