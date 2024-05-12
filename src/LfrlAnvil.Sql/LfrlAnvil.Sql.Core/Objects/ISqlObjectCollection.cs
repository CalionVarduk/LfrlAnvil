using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents a collection of SQL schema objects.
/// </summary>
public interface ISqlObjectCollection : IReadOnlyCollection<ISqlObject>
{
    /// <summary>
    /// Schema that this collection belongs to.
    /// </summary>
    ISqlSchema Schema { get; }

    /// <summary>
    /// Checks whether or not an object with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when object exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns an object with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the object to return.</param>
    /// <returns>Existing object.</returns>
    /// <exception cref="KeyNotFoundException">When object does not exist.</exception>
    [Pure]
    ISqlObject Get(string name);

    /// <summary>
    /// Attempts to return an object with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the object to return.</param>
    /// <returns>Existing object or null when object does not exist.</returns>
    [Pure]
    ISqlObject? TryGet(string name);

    /// <summary>
    /// Returns a table with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the table to return.</param>
    /// <returns>Existing table.</returns>
    /// <exception cref="KeyNotFoundException">When table does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When object exists but is not a table.</exception>
    [Pure]
    ISqlTable GetTable(string name);

    /// <summary>
    /// Attempts to return a table with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the table to return.</param>
    /// <returns>Existing table or null when table does not exist.</returns>
    [Pure]
    ISqlTable? TryGetTable(string name);

    /// <summary>
    /// Returns an index with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the index to return.</param>
    /// <returns>Existing index.</returns>
    /// <exception cref="KeyNotFoundException">When index does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When object exists but is not an index.</exception>
    [Pure]
    ISqlIndex GetIndex(string name);

    /// <summary>
    /// Attempts to return an index with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the index to return.</param>
    /// <returns>Existing index or null when index does not exist.</returns>
    [Pure]
    ISqlIndex? TryGetIndex(string name);

    /// <summary>
    /// Returns a primary key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the primary key to return.</param>
    /// <returns>Existing primary key.</returns>
    /// <exception cref="KeyNotFoundException">When primary key does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When object exists but is not a primary key.</exception>
    [Pure]
    ISqlPrimaryKey GetPrimaryKey(string name);

    /// <summary>
    /// Attempts to return a primary key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the primary key to return.</param>
    /// <returns>Existing primary key or null when primary key does not exist.</returns>
    [Pure]
    ISqlPrimaryKey? TryGetPrimaryKey(string name);

    /// <summary>
    /// Returns a foreign key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the foreign key to return.</param>
    /// <returns>Existing foreign key.</returns>
    /// <exception cref="KeyNotFoundException">When foreign key does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When object exists but is not a foreign key.</exception>
    [Pure]
    ISqlForeignKey GetForeignKey(string name);

    /// <summary>
    /// Attempts to return a foreign key with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the foreign key to return.</param>
    /// <returns>Existing foreign key or null when foreign key does not exist.</returns>
    [Pure]
    ISqlForeignKey? TryGetForeignKey(string name);

    /// <summary>
    /// Returns a check with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the check to return.</param>
    /// <returns>Existing check.</returns>
    /// <exception cref="KeyNotFoundException">When check does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When object exists but is not a check.</exception>
    [Pure]
    ISqlCheck GetCheck(string name);

    /// <summary>
    /// Attempts to return a check with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the check to return.</param>
    /// <returns>Existing check or null when check does not exist.</returns>
    [Pure]
    ISqlCheck? TryGetCheck(string name);

    /// <summary>
    /// Returns a view with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the view to return.</param>
    /// <returns>Existing view.</returns>
    /// <exception cref="KeyNotFoundException">When view does not exist.</exception>
    /// <exception cref="SqlObjectCastException">When object exists but is not a view.</exception>
    [Pure]
    ISqlView GetView(string name);

    /// <summary>
    /// Attempts to return a view with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the view to return.</param>
    /// <returns>Existing view or null when view does not exist.</returns>
    [Pure]
    ISqlView? TryGetView(string name);
}
