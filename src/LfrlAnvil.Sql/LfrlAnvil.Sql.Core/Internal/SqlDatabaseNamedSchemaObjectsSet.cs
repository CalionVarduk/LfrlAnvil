using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a set of named <see cref="SqlObjectBuilder"/> instances that may belong to an SQL schema.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlDatabaseNamedSchemaObjectsSet<T>
    where T : SqlObjectBuilder
{
    private readonly Dictionary<SqlSchemaObjectName, T> _map;

    private SqlDatabaseNamedSchemaObjectsSet(Dictionary<SqlSchemaObjectName, T> map)
    {
        _map = map;
    }

    /// <summary>
    /// Number of elements in this set.
    /// </summary>
    public int Count => _map.Count;

    /// <summary>
    /// Creates a new empty <see cref="SqlDatabaseNamedSchemaObjectsSet{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlDatabaseNamedSchemaObjectsSet{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseNamedSchemaObjectsSet<T> Create()
    {
        return new SqlDatabaseNamedSchemaObjectsSet<T>( new Dictionary<SqlSchemaObjectName, T>() );
    }

    /// <summary>
    /// Attempts to add the provided <paramref name="obj"/> to this set.
    /// </summary>
    /// <param name="name">Name of the object.</param>
    /// <param name="obj">Object to add.</param>
    /// <returns><b>true</b> when object was added, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(SqlSchemaObjectName name, T obj)
    {
        return _map.TryAdd( name, obj );
    }

    /// <summary>
    /// Attempts to remove an object by its <paramref name="name"/> from this set.
    /// </summary>
    /// <param name="name">Name of the object to remove.</param>
    /// <returns>Removed object or null when it does not exist.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? Remove(SqlSchemaObjectName name)
    {
        return _map.Remove( name, out var removed ) ? removed : null;
    }

    /// <summary>
    /// Attempts to retrieve an object associated with the provided <paramref name="name"/> from this set.
    /// </summary>
    /// <param name="name">Name of the object to retrieve.</param>
    /// <returns>Existing object or null when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? TryGetObject(SqlSchemaObjectName name)
    {
        return _map.GetValueOrDefault( name );
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
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _map );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="SqlDatabaseNamedSchemaObjectsSet{T}"/>.
    /// </summary>
    public struct Enumerator
    {
        private Dictionary<SqlSchemaObjectName, T>.Enumerator _base;

        internal Enumerator(Dictionary<SqlSchemaObjectName, T> map)
        {
            _base = map.GetEnumerator();
        }

        /// <summary>
        /// Gets the element at the current position of this enumerator.
        /// </summary>
        public SqlNamedSchemaObject<T> Current
        {
            get
            {
                var current = _base.Current;
                return new SqlNamedSchemaObject<T>( current.Key, current.Value );
            }
        }

        /// <summary>
        /// Advances this enumerator to the next element.
        /// </summary>
        /// <returns><b>true</b> when next element exists, otherwise <b>false</b>.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _base.MoveNext();
        }
    }
}
