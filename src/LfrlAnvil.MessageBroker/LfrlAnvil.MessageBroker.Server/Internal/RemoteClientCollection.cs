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
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RemoteClientCollection
{
    private ObjectStore<MessageBrokerRemoteClient> _store;
    private readonly int _tcpSocketBufferSize;
    private readonly bool _tcpNoDelay;
    private readonly int _minMemoryPoolSegmentLength;

    private RemoteClientCollection(MessageBrokerTcpServerOptions options, MemorySize? minMemoryPoolSegmentLength)
    {
        _store = ObjectStore<MessageBrokerRemoteClient>.Create( StringComparer.OrdinalIgnoreCase );
        _tcpSocketBufferSize = Defaults.Tcp.GetActualSocketBufferSize( options.SocketBufferSize );
        _tcpNoDelay = options.NoDelay ?? Defaults.Tcp.NoDelay;
        _minMemoryPoolSegmentLength = Defaults.Memory.GetActualMinSegmentLength( minMemoryPoolSegmentLength );
    }

    [Pure]
    internal static RemoteClientCollection Create(MessageBrokerTcpServerOptions options, MemorySize? minMemoryPoolSegmentLength)
    {
        return new RemoteClientCollection( options, minMemoryPoolSegmentLength );
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._store.Count;
    }

    [Pure]
    internal static ReadOnlyArray<MessageBrokerRemoteClient> GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._store.GetAll();
    }

    [Pure]
    internal static MessageBrokerRemoteClient? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._store.TryGetById( id );
    }

    [Pure]
    internal static MessageBrokerRemoteClient? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._store.TryGetByName( name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<MessageBrokerRemoteClient> Register(MessageBrokerServer server, TcpClient tcp, ulong traceId)
    {
        try
        {
            tcp.NoDelay = server.RemoteClientCollection._tcpNoDelay;
            tcp.ReceiveBufferSize = server.RemoteClientCollection._tcpSocketBufferSize;
            tcp.SendBufferSize = server.RemoteClientCollection._tcpSocketBufferSize;

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                tcp.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

            MessageBrokerRemoteClient client;
            using ( server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                var token = server.RemoteClientCollection._store.RegisterNull();
                try
                {
                    client = token.SetObject(
                        ref server.RemoteClientCollection._store,
                        new MessageBrokerRemoteClient(
                            token.Id,
                            server,
                            tcp,
                            server.RemoteClientCollection._minMemoryPoolSegmentLength ) );
                }
                catch
                {
                    token.Revert( ref server.RemoteClientCollection._store );
                    throw;
                }
            }

            MessageBrokerServerClientAcceptedEvent.Create( server, traceId, client ).Emit( server.Logger.ClientAccepted );
            return client;
        }
        catch ( Exception exc )
        {
            MessageBrokerServerErrorEvent.Create( server, traceId, exc ).Emit( server.Logger.Error );
            var exception = tcp.TryDispose().Exception;
            if ( exception is not null )
                MessageBrokerServerErrorEvent.Create( server, traceId, exception ).Emit( server.Logger.Error );

            return exc;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<bool> RegisterName(MessageBrokerRemoteClient client, string name)
    {
        try
        {
            using ( client.Server.AcquireLock() )
            {
                if ( client.Server.ShouldCancel )
                    return client.Server.DisposedException();

                return client.Server.RemoteClientCollection._store.TrySetName( client, name );
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result Remove(MessageBrokerRemoteClient client)
    {
        try
        {
            using ( client.Server.AcquireLock() )
            {
                if ( ! client.Server.ShouldCancel )
                    client.Server.RemoteClientCollection._store.Remove( client.Id, client.Name );
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }

    internal MessageBrokerRemoteClient[] DisposeUnsafe()
    {
        return _store.Clear();
    }
}
