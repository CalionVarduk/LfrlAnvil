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
                MessageBrokerClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
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

        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        while ( true )
        {
            MessageBrokerClientAwaitPacketEvent.Create( client ).Emit( client.Logger.AwaitPacket );

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
                MessageBrokerClientAwaitPacketEvent.Create( client, exc ).Emit( client.Logger.AwaitPacket );

                ulong traceId;
                var isCancelException = exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken;

                using ( client.AcquireLock() )
                {
                    if ( isCancelException && ! client.TryBeginDispose() )
                        return;

                    client.PacketListener._task = null;
                    traceId = client.GetTraceId();
                }

                using ( MessageBrokerClientTraceEvent.CreateScope( client, traceId, MessageBrokerClientTraceEventType.Dispose ) )
                {
                    var disposeTask = isCancelException ? client.DisposeAsyncCore( traceId ) : client.DisposeAsync( traceId );
                    await disposeTask.ConfigureAwait( false );
                }

                return;
            }

            MessageBrokerClientAwaitPacketEvent.Create( client, header ).Emit( client.Logger.AwaitPacket );

            var packetPoolToken = default( MemoryPoolToken<byte> );
            var packetBuffer = Memory<byte>.Empty;
            if ( header.GetClientEndpoint() == MessageBrokerClientEndpoint.MessageNotification )
            {
                var packetLength = Protocol.AssertPacketLength( client, header );
                if ( packetLength.Exception is not null )
                {
                    MessageBrokerClientAwaitPacketEvent.Create( client, header, packetLength.Exception ).Emit( client.Logger.AwaitPacket );
                    await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                    return;
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
                        MessageBrokerClientAwaitPacketEvent.Create( client, header, exc ).Emit( client.Logger.AwaitPacket );
                        await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                        return;
                    }
                }

                using ( AcquireActiveLock( client, out var acquired ) )
                {
                    if ( ! acquired )
                    {
                        Return( client, packetPoolToken );
                        return;
                    }

                    client.MessageNotifications.Enqueue( header, client.GetTimestamp(), packetPoolToken, packetBuffer );
                    client.MessageNotifications.SignalContinuation();
                }
            }
            else
            {
                PendingResponseSource target;
                using ( AcquireActiveLock( client, out var acquired ) )
                {
                    if ( ! acquired )
                        return;

                    target = client.ResponseQueue.GetNext();
                }

                if ( target.Source is null )
                {
                    var error = Protocol.UnexpectedClientEndpointException( client, header );
                    MessageBrokerClientAwaitPacketEvent.Create( client, header, error ).Emit( client.Logger.AwaitPacket );
                    await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                    return;
                }

                if ( header.GetClientEndpoint() != MessageBrokerClientEndpoint.Pong )
                {
                    var packetLength = Protocol.AssertPacketLength( client, header );
                    if ( packetLength.Exception is not null )
                    {
                        MessageBrokerClientAwaitPacketEvent.Create( client, header, packetLength.Exception )
                            .Emit( client.Logger.AwaitPacket );

                        await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                        return;
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
                            MessageBrokerClientAwaitPacketEvent.Create( client, header, exc ).Emit( client.Logger.AwaitPacket );
                            await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                            return;
                        }
                    }
                }

                using ( AcquireActiveLock( client, out var acquired ) )
                {
                    if ( ! acquired )
                    {
                        Return( client, packetPoolToken );
                        return;
                    }

                    client.ResponseQueue.Signal( target.Source, IncomingPacketToken.Ok( header, packetPoolToken, packetBuffer ) );
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
        if ( exc is not null )
            MessageBrokerClientAwaitPacketEvent.Create( client, exc ).Emit( client.Logger.AwaitPacket );
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
        MessageBrokerClientAwaitPacketEvent.Create( client, client.DisposedException() ).Emit( client.Logger.AwaitPacket );
        acquired = false;
        return default;
    }
}
