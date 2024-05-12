using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of SQL schemas.
/// </summary>
public interface ISqlSchemaCollection : IReadOnlyCollection<ISqlSchema>
{
    /// <summary>
    /// Database that this collection belongs to.
    /// </summary>
    ISqlDatabase Database { get; }

    /// <summary>
    /// Default schema.
    /// </summary>
    ISqlSchema Default { get; }

    /// <summary>
    /// Checks whether or not a schema with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when schema exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a schema with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the schema to return.</param>
    /// <returns>Existing schema.</returns>
    /// <exception cref="KeyNotFoundException">When schema does not exist.</exception>
    [Pure]
    ISqlSchema Get(string name);

    /// <summary>
    /// Attempts to return a schema with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the schema to return.</param>
    /// <returns>Existing schema or null when schema does not exist.</returns>
    [Pure]
    ISqlSchema? TryGet(string name);
}
