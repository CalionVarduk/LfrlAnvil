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
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client.Internal;

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
    internal static async Task StartUnderlyingTask(MessageBrokerClient client, Stream stream)
    {
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( client, stream ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            client.Emit( MessageBrokerClientEvent.Unexpected( client, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( client.AcquireLock() )
            client.MessageListener._task = null;

        await client.DisposeAsync().ConfigureAwait( false );
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
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerClient client, Stream stream)
    {
        bool reverseEndianness;
        using ( client.AcquireLock() )
        {
            if ( client.ShouldCancel )
                return TaskStopReason.OwnerDisposed;

            reverseEndianness = client.IsServerLittleEndian != BitConverter.IsLittleEndian;
        }

        var buffer = new byte[Protocol.PacketHeader.Length].AsMemory();
        while ( true )
        {
            client.Emit( MessageBrokerClientEvent.WaitingForMessage( client ) );

            Protocol.PacketHeader header;
            CancellationToken timeoutToken;
            try
            {
                using ( client.AcquireLock() )
                {
                    if ( client.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    timeoutToken = client.SynchronousScheduler.GetReadTimeoutToken();
                }

                await stream.ReadExactlyAsync( buffer, timeoutToken ).ConfigureAwait( false );
                header = Protocol.PacketHeader.Parse( buffer, reverseEndianness );
            }
            catch ( Exception exc )
            {
                client.Emit( MessageBrokerClientEvent.WaitingForMessage( client, exc ) );
                break;
            }

            client.Emit( MessageBrokerClientEvent.MessageReceived( client, header ) );

            PendingResponseSource target;
            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                target = client.MessageContextQueue.GetNextPendingResponse();
            }

            if ( target.Source is null )
            {
                client.HandleUnexpectedEndpoint( header );
                break;
            }

            Assume.Equals( target.ServerEndpoint, MessageBrokerServerEndpoint.PingRequest );

            using ( client.AcquireLock() )
            {
                if ( client.ShouldCancel )
                    return TaskStopReason.OwnerDisposed;

                target.Source.SetResult( IncomingPacketToken.Ok( header, default, default ) );
            }
        }

        return TaskStopReason.Error;
    }
}
