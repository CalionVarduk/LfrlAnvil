// Copyright 2026 Łukasz Furlepa
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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/> when data storage is about to start being loaded.
/// </summary>
public readonly struct MessageBrokerServerStorageLoadingEvent
{
    private MessageBrokerServerStorageLoadingEvent(MessageBrokerServer server, ulong traceId, string directory)
    {
        Source = MessageBrokerServerEventSource.Create( server, traceId );
        Directory = directory;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// Directory from which the data will be loaded.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerStorageLoadingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[StorageLoading] {Source}, Directory = '{Directory}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerStorageLoadingEvent Create(MessageBrokerServer server, ulong traceId, string directory)
    {
        return new MessageBrokerServerStorageLoadingEvent( server, traceId, directory );
    }
}
