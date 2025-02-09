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

        await client.DisposeAsync().ConfigureAwait( false );
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
            try
            {
                CancellationToken timeoutToken;
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    timeoutToken = client.SynchronousScheduler.ScheduleMaxReadTimeout( client );
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

            if ( header.GetServerEndpoint() != MessageBrokerServerEndpoint.PingRequest )
            {
                client.HandleUnexpectedEndpoint( header );
                break;
            }

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                client.SynchronousScheduler.ResetReadTimeout();
                client.MessageContextQueue.EnqueueRequest( header, default, default );
                client.RequestHandler.SignalContinuation();
            }
        }

        return TaskStopReason.Error;
    }
}
