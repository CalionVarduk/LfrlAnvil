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
        Action<MessageBrokerClientPublisherChangeEvent>? publisherChange,
        Action<MessageBrokerClientBindingListenerEvent>? bindingListener,
        Action<MessageBrokerClientListenerChangeEvent>? listenerChange,
        Action<MessageBrokerClientMessagePushingEvent>? messagePushing,
        Action<MessageBrokerClientMessagePushedEvent>? messagePushed,
        Action<MessageBrokerClientMessageProcessingEvent>? messageProcessing,
        Action<MessageBrokerClientMessageProcessedEvent>? messageProcessed,
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
        PublisherChange = publisherChange;
        BindingListener = bindingListener;
        ListenerChange = listenerChange;
        MessagePushing = messagePushing;
        MessagePushed = messagePushed;
        MessageProcessing = messageProcessing;
        MessageProcessed = messageProcessed;
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
    /// Optional callback for a <see cref="MessageBrokerClientPublisherChangeEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientPublisherChangeEvent>? PublisherChange;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientBindingListenerEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientBindingListenerEvent>? BindingListener;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientListenerChangeEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientListenerChangeEvent>? ListenerChange;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessagePushingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessagePushingEvent>? MessagePushing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessagePushedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessagePushedEvent>? MessagePushed;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessageProcessingEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessageProcessingEvent>? MessageProcessing;

    /// <summary>
    /// Optional callback for a <see cref="MessageBrokerClientMessageProcessedEvent"/>.
    /// </summary>
    public readonly Action<MessageBrokerClientMessageProcessedEvent>? MessageProcessed;

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
    /// <param name="publisherChange">Optional <see cref="PublisherChange"/> callback.</param>
    /// <param name="bindingListener">Optional <see cref="BindingListener"/> callback.</param>
    /// <param name="listenerChange">Optional <see cref="ListenerChange"/> callback.</param>
    /// <param name="messagePushing">Optional <see cref="MessagePushing"/> callback.</param>
    /// <param name="messagePushed">Optional <see cref="MessagePushed"/> callback.</param>
    /// <param name="messageProcessing">Optional <see cref="MessageProcessing"/> callback.</param>
    /// <param name="messageProcessed">Optional <see cref="MessageProcessed"/> callback.</param>
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
        Action<MessageBrokerClientPublisherChangeEvent>? publisherChange = null,
        Action<MessageBrokerClientBindingListenerEvent>? bindingListener = null,
        Action<MessageBrokerClientListenerChangeEvent>? listenerChange = null,
        Action<MessageBrokerClientMessagePushingEvent>? messagePushing = null,
        Action<MessageBrokerClientMessagePushedEvent>? messagePushed = null,
        Action<MessageBrokerClientMessageProcessingEvent>? messageProcessing = null,
        Action<MessageBrokerClientMessageProcessedEvent>? messageProcessed = null,
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
            publisherChange,
            bindingListener,
            listenerChange,
            messagePushing,
            messagePushed,
            messageProcessing,
            messageProcessed,
            disposing,
            disposed,
            error );
    }
}
