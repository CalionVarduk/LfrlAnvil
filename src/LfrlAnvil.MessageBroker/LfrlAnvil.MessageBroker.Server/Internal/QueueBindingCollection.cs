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
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueBindingCollection
{
    // TODO: make Primary interlocked ref
    internal MessageBrokerQueueListenerBinding Primary;
    internal MessageBrokerQueueListenerBinding[] Secondary;
    private MessageBrokerQueueListenerBinding[]? _cache;

    private QueueBindingCollection(MessageBrokerQueueListenerBinding primary)
    {
        Primary = primary;
        Secondary = Array.Empty<MessageBrokerQueueListenerBinding>();
        _cache = null;
    }

    internal int Count => Secondary.Length + 1;

    [Pure]
    internal static QueueBindingCollection Create(MessageBrokerQueueListenerBinding primary)
    {
        return new QueueBindingCollection( primary );
    }

    [Pure]
    internal ReadOnlyArray<MessageBrokerQueueListenerBinding> GetAllUnsafe()
    {
        if ( _cache is not null )
            return _cache;

        _cache = new MessageBrokerQueueListenerBinding[Count];
        _cache[0] = Primary;
        for ( var i = 0; i < Secondary.Length; ++i )
            _cache[i + 1] = Secondary[i];

        return _cache;
    }

    internal void AddSecondaryUnsafe(MessageBrokerQueueListenerBinding listener)
    {
        Assume.Equals( listener.Client, Primary.Client );
        Assume.NotEquals( listener.Queue, Primary.Queue );
        Assume.False( Secondary.Any( b => ReferenceEquals( b.Queue, listener.Queue ) ) );

        try
        {
            var prev = Secondary;
            Secondary = new MessageBrokerQueueListenerBinding[prev.Length + 1];
            prev.AsSpan().CopyTo( Secondary );
            Secondary[^1] = listener;
        }
        finally
        {
            _cache = null;
        }
    }

    internal void RemoveSecondaryUnsafe(MessageBrokerQueueListenerBinding listener)
    {
        Assume.Equals( listener.Owner, Primary.Owner );
        Assume.False( listener.IsPrimary );

        var index = Array.IndexOf( Secondary, listener );
        if ( index >= 0 )
        {
            try
            {
                var prev = Secondary;
                if ( prev.Length == 1 )
                {
                    Secondary = Array.Empty<MessageBrokerQueueListenerBinding>();
                    return;
                }

                Secondary = new MessageBrokerQueueListenerBinding[prev.Length - 1];
                prev.AsSpan( 0, index ).CopyTo( Secondary );
                prev.AsSpan( index + 1 ).CopyTo( Secondary.AsSpan( index ) );
            }
            finally
            {
                _cache = null;
            }
        }
    }

    internal void RemoveAllFromQueues(int channelId)
    {
        using ( Primary.Queue.AcquireLock() )
            Primary.Queue.ListenersByChannelId.Remove( channelId );

        foreach ( var binding in Secondary )
        {
            using ( binding.Queue.AcquireLock() )
                binding.Queue.ListenersByChannelId.Remove( channelId );
        }
    }

    internal void DeactivateAll()
    {
        Primary.MarkAsInactive();
        foreach ( var binding in Secondary )
            binding.MarkAsInactive();
    }

    internal void DisposeAll()
    {
        Primary.TryMarkAsDisposed();
        foreach ( var binding in Secondary )
            binding.TryMarkAsDisposed();
    }

    internal void Clear()
    {
        try
        {
            Secondary = Array.Empty<MessageBrokerQueueListenerBinding>();
        }
        finally
        {
            _cache = null;
        }
    }
}
