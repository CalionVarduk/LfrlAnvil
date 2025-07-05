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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/> due to an operation trace starting or ending.
/// </summary>
public readonly struct MessageBrokerChannelTraceEvent
{
    private MessageBrokerChannelTraceEvent(MessageBrokerChannel channel, ulong traceId, MessageBrokerChannelTraceEventType type)
    {
        Source = MessageBrokerChannelEventSource.Create( channel, traceId );
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerChannelEventSource Source { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerChannelTraceEventType"/> for more information.</remarks>
    public MessageBrokerChannelTraceEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Trace:{Type}] {Source}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelTraceEvent Create(
        MessageBrokerChannel channel,
        ulong traceId,
        MessageBrokerChannelTraceEventType type)
    {
        return new MessageBrokerChannelTraceEvent( channel, traceId, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Scope CreateScope(MessageBrokerChannel channel, ulong traceId, MessageBrokerChannelTraceEventType type)
    {
        var result = new Scope( channel, traceId, type );
        if ( channel.Logger.TraceStart is { } traceStart )
            traceStart.Emit( result.Event );

        return result;
    }

    internal readonly struct Scope : IDisposable
    {
        internal Scope(MessageBrokerChannel channel, ulong traceId, MessageBrokerChannelTraceEventType type)
        {
            Event = Create( channel, traceId, type );
        }

        internal readonly MessageBrokerChannelTraceEvent Event;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            if ( Event.Source.Channel.Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit( Event );
        }
    }
}
