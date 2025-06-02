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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> after successfully processing
/// system notification of <see cref="MessageBrokerSystemNotificationType.StreamName"/> type.
/// </summary>
public readonly struct MessageBrokerClientStreamNameProcessedEvent
{
    private MessageBrokerClientStreamNameProcessedEvent(
        MessageBrokerClient client,
        ulong traceId,
        int streamId,
        string? oldName,
        string newName)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        StreamId = streamId;
        OldName = oldName;
        NewName = newName;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Unique id of the sender.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Previous sender's name.
    /// </summary>
    public string? OldName { get; }

    /// <summary>
    /// Sender's name.
    /// </summary>
    public string NewName { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientStreamNameProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var oldName = OldName is not null ? $", OldName = '{OldName}'" : string.Empty;
        return $"[StreamNameProcessed] {Source}, StreamId = {StreamId}{oldName}, NewName = '{NewName}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientStreamNameProcessedEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        int streamId,
        string? oldName,
        string newName)
    {
        return new MessageBrokerClientStreamNameProcessedEvent( client, traceId, streamId, oldName, newName );
    }
}
