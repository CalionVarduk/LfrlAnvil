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
/// Represents an event emitted by <see cref="MessageBrokerServer"/>
/// when a new <see cref="MessageBrokerRemoteClientConnector"/> has started to wait for remote client's handshake.
/// </summary>
public readonly struct MessageBrokerServerConnectorStartedEvent
{
    private MessageBrokerServerConnectorStartedEvent(MessageBrokerRemoteClientConnector connector, ulong traceId)
    {
        Source = MessageBrokerServerEventSource.Create( connector.Server, traceId );
        Connector = connector;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClientConnector"/> associated with this event.
    /// </summary>
    public MessageBrokerRemoteClientConnector Connector { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerConnectorStartedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ConnectorStarted] {Source}, ConnectorId = {Connector.Id}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerConnectorStartedEvent Create(MessageBrokerRemoteClientConnector connector, ulong traceId)
    {
        return new MessageBrokerServerConnectorStartedEvent( connector, traceId );
    }
}
