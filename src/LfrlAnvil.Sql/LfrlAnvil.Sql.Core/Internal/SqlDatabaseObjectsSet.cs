using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a set of <see cref="SqlObjectBuilder"/> instances.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlDatabaseObjectsSet<T>
    where T : SqlObjectBuilder
{
    private readonly Dictionary<ulong, T> _map;

    private SqlDatabaseObjectsSet(Dictionary<ulong, T> map)
    {
        _map = map;
    }

    /// <summary>
    /// Number of elements in this set.
    /// </summary>
    public int Count => _map.Count;

    /// <summary>
    /// Creates a new empty <see cref="SqlDatabaseObjectsSet{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlDatabaseObjectsSet{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseObjectsSet<T> Create()
    {
        return new SqlDatabaseObjectsSet<T>( new Dictionary<ulong, T>() );
    }

    /// <summary>
    /// Attempts to add the provided <paramref name="obj"/> to this set.
    /// </summary>
    /// <param name="obj">Object to add.</param>
    /// <returns><b>true</b> when object was added, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(T obj)
    {
        return _map.TryAdd( obj.Id, obj );
    }

    /// <summary>
    /// Attempts to remove the provided <paramref name="obj"/> from this set.
    /// </summary>
    /// <param name="obj">Object to remove.</param>
    /// <returns><b>true</b> when object was removed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Remove(T obj)
    {
        return _map.Remove( obj.Id );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="obj"/> exists in this set.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns><b>true</b> when object exists, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(T obj)
    {
        return _map.ContainsKey( obj.Id );
    }

    /// <summary>
    /// Removes all objects from this set.
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Clear()
    {
        _map.Clear();
    }

    /// <summary>
    /// Creates a new enumerator for this set.
    /// </summary>
    /// <returns>New enumerator.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Dictionary<ulong, T>.ValueCollection.Enumerator GetEnumerator()
    {
        return _map.Values.GetEnumerator();
    }
}
