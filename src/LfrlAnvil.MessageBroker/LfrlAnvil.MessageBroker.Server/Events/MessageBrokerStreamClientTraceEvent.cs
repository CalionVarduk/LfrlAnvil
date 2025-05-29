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
/// Represents an event emitted by <see cref="MessageBrokerStream"/> that specifies correlation to a client trace.
/// </summary>
public readonly struct MessageBrokerStreamClientTraceEvent
{
    private MessageBrokerStreamClientTraceEvent(
        MessageBrokerStream stream,
        ulong traceId,
        MessageBrokerRemoteClient client,
        ulong clientTraceId)
    {
        Source = MessageBrokerStreamEventSource.Create( stream, traceId );
        Correlation = MessageBrokerRemoteClientEventSource.Create( client, clientTraceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerStreamEventSource Source { get; }

    /// <summary>
    /// Correlation to a client event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Correlation { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamClientTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ClientTrace] {Source}, Correlation = ({Correlation})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamClientTraceEvent Create(
        MessageBrokerStream stream,
        ulong traceId,
        MessageBrokerRemoteClient client,
        ulong clientTraceId)
    {
        return new MessageBrokerStreamClientTraceEvent( stream, traceId, client, clientTraceId );
    }
}
