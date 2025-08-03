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
using LfrlAnvil.Async;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal static class MessageBrokerExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Emit<T>(this Action<T> emitter, T @event)
        where T : struct
    {
        try
        {
            emitter( @event );
        }
        catch
        {
            // NOTE: do nothing
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MemoryPoolToken<byte> Rent(this MemoryPool<byte> pool, int length, bool clear, out Memory<byte> data)
    {
        MemoryPoolToken<byte> token;
        using ( pool.AcquireLock() )
        {
            token = pool.Rent( length );
            data = token.AsMemory();
        }

        return token.EnableClearing( clear );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void IncreaseLength(this MemoryPoolToken<byte> token, int length, out Memory<byte> data)
    {
        using ( token.AcquireLock() )
        {
            Assume.IsGreaterThan( length, token.AsMemory().Length );
            token.SetLength( length );
            data = token.AsMemory();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void DecreaseLengthAtStart(this MemoryPoolToken<byte> token, int length)
    {
        using ( token.AcquireLock() )
        {
            Assume.IsLessThan( length, token.AsMemory().Length );
            token.SetLength( length, trimStart: true );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MemoryPoolToken<byte> Split(
        this MemoryPoolToken<byte> batchToken,
        ref Memory<byte> batchData,
        int length,
        out Memory<byte> data)
    {
        Assume.IsInExclusiveRange( length, 0, batchData.Length );
        data = batchData.Slice( 0, length );
        batchData = batchData.Slice( length );
        using ( batchToken.AcquireLock() )
        {
            Assume.Equals( batchToken.AsMemory().Length, batchData.Length + data.Length );
            return batchToken.Split( length );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Exception? Return(this MemoryPoolToken<byte> token)
    {
        using ( token.AcquireLock() )
            return token.TryDispose().Exception;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Return(this MemoryPoolToken<byte> token, MessageBrokerRemoteClient client, ulong traceId)
    {
        var exception = token.Return();
        if ( exception is not null && client.Logger.Error is { } error )
            error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exception ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerDisposedException DisposedException(this MessageBrokerServer server)
    {
        return new MessageBrokerServerDisposedException( server );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelDisposedException DisposedException(this MessageBrokerChannel channel)
    {
        return new MessageBrokerChannelDisposedException( channel );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueDisposedException DisposedException(this MessageBrokerQueue queue)
    {
        return new MessageBrokerQueueDisposedException( queue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamDisposedException DisposedException(this MessageBrokerStream stream)
    {
        return new MessageBrokerStreamDisposedException( stream );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientDisposedException DisposedException(this MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientDisposedException( client );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBindingDisposedException DisposedException(
        this MessageBrokerChannelPublisherBinding publisher)
    {
        return new MessageBrokerChannelPublisherBindingDisposedException( publisher );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelListenerBindingDisposedException DisposedException(
        this MessageBrokerChannelListenerBinding listener)
    {
        return new MessageBrokerChannelListenerBindingDisposedException( listener );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerException Exception(this MessageBrokerServer server, string error)
    {
        return new MessageBrokerServerException( server, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelException Exception(this MessageBrokerChannel channel, string error)
    {
        return new MessageBrokerChannelException( channel, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueException Exception(this MessageBrokerQueue queue, string error)
    {
        return new MessageBrokerQueueException( queue, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamException Exception(this MessageBrokerStream stream, string error)
    {
        return new MessageBrokerStreamException( stream, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientException Exception(this MessageBrokerRemoteClient client, string error)
    {
        return new MessageBrokerRemoteClientException( client, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBindingException PublisherException(
        this MessageBrokerRemoteClient client,
        MessageBrokerChannelPublisherBinding? publisher,
        string error)
    {
        return new MessageBrokerChannelPublisherBindingException( client, publisher, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelListenerBindingException ListenerException(
        this MessageBrokerRemoteClient client,
        MessageBrokerChannelListenerBinding? listener,
        string error)
    {
        return new MessageBrokerChannelListenerBindingException( client, listener, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException ProtocolException(
        this MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerServerProtocolException( client, header.GetServerEndpoint(), errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException ProtocolException(
        this MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        string error)
    {
        return client.ProtocolException( header, Chain.Create( error ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientRequestTimeoutException RequestTimeoutException(this MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientRequestTimeoutException( client );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException? AssertExactPayload(
        this Protocol.PacketHeader header,
        MessageBrokerRemoteClient client,
        uint expected)
    {
        return header.Payload != expected
            ? client.ProtocolException( header, Resources.InvalidHeaderPayload( header.Payload, expected ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException? AssertMinPayload(
        this Protocol.PacketHeader header,
        MessageBrokerRemoteClient client,
        uint expectedMin)
    {
        return header.Payload < expectedMin
            ? client.ProtocolException( header, Resources.TooShortHeaderPayload( header.Payload, expectedMin ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<int> AssertPacketLength(this Protocol.PacketHeader header, MessageBrokerRemoteClient client, int expectedMax)
    {
        Assume.IsGreaterThanOrEqualTo( expectedMax, Protocol.PacketHeader.Length );
        var result = unchecked( ( int )header.Payload );
        if ( result >= 0 && result <= unchecked( expectedMax - Protocol.PacketHeader.Length ) )
            return result;

        return client.ProtocolException( header, Resources.UnexpectedPacketLength( result, expectedMax ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireLock(this MemoryPool<byte> pool)
    {
        return ExclusiveLock.SpinWaitEnter( pool, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireLock(this MemoryPoolToken<byte> token)
    {
        return token.Owner is not null ? token.Owner.AcquireLock() : default;
    }
}
