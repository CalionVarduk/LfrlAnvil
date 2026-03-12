// Copyright 2025-2026 Łukasz Furlepa
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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents a collection of event callbacks for events emitted by a <see cref="MessageBrokerClient"/>.
/// </summary>
public readonly struct MessageBrokerClientLogger
{
    private MessageBrokerClientLogger(
        Action<MessageBrokerClientTraceEvent>? traceStart,
        Action<MessageBrokerClientTraceEvent>? traceEnd,
        Action<MessageBrokerClientConnectingEvent>? connecting,
        Action<MessageBrokerClientConnectedEvent>? connected,
        Action<MessageBrokerClientHandshakingEvent>? handshaking,
        Action<MessageBrokerClientHandshakeEstablishedEvent>? handshakeEstablished,
        Action<MessageBrokerClientAwaitPacketEvent>? awaitPacket,
        Action<MessageBrokerClientSendPacketEvent>? sendPacket,
        Action<MessageBrokerClientReadPacketEvent>? readPacket,
        Action<MessageBrokerClientBindingPublisherEvent>? bindingPublisher,
        Action<MessageBrokerClientPublisherBoundEvent>? publisherBound,
        Action<MessageBrokerClientUnbindingPublisherEvent>? unbindingPublisher,
        Action<MessageBrokerClientPublisherUnboundEvent>? publisherUnbound,
        Action<MessageBrokerClientBindingListenerEvent>? bindingListener,
        Action<MessageBrokerClientListenerBoundEvent>? listenerBound,
        Action<MessageBrokerClientUnbindingListenerEvent>? unbindingListener,
        Action<MessageBrokerClientListenerUnboundEvent>? listenerUnbound,
        Action<MessageBrokerClientPushingMessageEvent>? pushingMessage,
        Action<MessageBrokerClientMessagePushedEvent>? messagePushed,
        Action<MessageBrokerClientProcessingMessageEvent>? processingMessage,
        Action<MessageBrokerClientMessageProcessedEvent>? messageProcessed,
        Action<MessageBrokerClientAcknowledgingMessageEvent>? acknowledgingMessage,
        Action<MessageBrokerClientMessageAcknowledgedEvent>? messageAcknowledged,
        Action<MessageBrokerClientQueryingDeadLetterEvent>? queryingDeadLetter,
        Action<MessageBrokerClientDeadLetterQueriedEvent>? deadLetterQueried,
        Action<MessageBrokerClientProcessingSystemNotificationEvent>? processingSystemNotification,
        Action<MessageBrokerClientSenderNameProcessedEvent>? senderNameProcessed,
        Action<MessageBrokerClientStreamNameProcessedEvent>? streamNameProcessed,
        Action<MessageBrokerClientPublisherDeletedEvent>? publisherDeleted,
        Action<MessageBrokerClientListenerDeletedEvent>? listenerDeleted,
        Action<MessageBrokerClientDisposingEvent>? disposing,
        Action<MessageBrokerClientDisposedEvent>? disposed,
        Action<MessageBrokerClientErrorEvent>? error)
    {
        TraceStart = traceStart;
        TraceEnd = traceEnd;
        Connecting = connecting;
        Connected = connected;
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
        AcknowledgingMessage = acknowledgingMessage;
        MessageAcknowledged = messageAcknowledged;
        QueryingDeadLetter = queryingDeadLetter;
        DeadLetterQueried = deadLetterQueried;
        ProcessingSystemNotification = processingSystemNotification;
        SenderNameProcessed = senderNameProcessed;
        StreamNameProcessed = streamNameProcessed;
        PublisherDeleted = publisherDeleted;
        ListenerDeleted = listenerDeleted;
        Disposing = disposing;
        Disposed = disposed;
        Error = error;
    }

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientTraceEvent"/> emitted during operation trace start.
    /// </summary>
    public readonly Action<MessageBrokerClientTraceEvent>? TraceStart;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientTraceEvent"/> emitted during operation trace end.
    /// </summary>
    public readonly Action<MessageBrokerClientTraceEvent>? TraceEnd;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientConnectingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientConnectingEvent>? Connecting;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientConnectedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientConnectedEvent>? Connected;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientHandshakingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientHandshakingEvent>? Handshaking;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientHandshakeEstablishedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientHandshakeEstablishedEvent>? HandshakeEstablished;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientAwaitPacketEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientAwaitPacketEvent>? AwaitPacket;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientSendPacketEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientSendPacketEvent>? SendPacket;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientReadPacketEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientReadPacketEvent>? ReadPacket;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientBindingPublisherEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientBindingPublisherEvent>? BindingPublisher;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientPublisherBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientPublisherBoundEvent>? PublisherBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientUnbindingPublisherEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientUnbindingPublisherEvent>? UnbindingPublisher;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientPublisherUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientPublisherUnboundEvent>? PublisherUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientBindingListenerEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientBindingListenerEvent>? BindingListener;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientListenerBoundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientListenerBoundEvent>? ListenerBound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientUnbindingListenerEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientUnbindingListenerEvent>? UnbindingListener;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientListenerUnboundEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientListenerUnboundEvent>? ListenerUnbound;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientPushingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientPushingMessageEvent>? PushingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessagePushedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessagePushedEvent>? MessagePushed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientProcessingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientProcessingMessageEvent>? ProcessingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessageProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessageProcessedEvent>? MessageProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientAcknowledgingMessageEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientAcknowledgingMessageEvent>? AcknowledgingMessage;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessageAcknowledgedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessageAcknowledgedEvent>? MessageAcknowledged;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientQueryingDeadLetterEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientQueryingDeadLetterEvent>? QueryingDeadLetter;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientDeadLetterQueriedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientDeadLetterQueriedEvent>? DeadLetterQueried;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientProcessingSystemNotificationEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientProcessingSystemNotificationEvent>? ProcessingSystemNotification;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientSenderNameProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientSenderNameProcessedEvent>? SenderNameProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientStreamNameProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientStreamNameProcessedEvent>? StreamNameProcessed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientPublisherDeletedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientPublisherDeletedEvent>? PublisherDeleted;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientListenerDeletedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientListenerDeletedEvent>? ListenerDeleted;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientDisposingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientDisposingEvent>? Disposing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientDisposedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientDisposedEvent>? Disposed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientErrorEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientErrorEvent>? Error;

    /// <summary>
    /// Creates a new <see cref="MessageBrokerClientLogger"/> instance.
    /// </summary>
    /// <param name="traceStart">Optional <see cref="TraceStart"/> callback.</param>
    /// <param name="traceEnd">Optional <see cref="TraceEnd"/> callback.</param>
    /// <param name="connecting">Optional <see cref="Connecting"/> callback.</param>
    /// <param name="connected">Optional <see cref="Connected"/> callback.</param>
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
    /// <param name="acknowledgingMessage">Optional <see cref="AcknowledgingMessage"/> callback.</param>
    /// <param name="messageAcknowledged">Optional <see cref="MessageAcknowledged"/> callback.</param>
    /// <param name="queryingDeadLetter">Optional <see cref="QueryingDeadLetter"/> callback.</param>
    /// <param name="deadLetterQueried">Optional <see cref="DeadLetterQueried"/> callback.</param>
    /// <param name="processingSystemNotification">Optional <see cref="ProcessingSystemNotification"/> callback.</param>
    /// <param name="senderNameProcessed">Optional <see cref="SenderNameProcessed"/> callback.</param>
    /// <param name="streamNameProcessed">Optional <see cref="StreamNameProcessed"/> callback.</param>
    /// <param name="publisherDeleted">Optional <see cref="PublisherDeleted"/> callback.</param>
    /// <param name="listenerDeleted">Optional <see cref="ListenerDeleted"/> callback.</param>
    /// <param name="disposing">Optional <see cref="Disposing"/> callback.</param>
    /// <param name="disposed">Optional <see cref="Disposed"/> callback.</param>
    /// <param name="error">Optional <see cref="Error"/> callback.</param>
    /// <returns>New <see cref="MessageBrokerClientLogger"/> instance.</returns>
    [Pure]
    public static MessageBrokerClientLogger Create(
        Action<MessageBrokerClientTraceEvent>? traceStart = null,
        Action<MessageBrokerClientTraceEvent>? traceEnd = null,
        Action<MessageBrokerClientConnectingEvent>? connecting = null,
        Action<MessageBrokerClientConnectedEvent>? connected = null,
        Action<MessageBrokerClientHandshakingEvent>? handshaking = null,
        Action<MessageBrokerClientHandshakeEstablishedEvent>? handshakeEstablished = null,
        Action<MessageBrokerClientAwaitPacketEvent>? awaitPacket = null,
        Action<MessageBrokerClientSendPacketEvent>? sendPacket = null,
        Action<MessageBrokerClientReadPacketEvent>? readPacket = null,
        Action<MessageBrokerClientBindingPublisherEvent>? bindingPublisher = null,
        Action<MessageBrokerClientPublisherBoundEvent>? publisherBound = null,
        Action<MessageBrokerClientUnbindingPublisherEvent>? unbindingPublisher = null,
        Action<MessageBrokerClientPublisherUnboundEvent>? publisherUnbound = null,
        Action<MessageBrokerClientBindingListenerEvent>? bindingListener = null,
        Action<MessageBrokerClientListenerBoundEvent>? listenerBound = null,
        Action<MessageBrokerClientUnbindingListenerEvent>? unbindingListener = null,
        Action<MessageBrokerClientListenerUnboundEvent>? listenerUnbound = null,
        Action<MessageBrokerClientPushingMessageEvent>? pushingMessage = null,
        Action<MessageBrokerClientMessagePushedEvent>? messagePushed = null,
        Action<MessageBrokerClientProcessingMessageEvent>? processingMessage = null,
        Action<MessageBrokerClientMessageProcessedEvent>? messageProcessed = null,
        Action<MessageBrokerClientAcknowledgingMessageEvent>? acknowledgingMessage = null,
        Action<MessageBrokerClientMessageAcknowledgedEvent>? messageAcknowledged = null,
        Action<MessageBrokerClientQueryingDeadLetterEvent>? queryingDeadLetter = null,
        Action<MessageBrokerClientDeadLetterQueriedEvent>? deadLetterQueried = null,
        Action<MessageBrokerClientProcessingSystemNotificationEvent>? processingSystemNotification = null,
        Action<MessageBrokerClientSenderNameProcessedEvent>? senderNameProcessed = null,
        Action<MessageBrokerClientStreamNameProcessedEvent>? streamNameProcessed = null,
        Action<MessageBrokerClientPublisherDeletedEvent>? publisherDeleted = null,
        Action<MessageBrokerClientListenerDeletedEvent>? listenerDeleted = null,
        Action<MessageBrokerClientDisposingEvent>? disposing = null,
        Action<MessageBrokerClientDisposedEvent>? disposed = null,
        Action<MessageBrokerClientErrorEvent>? error = null)
    {
        return new MessageBrokerClientLogger(
            traceStart,
            traceEnd,
            connecting,
            connected,
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
            acknowledgingMessage,
            messageAcknowledged,
            queryingDeadLetter,
            deadLetterQueried,
            processingSystemNotification,
            senderNameProcessed,
            streamNameProcessed,
            publisherDeleted,
            listenerDeleted,
            disposing,
            disposed,
            error );
    }
}
