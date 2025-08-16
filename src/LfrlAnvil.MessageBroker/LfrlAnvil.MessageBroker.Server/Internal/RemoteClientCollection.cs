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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct RemoteClientCollection
{
    private ObjectStore<MessageBrokerRemoteClient> _store;

    private RemoteClientCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerRemoteClient>.Create( nameComparer );
    }

    [Pure]
    internal static RemoteClientCollection Create()
    {
        return new RemoteClientCollection( StringComparer.OrdinalIgnoreCase );
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
    internal static Result<MessageBrokerRemoteClient> TryRegisterUnsafe(
        MessageBrokerServer server,
        TcpClient tcp,
        Stream stream,
        MemoryPool<byte> memoryPool,
        string name,
        in Protocol.HandshakeRequestHeader handshake,
        out bool alreadyConnected)
    {
        alreadyConnected = false;
        try
        {
            var token = server.RemoteClientCollection._store.GetOrAddNull( name );
            if ( token.Exists )
            {
                alreadyConnected = true;
                return server.Exception( Resources.ClientAlreadyConnected( name ) );
            }

            try
            {
                return token.SetObject(
                    ref server.RemoteClientCollection._store,
                    new MessageBrokerRemoteClient( token.Id, server, name, tcp, stream, memoryPool, in handshake ) );
            }
            catch
            {
                token.Revert( ref server.RemoteClientCollection._store, name );
                throw;
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
