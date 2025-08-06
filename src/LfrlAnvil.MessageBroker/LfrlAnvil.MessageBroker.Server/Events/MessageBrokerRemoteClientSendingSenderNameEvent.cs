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
/// when sending a <see cref="MessageBrokerSystemNotificationType.SenderName"/> system notification to the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientSendingSenderNameEvent
{
    private MessageBrokerRemoteClientSendingSenderNameEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        MessageBrokerRemoteClient sender)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        Sender = sender;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Sender <see cref="MessageBrokerRemoteClient"/> whose name is to be sent.
    /// </summary>
    public MessageBrokerRemoteClient Sender { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientSendingSenderNameEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[SendingSenderName] {Source}, Sender = [{Sender.Id}] '{Sender.Name}'";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientSendingSenderNameEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        MessageBrokerRemoteClient sender)
    {
        return new MessageBrokerRemoteClientSendingSenderNameEvent( client, traceId, sender );
    }
}
