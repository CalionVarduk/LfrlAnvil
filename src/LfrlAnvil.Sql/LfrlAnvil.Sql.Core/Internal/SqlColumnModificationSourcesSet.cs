using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a set of <see cref="SqlColumnModificationSource{T}"/> instances
/// identified by <see cref="SqlColumnModificationSource{T}.Column"/>.
/// </summary>
/// <typeparam name="T">SQL column builder type.</typeparam>
public readonly struct SqlColumnModificationSourcesSet<T>
    where T : SqlColumnBuilder
{
    private readonly Dictionary<ulong, SqlColumnModificationSource<T>> _map;

    private SqlColumnModificationSourcesSet(Dictionary<ulong, SqlColumnModificationSource<T>> map)
    {
        _map = map;
    }

    /// <summary>
    /// Number of elements in this set.
    /// </summary>
    public int Count => _map.Count;

    /// <summary>
    /// Creates a new empty <see cref="SqlColumnModificationSourcesSet{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlColumnModificationSourcesSet{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnModificationSourcesSet<T> Create()
    {
        return new SqlColumnModificationSourcesSet<T>( new Dictionary<ulong, SqlColumnModificationSource<T>>() );
    }

    /// <summary>
    /// Attempts to add the provided <paramref name="source"/> to this set.
    /// </summary>
    /// <param name="source">Modification source to add.</param>
    /// <returns><b>true</b> when modification source was added, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(SqlColumnModificationSource<T> source)
    {
        return _map.TryAdd( source.Column.Id, source );
    }

    /// <summary>
    /// Attempts to add the provided <paramref name="column"/> as self modification to this set.
    /// </summary>
    /// <param name="column">Column to add.</param>
    /// <returns><b>true</b> when column was added, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(T column)
    {
        return Add( SqlColumnModificationSource<T>.Self( column ) );
    }

    /// <summary>
    /// Attempts to remove a modification source by its <see cref="SqlColumnModificationSource{T}.Column"/> from this set.
    /// </summary>
    /// <param name="column">Source column to remove.</param>
    /// <returns>Removed modification source or null when it does not exist.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlColumnModificationSource<T>? Remove(T column)
    {
        return _map.Remove( column.Id, out var removed ) ? removed : null;
    }

    /// <summary>
    /// Attempts to retrieve a modification source by its <see cref="SqlColumnModificationSource{T}.Column"/> from this set.
    /// </summary>
    /// <param name="column">Source column to retrieve.</param>
    /// <returns>Existing modification source or null when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlColumnModificationSource<T>? TryGetSource(T column)
    {
        return _map.TryGetValue( column.Id, out var value ) ? value : null;
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
    /// Lightweight enumerator implementation for <see cref="SqlColumnModificationSourcesSet{T}"/>.
    /// </summary>
    public struct Enumerator
    {
        private Dictionary<ulong, SqlColumnModificationSource<T>>.ValueCollection.Enumerator _base;

        internal Enumerator(Dictionary<ulong, SqlColumnModificationSource<T>> map)
        {
            _base = map.Values.GetEnumerator();
        }

        /// <summary>
        /// Gets the element at the current position of this enumerator.
        /// </summary>
        public SqlColumnModificationSource<T> Current => _base.Current;

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
