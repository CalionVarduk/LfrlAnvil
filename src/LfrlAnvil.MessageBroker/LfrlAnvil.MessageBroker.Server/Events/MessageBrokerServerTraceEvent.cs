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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/> due to an operation trace starting or ending.
/// </summary>
public readonly struct MessageBrokerServerTraceEvent
{
    private MessageBrokerServerTraceEvent(MessageBrokerServer server, ulong traceId, MessageBrokerServerTraceEventType type)
    {
        Source = MessageBrokerServerEventSource.Create( server, traceId );
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerServerEventSource Source { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerServerTraceEventType"/> for more information.</remarks>
    public MessageBrokerServerTraceEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Trace:{Type}] {Source}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerTraceEvent Create(MessageBrokerServer server, ulong traceId, MessageBrokerServerTraceEventType type)
    {
        return new MessageBrokerServerTraceEvent( server, traceId, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Scope CreateScope(MessageBrokerServer server, ulong traceId, MessageBrokerServerTraceEventType type)
    {
        var result = new Scope( server, traceId, type );
        result.Event.Emit( server.Logger.TraceStart );
        return result;
    }

    internal readonly struct Scope : IDisposable
    {
        internal Scope(MessageBrokerServer server, ulong traceId, MessageBrokerServerTraceEventType type)
        {
            Event = Create( server, traceId, type );
        }

        internal readonly MessageBrokerServerTraceEvent Event;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            Event.Emit( Event.Source.Server.Logger.TraceEnd );
        }
    }
}
