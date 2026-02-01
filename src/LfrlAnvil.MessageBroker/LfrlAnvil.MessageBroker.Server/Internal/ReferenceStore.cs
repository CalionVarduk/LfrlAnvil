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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct ReferenceStore<TKey, T>
    where TKey : notnull
    where T : class
{
    private readonly Dictionary<TKey, T> _map;
    private T[]? _cache;

    private ReferenceStore(IEqualityComparer<TKey>? keyComparer)
    {
        _map = new Dictionary<TKey, T>( keyComparer );
        _cache = null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ReferenceStore<TKey, T> Create(IEqualityComparer<TKey>? keyComparer = null)
    {
        return new ReferenceStore<TKey, T>( keyComparer );
    }

    internal int Count => _map.Count;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlyArray<T> GetAll()
    {
        _cache ??= ToArray();
        return _cache;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryGet(TKey key, [MaybeNullWhen( false )] out T obj)
    {
        return _map.TryGetValue( key, out obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal GetOrAddToken GetOrAddNull(TKey key)
    {
        ref var @ref = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        return new GetOrAddToken( ref @ref, exists );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Add(TKey key, T obj)
    {
        try
        {
            _map.Add( key, obj );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryAdd(TKey key, T obj)
    {
        try
        {
            return _map.TryAdd( key, obj );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool Remove(TKey key)
    {
        try
        {
            return _map.Remove( key );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool Remove(TKey key, [MaybeNullWhen( false )] out T removed)
    {
        try
        {
            return _map.Remove( key, out removed );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _map.Clear();
        _cache = null;
    }

    internal readonly ref struct GetOrAddToken
    {
        private readonly ref T _ref;

        internal GetOrAddToken(ref T @ref, bool exists)
        {
            _ref = ref @ref;
            Exists = exists;
        }

        internal readonly bool Exists;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T GetObject()
        {
            Assume.True( Exists );
            return _ref;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T SetObject(ref ReferenceStore<TKey, T> store, T obj)
        {
            Assume.False( Exists );
            _ref = obj;
            store._cache = null;
            return obj;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Revert(ref ReferenceStore<TKey, T> store, TKey key)
        {
            Assume.False( Exists );
            store._map.Remove( key );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T[] ToArray()
    {
        if ( _map.Count == 0 )
            return Array.Empty<T>();

        var i = 0;
        var result = new T[_map.Count];
        foreach ( var obj in _map.Values )
            result[i++] = obj;

        return result;
    }
}
