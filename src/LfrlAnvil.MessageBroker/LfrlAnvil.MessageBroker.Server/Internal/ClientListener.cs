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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct ClientListener
{
    private readonly CancellationTokenSource _cancellation;
    private Task? _task;

    private ClientListener(CancellationTokenSource cancellation)
    {
        _cancellation = cancellation;
        _task = null;
    }

    [Pure]
    internal static ClientListener Create()
    {
        return new ClientListener( new CancellationTokenSource() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerServer server, TcpListener listener)
    {
        TaskStopReason stopReason;
        try
        {
            stopReason = await RunCore( server, listener ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            server.Emit( MessageBrokerServerEvent.Unexpected( server, exc ) );
            stopReason = TaskStopReason.Error;
        }

        if ( stopReason == TaskStopReason.OwnerDisposed )
            return;

        using ( server.AcquireLock() )
            server.ClientListener._task = null;

        await server.DisposeAsync().ConfigureAwait( false );
    }

    internal void Dispose()
    {
        try
        {
            _cancellation.Cancel();
        }
        catch
        {
            // NOTE: do nothing
        }

        _cancellation.TryDispose();
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
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerServer server, TcpListener listener)
    {
        while ( true )
        {
            server.Emit( MessageBrokerServerEvent.WaitingForClient( server ) );

            TcpClient tcp;
            try
            {
                tcp = await listener.AcceptTcpClientAsync( server.ClientListener._cancellation.Token ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                server.Emit( MessageBrokerServerEvent.WaitingForClient( server, exc ) );
                break;
            }

            var result = RemoteClientCollection.Register( server, tcp );
            if ( result.Exception is not null )
            {
                if ( result.Exception is MessageBrokerServerDisposedException )
                    return TaskStopReason.OwnerDisposed;

                continue;
            }

            Assume.IsNotNull( result.Value );
            result.Value.Start();
        }

        return TaskStopReason.Error;
    }
}
