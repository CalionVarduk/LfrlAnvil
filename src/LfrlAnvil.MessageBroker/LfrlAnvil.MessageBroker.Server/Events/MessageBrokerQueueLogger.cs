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
        Action<MessageBrokerQueueEnqueueingMessagesEvent>? enqueueingMessages,
        Action<MessageBrokerQueueMessagesEnqueuedEvent>? messagesEnqueued,
        Action<MessageBrokerQueueProcessingMessagesEvent>? processingMessages,
        Action<MessageBrokerQueueMessageProcessedEvent>? messageProcessed,
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
        EnqueueingMessages = enqueueingMessages;
        MessagesEnqueued = messagesEnqueued;
        ProcessingMessages = processingMessages;
        MessageProcessed = messageProcessed;
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
    /// Optional callback for a <see cref="MessageBrokerQueueEnqueueingMessagesEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueEnqueueingMessagesEvent>? EnqueueingMessages;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueMessagesEnqueuedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueMessagesEnqueuedEvent>? MessagesEnqueued;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueProcessingMessagesEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueProcessingMessagesEvent>? ProcessingMessages;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerQueueMessageProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerQueueMessageProcessedEvent>? MessageProcessed;

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
    /// <param name="enqueueingMessages">Optional <see cref="EnqueueingMessages"/> callback.</param>
    /// <param name="messagesEnqueued">Optional <see cref="MessagesEnqueued"/> callback.</param>
    /// <param name="processingMessages">Optional <see cref="ProcessingMessages"/> callback.</param>
    /// <param name="messageProcessed">Optional <see cref="MessageProcessed"/> callback.</param>
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
        Action<MessageBrokerQueueEnqueueingMessagesEvent>? enqueueingMessages = null,
        Action<MessageBrokerQueueMessagesEnqueuedEvent>? messagesEnqueued = null,
        Action<MessageBrokerQueueProcessingMessagesEvent>? processingMessages = null,
        Action<MessageBrokerQueueMessageProcessedEvent>? messageProcessed = null,
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
            enqueueingMessages,
            messagesEnqueued,
            processingMessages,
            messageProcessed,
            disposing,
            disposed,
            error );
    }
}
