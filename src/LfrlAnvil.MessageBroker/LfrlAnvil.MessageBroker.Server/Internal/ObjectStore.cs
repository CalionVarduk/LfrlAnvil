// Copyright 2025 Łukasz Furlepa
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
using System.Runtime.InteropServices;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct ObjectStore<T>
    where T : class
{
    private readonly Dictionary<string, T> _byName;
    private SparseListSlim<T> _byId;
    private T[]? _cache;

    private ObjectStore(StringComparer nameComparer)
    {
        _byName = new Dictionary<string, T>( nameComparer );
        _byId = SparseListSlim<T>.Create();
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
        ref var obj = ref _byId[id - 1];
        return Unsafe.IsNullRef( ref obj ) ? null : obj;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T? TryGetByName(string name)
    {
        return _byName.TryGetValue( name, out var obj ) ? obj : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(int id, string name)
    {
        try
        {
            _byId.Remove( id - 1 );
            _byName.Remove( name );
        }
        finally
        {
            _cache = null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal GetOrAddToken GetOrAddNull(string name)
    {
        ref var byName = ref CollectionsMarshal.GetValueRefOrAddDefault( _byName, name, out var exists )!;
        if ( exists )
            return new GetOrAddToken( ref Unsafe.NullRef<T>(), ref byName, 0 );

        ref var byId = ref _byId.AddDefault( out var index )!;
        return new GetOrAddToken( ref byId, ref byName, index + 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal RegisterToken RegisterNull()
    {
        ref var byId = ref _byId.AddDefault( out var index )!;
        return new RegisterToken( ref byId, index + 1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TrySetName(T obj, string name)
    {
        return _byName.TryAdd( name, obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal T[] Clear()
    {
        var result = _cache;
        _cache = null;
        _byName.Clear();
        result ??= ToArray();
        _byId = SparseListSlim<T>.Create();
        return result;
    }

    internal readonly ref struct GetOrAddToken
    {
        private readonly ref T _byIdRef;
        private readonly ref T _byNameRef;

        internal GetOrAddToken(ref T byIdRef, ref T byNameRef, int id)
        {
            _byIdRef = ref byIdRef;
            _byNameRef = ref byNameRef;
            Id = id;
        }

        internal readonly int Id;
        internal bool Exists => Id == 0;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T GetObject()
        {
            Assume.True( Exists );
            return _byNameRef;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T SetObject(ref ObjectStore<T> store, T obj)
        {
            Assume.False( Exists );
            _byIdRef = obj;
            _byNameRef = obj;
            store._cache = null;
            return obj;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Revert(ref ObjectStore<T> store, string name)
        {
            Assume.False( Exists );
            store._byId.Remove( Id - 1 );
            store._byName.Remove( name );
        }
    }

    internal readonly ref struct RegisterToken
    {
        private readonly ref T _ref;

        internal RegisterToken(ref T @ref, int id)
        {
            _ref = ref @ref;
            Id = id;
        }

        internal readonly int Id;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T SetObject(ref ObjectStore<T> store, T obj)
        {
            _ref = obj;
            store._cache = null;
            return obj;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Revert(ref ObjectStore<T> store)
        {
            store._byId.Remove( Id - 1 );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T[] ToArray()
    {
        if ( _byId.IsEmpty )
            return Array.Empty<T>();

        var i = 0;
        var result = new T[_byId.Count];
        foreach ( var (_, obj) in _byId )
            result[i++] = obj;

        return result;
    }
}
