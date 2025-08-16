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
using System.Runtime.InteropServices;
using System.Threading;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RemoteClientConnectorCollection
{
    private StackSlim<CancellationTokenSource> _readCancellationSourceCache;
    private SparseListSlim<MessageBrokerRemoteClientConnector> _connectors;
    private MessageBrokerRemoteClientConnector[]? _cache;
    private readonly int _tcpSocketBufferSize;
    private readonly bool _tcpNoDelay;

    private RemoteClientConnectorCollection(MessageBrokerServerTcpOptions options)
    {
        _readCancellationSourceCache = StackSlim<CancellationTokenSource>.Create();
        _connectors = SparseListSlim<MessageBrokerRemoteClientConnector>.Create();
        _cache = null;
        _tcpSocketBufferSize = Defaults.Tcp.GetActualSocketBufferSize( options.SocketBufferSize );
        _tcpNoDelay = options.NoDelay ?? Defaults.Tcp.NoDelay;
    }

    [Pure]
    internal static RemoteClientConnectorCollection Create(MessageBrokerServerTcpOptions options)
    {
        return new RemoteClientConnectorCollection( options );
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientConnectorCollection._connectors.Count;
    }

    [Pure]
    internal static ReadOnlyArray<MessageBrokerRemoteClientConnector> GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
        {
            server.RemoteClientConnectorCollection._cache ??= server.RemoteClientConnectorCollection.ToArray();
            return server.RemoteClientConnectorCollection._cache;
        }
    }

    [Pure]
    internal static MessageBrokerRemoteClientConnector? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
        {
            ref var obj = ref server.RemoteClientConnectorCollection._connectors[id - 1];
            return Unsafe.IsNullRef( ref obj ) ? null : obj;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static CancellationTokenSource GetCancellationSourceUnsafe(MessageBrokerServer server)
    {
        if ( ! server.RemoteClientConnectorCollection._readCancellationSourceCache.TryPop( out var result ) )
            result = new CancellationTokenSource();

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<MessageBrokerRemoteClientConnector> Register(MessageBrokerServer server, TcpClient tcp, ulong traceId)
    {
        MessageBrokerRemoteClientConnector connector;
        try
        {
            tcp.NoDelay = server.RemoteClientConnectorCollection._tcpNoDelay;
            tcp.ReceiveBufferSize = server.RemoteClientConnectorCollection._tcpSocketBufferSize;
            tcp.SendBufferSize = server.RemoteClientConnectorCollection._tcpSocketBufferSize;

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                tcp.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

            using ( server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                var cancellationSource = GetCancellationSourceUnsafe( server );
                try
                {
                    ref var entry = ref server.RemoteClientConnectorCollection._connectors.AddDefault( out var index );
                    entry = new MessageBrokerRemoteClientConnector( index + 1, server, tcp, cancellationSource );
                    connector = entry;
                }
                finally
                {
                    server.RemoteClientConnectorCollection._cache = null;
                }
            }
        }
        catch ( Exception exc )
        {
            var tcpExc = tcp.TryDispose().Exception;
            if ( server.Logger.Error is { } error )
            {
                error.Emit( MessageBrokerServerErrorEvent.Create( server, traceId, exc ) );
                if ( tcpExc is not null )
                    error.Emit( MessageBrokerServerErrorEvent.Create( server, traceId, tcpExc ) );
            }

            return exc;
        }

        if ( server.Logger.ClientAccepted is { } clientAccepted )
            clientAccepted.Emit( MessageBrokerServerClientAcceptedEvent.Create( server, traceId, connector ) );

        return connector;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void RemoveUnsafe(MessageBrokerRemoteClientConnector connector, CancellationTokenSource? cancellationTokenSource)
    {
        if ( cancellationTokenSource is not null )
            connector.Server.RemoteClientConnectorCollection._readCancellationSourceCache.Push( cancellationTokenSource );

        try
        {
            connector.Server.RemoteClientConnectorCollection._connectors.Remove( connector.Id - 1 );
        }
        finally
        {
            connector.Server.RemoteClientConnectorCollection._cache = null;
        }
    }

    internal void DisposeCancellationSources(ref Chain<Exception> exceptions)
    {
        foreach ( var source in _readCancellationSourceCache )
            source.TryCleanUp( ref exceptions );

        _readCancellationSourceCache = StackSlim<CancellationTokenSource>.Create();
    }

    internal MessageBrokerRemoteClientConnector[] DisposeUnsafe()
    {
        var result = _cache;
        _cache = null;
        result ??= ToArray();
        _connectors = SparseListSlim<MessageBrokerRemoteClientConnector>.Create();
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private MessageBrokerRemoteClientConnector[] ToArray()
    {
        if ( _connectors.IsEmpty )
            return Array.Empty<MessageBrokerRemoteClientConnector>();

        var i = 0;
        var result = new MessageBrokerRemoteClientConnector[_connectors.Count];
        foreach ( var (_, obj) in _connectors )
            result[i++] = obj;

        return result;
    }
}
