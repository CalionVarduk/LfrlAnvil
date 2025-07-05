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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct StreamMessageStore
{
    private SparseListSlim<Entry> _store;
    private ulong _nextMessageId;
    private NullableIndex _nextPendingNodeId;

    private StreamMessageStore(int capacity)
    {
        _store = SparseListSlim<Entry>.Create( capacity );
        _nextMessageId = 0;
        _nextPendingNodeId = NullableIndex.Null;
    }

    internal bool IsEmpty => _store.IsEmpty;
    internal int Count => _store.Count;

    [Pure]
    internal static StreamMessageStore Create()
    {
        return new StreamMessageStore( 0 );
    }

    internal ulong Add(
        MessageBrokerChannelPublisherBinding publisher,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> data,
        out int key)
    {
        var messageId = unchecked( _nextMessageId++ );
        key = _store.Add( new Entry( publisher, messageId, publisher.Client.GetTimestamp(), poolToken, data ) );
        if ( ! _nextPendingNodeId.HasValue )
            _nextPendingNodeId = NullableIndex.Create( key );

        return messageId;
    }

    internal bool TryPeekPending(out int key, out StreamMessage message)
    {
        if ( ! _nextPendingNodeId.HasValue )
        {
            key = default;
            message = default;
            return false;
        }

        key = _nextPendingNodeId.Value;
        ref var entryRef = ref _store[key];
        Assume.False( Unsafe.IsNullRef( ref entryRef ) );
        message = entryRef.Message;
        return true;
    }

    internal Result<bool> DequeuePending(int failCount)
    {
        Assume.IsGreaterThanOrEqualTo( failCount, 0 );
        Assume.True( _nextPendingNodeId.HasValue );
        var node = _store.GetNode( _nextPendingNodeId.Value );
        Assume.IsNotNull( node );

        var nextNode = node.Value.Next;
        _nextPendingNodeId = nextNode is null ? NullableIndex.Null : NullableIndex.Create( nextNode.Value.Index );
        ref var entry = ref node.Value.Value;
        entry.RefCount -= failCount + 1;
        if ( entry.RefCount > 0 )
            return false;

        var poolToken = entry.Message.PoolToken;
        _store.Remove( node.Value.Index );
        var exc = poolToken.Return();
        return exc is null ? Result.Create( true ) : Result.Error( exc, true );
    }

    internal bool TryGet(int key, out StreamMessage message)
    {
        ref var entry = ref _store[key];
        if ( Unsafe.IsNullRef( ref entry ) )
        {
            message = default;
            return false;
        }

        message = entry.Message;
        return true;
    }

    internal bool TryGet(int key, out StreamMessage message, out int refCount)
    {
        ref var entry = ref _store[key];
        if ( Unsafe.IsNullRef( ref entry ) )
        {
            message = default;
            refCount = default;
            return false;
        }

        message = entry.Message;
        refCount = entry.RefCount;
        return true;
    }

    internal void IncreaseRefCount(int key, int count)
    {
        Assume.IsGreaterThan( count, 0 );
        Assume.True( _nextPendingNodeId.HasValue );
        Assume.Equals( key, _nextPendingNodeId.Value );

        ref var entry = ref _store[key];
        Assume.False( Unsafe.IsNullRef( ref entry ) );
        entry.RefCount += count;
    }

    internal Result<bool> DecrementRefCount(int key, out bool removed)
    {
        ref var entry = ref _store[key];
        if ( Unsafe.IsNullRef( ref entry ) )
        {
            removed = false;
            return false;
        }

        if ( --entry.RefCount <= 0 )
        {
            removed = true;
            var poolToken = entry.Message.PoolToken;
            _store.Remove( key );
            var exc = poolToken.Return();
            return exc is null ? Result.Create( true ) : Result.Error( exc, true );
        }

        removed = false;
        return true;
    }

    internal (int DiscardedMessageCount, Chain<Exception> Exceptions) ClearPending(bool extractExceptions)
    {
        var discardedMessageCount = 0;
        var exceptions = Chain<Exception>.Empty;

        if ( _nextPendingNodeId.HasValue )
        {
            var node = _store.GetNode( _nextPendingNodeId.Value );
            while ( node is not null )
            {
                var next = node.Value.Next;
                ref var entry = ref node.Value.Value;
                if ( --entry.RefCount <= 0 )
                {
                    var exc = entry.Message.PoolToken.Return();
                    if ( exc is not null && extractExceptions )
                        exceptions = exceptions.Extend( exc );

                    _store.Remove( node.Value.Index );
                    ++discardedMessageCount;
                }

                node = next;
            }

            _nextPendingNodeId = NullableIndex.Null;
        }

        return (discardedMessageCount, exceptions);
    }

    internal Chain<Exception> Clear()
    {
        Assume.Equals( _nextPendingNodeId, NullableIndex.Null );
        var exceptions = Chain<Exception>.Empty;

        var node = _store.First;
        while ( node is not null )
        {
            ref var entry = ref node.Value.Value;
            var exc = entry.Message.PoolToken.Return();
            if ( exc is not null )
                exceptions = exceptions.Extend( exc );

            node = node.Value.Next;
        }

        _store = SparseListSlim<Entry>.Create();
        return exceptions;
    }

    private struct Entry
    {
        internal Entry(
            MessageBrokerChannelPublisherBinding publisher,
            ulong messageId,
            Timestamp pushedAt,
            MemoryPoolToken<byte> poolToken,
            ReadOnlyMemory<byte> data)
        {
            Message = new StreamMessage( publisher, messageId, pushedAt, poolToken, data );
            RefCount = 1;
        }

        internal readonly StreamMessage Message;
        internal int RefCount;

        [Pure]
        public override string ToString()
        {
            return $"Message = ({Message}), RefCount = {RefCount}";
        }
    }
}
