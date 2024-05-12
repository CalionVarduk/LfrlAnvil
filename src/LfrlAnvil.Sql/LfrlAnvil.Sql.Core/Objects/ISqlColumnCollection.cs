using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of SQL table columns.
/// </summary>
public interface ISqlColumnCollection : IReadOnlyCollection<ISqlColumn>
{
    /// <summary>
    /// Table that this collection belongs to.
    /// </summary>
    ISqlTable Table { get; }

    /// <summary>
    /// Checks whether or not a column with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when column exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to return.</param>
    /// <returns>Existing column.</returns>
    /// <exception cref="KeyNotFoundException">When column does not exist.</exception>
    [Pure]
    ISqlColumn Get(string name);

    /// <summary>
    /// Attempts to return a column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column to return.</param>
    /// <returns>Existing column or null when column does not exist.</returns>
    [Pure]
    ISqlColumn? TryGet(string name);
}
