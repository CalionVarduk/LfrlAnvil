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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/> that specifies correlation to a server trace.
/// </summary>
public readonly struct MessageBrokerChannelServerTraceEvent
{
    private MessageBrokerChannelServerTraceEvent(MessageBrokerChannel channel, ulong traceId, ulong serverTraceId)
    {
        Source = MessageBrokerChannelEventSource.Create( channel, traceId );
        Correlation = MessageBrokerServerEventSource.Create( channel.Server, serverTraceId );
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerChannelEventSource Source { get; }

    /// <summary>
    /// Correlation to a server event source.
    /// </summary>
    public MessageBrokerServerEventSource Correlation { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelServerTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[ServerTrace] {Source}, Correlation = ({Correlation})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelServerTraceEvent Create(MessageBrokerChannel channel, ulong traceId, ulong serverTraceId)
    {
        return new MessageBrokerChannelServerTraceEvent( channel, traceId, serverTraceId );
    }
}
