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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> when attempting to connect to the server.
/// </summary>
public readonly struct MessageBrokerClientConnectingEvent
{
    private MessageBrokerClientConnectingEvent(MessageBrokerClient client, ulong traceId)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientConnectingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Connecting] {Source}, Server = {Source.Client.RemoteEndPoint}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientConnectingEvent Create(MessageBrokerClient client, ulong traceId)
    {
        return new MessageBrokerClientConnectingEvent( client, traceId );
    }
}
