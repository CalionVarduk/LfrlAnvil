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
using System.Runtime.InteropServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct StreamMessage
{
    internal StreamMessage(
        IMessageBrokerMessagePublisher publisher,
        ulong id,
        Timestamp pushedAt,
        MemoryPoolToken<byte> poolToken,
        ReadOnlyMemory<byte> data)
    {
        Publisher = publisher;
        Id = id;
        PushedAt = pushedAt;
        PoolToken = poolToken;
        Data = data;
    }

    internal readonly IMessageBrokerMessagePublisher Publisher;
    internal readonly ulong Id;
    internal readonly Timestamp PushedAt;
    internal readonly MemoryPoolToken<byte> PoolToken;
    internal readonly ReadOnlyMemory<byte> Data;

    [Pure]
    public override string ToString()
    {
        return $"Publisher = ({Publisher}), Id = {Id}, PushedAt = {PushedAt}, Length = {Data.Length}";
    }

    internal readonly record struct Builder(
        int StoreKey,
        IMessageBrokerMessagePublisher? Publisher,
        MessageBrokerChannel Channel,
        int SenderId,
        ulong Id,
        Timestamp PushedAt,
        MemoryPoolToken<byte> PoolToken,
        ReadOnlyMemory<byte> Data
    )
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool TryBuild(Dictionary<int, EphemeralPublisher.Builder> ephemeralPublishersByVirtualId, out StreamMessage message)
        {
            if ( Publisher is not null )
            {
                message = new StreamMessage( Publisher, Id, PushedAt, PoolToken, Data );
                return true;
            }

            Assume.IsLessThan( SenderId, 0 );
            ref var builder = ref CollectionsMarshal.GetValueRefOrNullRef( ephemeralPublishersByVirtualId, -SenderId );
            if ( Unsafe.IsNullRef( ref builder ) )
            {
                message = default;
                return false;
            }

            message = new StreamMessage( builder.Build( Channel ), Id, PushedAt, PoolToken, Data );
            return true;
        }
    }
}
