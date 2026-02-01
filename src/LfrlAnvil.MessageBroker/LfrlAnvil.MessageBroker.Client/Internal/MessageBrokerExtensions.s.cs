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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

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
        if ( token.Owner is null )
            return null;

        using ( token.Owner.AcquireLock() )
            return token.TryDispose().Exception;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void Return(this MemoryPoolToken<byte> token, MessageBrokerClient client, ulong traceId)
    {
        if ( token.Owner is not null )
        {
            Exception? exception;
            using ( token.Owner.AcquireLock() )
                exception = token.TryDispose().Exception;

            if ( exception is not null && client.Logger.Error is { } error )
                error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exception ) );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result TryCancel(this CancellationTokenSource source)
    {
        try
        {
            source.Cancel();
            return Result.Valid;
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ValueTask<Result> AsSafeCancellable(this Task? task, Duration? timeout = null)
    {
        return task is null
            ? ValueTask.FromResult( Result.Valid )
            : task.WaitAsync( timeout ?? Defaults.Temporal.TaskWaitTimeout ).AsSafe();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientDisposedException DisposedException(this MessageBrokerClient client)
    {
        return new MessageBrokerClientDisposedException( client );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException ProtocolException(
        this MessageBrokerClient client,
        Protocol.PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerClientProtocolException( client, header.GetClientEndpoint(), errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException ProtocolException(
        this MessageBrokerClient client,
        Protocol.PacketHeader header,
        string error)
    {
        return client.ProtocolException( header, Chain.Create( error ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientMessageException MessageException(
        this MessageBrokerClient client,
        MessageBrokerListener? listener,
        string error)
    {
        return new MessageBrokerClientMessageException( client, listener, error );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientResponseTimeoutException ResponseTimeoutException(
        this MessageBrokerClient client,
        MessageBrokerServerEndpoint endpoint)
    {
        return new MessageBrokerClientResponseTimeoutException( client, endpoint );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientResponseTimeoutException ResponseTimeoutException(
        this MessageBrokerClient client,
        Protocol.PacketHeader header)
    {
        return client.ResponseTimeoutException( header.GetServerEndpoint() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientRequestException RequestException(
        this MessageBrokerClient client,
        Protocol.PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerClientRequestException( client, header.GetServerEndpoint(), errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientRequestException RequestException(
        this MessageBrokerClient client,
        Protocol.PacketHeader header,
        string error)
    {
        return client.RequestException( header, Chain.Create( error ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException? AssertExactPayload(
        this Protocol.PacketHeader header,
        MessageBrokerClient client,
        uint expected)
    {
        return header.Payload != expected
            ? client.ProtocolException( header, Resources.InvalidHeaderPayload( header.Payload, expected ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException? AssertMinPayload(
        this Protocol.PacketHeader header,
        MessageBrokerClient client,
        uint expectedMin)
    {
        return header.Payload < expectedMin
            ? client.ProtocolException( header, Resources.TooShortHeaderPayload( header.Payload, expectedMin ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<int> AssertPacketLength(this Protocol.PacketHeader header, MessageBrokerClient client, int expectedMax)
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
        return ExclusiveLock.Enter( pool );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireLock(this MemoryPoolToken<byte> token)
    {
        return token.Owner is not null ? token.Owner.AcquireLock() : default;
    }
}
