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
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct ClientListener
{
    private Task? _task;

    private ClientListener(Task? task)
    {
        _task = task;
    }

    [Pure]
    internal static ClientListener Create()
    {
        return new ClientListener( null );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerServer server, TcpListener listener)
    {
        try
        {
            await RunCore( server, listener ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            ulong traceId;
            using ( server.AcquireLock() )
            {
                server.ClientListener._task = null;
                traceId = server.GetTraceId();
            }

            using ( MessageBrokerServerTraceEvent.CreateScope( server, traceId, MessageBrokerServerTraceEventType.Unexpected ) )
            {
                MessageBrokerServerErrorEvent.Create( server, traceId, exc ).Emit( server.Logger.Error );
                await server.DisposeAsync( traceId ).ConfigureAwait( false );
            }
        }

        Assume.IsGreaterThanOrEqualTo( server.State, MessageBrokerServerState.Disposing );
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
    private static async ValueTask RunCore(MessageBrokerServer server, TcpListener listener)
    {
        while ( true )
        {
            ulong traceId;
            MessageBrokerServerAwaitClientEvent.Create( server ).Emit( server.Logger.AwaitClient );

            TcpClient tcp;
            try
            {
                tcp = await listener.AcceptTcpClientAsync().ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                MessageBrokerServerAwaitClientEvent.Create( server, exc ).Emit( server.Logger.AwaitClient );

                using ( server.AcquireLock() )
                {
                    if ( ! server.TryBeginDispose() )
                        return;

                    server.ClientListener._task = null;
                    traceId = server.GetTraceId();
                }

                using ( MessageBrokerServerTraceEvent.CreateScope( server, traceId, MessageBrokerServerTraceEventType.Dispose ) )
                    await server.DisposeAsyncCore( traceId ).ConfigureAwait( false );

                return;
            }

            EndPoint? endPoint;
            try
            {
                endPoint = tcp.Client.RemoteEndPoint;
            }
            catch
            {
                endPoint = null;
            }

            if ( endPoint is not null )
                MessageBrokerServerAwaitClientEvent.Create( server, endPoint ).Emit( server.Logger.AwaitClient );

            using ( server.AcquireLock() )
                traceId = server.GetTraceId();

            using ( MessageBrokerServerTraceEvent.CreateScope( server, traceId, MessageBrokerServerTraceEventType.AcceptClient ) )
            {
                var result = RemoteClientCollection.Register( server, tcp, traceId );
                if ( result.Exception is not null )
                {
                    if ( result.Exception is MessageBrokerServerDisposedException )
                        return;

                    continue;
                }

                Assume.IsNotNull( result.Value );
                await result.Value.StartAsync( traceId ).ConfigureAwait( false );
            }
        }
    }
}
