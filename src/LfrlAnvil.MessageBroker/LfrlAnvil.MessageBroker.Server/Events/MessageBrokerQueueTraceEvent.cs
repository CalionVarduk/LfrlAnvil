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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> due to an operation trace starting or ending.
/// </summary>
public readonly struct MessageBrokerQueueTraceEvent
{
    private MessageBrokerQueueTraceEvent(MessageBrokerQueue queue, ulong traceId, MessageBrokerQueueTraceEventType type)
    {
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
        Type = type;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See<see cref="MessageBrokerQueueTraceEventType"/> for more information.</remarks>
    public MessageBrokerQueueTraceEventType Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueTraceEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Trace:{Type}] {Source}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueTraceEvent Create(MessageBrokerQueue queue, ulong traceId, MessageBrokerQueueTraceEventType type)
    {
        return new MessageBrokerQueueTraceEvent( queue, traceId, type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Scope CreateScope(MessageBrokerQueue queue, ulong traceId, MessageBrokerQueueTraceEventType type)
    {
        var result = new Scope( queue, traceId, type );
        if ( queue.Logger.TraceStart is { } traceStart )
            traceStart.Emit( result.Event );

        return result;
    }

    internal readonly struct Scope : IDisposable
    {
        internal Scope(MessageBrokerQueue queue, ulong traceId, MessageBrokerQueueTraceEventType type)
        {
            Event = Create( queue, traceId, type );
        }

        internal readonly MessageBrokerQueueTraceEvent Event;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            if ( Event.Source.Queue.Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit( Event );
        }
    }
}
