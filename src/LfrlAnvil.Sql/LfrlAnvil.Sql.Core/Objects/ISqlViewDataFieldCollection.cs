using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of SQL view data fields.
/// </summary>
public interface ISqlViewDataFieldCollection : IReadOnlyCollection<ISqlViewDataField>
{
    /// <summary>
    /// View that this collection belongs to.
    /// </summary>
    ISqlView View { get; }

    /// <summary>
    /// Checks whether or not a data field with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when data field exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a data field with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the data field to return.</param>
    /// <returns>Existing data field.</returns>
    /// <exception cref="KeyNotFoundException">When data field does not exist.</exception>
    [Pure]
    ISqlViewDataField Get(string name);

    /// <summary>
    /// Attempts to return a data field with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the data field to return.</param>
    /// <returns>Existing data field or null when data field does not exist.</returns>
    [Pure]
    ISqlViewDataField? TryGet(string name);
}
