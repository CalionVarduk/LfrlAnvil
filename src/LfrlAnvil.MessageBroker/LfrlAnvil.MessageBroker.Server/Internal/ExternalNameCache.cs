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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Internal;

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
    internal bool RequiresUpdate(MessageBrokerStream stream)
    {
        ref var entry = ref _entries.GetValueRefOrAddDefault( stream.Id, out var exists );
        return ! exists || ! stream.Name.Equals( entry.Stream, StringComparison.Ordinal );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool RequiresUpdate(MessageBrokerRemoteClient client, MessageBrokerRemoteClient sender)
    {
        if ( ReferenceEquals( client, sender ) )
            return false;

        ref var entry = ref _entries.GetValueRefOrAddDefault( sender.Id, out var exists );
        return ! exists || ! sender.Name.Equals( entry.Client, StringComparison.Ordinal );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryUpdate(MessageBrokerStream stream)
    {
        ref var entry = ref _entries.GetValueRefOrAddDefault( stream.Id, out var exists );
        var changed = ! exists || ! stream.Name.Equals( entry.Stream, StringComparison.Ordinal );
        entry.Stream = stream.Name;
        return changed;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryUpdate(MessageBrokerRemoteClient client, MessageBrokerRemoteClient sender)
    {
        if ( ReferenceEquals( client, sender ) )
            return false;

        ref var entry = ref _entries.GetValueRefOrAddDefault( sender.Id, out var exists );
        var changed = ! exists || ! sender.Name.Equals( entry.Client, StringComparison.Ordinal );
        entry.Client = sender.Name;
        return changed;
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
