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
using LfrlAnvil.Memory;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct QueueMessage
{
    internal QueueMessage(in StreamMessage message, MessageBrokerSubscription subscription)
    {
        Id = message.Id;
        Timestamp = message.Timestamp;
        Binding = message.Binding;
        Subscription = subscription;

        PoolToken = Subscription.Client.MemoryPool
            .Rent( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload + message.Data.Length, out var data )
            .EnableClearing( message.BufferClearEnabled );

        var header = new Protocol.MessageNotificationHeader(
            Id,
            Timestamp,
            Binding.Client.Id,
            Binding.Channel.Id,
            Binding.Stream.Id,
            0,
            0,
            message.Data.Length );

        header.Serialize( data.Slice( 0, Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload ) );
        message.Data.CopyTo( data.Slice( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload ) );

        PacketHeader = header.Header;
        Packet = data;
    }

    internal readonly Protocol.PacketHeader PacketHeader;
    internal readonly ulong Id;
    internal readonly Timestamp Timestamp;
    internal readonly MessageBrokerChannelBinding Binding;
    internal readonly MessageBrokerSubscription Subscription;
    internal readonly MemoryPoolToken<byte> PoolToken;
    internal readonly ReadOnlyMemory<byte> Packet;

    [Pure]
    public override string ToString()
    {
        return
            $"Id = {Id}, Timestamp = {Timestamp}, Length = {Packet.Length - Protocol.PacketHeader.Length - Protocol.MessageNotificationHeader.Payload}, Binding = ({Binding}), Subscription = ({Subscription})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Return()
    {
        PoolToken.Return( Subscription.Client );
    }
}
