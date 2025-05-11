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
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct MessageListener
{
    private Task? _task;

    private MessageListener(Task? task)
    {
        _task = task;
    }

    [Pure]
    internal static MessageListener Create()
    {
        return new MessageListener( null );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerRemoteClient client, Stream stream)
    {
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client, stream ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerRemoteClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.MessageListener._task = null;

        await client.DisconnectAsync().ConfigureAwait( false );
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
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerRemoteClient client, Stream stream)
    {
        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        while ( true )
        {
            client.Emit( MessageBrokerRemoteClientEvent.WaitingForMessage( client ) );

            Protocol.PacketHeader header;
            CancellationToken timeoutToken;
            try
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    timeoutToken = client.EventScheduler.ScheduleMaxReadTimeout( client );
                }

                await stream.ReadExactlyAsync( buffer, timeoutToken ).ConfigureAwait( false );
                header = Protocol.PacketHeader.Parse( buffer );
            }
            catch ( Exception exc )
            {
                client.Emit( MessageBrokerRemoteClientEvent.WaitingForMessage( client, exc ) );
                break;
            }

            client.Emit( MessageBrokerRemoteClientEvent.MessageReceived( client, header ) );

            var packetPoolToken = default( MemoryPoolToken<byte> );
            var packetBuffer = Memory<byte>.Empty;
            if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.Ping )
            {
                var packetLength = Protocol.AssertPacketLength( client, header );
                if ( packetLength.Exception is not null )
                {
                    client.Emit( MessageBrokerRemoteClientEvent.MessageRejected( client, header, packetLength.Exception ) );
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
                        client.Emit( MessageBrokerRemoteClientEvent.WaitingForMessage( client, exc ) );
                        packetPoolToken.Return( client );
                        break;
                    }
                }
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                {
                    packetPoolToken.Return( client );
                    return TaskStopReason.OwnerDisposed;
                }

                client.EventScheduler.ResetReadTimeout();
                client.MessageContextQueue.EnqueueRequest( header, packetPoolToken, packetBuffer );
                client.RequestHandler.SignalContinuation();
            }
        }

        return TaskStopReason.Error;
    }
}
