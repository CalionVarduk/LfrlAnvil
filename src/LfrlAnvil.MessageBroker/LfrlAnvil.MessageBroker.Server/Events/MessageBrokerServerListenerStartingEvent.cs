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
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/> when attempting to start the internal <see cref="TcpListener"/>.
/// </summary>
public readonly struct MessageBrokerServerListenerStartingEvent
{
    private MessageBrokerServerListenerStartingEvent(MessageBrokerServer server, ulong traceId)
    {
        Source = MessageBrokerServerEventSource.Create( server, traceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerListenerStartingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[ListenerStarting] {Source}, HandshakeTimeout = {Source.Server.HandshakeTimeout}, AcceptableMessageTimeout = [{Source.Server.AcceptableMessageTimeout.Min}, {Source.Server.AcceptableMessageTimeout.Max}], AcceptablePingInterval = [{Source.Server.AcceptablePingInterval.Min}, {Source.Server.AcceptablePingInterval.Max}]";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerListenerStartingEvent Create(MessageBrokerServer server, ulong traceId)
    {
        return new MessageBrokerServerListenerStartingEvent( server, traceId );
    }
}
