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
/// Represents a collection of event callbacks for events emitted by a <see cref="MessageBrokerRemoteClient"/>.
/// </summary>
public readonly struct MessageBrokerRemoteClientLogger
{
    private MessageBrokerRemoteClientLogger(
        Action<MessageBrokerRemoteClientTraceEvent>? traceStart,
        Action<MessageBrokerRemoteClientTraceEvent>? traceEnd,
        Action<MessageBrokerRemoteClientServerTraceEvent>? serverTrace,
        Action<MessageBrokerRemoteClientHandshakingEvent>? handshaking,
        Action<MessageBrokerRemoteClientHandshakeEstablishedEvent>? handshakeEstablished,
        Action<MessageBrokerRemoteClientAwaitPacketEvent>? awaitPacket,
        Action<MessageBrokerRemoteClientSendPacketEvent>? sendPacket,
        Action<MessageBrokerRemoteClientReadPacketEvent>? readPacket,
        Action<MessageBrokerRemoteClientBindingPublisherEvent>? bindingPublisher,
        Action<MessageBrokerRemoteClientPublisherBoundEvent>? publisherBound,
        Action<MessageBrokerRemoteClientUnbindingPublisherEvent>? unbindingPublisher,
        Action<MessageBrokerRemoteClientPublisherUnboundEvent>? publisherUnbound,
        Action<MessageBrokerRemoteClientBindingListenerEvent>? bindingListener,
        Action<MessageBrokerRemoteClientListenerBoundEvent>? listenerBound,
        Action<MessageBrokerRemoteClientUnbindingListenerEvent>? unbindingListener,
        Action<MessageBrokerRemoteClientListenerUnboundEvent>? listenerUnbound,
        Action<MessageBrokerRemoteClientPushingMessageEvent>? pushingMessage,
        Action<MessageBrokerRemoteClientMessagePushedEvent>? messagePushed,
        Action<MessageBrokerRemoteClientProcessingMessageEvent>? processingMessage,
        Action<MessageBrokerRemoteClientMessageProcessedEvent>? messageProcessed,
        Action<MessageBrokerRemoteClientDisposingEvent>? disposing,
        Action<MessageBrokerRemoteClientDisposedEvent>? disposed,
        Action<MessageBrokerRemoteClientErrorEvent>? error)
    {
        TraceStart = traceStart;
        TraceEnd = traceEnd;
        ServerTrace = serverTrace;
        Handshaking = handshaking;
        HandshakeEstablished = handshakeEstablished;
        AwaitPacket = awaitPacket;
        SendPacket = sendPacket;
        ReadPacket = readPacket;
        BindingPublisher = bindingPublisher;
        PublisherBound = publisherBound;
        UnbindingPublisher = unbindingPublisher;
        PublisherUnbound = publisherUnbound;
        BindingListener = bindingListener;
        ListenerBound = listenerBound;
        UnbindingListener = unbindingListener;
        ListenerUnbound = listenerUnbound;
        PushingMessage = pushingMessage;
        MessagePushed = messagePushed;
        ProcessingMessage = processingMessage;
        MessageProcessed = messageProcessed;
        Disposing = disposing;
        Disposed = disposed;
        Error = error;
    }

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientTraceEvent"/> emitted during operation trace start.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientTraceEvent>? TraceStart;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientTraceEvent"/> emitted during operation trace end.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientTraceEvent>? TraceEnd;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientServerTraceEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientServerTraceEvent>? ServerTrace;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientHandshakingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientHandshakingEvent>? Handshaking;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientHandshakeEstablishedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientHandshakeEstablishedEvent>? HandshakeEstablished;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientAwaitPacketEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientAwaitPacketEvent>? AwaitPacket;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientSendPacketEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientSendPacketEvent>? SendPacket;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientReadPacketEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientReadPacketEvent>? ReadPacket;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientBindingPublisherEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientBindingPublisherEvent>? BindingPublisher;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientPublisherBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientPublisherBoundEvent>? PublisherBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientUnbindingPublisherEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientUnbindingPublisherEvent>? UnbindingPublisher;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientPublisherUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientPublisherUnboundEvent>? PublisherUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientBindingListenerEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientBindingListenerEvent>? BindingListener;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientListenerBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientListenerBoundEvent>? ListenerBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientUnbindingListenerEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientUnbindingListenerEvent>? UnbindingListener;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientListenerUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientListenerUnboundEvent>? ListenerUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientPushingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientPushingMessageEvent>? PushingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientMessagePushedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientMessagePushedEvent>? MessagePushed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientProcessingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientProcessingMessageEvent>? ProcessingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientMessageProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientMessageProcessedEvent>? MessageProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientDisposingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientDisposingEvent>? Disposing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientDisposedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientDisposedEvent>? Disposed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerRemoteClientErrorEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerRemoteClientErrorEvent>? Error;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerRemoteClientLogger"/> instance.
    /// </summary>
    /// <param name="traceStart">Optional <see cref="TraceStart"/> callback.</param>
    /// <param name="traceEnd">Optional <see cref="TraceEnd"/> callback.</param>
    /// <param name="serverTrace">Optional <see cref="ServerTrace"/> callback.</param>
    /// <param name="handshaking">Optional <see cref="Handshaking"/> callback.</param>
    /// <param name="handshakeEstablished">Optional <see cref="HandshakeEstablished"/> callback.</param>
    /// <param name="awaitPacket">Optional <see cref="AwaitPacket"/> callback.</param>
    /// <param name="sendPacket">Optional <see cref="SendPacket"/> callback.</param>
    /// <param name="readPacket">Optional <see cref="ReadPacket"/> callback.</param>
    /// <param name="bindingPublisher">Optional <see cref="BindingPublisher"/> callback.</param>
    /// <param name="publisherBound">Optional <see cref="PublisherBound"/> callback.</param>
    /// <param name="unbindingPublisher">Optional <see cref="UnbindingPublisher"/> callback.</param>
    /// <param name="publisherUnbound">Optional <see cref="PublisherUnbound"/> callback.</param>
    /// <param name="bindingListener">Optional <see cref="BindingListener"/> callback.</param>
    /// <param name="listenerBound">Optional <see cref="ListenerBound"/> callback.</param>
    /// <param name="unbindingListener">Optional <see cref="UnbindingListener"/> callback.</param>
    /// <param name="listenerUnbound">Optional <see cref="ListenerUnbound"/> callback.</param>
    /// <param name="pushingMessage">Optional <see cref="PushingMessage"/> callback.</param>
    /// <param name="messagePushed">Optional <see cref="MessagePushed"/> callback.</param>
    /// <param name="processingMessage">Optional <see cref="ProcessingMessage"/> callback.</param>
    /// <param name="messageProcessed">Optional <see cref="MessageProcessed"/> callback.</param>
    /// <param name="disposing">Optional <see cref="Disposing"/> callback.</param>
    /// <param name="disposed">Optional <see cref="Disposed"/> callback.</param>
    /// <param name="error">Optional <see cref="Error"/> callback.</param>
    /// <returns>New <see cref="MessageBrokerRemoteClientLogger"/> instance.</returns>
    [Pure]
    public static MessageBrokerRemoteClientLogger Create(
        Action<MessageBrokerRemoteClientTraceEvent>? traceStart = null,
        Action<MessageBrokerRemoteClientTraceEvent>? traceEnd = null,
        Action<MessageBrokerRemoteClientServerTraceEvent>? serverTrace = null,
        Action<MessageBrokerRemoteClientHandshakingEvent>? handshaking = null,
        Action<MessageBrokerRemoteClientHandshakeEstablishedEvent>? handshakeEstablished = null,
        Action<MessageBrokerRemoteClientAwaitPacketEvent>? awaitPacket = null,
        Action<MessageBrokerRemoteClientSendPacketEvent>? sendPacket = null,
        Action<MessageBrokerRemoteClientReadPacketEvent>? readPacket = null,
        Action<MessageBrokerRemoteClientBindingPublisherEvent>? bindingPublisher = null,
        Action<MessageBrokerRemoteClientPublisherBoundEvent>? publisherBound = null,
        Action<MessageBrokerRemoteClientUnbindingPublisherEvent>? unbindingPublisher = null,
        Action<MessageBrokerRemoteClientPublisherUnboundEvent>? publisherUnbound = null,
        Action<MessageBrokerRemoteClientBindingListenerEvent>? bindingListener = null,
        Action<MessageBrokerRemoteClientListenerBoundEvent>? listenerBound = null,
        Action<MessageBrokerRemoteClientUnbindingListenerEvent>? unbindingListener = null,
        Action<MessageBrokerRemoteClientListenerUnboundEvent>? listenerUnbound = null,
        Action<MessageBrokerRemoteClientPushingMessageEvent>? pushingMessage = null,
        Action<MessageBrokerRemoteClientMessagePushedEvent>? messagePushed = null,
        Action<MessageBrokerRemoteClientProcessingMessageEvent>? processingMessage = null,
        Action<MessageBrokerRemoteClientMessageProcessedEvent>? messageProcessed = null,
        Action<MessageBrokerRemoteClientDisposingEvent>? disposing = null,
        Action<MessageBrokerRemoteClientDisposedEvent>? disposed = null,
        Action<MessageBrokerRemoteClientErrorEvent>? error = null)
    {
        return new MessageBrokerRemoteClientLogger(
            traceStart,
            traceEnd,
            serverTrace,
            handshaking,
            handshakeEstablished,
            awaitPacket,
            sendPacket,
            readPacket,
            bindingPublisher,
            publisherBound,
            unbindingPublisher,
            publisherUnbound,
            bindingListener,
            listenerBound,
            unbindingListener,
            listenerUnbound,
            pushingMessage,
            messagePushed,
            processingMessage,
            messageProcessed,
            disposing,
            disposed,
            error );
    }
}
