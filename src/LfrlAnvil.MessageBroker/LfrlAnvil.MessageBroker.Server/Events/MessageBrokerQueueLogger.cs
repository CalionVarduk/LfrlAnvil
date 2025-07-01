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
/// Represents a collection of event callbacks for events emitted by a <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueueLogger
{
    private MessageBrokerQueueLogger(
        Action<MessageBrokerQueueTraceEvent>? traceStart,
        Action<MessageBrokerQueueTraceEvent>? traceEnd,
        Action<MessageBrokerQueueClientTraceEvent>? clientTrace,
        Action<MessageBrokerQueueStreamTraceEvent>? streamTrace,
        Action<MessageBrokerQueueCreatedEvent>? created,
        Action<MessageBrokerQueueListenerBoundEvent>? listenerBound,
        Action<MessageBrokerQueueListenerUnboundEvent>? listenerUnbound,
        Action<MessageBrokerQueueEnqueueingMessageEvent>? enqueueingMessage,
        Action<MessageBrokerQueueMessageEnqueuedEvent>? messageEnqueued,
        Action<MessageBrokerQueueProcessingMessageEvent>? processingMessage,
        Action<MessageBrokerQueueMessageProcessedEvent>? messageProcessed,
        Action<MessageBrokerQueueMessageDiscardedEvent>? messageDiscarded,
        Action<MessageBrokerQueueAckProcessedEvent>? ackProcessed,
        Action<MessageBrokerQueueNegativeAckProcessedEvent>? negativeAckProcessed,
        Action<MessageBrokerQueueDisposingEvent>? disposing,
        Action<MessageBrokerQueueDisposedEvent>? disposed,
        Action<MessageBrokerQueueErrorEvent>? error)
    {
        TraceStart = traceStart;
        TraceEnd = traceEnd;
        StreamTrace = streamTrace;
        ClientTrace = clientTrace;
        Created = created;
        ListenerBound = listenerBound;
        ListenerUnbound = listenerUnbound;
        EnqueueingMessage = enqueueingMessage;
        MessageEnqueued = messageEnqueued;
        ProcessingMessage = processingMessage;
        MessageProcessed = messageProcessed;
        MessageDiscarded = messageDiscarded;
        AckProcessed = ackProcessed;
        NegativeAckProcessed = negativeAckProcessed;
        Disposing = disposing;
        Disposed = disposed;
        Error = error;
    }

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueTraceEvent"/> emitted during operation trace start.
    /// </summary>
    public readonly Action<MessageBrokerQueueTraceEvent>? TraceStart;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueTraceEvent"/> emitted during operation trace end.
    /// </summary>
    public readonly Action<MessageBrokerQueueTraceEvent>? TraceEnd;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueStreamTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueStreamTraceEvent>? StreamTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueClientTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueClientTraceEvent>? ClientTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueCreatedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueCreatedEvent>? Created;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueListenerBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueListenerBoundEvent>? ListenerBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueListenerUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueListenerUnboundEvent>? ListenerUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueEnqueueingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueEnqueueingMessageEvent>? EnqueueingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueMessageEnqueuedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueMessageEnqueuedEvent>? MessageEnqueued;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueProcessingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueProcessingMessageEvent>? ProcessingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueMessageProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueMessageProcessedEvent>? MessageProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueMessageDiscardedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueMessageDiscardedEvent>? MessageDiscarded;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueAckProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueAckProcessedEvent>? AckProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueNegativeAckProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueNegativeAckProcessedEvent>? NegativeAckProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueDisposingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueDisposingEvent>? Disposing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueDisposedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueDisposedEvent>? Disposed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueErrorEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueErrorEvent>? Error;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerQueueLogger"/> instance.
    /// </summary>
    /// <param name="traceStart">Optional <see cref="TraceStart"/> callback.</param>
    /// <param name="traceEnd">Optional <see cref="TraceEnd"/> callback.</param>
    /// <param name="clientTrace">Optional <see cref="ClientTrace"/> callback.</param>
    /// <param name="streamTrace">Optional <see cref="StreamTrace"/> callback.</param>
    /// <param name="created">Optional <see cref="Created"/> callback.</param>
    /// <param name="listenerBound">Optional <see cref="ListenerBound"/> callback.</param>
    /// <param name="listenerUnbound">Optional <see cref="ListenerUnbound"/> callback.</param>
    /// <param name="enqueueingMessage">Optional <see cref="EnqueueingMessage"/> callback.</param>
    /// <param name="messageEnqueued">Optional <see cref="MessageEnqueued"/> callback.</param>
    /// <param name="processingMessage">Optional <see cref="ProcessingMessage"/> callback.</param>
    /// <param name="messageProcessed">Optional <see cref="MessageProcessed"/> callback.</param>
    /// <param name="messageDiscarded">Optional <see cref="MessageDiscarded"/> callback.</param>
    /// <param name="ackProcessed">Optional <see cref="AckProcessed"/> callback.</param>
    /// <param name="negativeAckProcessed">Optional <see cref="NegativeAckProcessed"/> callback.</param>
    /// <param name="disposing">Optional <see cref="Disposing"/> callback.</param>
    /// <param name="disposed">Optional <see cref="Disposed"/> callback.</param>
    /// <param name="error">Optional <see cref="Error"/> callback.</param>
    /// <returns>New <see cref="MessageBrokerQueueLogger"/> instance.</returns>
    [Pure]
    public static MessageBrokerQueueLogger Create(
        Action<MessageBrokerQueueTraceEvent>? traceStart = null,
        Action<MessageBrokerQueueTraceEvent>? traceEnd = null,
        Action<MessageBrokerQueueClientTraceEvent>? clientTrace = null,
        Action<MessageBrokerQueueStreamTraceEvent>? streamTrace = null,
        Action<MessageBrokerQueueCreatedEvent>? created = null,
        Action<MessageBrokerQueueListenerBoundEvent>? listenerBound = null,
        Action<MessageBrokerQueueListenerUnboundEvent>? listenerUnbound = null,
        Action<MessageBrokerQueueEnqueueingMessageEvent>? enqueueingMessage = null,
        Action<MessageBrokerQueueMessageEnqueuedEvent>? messageEnqueued = null,
        Action<MessageBrokerQueueProcessingMessageEvent>? processingMessage = null,
        Action<MessageBrokerQueueMessageProcessedEvent>? messageProcessed = null,
        Action<MessageBrokerQueueMessageDiscardedEvent>? messageDiscarded = null,
        Action<MessageBrokerQueueAckProcessedEvent>? ackProcessed = null,
        Action<MessageBrokerQueueNegativeAckProcessedEvent>? negativeAckProcessed = null,
        Action<MessageBrokerQueueDisposingEvent>? disposing = null,
        Action<MessageBrokerQueueDisposedEvent>? disposed = null,
        Action<MessageBrokerQueueErrorEvent>? error = null)
    {
        return new MessageBrokerQueueLogger(
            traceStart,
            traceEnd,
            clientTrace,
            streamTrace,
            created,
            listenerBound,
            listenerUnbound,
            enqueueingMessage,
            messageEnqueued,
            processingMessage,
            messageProcessed,
            messageDiscarded,
            ackProcessed,
            negativeAckProcessed,
            disposing,
            disposed,
            error );
    }
}
