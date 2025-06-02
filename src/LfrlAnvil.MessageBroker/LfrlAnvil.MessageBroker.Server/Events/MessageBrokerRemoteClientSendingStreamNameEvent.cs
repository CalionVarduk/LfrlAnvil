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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/>
/// when sending a <see cref="MessageBrokerSystemNotificationType.StreamName"/> system notification to the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientSendingStreamNameEvent
{
    private MessageBrokerRemoteClientSendingStreamNameEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int streamId,
        string streamName)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        StreamId = streamId;
        StreamName = streamName;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Stream's unique id.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Stream's name to send.
    /// </summary>
    public string StreamName { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientSendingStreamNameEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[SendingStreamName] {Source}, Stream = [{StreamId}] '{StreamName}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientSendingStreamNameEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int streamId,
        string streamName)
    {
        return new MessageBrokerRemoteClientSendingStreamNameEvent( client, traceId, streamId, streamName );
    }
}
