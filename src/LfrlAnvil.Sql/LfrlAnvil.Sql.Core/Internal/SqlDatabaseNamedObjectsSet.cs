// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a set of named <see cref="SqlObjectBuilder"/> instances.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public readonly struct SqlDatabaseNamedObjectsSet<T>
    where T : SqlObjectBuilder
{
    private readonly Dictionary<string, T> _map;

    private SqlDatabaseNamedObjectsSet(Dictionary<string, T> map)
    {
        _map = map;
    }

    /// <summary>
    /// Number of elements in this set.
    /// </summary>
    public int Count => _map.Count;

    /// <summary>
    /// Creates a new empty <see cref="SqlDatabaseNamedObjectsSet{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlDatabaseNamedObjectsSet{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseNamedObjectsSet<T> Create()
    {
        return new SqlDatabaseNamedObjectsSet<T>( new Dictionary<string, T>( SqlHelpers.NameComparer ) );
    }

    /// <summary>
    /// Attempts to add the provided <paramref name="obj"/> to this set.
    /// </summary>
    /// <param name="name">Name of the object.</param>
    /// <param name="obj">Object to add.</param>
    /// <returns><b>true</b> when object was added, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Add(string name, T obj)
    {
        return _map.TryAdd( name, obj );
    }

    /// <summary>
    /// Attempts to remove an object by its <paramref name="name"/> from this set.
    /// </summary>
    /// <param name="name">Name of the object to remove.</param>
    /// <returns>Removed object or null when it does not exist.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? Remove(string name)
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
    public T? TryGetObject(string name)
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
    /// Lightweight enumerator implementation for <see cref="SqlDatabaseNamedObjectsSet{T}"/>.
    /// </summary>
    public struct Enumerator
    {
        private Dictionary<string, T>.Enumerator _base;

        internal Enumerator(Dictionary<string, T> map)
        {
            _base = map.GetEnumerator();
        }

        /// <summary>
        /// Gets the element at the current position of this enumerator.
        /// </summary>
        public SqlNamedObject<T> Current
        {
            get
            {
                var current = _base.Current;
                return new SqlNamedObject<T>( current.Key, current.Value );
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
