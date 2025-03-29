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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueCollection
{
    private SparseListSlim<MessageBrokerQueue> _byId;
    private readonly Dictionary<string, MessageBrokerQueue> _byName;

    private QueueCollection(StringComparer nameComparer)
    {
        _byId = SparseListSlim<MessageBrokerQueue>.Create();
        _byName = new Dictionary<string, MessageBrokerQueue>( nameComparer );
    }

    [Pure]
    internal static QueueCollection Create()
    {
        return new QueueCollection( StringComparer.OrdinalIgnoreCase );
    }

    [Pure]
    internal static int GetCount(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
            return server.QueueCollection._byId.Count;
    }

    [Pure]
    internal static MessageBrokerQueue[] GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
        {
            if ( server.QueueCollection._byId.IsEmpty )
                return Array.Empty<MessageBrokerQueue>();

            var i = 0;
            var result = new MessageBrokerQueue[server.QueueCollection._byId.Count];
            foreach ( var (_, queue) in server.QueueCollection._byId )
                result[i++] = queue;

            return result;
        }
    }

    [Pure]
    internal static MessageBrokerQueue? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
        {
            ref var result = ref server.QueueCollection._byId[id - 1];
            return Unsafe.IsNullRef( ref result ) ? null : result;
        }
    }

    [Pure]
    internal static MessageBrokerQueue? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.QueueCollection._byName.TryGetValue( name, out var queue ) ? queue : null;
    }

    internal static Result Remove(MessageBrokerQueue queue)
    {
        try
        {
            using ( queue.Server.AcquireLock() )
            {
                if ( ! queue.Server.ShouldCancel )
                {
                    queue.Server.QueueCollection._byId.Remove( queue.Id - 1 );
                    queue.Server.QueueCollection._byName.Remove( queue.Name );
                }
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static void RemoveUnsafe(MessageBrokerQueue queue)
    {
        queue.Server.QueueCollection._byId.Remove( queue.Id - 1 );
        queue.Server.QueueCollection._byName.Remove( queue.Name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueue RegisterUnsafe(MessageBrokerServer server, string name, out bool created)
    {
        ref var queue = ref CollectionsMarshal.GetValueRefOrAddDefault( server.QueueCollection._byName, name, out var exists )!;
        if ( exists )
            created = false;
        else
        {
            created = true;
            ref var byId = ref server.QueueCollection._byId.AddDefault( out var index );
            try
            {
                queue = new MessageBrokerQueue( server, index + 1, name );
                byId = queue;
            }
            catch
            {
                server.QueueCollection._byId.Remove( index );
                server.QueueCollection._byName.Remove( name );
                throw;
            }
        }

        return queue;
    }

    internal MessageBrokerQueue[] DisposeUnsafe()
    {
        if ( _byId.IsEmpty )
            return Array.Empty<MessageBrokerQueue>();

        var i = 0;
        var result = new MessageBrokerQueue[_byId.Count];
        foreach ( var (_, queue) in _byId )
            result[i++] = queue;

        _byId.Clear();
        _byName.Clear();
        return result;
    }
}
