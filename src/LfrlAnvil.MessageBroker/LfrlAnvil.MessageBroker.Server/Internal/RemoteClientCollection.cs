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
using System.Collections.Generic;
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
    private SparseListSlim<MessageBrokerRemoteClient> _byId;
    private readonly Dictionary<string, MessageBrokerRemoteClient> _byName;
    private readonly int _tcpSocketBufferSize;
    private readonly bool _tcpNoDelay;
    private readonly int _minMemoryPoolSegmentLength;

    private RemoteClientCollection(MessageBrokerTcpServerOptions options, MemorySize? minMemoryPoolSegmentLength)
    {
        _byId = SparseListSlim<MessageBrokerRemoteClient>.Create();
        _byName = new Dictionary<string, MessageBrokerRemoteClient>( StringComparer.OrdinalIgnoreCase );
        _tcpSocketBufferSize = Defaults.Tcp.GetActualSocketBufferSize( options.SocketBufferSize );
        _tcpNoDelay = options.NoDelay ?? Defaults.Tcp.NoDelay;
        _minMemoryPoolSegmentLength = Defaults.Memory.GetActualMinSegmentLength( minMemoryPoolSegmentLength );
    }

    [Pure]
    internal static RemoteClientCollection Create(MessageBrokerTcpServerOptions options, MemorySize? minMemoryPoolSegmentLength)
    {
        return new RemoteClientCollection( options, minMemoryPoolSegmentLength );
    }

    internal MessageBrokerRemoteClient[] Dispose()
    {
        _byName.Clear();
        if ( _byId.Count == 0 )
            return Array.Empty<MessageBrokerRemoteClient>();

        var i = 0;
        var result = new MessageBrokerRemoteClient[_byId.Count];
        foreach ( var (_, client) in _byId )
            result[i++] = client;

        _byId.Clear();
        return result;
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._byId.Count;
    }

    [Pure]
    internal static MessageBrokerRemoteClient[] GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
        {
            if ( server.RemoteClientCollection._byId.Count == 0 )
                return Array.Empty<MessageBrokerRemoteClient>();

            var i = 0;
            var result = new MessageBrokerRemoteClient[server.RemoteClientCollection._byId.Count];
            foreach ( var (_, client) in server.RemoteClientCollection._byId )
                result[i++] = client;

            return result;
        }
    }

    [Pure]
    internal static MessageBrokerRemoteClient? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
        {
            ref var result = ref server.RemoteClientCollection._byId[id - 1];
            return Unsafe.IsNullRef( ref result ) ? null : result;
        }
    }

    [Pure]
    internal static MessageBrokerRemoteClient? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.RemoteClientCollection._byName.TryGetValue( name, out var client ) ? client : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<MessageBrokerRemoteClient> Register(MessageBrokerServer server, TcpClient tcp)
    {
        try
        {
            tcp.NoDelay = server.RemoteClientCollection._tcpNoDelay;
            tcp.ReceiveBufferSize = server.RemoteClientCollection._tcpSocketBufferSize;
            tcp.SendBufferSize = server.RemoteClientCollection._tcpSocketBufferSize;

            if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                tcp.Client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );

            using ( server.AcquireLock() )
            {
                if ( server.ShouldCancel )
                    return server.DisposedException();

                ref var client = ref server.RemoteClientCollection._byId.AddDefault( out var index );
                try
                {
                    client = new MessageBrokerRemoteClient(
                        index + 1,
                        server,
                        tcp,
                        server.RemoteClientCollection._minMemoryPoolSegmentLength );

                    return client;
                }
                catch
                {
                    server.RemoteClientCollection._byId.Remove( index );
                    throw;
                }
            }
        }
        catch ( Exception exc )
        {
            server.Emit( MessageBrokerServerEvent.ClientRejected( server, exc ) );
            var result = tcp.TryDispose().Exception;
            if ( result is not null )
                server.Emit( MessageBrokerServerEvent.Unexpected( server, result ) );

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

                ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    client.Server.RemoteClientCollection._byName,
                    name,
                    out var exists );

                if ( ! exists )
                    entry = client;

                return ! exists;
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
                {
                    client.Server.RemoteClientCollection._byId.Remove( client.Id - 1 );
                    client.Server.RemoteClientCollection._byName.Remove( client.Name );
                }
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }
}
