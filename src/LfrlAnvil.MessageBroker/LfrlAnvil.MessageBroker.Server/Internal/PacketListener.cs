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

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client, Stream stream)
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

            using ( MessageBrokerRemoteClientTraceEvent.CreateScope( client, traceId, MessageBrokerRemoteClientTraceEventType.Unexpected ) )
            {
                MessageBrokerRemoteClientErrorEvent.Create( client, traceId, exc ).Emit( client.Logger.Error );
                await client.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( client.State, MessageBrokerRemoteClientState.Disposing );
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
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        while ( true )
        {
            MessageBrokerRemoteClientAwaitPacketEvent.Create( client ).Emit( client.Logger.AwaitPacket );

            Protocol.PacketHeader header;
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
                header = Protocol.PacketHeader.Parse( buffer );
            }
            catch ( Exception exc )
            {
                MessageBrokerRemoteClientAwaitPacketEvent.Create( client, exc ).Emit( client.Logger.AwaitPacket );

                ulong traceId;
                var isCancelException = exc is OperationCanceledException cancelExc && cancelExc.CancellationToken == timeoutToken;

                using ( client.AcquireLock() )
                {
                    if ( isCancelException && ! client.TryBeginDispose() )
                        return;

                    client.PacketListener._task = null;
                    traceId = client.GetTraceId();
                }

                using ( MessageBrokerRemoteClientTraceEvent.CreateScope(
                    client,
                    traceId,
                    MessageBrokerRemoteClientTraceEventType.Dispose ) )
                {
                    ValueTask disposeTask;
                    if ( isCancelException )
                    {
                        var error = new MessageBrokerRemoteClientRequestTimeoutException( client );
                        MessageBrokerRemoteClientErrorEvent.Create( client, traceId, error ).Emit( client.Logger.Error );
                        disposeTask = client.DisposeAsyncCore( traceId );
                    }
                    else
                        disposeTask = client.DisposeAsync( traceId );

                    await disposeTask.ConfigureAwait( false );
                }

                return;
            }

            MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header ).Emit( client.Logger.AwaitPacket );

            var packetPoolToken = MemoryPoolToken<byte>.Empty;
            var packetBuffer = Memory<byte>.Empty;
            if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.Ping )
            {
                var packetLength = Protocol.AssertPacketLength( client, header );
                if ( packetLength.Exception is not null )
                {
                    MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, packetLength.Exception )
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
                        MessageBrokerRemoteClientAwaitPacketEvent.Create( client, header, exc ).Emit( client.Logger.AwaitPacket );
                        await DisposeClientAsync( client, packetPoolToken ).ConfigureAwait( false );
                        return;
                    }
                }
            }

            using ( AcquireActiveLock( client, out var acquired ) )
            {
                if ( ! acquired )
                {
                    var exc = packetPoolToken.Return();
                    if ( exc is not null )
                        MessageBrokerRemoteClientAwaitPacketEvent.Create( client, exc ).Emit( client.Logger.AwaitPacket );

                    return;
                }

                client.EventScheduler.ResetReadTimeout();
                client.RequestQueue.Enqueue( header, packetPoolToken, packetBuffer );
                client.RequestHandler.SignalContinuation();
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
            await client.DisposeAsync( traceId ).ConfigureAwait( false );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireActiveLock(MessageBrokerRemoteClient client, out bool acquired)
    {
        var @lock = client.AcquireLock();
        if ( ! client.ShouldCancel )
        {
            acquired = true;
            return @lock;
        }

        @lock.Dispose();
        MessageBrokerRemoteClientAwaitPacketEvent.Create( client, client.DisposedException() ).Emit( client.Logger.AwaitPacket );
        acquired = false;
        return default;
    }
}
