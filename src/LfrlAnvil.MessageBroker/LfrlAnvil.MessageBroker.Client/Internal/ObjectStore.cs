// Copyright 2025-2026 Łukasz Furlepa
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct ObjectStore<T>
    where T : class
{
    private readonly Dictionary<int, T> _byId;
    private readonly Dictionary<string, T> _byName;
    private T[]? _cache;

    private ObjectStore(StringComparer nameComparer)
    {
        _byId = new Dictionary<int, T>();
        _byName = new Dictionary<string, T>( nameComparer );
        _cache = null;
    }

    internal int Count => _byId.Count;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ObjectStore<T> Create(StringComparer nameComparer)
    {
        return new ObjectStore<T>( nameComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlyArray<T> GetAll()
    {
        _cache ??= ToArray();
        return _cache;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T? TryGetById(int id)
    {
        return _byId.TryGetValue( id, out var obj ) ? obj : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T? TryGetByName(string name)
    {
        return _byName.TryGetValue( name, out var obj ) ? obj : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(int id, string name, T obj)
    {
        try
        {
            _byId.Add( id, obj );
            _byName.Add( name, obj );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(int id, string name)
    {
        try
        {
            _byId.Remove( id );
            _byName.Remove( name );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T? TryRemoveByName(string name)
    {
        return _byName.Remove( name, out var obj ) ? obj : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveById(int id)
    {
        try
        {
            _byId.Remove( id );
        }
        finally
        {
            _cache = null;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Dictionary<int, T>.ValueCollection.Enumerator GetEnumerator()
    {
        return _byId.Values.GetEnumerator();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _cache = null;
        _byId.Clear();
        _byName.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T[] ClearAndExtract()
    {
        var result = _cache;
        _cache = null;
        _byName.Clear();
        result ??= ToArray();
        _byId.Clear();
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T[] ToArray()
    {
        if ( _byId.Count == 0 )
            return Array.Empty<T>();

        var i = 0;
        var result = new T[_byId.Count];
        foreach ( var obj in _byId.Values )
            result[i++] = obj;

        return result;
    }
}
