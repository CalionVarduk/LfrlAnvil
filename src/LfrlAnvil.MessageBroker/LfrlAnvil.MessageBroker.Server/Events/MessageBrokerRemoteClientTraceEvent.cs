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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> due to an operation trace starting or ending.
/// </summary>
public readonly struct MessageBrokerRemoteClientTraceEvent
{
    private MessageBrokerRemoteClientTraceEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        MessageBrokerRemoteClientTraceEventType type)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerRemoteClientTraceEventType"/> for more information.</remarks>
    public MessageBrokerRemoteClientTraceEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Trace:{Type}] {Source}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientTraceEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        MessageBrokerRemoteClientTraceEventType type)
    {
        return new MessageBrokerRemoteClientTraceEvent( client, traceId, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Scope CreateScope(MessageBrokerRemoteClient client, ulong traceId, MessageBrokerRemoteClientTraceEventType type)
    {
        var result = new Scope( client, traceId, type );
        result.Event.Emit( client.Logger.TraceStart );
        return result;
    }

    internal readonly struct Scope : IDisposable
    {
        internal Scope(MessageBrokerRemoteClient client, ulong traceId, MessageBrokerRemoteClientTraceEventType type)
        {
            Event = Create( client, traceId, type );
        }

        internal readonly MessageBrokerRemoteClientTraceEvent Event;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            Event.Emit( Event.Source.Client.Logger.TraceEnd );
        }
    }
}
