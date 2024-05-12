using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a builder of <see cref="ISqlColumnTypeDefinitionProvider"/> instances.
/// </summary>
public interface ISqlColumnTypeDefinitionProviderBuilder
{
    /// <summary>
    /// Specifies the SQL dialect of this builder.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Checks whether or not a column type definition for the provided <paramref name="type"/> has been registered.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns><b>true</b> when column type definition exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(Type type);

    /// <summary>
    /// Adds or updates the provided column type <paramref name="definition"/>
    /// by its <see cref="ISqlColumnTypeDefinition.RuntimeType"/> to this builder.
    /// </summary>
    /// <param name="definition">Definition to register.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="ArgumentException">When definition's dialect is not the same as this builder's dialect.</exception>
    ISqlColumnTypeDefinitionProviderBuilder Register(ISqlColumnTypeDefinition definition);

    /// <summary>
    /// Creates a new <see cref="ISqlColumnTypeDefinitionProvider"/> instance.
    /// </summary>
    /// <returns>New <see cref="ISqlColumnTypeDefinitionProvider"/> instance.</returns>
    [Pure]
    ISqlColumnTypeDefinitionProvider Build();
}
