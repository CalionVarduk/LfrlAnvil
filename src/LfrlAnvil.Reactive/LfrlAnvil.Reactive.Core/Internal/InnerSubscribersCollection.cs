// Copyright 2026 Łukasz Furlepa
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
using System.Buffers;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Reactive.Internal;

internal struct InnerSubscribersCollection
{
    private LinkedListSlim<Entry> _subscribers;

    internal InnerSubscribersCollection(int capacity)
    {
        _subscribers = LinkedListSlim<Entry>.Create( capacity );
        Count = 0;
    }

    internal int Count { get; private set; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int Reserve()
    {
        ++Count;
        return _subscribers.AddLast( default );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Set(int nodeId, IEventSubscriber subscriber)
    {
        var node = _subscribers.GetNode( nodeId );
        if ( node is not null )
        {
            ref var entry = ref node.Value.Value;
            if ( entry.Disposed )
                _subscribers.Remove( nodeId );
            else
                entry.Subscriber = subscriber;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Remove(int nodeId)
    {
        --Count;
        var node = _subscribers.GetNode( nodeId );
        if ( node is not null )
        {
            ref var entry = ref node.Value.Value;
            if ( entry.Subscriber is not null )
                _subscribers.Remove( nodeId );
            else
                entry.Disposed = true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        Count = 0;
        _subscribers = LinkedListSlim<Entry>.Create();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlySpan<IEventSubscriber?> Clear(out ArrayPoolToken<IEventSubscriber?> poolToken)
    {
        var result = Span<IEventSubscriber?>.Empty;
        if ( _subscribers.IsEmpty )
            poolToken = default;
        else
        {
            poolToken = ArrayPool<IEventSubscriber?>.Shared.RentToken( _subscribers.Count, clearArray: true );
            result = poolToken.AsSpan();

            var i = 0;
            foreach ( var (_, e) in _subscribers )
                result[i++] = e.Subscriber;
        }

        Clear();
        return result;
    }

    private struct Entry
    {
        internal IEventSubscriber? Subscriber;
        internal bool Disposed;
    }
}
