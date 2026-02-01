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
/// Represents an event emitted by <see cref="MessageBrokerServer"/> when data storage has been successfully loaded.
/// </summary>
public readonly struct MessageBrokerServerStorageLoadedEvent
{
    private MessageBrokerServerStorageLoadedEvent(
        MessageBrokerServer server,
        ulong traceId,
        string directory,
        int channelCount,
        int streamCount,
        int clientCount,
        int queueCount,
        int publisherCount,
        int listenerCount)
    {
        Source = MessageBrokerServerEventSource.Create( server, traceId );
        Directory = directory;
        ChannelCount = channelCount;
        StreamCount = streamCount;
        ClientCount = clientCount;
        QueueCount = queueCount;
        PublisherCount = publisherCount;
        ListenerCount = listenerCount;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// Directory from which the data has been loaded.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// Specifies the number of loaded channels.
    /// </summary>
    public int ChannelCount { get; }

    /// <summary>
    /// Specifies the number of loaded streams.
    /// </summary>
    public int StreamCount { get; }

    /// <summary>
    /// Specifies the number of loaded clients.
    /// </summary>
    public int ClientCount { get; }

    /// <summary>
    /// Specifies the number of loaded queues.
    /// </summary>
    public int QueueCount { get; }

    /// <summary>
    /// Specifies the number of loaded publishers.
    /// </summary>
    public int PublisherCount { get; }

    /// <summary>
    /// Specifies the number of loaded listeners.
    /// </summary>
    public int ListenerCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerStorageLoadedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[StorageLoaded] {Source}, Directory = '{Directory}', ChannelCount = {ChannelCount}, StreamCount = {StreamCount}, ClientCount = {ClientCount}, QueueCount = {QueueCount}, PublisherCount = {PublisherCount}, ListenerCount = {ListenerCount}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerStorageLoadedEvent Create(
        MessageBrokerServer server,
        ulong traceId,
        string directory,
        int channelCount,
        int streamCount,
        int clientCount,
        int queueCount,
        int publisherCount,
        int listenerCount)
    {
        return new MessageBrokerServerStorageLoadedEvent(
            server,
            traceId,
            directory,
            channelCount,
            streamCount,
            clientCount,
            queueCount,
            publisherCount,
            listenerCount );
    }
}
