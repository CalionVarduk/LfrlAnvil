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
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

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

    internal static Task StartUnderlyingTask(MessageBrokerRemoteClient client, Stream stream)
    {
        return Task.Run( async () =>
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

                using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                    client,
                    traceId,
                    MessageBrokerRemoteClientTraceEventType.Unexpected ) )
                {
                    if ( client.Logger.Error is { } error )
                        error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );

                    await client.DeactivateAsync( traceId ).ConfigureAwait( false );
                }
            }

            Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerRemoteClientState.Disposing );
        } );
    }

    internal void SetUnderlyingTask(Task? task)
    {
        _task = task;
    }

    internal Task? DiscardUnderlyingTask()
    {
        var result = _task;
        _task = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask RunCore(MessageBrokerRemoteClient client, Stream stream)
    {
        var awaitPacket = client.Logger.AwaitPacket;
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        while ( true )
        {
            awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client ) );

            var timeoutToken = default( CancellationToken );
            try
            {
                using ( AcquireActiveLock( client, out var acquired ) )
                {
                    if ( ! acquired )
                        return;

                    timeoutToken = client.EventScheduler.ScheduleMaxReadTimeout( client );
                }

                await stream.ReadExactlyAsync( buffer, timeoutToken ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                await DisposeClientDueToStreamFailureAsync( client, exc, MemoryPoolToken<byte>.Empty, timeoutToken )
                    .ConfigureAwait( false );

                return;
            }

            var header = Protocol.PacketHeader.Parse( buffer );
            if ( header.GetClientEndpoint() == MessageBrokerClientEndpoint.Batch )
            {
                awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, 0 ) );
                var batchPoolToken = MemoryPoolToken<byte>.Empty;
                var batchBuffer = Memory<byte>.Empty;

                if ( client.MaxBatchPacketCount == 0 )
                {
                    var exc = client.ProtocolException( header, Resources.UnexpectedServerEndpoint );
                    awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: exc ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                var packetLength = header.AssertPacketLength( client, client.MaxNetworkBatchPacketBytes );
                if ( packetLength.Exception is not null )
                {
                    awaitPacket?.Emit(
                        MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: packetLength.Exception ) );

                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                if ( packetLength.Value > 0 )
                {
                    batchPoolToken = client.MemoryPool.Rent(
                        packetLength.Value.Min( client.MemoryPool.SegmentLength ),
                        client.ClearBuffers,
                        out batchBuffer );

                    try
                    {
                        await stream.ReadExactlyAsync( batchBuffer, timeoutToken ).ConfigureAwait( false );
                        if ( packetLength.Value > batchBuffer.Length )
                        {
                            var oldPoolToken = batchPoolToken;
                            var oldBuffer = batchBuffer;
                            batchPoolToken = client.Server.MemoryPool.Rent( packetLength.Value, client.ClearBuffers, out batchBuffer );
                            try
                            {
                                await stream.ReadExactlyAsync( batchBuffer.Slice( oldBuffer.Length ), timeoutToken )
                                    .ConfigureAwait( false );

                                oldBuffer.CopyTo( batchBuffer );
                            }
                            finally
                            {
                                Return( client, oldPoolToken );
                            }
                        }
                    }
                    catch ( Exception exc )
                    {
                        await DisposeClientDueToStreamFailureAsync( client, exc, batchPoolToken, timeoutToken ).ConfigureAwait( false );
                        return;
                    }
                }

                var exception = header.AssertMinPayload( client, Protocol.BatchHeader.Length );
                if ( exception is not null )
                {
                    awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                var batchHeader = Protocol.BatchHeader.Parse( batchBuffer );
                var errors = batchHeader.StringifyErrors( client.MaxBatchPacketCount );
                if ( errors.Count > 0 )
                {
                    var error = client.ProtocolException( header, errors );
                    awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: error ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }

                ReduceBatchBuffer( client, ref batchPoolToken, ref batchBuffer, Protocol.BatchHeader.Length );
                awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, batchHeader.PacketCount ) );
                for ( var i = 0; i < batchHeader.PacketCount; ++i )
                {
                    if ( batchBuffer.Length < Protocol.PacketHeader.Length )
                    {
                        exception = client.ProtocolException(
                            header,
                            Resources.BatchPacketElementHeaderIsTooShort( i, batchBuffer.Length ) );

                        awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                        await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                        return;
                    }

                    var elementHeader = Protocol.PacketHeader.Parse( batchBuffer );
                    ReduceBatchBuffer( client, ref batchPoolToken, ref batchBuffer, Protocol.PacketHeader.Length );
                    awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, elementHeader ) );
                    var elementPoolToken = MemoryPoolToken<byte>.Empty;
                    var elementBuffer = Memory<byte>.Empty;

                    if ( elementHeader.GetServerEndpoint() != MessageBrokerServerEndpoint.Ping )
                    {
                        var maxLength = elementHeader.GetServerEndpoint() == MessageBrokerServerEndpoint.PushMessage
                            ? client.MaxNetworkMessagePacketBytes
                            : client.MaxNetworkPacketBytes;

                        packetLength = elementHeader.AssertPacketLength( client, maxLength );
                        if ( packetLength.Exception is not null )
                        {
                            awaitPacket?.Emit(
                                MessageBrokerRemoteClientAwaitPacketEvent.Create(
                                    client,
                                    elementHeader,
                                    exception: packetLength.Exception ) );

                            await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                            return;
                        }

                        if ( packetLength.Value > 0 )
                        {
                            if ( packetLength.Value > batchBuffer.Length )
                            {
                                var exc = client.ProtocolException(
                                    elementHeader,
                                    Resources.BatchPacketElementPayloadIsTooLarge( i, packetLength.Value, batchBuffer.Length ) );

                                awaitPacket?.Emit(
                                    MessageBrokerRemoteClientAwaitPacketEvent.Create( client, elementHeader, exception: exc ) );

                                await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                                return;
                            }

                            SplitBatch(
                                ref batchPoolToken,
                                ref batchBuffer,
                                out elementPoolToken,
                                out elementBuffer,
                                packetLength.Value );
                        }
                    }

                    if ( ! EnqueueRequest( client, elementHeader, elementPoolToken, elementBuffer ) )
                    {
                        Return( client, elementPoolToken );
                        Return( client, batchPoolToken );
                        return;
                    }
                }

                if ( batchBuffer.Length > 0 )
                {
                    exception = client.ProtocolException( header, Resources.BatchPacketContainsTooMuchData( batchBuffer.Length ) );
                    awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: exception ) );
                    await DisposeClientAsync( client, batchPoolToken ).ConfigureAwait( false );
                    return;
                }
            }
            else
            {
                awaitPacket?.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header ) );
                var packetPoolToken = MemoryPoolToken<byte>.Empty;
                var packetBuffer = Memory<byte>.Empty;

                if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.Ping )
                {
                    var maxLength = header.GetServerEndpoint() == MessageBrokerServerEndpoint.PushMessage
                        ? client.MaxNetworkMessagePacketBytes
                        : client.MaxNetworkPacketBytes;

                    var packetLength = header.AssertPacketLength( client, maxLength );
                    if ( packetLength.Exception is not null )
                    {
                        awaitPacket?.Emit(
                            MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exception: packetLength.Exception ) );

                        await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                        return;
                    }

                    if ( packetLength.Value > 0 )
                    {
                        packetPoolToken = client.MemoryPool.Rent(
                            packetLength.Value.Min( client.MemoryPool.SegmentLength ),
                            client.ClearBuffers,
                            out packetBuffer );

                        try
                        {
                            await stream.ReadExactlyAsync( packetBuffer, timeoutToken ).ConfigureAwait( false );
                            if ( packetLength.Value > packetBuffer.Length )
                            {
                                var oldPoolToken = packetPoolToken;
                                var oldBuffer = packetBuffer;
                                packetPoolToken = client.Server.MemoryPool.Rent(
                                    packetLength.Value,
                                    client.ClearBuffers,
                                    out packetBuffer );

                                try
                                {
                                    await stream.ReadExactlyAsync( packetBuffer.Slice( oldBuffer.Length ), timeoutToken )
                                        .ConfigureAwait( false );

                                    oldBuffer.CopyTo( packetBuffer );
                                }
                                finally
                                {
                                    Return( client, oldPoolToken );
                                }
                            }
                        }
                        catch ( Exception exc )
                        {
                            await DisposeClientDueToStreamFailureAsync( client, exc, packetPoolToken, timeoutToken )
                                .ConfigureAwait( false );

                            return;
                        }
                    }
                }

                if ( ! EnqueueRequest( client, header, packetPoolToken, packetBuffer ) )
                {
                    Return( client, packetPoolToken );
                    return;
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ReduceBatchBuffer(
        MessageBrokerRemoteClient client,
        ref MemoryPoolToken<byte> token,
        ref Memory<byte> data,
        int offset)
    {
        Assume.IsInRange( offset, 0, data.Length );
        data = data.Slice( offset );
        if ( data.Length > 0 )
            token.DecreaseLengthAtStart( data.Length );
        else
        {
            Return( client, token );
            token = MemoryPoolToken<byte>.Empty;
            data = Memory<byte>.Empty;
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool EnqueueRequest(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        MemoryPoolToken<byte> poolToken,
        Memory<byte> buffer)
    {
        using ( AcquireActiveLock( client, out var acquired ) )
        {
            if ( ! acquired )
                return false;

            client.EventScheduler.ResetReadTimeout();
            client.RequestQueue.Enqueue( header, poolToken, buffer );
            client.RequestHandler.SignalContinuation();
        }

        return true;
    }

    private static async ValueTask DisposeClientAsync(MessageBrokerRemoteClient client, MemoryPoolToken<byte> poolToken)
    {
        ulong traceId;
        using ( client.AcquireLock() )
        {
            client.PacketListener._task = null;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
        {
            poolToken.Return( client, traceId );
            await client.DeactivateAsync( traceId ).ConfigureAwait( false );
        }
    }

    private static async ValueTask DisposeClientDueToStreamFailureAsync(
        MessageBrokerRemoteClient client,
        Exception exception,
        MemoryPoolToken<byte> poolToken,
        CancellationToken timeoutToken)
    {
        if ( client.Logger.AwaitPacket is { } awaitPacket )
            awaitPacket.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, exception ) );

        ulong traceId;
        var isEphemeral = false;
        var isCancelException = exception is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken;

        using ( client.AcquireLock() )
        {
            if ( isCancelException && ! client.TryBeginDeactivate( out isEphemeral ) )
                return;

            client.PacketListener._task = null;
            traceId = client.GetTraceId();
        }

        using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Deactivate ) )
        {
            poolToken.Return( client, traceId );

            ValueTask disposeTask;
            if ( isCancelException )
            {
                if ( client.Logger.Error is { } error )
                {
                    var exc = client.RequestTimeoutException();
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ) );
                }

                disposeTask = client.DeactivateAsyncCore( traceId, isEphemeral );
            }
            else
                disposeTask = client.DeactivateAsync( traceId );

            await disposeTask.ConfigureAwait( false );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void Return(MessageBrokerRemoteClient client, MemoryPoolToken<byte> poolToken)
    {
        var exc = poolToken.Return();
        if ( exc is not null && client.Logger.AwaitPacket is { } awaitPacket )
            awaitPacket.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, exc ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireActiveLock(MessageBrokerRemoteClient client, out bool acquired)
    {
        var @lock = client.AcquireLock();
        if ( ! client.IsInactive )
        {
            acquired = true;
            return @lock;
        }

        var disposed = client.IsDisposed;
        @lock.Dispose();
        if ( client.Logger.AwaitPacket is { } awaitPacket )
            awaitPacket.Emit( MessageBrokerRemoteClientAwaitPacketEvent.Create( client, client.DeactivatedException( disposed ) ) );

        acquired = false;
        return default;
    }
}
