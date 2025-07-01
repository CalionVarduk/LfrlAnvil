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
/// Represents a collection of event callbacks for events emitted by a <see cref="MessageBrokerStream"/>.
/// </summary>
public readonly struct MessageBrokerStreamLogger
{
    private MessageBrokerStreamLogger(
        Action<MessageBrokerStreamTraceEvent>? traceStart,
        Action<MessageBrokerStreamTraceEvent>? traceEnd,
        Action<MessageBrokerStreamServerTraceEvent>? serverTrace,
        Action<MessageBrokerStreamClientTraceEvent>? clientTrace,
        Action<MessageBrokerStreamCreatedEvent>? created,
        Action<MessageBrokerStreamPublisherBoundEvent>? publisherBound,
        Action<MessageBrokerStreamPublisherUnboundEvent>? publisherUnbound,
        Action<MessageBrokerStreamMessagePushedEvent>? messagePushed,
        Action<MessageBrokerStreamProcessingMessageEvent>? processingMessage,
        Action<MessageBrokerStreamMessageProcessedEvent>? messageProcessed,
        Action<MessageBrokerStreamDisposingEvent>? disposing,
        Action<MessageBrokerStreamDisposedEvent>? disposed,
        Action<MessageBrokerStreamErrorEvent>? error)
    {
        TraceStart = traceStart;
        TraceEnd = traceEnd;
        ServerTrace = serverTrace;
        ClientTrace = clientTrace;
        Created = created;
        PublisherBound = publisherBound;
        PublisherUnbound = publisherUnbound;
        MessagePushed = messagePushed;
        ProcessingMessage = processingMessage;
        MessageProcessed = messageProcessed;
        Disposing = disposing;
        Disposed = disposed;
        Error = error;
    }

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamTraceEvent"/> emitted during operation trace start.
    /// </summary>
    public readonly Action<MessageBrokerStreamTraceEvent>? TraceStart;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamTraceEvent"/> emitted during operation trace end.
    /// </summary>
    public readonly Action<MessageBrokerStreamTraceEvent>? TraceEnd;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamServerTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamServerTraceEvent>? ServerTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamClientTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamClientTraceEvent>? ClientTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamCreatedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamCreatedEvent>? Created;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamPublisherBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamPublisherBoundEvent>? PublisherBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamPublisherUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamPublisherUnboundEvent>? PublisherUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamMessagePushedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamMessagePushedEvent>? MessagePushed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamProcessingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamProcessingMessageEvent>? ProcessingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamMessageProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamMessageProcessedEvent>? MessageProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamDisposingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamDisposingEvent>? Disposing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamDisposedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamDisposedEvent>? Disposed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerStreamErrorEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerStreamErrorEvent>? Error;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerStreamLogger"/> instance.
    /// </summary>
    /// <param name="traceStart">Optional <see cref="TraceStart"/> callback.</param>
    /// <param name="traceEnd">Optional <see cref="TraceEnd"/> callback.</param>
    /// <param name="serverTrace">Optional <see cref="ServerTrace"/> callback.</param>
    /// <param name="clientTrace">Optional <see cref="ClientTrace"/> callback.</param>
    /// <param name="created">Optional <see cref="Created"/> callback.</param>
    /// <param name="publisherBound">Optional <see cref="PublisherBound"/> callback.</param>
    /// <param name="publisherUnbound">Optional <see cref="PublisherUnbound"/> callback.</param>
    /// <param name="messagePushed">Optional <see cref="MessagePushed"/> callback.</param>
    /// <param name="processingMessage">Optional <see cref="ProcessingMessage"/> callback.</param>
    /// <param name="messageProcessed">Optional <see cref="MessageProcessed"/> callback.</param>
    /// <param name="disposing">Optional <see cref="Disposing"/> callback.</param>
    /// <param name="disposed">Optional <see cref="Disposed"/> callback.</param>
    /// <param name="error">Optional <see cref="Error"/> callback.</param>
    /// <returns>New <see cref="MessageBrokerStreamLogger"/> instance.</returns>
    [Pure]
    public static MessageBrokerStreamLogger Create(
        Action<MessageBrokerStreamTraceEvent>? traceStart = null,
        Action<MessageBrokerStreamTraceEvent>? traceEnd = null,
        Action<MessageBrokerStreamServerTraceEvent>? serverTrace = null,
        Action<MessageBrokerStreamClientTraceEvent>? clientTrace = null,
        Action<MessageBrokerStreamCreatedEvent>? created = null,
        Action<MessageBrokerStreamPublisherBoundEvent>? publisherBound = null,
        Action<MessageBrokerStreamPublisherUnboundEvent>? publisherUnbound = null,
        Action<MessageBrokerStreamMessagePushedEvent>? messagePushed = null,
        Action<MessageBrokerStreamProcessingMessageEvent>? processingMessage = null,
        Action<MessageBrokerStreamMessageProcessedEvent>? messageProcessed = null,
        Action<MessageBrokerStreamDisposingEvent>? disposing = null,
        Action<MessageBrokerStreamDisposedEvent>? disposed = null,
        Action<MessageBrokerStreamErrorEvent>? error = null)
    {
        return new MessageBrokerStreamLogger(
            traceStart,
            traceEnd,
            serverTrace,
            clientTrace,
            created,
            publisherBound,
            publisherUnbound,
            messagePushed,
            processingMessage,
            messageProcessed,
            disposing,
            disposed,
            error );
    }
}
