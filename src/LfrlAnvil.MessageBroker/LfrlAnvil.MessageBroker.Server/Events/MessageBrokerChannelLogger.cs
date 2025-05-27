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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents a collection of event callbacks for events emitted by a <see cref="MessageBrokerChannel"/>.
/// </summary>
public readonly struct MessageBrokerChannelLogger
{
    private MessageBrokerChannelLogger(
        Action<MessageBrokerChannelTraceEvent>? traceStart,
        Action<MessageBrokerChannelTraceEvent>? traceEnd,
        Action<MessageBrokerChannelServerTraceEvent>? serverTrace,
        Action<MessageBrokerChannelClientTraceEvent>? clientTrace,
        Action<MessageBrokerChannelCreatedEvent>? created,
        Action<MessageBrokerChannelPublisherBoundEvent>? publisherBound,
        Action<MessageBrokerChannelPublisherUnboundEvent>? publisherUnbound,
        Action<MessageBrokerChannelListenerBoundEvent>? listenerBound,
        Action<MessageBrokerChannelListenerUnboundEvent>? listenerUnbound,
        Action<MessageBrokerChannelDisposingEvent>? disposing,
        Action<MessageBrokerChannelDisposedEvent>? disposed,
        Action<MessageBrokerChannelErrorEvent>? error)
    {
        TraceStart = traceStart;
        TraceEnd = traceEnd;
        ServerTrace = serverTrace;
        ClientTrace = clientTrace;
        Created = created;
        PublisherBound = publisherBound;
        PublisherUnbound = publisherUnbound;
        ListenerBound = listenerBound;
        ListenerUnbound = listenerUnbound;
        Disposing = disposing;
        Disposed = disposed;
        Error = error;
    }

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelTraceEvent"/> emitted during operation trace start.
    /// </summary>
    public readonly Action<MessageBrokerChannelTraceEvent>? TraceStart;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelTraceEvent"/> emitted during operation trace end.
    /// </summary>
    public readonly Action<MessageBrokerChannelTraceEvent>? TraceEnd;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelServerTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelServerTraceEvent>? ServerTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelClientTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelClientTraceEvent>? ClientTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelCreatedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelCreatedEvent>? Created;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelPublisherBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelPublisherBoundEvent>? PublisherBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelPublisherUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelPublisherUnboundEvent>? PublisherUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelListenerBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelListenerBoundEvent>? ListenerBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelListenerUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelListenerUnboundEvent>? ListenerUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelDisposingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelDisposingEvent>? Disposing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelDisposedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelDisposedEvent>? Disposed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerChannelErrorEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerChannelErrorEvent>? Error;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerChannelLogger"/> instance.
    /// </summary>
    /// <param name="traceStart">Optional <see cref="TraceStart"/> callback.</param>
    /// <param name="traceEnd">Optional <see cref="TraceEnd"/> callback.</param>
    /// <param name="serverTrace">Optional <see cref="ServerTrace"/> callback.</param>
    /// <param name="clientTrace">Optional <see cref="ClientTrace"/> callback.</param>
    /// <param name="created">Optional <see cref="Created"/> callback.</param>
    /// <param name="publisherBound">Optional <see cref="PublisherBound"/> callback.</param>
    /// <param name="publisherUnbound">Optional <see cref="PublisherUnbound"/> callback.</param>
    /// <param name="listenerBound">Optional <see cref="ListenerBound"/> callback.</param>
    /// <param name="listenerUnbound">Optional <see cref="ListenerUnbound"/> callback.</param>
    /// <param name="disposing">Optional <see cref="Disposing"/> callback.</param>
    /// <param name="disposed">Optional <see cref="Disposed"/> callback.</param>
    /// <param name="error">Optional <see cref="Error"/> callback.</param>
    /// <returns>New <see cref="MessageBrokerChannelLogger"/> instance.</returns>
    [Pure]
    public static MessageBrokerChannelLogger Create(
        Action<MessageBrokerChannelTraceEvent>? traceStart = null,
        Action<MessageBrokerChannelTraceEvent>? traceEnd = null,
        Action<MessageBrokerChannelServerTraceEvent>? serverTrace = null,
        Action<MessageBrokerChannelClientTraceEvent>? clientTrace = null,
        Action<MessageBrokerChannelCreatedEvent>? created = null,
        Action<MessageBrokerChannelPublisherBoundEvent>? publisherBound = null,
        Action<MessageBrokerChannelPublisherUnboundEvent>? publisherUnbound = null,
        Action<MessageBrokerChannelListenerBoundEvent>? listenerBound = null,
        Action<MessageBrokerChannelListenerUnboundEvent>? listenerUnbound = null,
        Action<MessageBrokerChannelDisposingEvent>? disposing = null,
        Action<MessageBrokerChannelDisposedEvent>? disposed = null,
        Action<MessageBrokerChannelErrorEvent>? error = null)
    {
        return new MessageBrokerChannelLogger(
            traceStart,
            traceEnd,
            serverTrace,
            clientTrace,
            created,
            publisherBound,
            publisherUnbound,
            listenerBound,
            listenerUnbound,
            disposing,
            disposed,
            error );
    }
}
