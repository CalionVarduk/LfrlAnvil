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
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct PacketListener
{
    private Task? _task;

    private PacketListener(Task? task)
    {
        _task = task;
    }

    [Pure]
    internal static PacketListener Create()
    {
        return new PacketListener( null );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerClient client, Stream stream)
    {
        try
        {
            await RunCore( client, stream ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            ulong traceId;
            using ( client.AcquireLock() )
            {
                client.PacketListener._task = null;
                traceId = client.GetTraceId();
            }

            using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Unexpected ) )
            {
                if ( client.Logger.Error is { } error )
                    error.Emit( MessageBrokerClientErrorEvent.Create( client, traceId, exc ) );

                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerClientState.Disposing );
    }

    internal void SetUnderlyingTask(Task task)
    {
        Assume.IsNull( _task );
        _task = task;
    }

    internal Task? DiscardUnderlyingTask()
    {
        var result = _task;
        _task = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerClient client, Stream stream)
    {
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return;

            reverseEndianness = client.IsServerLittleEndian != BitConverter.IsLittleEndian;
        }

        var awaitPacket = client.Logger.AwaitPacket;
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        while ( true )
        {
            awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client ) );

            Protocol.PacketHeader header;
            var timeoutToken = default( CancellationToken );
            try
            {
                using ( AcquireActiveLock( client, out var acquired ) )
                {
                    if ( ! acquired )
                        return;

                    timeoutToken = client.EventScheduler.GetReadTimeoutToken();
                }

                await stream.ReadExactlyAsync( buffer, timeoutToken ).ConfigureAwait( false );
                header = Protocol.PacketHeader.Parse( buffer, reverseEndianness );
            }
            catch ( Exception exc )
            {
                await DisposeClientDueToStreamFailureAsync( client, exc, MemoryPoolToken<byte>.Empty, timeoutToken )
                    .ConfigureAwait( false );

                return;
            }

            if ( header.GetClientEndpoint() == MessageBrokerClientEndpoint.Batch )
            {
                awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, 0 ) );
                var batchPoolToken = MemoryPoolToken<byte>.Empty;
                var batchBuffer = Memory<byte>.Empty;

                if ( client.MaxBatchPacketCount == 0 )
                {
                    var exc = client.ProtocolException( header, Resources.UnexpectedClientEndpoint );
                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: exc ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                var packetLength = header.AssertPacketLength( client, client.MaxNetworkBatchPacketBytes );
                if ( packetLength.Exception is not null )
                {
                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: packetLength.Exception ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                if ( packetLength.Value > 0 )
                {
                    batchPoolToken = client.MemoryPool.Rent( packetLength.Value, out batchBuffer ).EnableClearing();
                    try
                    {
                        await stream.ReadExactlyAsync( batchBuffer, timeoutToken ).ConfigureAwait( false );
                    }
                    catch ( Exception exc )
                    {
                        await DisposeClientDueToStreamFailureAsync( client, exc, batchPoolToken, timeoutToken ).ConfigureAwait( false );
                        return;
                    }
                }

                Exception? exception = header.AssertMinPayload( client, Protocol.BatchHeader.Length );
                if ( exception is not null )
                {
                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                var batchHeader = Protocol.BatchHeader.Parse( batchBuffer.Slice( 0, Protocol.BatchHeader.Length ), reverseEndianness );
                var errors = batchHeader.StringifyErrors( client.MaxBatchPacketCount );
                if ( errors.Count > 0 )
                {
                    var error = client.ProtocolException( header, errors );
                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: error ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                batchBuffer = batchBuffer.Slice( Protocol.BatchHeader.Length );
                awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, batchHeader.PacketCount ) );
                for ( var i = 0; i < batchHeader.PacketCount; ++i )
                {
                    if ( batchBuffer.Length < Protocol.PacketHeader.Length )
                    {
                        exception = client.ProtocolException(
                            header,
                            Resources.BatchPacketElementHeaderIsTooShort( i, batchBuffer.Length ) );

                        awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                        await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                        return;
                    }

                    var elementHeader = Protocol.PacketHeader.Parse(
                        batchBuffer.Slice( 0, Protocol.PacketHeader.Length ),
                        reverseEndianness );

                    batchBuffer = batchBuffer.Slice( Protocol.PacketHeader.Length );
                    if ( batchBuffer.Length > 0 )
                        batchPoolToken.DecreaseLengthAtStart( batchBuffer.Length );
                    else
                    {
                        Return( client, batchPoolToken );
                        batchPoolToken = MemoryPoolToken<byte>.Empty;
                        batchBuffer = Memory<byte>.Empty;
                    }

                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, elementHeader ) );
                    var elementPoolToken = MemoryPoolToken<byte>.Empty;
                    var elementBuffer = Memory<byte>.Empty;

                    switch ( elementHeader.GetClientEndpoint() )
                    {
                        case MessageBrokerClientEndpoint.MessageNotification:
                        {
                            packetLength = elementHeader.AssertPacketLength( client, client.MaxNetworkMessagePacketBytes );
                            if ( packetLength.Exception is not null )
                            {
                                exception = packetLength.Exception;
                                break;
                            }

                            if ( packetLength.Value > 0 )
                            {
                                if ( packetLength.Value > batchBuffer.Length )
                                {
                                    exception = client.ProtocolException(
                                        elementHeader,
                                        Resources.BatchPacketElementPayloadIsTooLarge( i, packetLength.Value, batchBuffer.Length ) );

                                    break;
                                }

                                SplitBatch(
                                    ref batchPoolToken,
                                    ref batchBuffer,
                                    out elementPoolToken,
                                    out elementBuffer,
                                    packetLength.Value );
                            }

                            if ( ! EnqueueMessageNotification( client, elementHeader, elementPoolToken, elementBuffer ) )
                            {
                                Return( client, elementPoolToken );
                                Return( client, batchPoolToken );
                                return;
                            }

                            break;
                        }
                        case MessageBrokerClientEndpoint.SystemNotification:
                        {
                            packetLength = elementHeader.AssertPacketLength( client, client.MaxNetworkPacketBytes );
                            if ( packetLength.Exception is not null )
                            {
                                exception = packetLength.Exception;
                                break;
                            }

                            if ( packetLength.Value > 0 )
                            {
                                if ( packetLength.Value > batchBuffer.Length )
                                {
                                    exception = client.ProtocolException(
                                        elementHeader,
                                        Resources.BatchPacketElementPayloadIsTooLarge( i, packetLength.Value, batchBuffer.Length ) );

                                    break;
                                }

                                SplitBatch(
                                    ref batchPoolToken,
                                    ref batchBuffer,
                                    out elementPoolToken,
                                    out elementBuffer,
                                    packetLength.Value );
                            }

                            if ( ! EnqueueSystemNotification( client, elementHeader, elementPoolToken, elementBuffer ) )
                            {
                                Return( client, elementPoolToken );
                                Return( client, batchPoolToken );
                                return;
                            }

                            break;
                        }
                        default:
                        {
                            var source = GetNextResponseSource( client, elementHeader );
                            if ( source.Exception is not null )
                            {
                                exception = source.Exception;
                                break;
                            }

                            if ( source.Value is null )
                                return;

                            if ( elementHeader.GetClientEndpoint() != MessageBrokerClientEndpoint.Pong )
                            {
                                packetLength = elementHeader.AssertPacketLength( client, client.MaxNetworkPacketBytes );
                                if ( packetLength.Exception is not null )
                                {
                                    exception = packetLength.Exception;
                                    break;
                                }

                                if ( packetLength.Value > 0 )
                                {
                                    if ( packetLength.Value > batchBuffer.Length )
                                    {
                                        exception = client.ProtocolException(
                                            elementHeader,
                                            Resources.BatchPacketElementPayloadIsTooLarge( i, packetLength.Value, batchBuffer.Length ) );

                                        break;
                                    }

                                    SplitBatch(
                                        ref batchPoolToken,
                                        ref batchBuffer,
                                        out elementPoolToken,
                                        out elementBuffer,
                                        packetLength.Value );
                                }
                            }

                            if ( ! EnqueueResponse( client, source.Value, elementHeader, elementPoolToken, elementBuffer ) )
                            {
                                Return( client, elementPoolToken );
                                Return( client, batchPoolToken );
                                return;
                            }

                            break;
                        }
                    }

                    if ( exception is not null )
                    {
                        awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, elementHeader, exception: exception ) );
                        Assume.Equals( elementPoolToken, MemoryPoolToken<byte>.Empty );
                        await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                        return;
                    }
                }

                if ( batchBuffer.Length > 0 )
                {
                    exception = client.ProtocolException( header, Resources.BatchPacketContainsTooMuchData( batchBuffer.Length ) );
                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }
            }
            else
            {
                awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header ) );
                var packetPoolToken = MemoryPoolToken<byte>.Empty;
                var packetBuffer = Memory<byte>.Empty;
                Exception? exception = null;

                switch ( header.GetClientEndpoint() )
                {
                    case MessageBrokerClientEndpoint.MessageNotification:
                    {
                        var packetLength = header.AssertPacketLength( client, client.MaxNetworkMessagePacketBytes );
                        if ( packetLength.Exception is not null )
                        {
                            exception = packetLength.Exception;
                            break;
                        }

                        if ( packetLength.Value > 0 )
                        {
                            packetPoolToken = client.MemoryPool.Rent( packetLength.Value, out packetBuffer ).EnableClearing();
                            try
                            {
                                await stream.ReadExactlyAsync( packetBuffer, timeoutToken ).ConfigureAwait( false );
                            }
                            catch ( Exception exc )
                            {
                                await DisposeClientDueToStreamFailureAsync( client, exc, packetPoolToken, timeoutToken )
                                    .ConfigureAwait( false );

                                return;
                            }
                        }

                        if ( ! EnqueueMessageNotification( client, header, packetPoolToken, packetBuffer ) )
                        {
                            Return( client, packetPoolToken );
                            return;
                        }

                        break;
                    }
                    case MessageBrokerClientEndpoint.SystemNotification:
                    {
                        var packetLength = header.AssertPacketLength( client, client.MaxNetworkPacketBytes );
                        if ( packetLength.Exception is not null )
                        {
                            exception = packetLength.Exception;
                            break;
                        }

                        if ( packetLength.Value > 0 )
                        {
                            packetPoolToken = client.MemoryPool.Rent( packetLength.Value, out packetBuffer ).EnableClearing();
                            try
                            {
                                await stream.ReadExactlyAsync( packetBuffer, timeoutToken ).ConfigureAwait( false );
                            }
                            catch ( Exception exc )
                            {
                                await DisposeClientDueToStreamFailureAsync( client, exc, packetPoolToken, timeoutToken )
                                    .ConfigureAwait( false );

                                return;
                            }
                        }

                        if ( ! EnqueueSystemNotification( client, header, packetPoolToken, packetBuffer ) )
                        {
                            Return( client, packetPoolToken );
                            return;
                        }

                        break;
                    }
                    default:
                    {
                        var source = GetNextResponseSource( client, header );
                        if ( source.Exception is not null )
                        {
                            exception = source.Exception;
                            break;
                        }

                        if ( source.Value is null )
                            return;

                        if ( header.GetClientEndpoint() != MessageBrokerClientEndpoint.Pong )
                        {
                            var packetLength = header.AssertPacketLength( client, client.MaxNetworkPacketBytes );
                            if ( packetLength.Exception is not null )
                            {
                                exception = packetLength.Exception;
                                break;
                            }

                            if ( packetLength.Value > 0 )
                            {
                                packetPoolToken = client.MemoryPool.Rent( packetLength.Value, out packetBuffer ).EnableClearing();
                                try
                                {
                                    await stream.ReadExactlyAsync( packetBuffer, timeoutToken ).ConfigureAwait( false );
                                }
                                catch ( Exception exc )
                                {
                                    await DisposeClientDueToStreamFailureAsync( client, exc, packetPoolToken, timeoutToken )
                                        .ConfigureAwait( false );

                                    return;
                                }
                            }
                        }

                        if ( ! EnqueueResponse( client, source.Value, header, packetPoolToken, packetBuffer ) )
                        {
                            Return( client, packetPoolToken );
                            return;
                        }

                        break;
                    }
                }

                if ( exception is not null )
                {
                    awaitPacket?.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                    await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                    return;
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void SplitBatch(
        ref MemoryPoolToken<byte> batchToken,
        ref Memory<byte> batchData,
        out MemoryPoolToken<byte> elementToken,
        out Memory<byte> elementData,
        int length)
    {
        if ( length < batchData.Length )
            elementToken = batchToken.Split( ref batchData, length, out elementData );
        else
        {
            elementToken = batchToken;
            elementData = batchData;
            batchToken = MemoryPoolToken<byte>.Empty;
            batchData = Memory<byte>.Empty;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Result<ManualResetValueTaskSource<IncomingPacketToken>?> GetNextResponseSource(
        MessageBrokerClient client,
        Protocol.PacketHeader header)
    {
        ManualResetValueTaskSource<IncomingPacketToken>? source;
        using ( AcquireActiveLock( client, out var acquired ) )
        {
            if ( ! acquired )
                return ( ManualResetValueTaskSource<IncomingPacketToken>? )null;

            source = client.ResponseQueue.GetNext().Source;
        }

        if ( source is null )
            return client.ProtocolException( header, Resources.UnexpectedClientEndpoint );

        return source;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool EnqueueMessageNotification(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        MemoryPoolToken<byte> poolToken,
        Memory<byte> buffer)
    {
        Assume.Equals( header.GetClientEndpoint(), MessageBrokerClientEndpoint.MessageNotification );
        using ( AcquireActiveLock( client, out var acquired ) )
        {
            if ( ! acquired )
                return false;

            client.NotificationHandler.Enqueue( header, client.GetTimestamp(), poolToken, buffer );
            client.NotificationHandler.SignalContinuation();
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool EnqueueSystemNotification(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        MemoryPoolToken<byte> poolToken,
        Memory<byte> buffer)
    {
        Assume.Equals( header.GetClientEndpoint(), MessageBrokerClientEndpoint.SystemNotification );
        using ( AcquireActiveLock( client, out var acquired ) )
        {
            if ( ! acquired )
                return false;

            client.NotificationHandler.Enqueue( header, default, poolToken, buffer );
            client.NotificationHandler.SignalContinuation();
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool EnqueueResponse(
        MessageBrokerClient client,
        ManualResetValueTaskSource<IncomingPacketToken> source,
        Protocol.PacketHeader header,
        MemoryPoolToken<byte> poolToken,
        Memory<byte> buffer)
    {
        using ( AcquireActiveLock( client, out var acquired ) )
        {
            if ( ! acquired )
                return false;

            client.ResponseQueue.Signal( source, IncomingPacketToken.Ok( header, poolToken, buffer ) );
        }

        return true;
    }

    private static async ValueTask DisposeClientDueToStreamFailureAsync(
        MessageBrokerClient client,
        Exception exception,
        MemoryPoolToken<byte> poolToken,
        CancellationToken timeoutToken)
    {
        if ( client.Logger.AwaitPacket is { } awaitPacket )
            awaitPacket.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, exception ) );

        ulong traceId;
        var isCancelException = exception is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken;

        using ( client.AcquireLock() )
        {
            if ( isCancelException && ! client.TryBeginDispose() )
                return;

            client.PacketListener._task = null;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Dispose ) )
        {
            poolToken.Return( client, traceId );
            var disposeTask = isCancelException ? client.DisposeAsyncCore( traceId ) : client.DisposeAsync( traceId );
            await disposeTask.ConfigureAwait( false );
        }
    }

    private static async ValueTask DisposeClientAsync(MessageBrokerClient client, MemoryPoolToken<byte> poolToken)
    {
        ulong traceId;
        using ( client.AcquireLock() )
        {
            client.PacketListener._task = null;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Unexpected ) )
        {
            poolToken.Return( client, traceId );
            await client.DisposeAsync( traceId ).ConfigureAwait( false );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void Return(MessageBrokerClient client, MemoryPoolToken<byte> poolToken)
    {
        var exc = poolToken.Return();
        if ( exc is not null && client.Logger.AwaitPacket is { } awaitPacket )
            awaitPacket.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, exc ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireActiveLock(MessageBrokerClient client, out bool acquired)
    {
        var @lock = client.AcquireLock();
        if ( ! client.ShouldCancel )
        {
            acquired = true;
            return @lock;
        }

        @lock.Dispose();
        if ( client.Logger.AwaitPacket is { } awaitPacket )
            awaitPacket.Emit( MessageBrokerClientAwaitPacketEvent.Create( client, client.DisposedException() ) );

        acquired = false;
        return default;
    }
}
