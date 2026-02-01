// Copyright 2025-2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct ChannelCollection
{
    private ObjectStore<MessageBrokerChannel> _store;

    private ChannelCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerChannel>.Create( nameComparer );
    }

    [Pure]
    internal static ChannelCollection Create()
    {
        return new ChannelCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.ChannelCollection._store.Count;
    }

    [Pure]
    internal static ReadOnlyArray<MessageBrokerChannel> GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.ChannelCollection.GetAllUnsafe();
    }

    [Pure]
    internal static MessageBrokerChannel? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
            return server.ChannelCollection.TryGetByIdUnsafe( id );
    }

    [Pure]
    internal static MessageBrokerChannel? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.ChannelCollection._store.TryGetByName( name );
    }

    internal static Result Remove(MessageBrokerChannel channel)
    {
        try
        {
            using ( channel.Server.AcquireLock() )
            {
                if ( ! channel.Server.IsDisposed )
                    RemoveUnsafe( channel );
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }

    [Pure]
    internal ReadOnlyArray<MessageBrokerChannel> GetAllUnsafe()
    {
        return _store.GetAll();
    }

    [Pure]
    internal MessageBrokerChannel? TryGetByIdUnsafe(int id)
    {
        return _store.TryGetById( id );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void RemoveUnsafe(MessageBrokerChannel channel)
    {
        channel.Server.ChannelCollection._store.Remove( channel.Id, channel.Name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannel RegisterUnsafe(MessageBrokerServer server, string name, out bool created)
    {
        var token = server.ChannelCollection._store.GetOrAddNull( name );
        if ( token.Exists )
        {
            created = false;
            return token.GetObject();
        }

        try
        {
            created = true;
            return token.SetObject( ref server.ChannelCollection._store, new MessageBrokerChannel( server, token.Id, name ) );
        }
        catch
        {
            token.Revert( ref server.ChannelCollection._store, name );
            throw;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannel? TryRegisterUnsafe(
        MessageBrokerServer server,
        string name,
        bool createIfNotExists,
        ref bool created)
    {
        return createIfNotExists
            ? RegisterUnsafe( server, name, out created )
            : server.ChannelCollection._store.TryGetByName( name );
    }

    internal MessageBrokerChannel[] DisposeUnsafe()
    {
        return _store.Clear();
    }

    internal static async ValueTask<Result> LoadChannelsAsync(
        MessageBrokerServer server,
        ulong traceId,
        CancellationToken cancellationToken)
    {
        await foreach ( var info in server.Storage.LoadChannelsAsync( server, traceId )
            .WithCancellation( cancellationToken )
            .ConfigureAwait( false ) )
        {
            MessageBrokerChannel channel;
            using ( server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                // TODO: tests
                // - channel duplicate (by id or name)
                channel = new MessageBrokerChannel( server, info.Key, info.Value.Name.Value.ToString(), info.Value.TraceId );
                if ( ! server.ChannelCollection._store.TryAdd( channel.Id, channel.Name, channel ) )
                    ExceptionThrower.Throw( server.Exception( Resources.RecreatedChannelDuplicate( channel.Id, channel.Name ) ) );
            }

            ulong channelTraceId;
            using ( channel.AcquireLock() )
                channelTraceId = channel.GetTraceId();

            using ( MessageBrokerChannelTraceEvent.CreateScope( channel, channelTraceId, MessageBrokerChannelTraceEventType.Recreated ) )
            {
                if ( channel.Logger.ServerTrace is { } serverTrace )
                    serverTrace.Emit( MessageBrokerChannelServerTraceEvent.Create( channel, channelTraceId, traceId ) );
            }
        }

        return Result.Valid;
    }
}
