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
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> due to an operation trace starting or ending.
/// </summary>
public readonly struct MessageBrokerClientTraceEvent
{
    private MessageBrokerClientTraceEvent(MessageBrokerClient client, ulong traceId, MessageBrokerClientTraceEventType type)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerClientTraceEventType"/> for more information.</remarks>
    public MessageBrokerClientTraceEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Trace:{Type}] {Source}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientTraceEvent Create(MessageBrokerClient client, ulong traceId, MessageBrokerClientTraceEventType type)
    {
        return new MessageBrokerClientTraceEvent( client, traceId, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Scope CreateScope(MessageBrokerClient client, ulong traceId, MessageBrokerClientTraceEventType type)
    {
        var result = new Scope( client, traceId, type );
        if ( client.Logger.TraceStart is { } traceStart )
            traceStart.Emit( result.Event );

        return result;
    }

    internal readonly struct Scope : IDisposable
    {
        internal Scope(MessageBrokerClient client, ulong traceId, MessageBrokerClientTraceEventType type)
        {
            Event = Create( client, traceId, type );
        }

        internal readonly MessageBrokerClientTraceEvent Event;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            if ( Event.Source.Client.Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit( Event );
        }
    }
}
