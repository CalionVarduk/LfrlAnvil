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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct ExternalNameCache
{
    private SegmentedSparseDictionary<Entry> _entries;

    private ExternalNameCache(int segmentLength)
    {
        _entries = SegmentedSparseDictionary<Entry>.Create( segmentLength );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ExternalNameCache Create()
    {
        return new ExternalNameCache( 64 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerExternalObject GetStream(int streamId)
    {
        return streamId == 0
            ? new MessageBrokerExternalObject( streamId )
            : new MessageBrokerExternalObject( streamId, _entries.TryGetValue( streamId - 1, out var entry ) ? entry.Stream : null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerExternalObject GetSender(MessageBrokerClient client, int senderId)
    {
        if ( senderId == 0 )
            return new MessageBrokerExternalObject( senderId );

        return senderId == client.Id
            ? new MessageBrokerExternalObject( senderId, client.Name )
            : new MessageBrokerExternalObject( senderId, _entries.TryGetValue( senderId - 1, out var entry ) ? entry.Client : null );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal string? SetSender(int senderId, string name)
    {
        ref var entry = ref _entries.GetValueRefOrAddDefault( senderId - 1, out var exists );
        var prev = exists ? entry.Client : null;
        entry.Client = name;
        return prev;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal string? SetStream(int streamId, string name)
    {
        ref var entry = ref _entries.GetValueRefOrAddDefault( streamId - 1, out var exists );
        var prev = exists ? entry.Stream : null;
        entry.Stream = name;
        return prev;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        _entries = SegmentedSparseDictionary<Entry>.Create( 64 );
    }

    private struct Entry
    {
        internal string? Client;
        internal string? Stream;

        [Pure]
        public override string ToString()
        {
            if ( Client is null )
                return Stream is null ? "<empty>" : $"Stream = '{Stream}'";

            return Stream is null ? $"Client = '{Client}'" : $"Client = '{Client}', Stream = '{Stream}'";
        }
    }
}
