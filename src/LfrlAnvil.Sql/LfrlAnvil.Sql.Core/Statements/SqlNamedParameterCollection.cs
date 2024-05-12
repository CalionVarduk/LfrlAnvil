using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a collection of type-erased bindable named SQL parameters.
/// </summary>
public sealed class SqlNamedParameterCollection : IReadOnlyCollection<SqlParameter>
{
    private readonly Dictionary<string, SqlParameter> _map;

    /// <summary>
    /// Creates a new empty <see cref="SqlNamedParameterCollection"/> instance.
    /// </summary>
    /// <param name="capacity">Initial capacity.</param>
    /// <param name="comparer">Optional parameter name equality comparer.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="capacity"/> is less than <b>0</b>.</exception>
    public SqlNamedParameterCollection(int capacity = 0, IEqualityComparer<string>? comparer = null)
    {
        _map = new Dictionary<string, SqlParameter>( capacity, comparer ?? SqlHelpers.NameComparer );
    }

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <summary>
    /// Checks whether or not a parameter with the specified <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when parameter exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(string name)
    {
        return _map.ContainsKey( name );
    }

    /// <summary>
    /// Attempts to return an <see cref="SqlParameter"/> instance associated with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the parameter to get.</param>
    /// <returns><see cref="SqlParameter"/> instance or null when parameter does not exist.</returns>
    [Pure]
    public SqlParameter? TryGet(string name)
    {
        return _map.TryGetValue( name, out var result ) ? result : null;
    }

    /// <summary>
    /// Attempts to add a new parameter to this collection.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns><b>true</b> when parameter was added, otherwise <b>false</b>.</returns>
    public bool TryAdd(string name, object? value)
    {
        return _map.TryAdd( name, SqlParameter.Named( name, value ) );
    }

    /// <summary>
    /// Adds a new parameter to this collection or updates the value of an existing parameter.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    public void AddOrUpdate(string name, object? value)
    {
        _map[name] = SqlParameter.Named( name, value );
    }

    /// <summary>
    /// Removes all parameters from this collection.
    /// </summary>
    public void Clear()
    {
        _map.Clear();
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<SqlParameter> GetEnumerator()
    {
        return _map.Values.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
