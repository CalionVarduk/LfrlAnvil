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

internal struct StreamCollection
{
    private ObjectStore<MessageBrokerStream> _store;

    private StreamCollection(StringComparer nameComparer)
    {
        _store = ObjectStore<MessageBrokerStream>.Create( nameComparer );
    }

    [Pure]
    internal static StreamCollection Create()
    {
        return new StreamCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.StreamCollection._store.Count;
    }

    [Pure]
    internal static ReadOnlyArray<MessageBrokerStream> GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.StreamCollection.GetAllUnsafe();
    }

    [Pure]
    internal static MessageBrokerStream? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
            return server.StreamCollection.TryGetByIdUnsafe( id );
    }

    [Pure]
    internal static MessageBrokerStream? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.StreamCollection._store.TryGetByName( name );
    }

    internal static Result Remove(MessageBrokerStream stream)
    {
        try
        {
            using ( stream.Server.AcquireLock() )
            {
                if ( ! stream.Server.IsDisposed )
                    RemoveUnsafe( stream );
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }

    [Pure]
    internal ReadOnlyArray<MessageBrokerStream> GetAllUnsafe()
    {
        return _store.GetAll();
    }

    [Pure]
    internal MessageBrokerStream? TryGetByIdUnsafe(int id)
    {
        return _store.TryGetById( id );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void RemoveUnsafe(MessageBrokerStream stream)
    {
        stream.Server.StreamCollection._store.Remove( stream.Id, stream.Name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStream RegisterUnsafe(MessageBrokerServer server, string name, out bool created)
    {
        var token = server.StreamCollection._store.GetOrAddNull( name );
        if ( token.Exists )
        {
            created = false;
            return token.GetObject();
        }

        try
        {
            created = true;
            return token.SetObject( ref server.StreamCollection._store, new MessageBrokerStream( server, token.Id, name ) );
        }
        catch
        {
            token.Revert( ref server.StreamCollection._store, name );
            throw;
        }
    }

    internal MessageBrokerStream[] DisposeUnsafe()
    {
        return _store.Clear();
    }

    internal static async ValueTask<Result> LoadStreamsAsync(MessageBrokerServer server, ulong traceId, CancellationToken cancellationToken)
    {
        await foreach ( var info in server.Storage.LoadStreamsAsync( server, traceId )
            .WithCancellation( cancellationToken )
            .ConfigureAwait( false ) )
        {
            MessageBrokerStream stream;
            using ( server.AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return exc;

                stream = new MessageBrokerStream( server, info.Key, info.Value.Name.Value.ToString(), info.Value.TraceId );
                if ( ! server.StreamCollection._store.TryAdd( stream.Id, stream.Name, stream ) )
                    ExceptionThrower.Throw( server.Exception( Resources.RecreatedStreamDuplicate( stream.Id, stream.Name ) ) );
            }

            ulong streamTraceId;
            using ( stream.AcquireLock() )
                streamTraceId = stream.GetTraceId();

            using ( MessageBrokerStreamTraceEvent.CreateScope( stream, streamTraceId, MessageBrokerStreamTraceEventType.Recreated ) )
            {
                if ( stream.Logger.ServerTrace is { } serverTrace )
                    serverTrace.Emit( MessageBrokerStreamServerTraceEvent.Create( stream, streamTraceId, traceId ) );
            }
        }

        return Result.Valid;
    }
}
