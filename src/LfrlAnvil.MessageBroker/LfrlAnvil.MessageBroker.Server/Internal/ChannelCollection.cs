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

internal struct ChannelCollection
{
    private SparseListSlim<MessageBrokerChannel> _byId;
    private readonly Dictionary<string, MessageBrokerChannel> _byName;

    private ChannelCollection(StringComparer nameComparer)
    {
        _byId = SparseListSlim<MessageBrokerChannel>.Create();
        _byName = new Dictionary<string, MessageBrokerChannel>( nameComparer );
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
            return server.ChannelCollection._byId.Count;
    }

    [Pure]
    internal static MessageBrokerChannel[] GetAll(MessageBrokerServer server)
    {
        using ( server.AcquireLock() )
        {
            if ( server.ChannelCollection._byId.IsEmpty )
                return Array.Empty<MessageBrokerChannel>();

            var i = 0;
            var result = new MessageBrokerChannel[server.ChannelCollection._byId.Count];
            foreach ( var (_, client) in server.ChannelCollection._byId )
                result[i++] = client;

            return result;
        }
    }

    [Pure]
    internal static MessageBrokerChannel? TryGetById(MessageBrokerServer server, int id)
    {
        using ( server.AcquireLock() )
        {
            ref var result = ref server.ChannelCollection._byId[id - 1];
            return Unsafe.IsNullRef( ref result ) ? null : result;
        }
    }

    [Pure]
    internal static MessageBrokerChannel? TryGetByName(MessageBrokerServer server, string name)
    {
        using ( server.AcquireLock() )
            return server.ChannelCollection._byName.TryGetValue( name, out var client ) ? client : null;
    }

    internal readonly record struct RegistrationResult(MessageBrokerChannel Channel, bool Exists);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static RegistrationResult Register(MessageBrokerServer server, string name)
    {
        ref var channel = ref CollectionsMarshal.GetValueRefOrAddDefault( server.ChannelCollection._byName, name, out var exists )!;
        if ( ! exists )
        {
            ref var byId = ref server.ChannelCollection._byId.AddDefault( out var index );
            try
            {
                channel = new MessageBrokerChannel( server, index + 1, name );
                byId = channel;
            }
            catch
            {
                server.ChannelCollection._byId.Remove( index );
                server.ChannelCollection._byName.Remove( name );
                throw;
            }
        }

        return new RegistrationResult( channel, exists );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static RegistrationResult? TryRegister(MessageBrokerServer server, string name, bool createIfNotExists)
    {
        ref var channel = ref CollectionsMarshal.GetValueRefOrAddDefault( server.ChannelCollection._byName, name, out var exists );
        if ( exists )
            return new RegistrationResult( channel!, exists );

        if ( createIfNotExists )
        {
            ref var byId = ref server.ChannelCollection._byId.AddDefault( out var index );
            try
            {
                channel = new MessageBrokerChannel( server, index + 1, name );
                byId = channel;
            }
            catch
            {
                server.ChannelCollection._byId.Remove( index );
                server.ChannelCollection._byName.Remove( name );
                throw;
            }

            return new RegistrationResult( channel, exists );
        }

        return null;
    }

    internal static Result Remove(MessageBrokerChannel channel)
    {
        try
        {
            using ( channel.Server.AcquireLock() )
            {
                if ( ! channel.Server.ShouldCancel )
                {
                    channel.Server.ChannelCollection._byId.Remove( channel.Id - 1 );
                    channel.Server.ChannelCollection._byName.Remove( channel.Name );
                }
            }
        }
        catch ( Exception exc )
        {
            return exc;
        }

        return Result.Valid;
    }

    internal MessageBrokerChannel[] Dispose()
    {
        if ( _byId.IsEmpty )
            return Array.Empty<MessageBrokerChannel>();

        var i = 0;
        var result = new MessageBrokerChannel[_byId.Count];
        foreach ( var (_, channel) in _byId )
            result[i++] = channel;

        _byId.Clear();
        _byName.Clear();
        return result;
    }
}
