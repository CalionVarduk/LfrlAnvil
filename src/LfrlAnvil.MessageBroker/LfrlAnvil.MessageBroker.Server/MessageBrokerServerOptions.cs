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
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents available <see cref="MessageBrokerServer"/> creation options.
/// </summary>
/// <param name="Tcp">Available <see cref="System.Net.Sockets.TcpClient"/> options.</param>
/// <param name="MinMemoryPoolSegmentLength">
/// Minimum segment length of an underlying <see cref="LfrlAnvil.Memory.MemoryPool{T}"/>. Equal to <b>16 KB</b> by default.
/// </param>
/// <param name="HandshakeTimeout">Handshake timeout for newly connected clients. Equal to <b>15 seconds</b> by default.</param>
/// <param name="AcceptableMessageTimeout">
/// Range of acceptable send or receive message timeout values. Equal to [<b>1 ms</b>, <b>2147483647 ms</b>] by default.
/// </param>
/// <param name="AcceptablePingInterval">
/// Range of acceptable send ping interval values. Equal to [<b>1 ms</b>, <b>24 hours</b>] by default.
/// </param>
/// <param name="TimestampsFactory">Factory of <see cref="Timestamp"/> providers.</param>
/// <param name="DelaySourceFactory">Factory of <see cref="ValueTaskDelaySource"/> instances used for scheduling future events.</param>
/// <param name="Logger"><see cref="MessageBrokerServerLogger"/> instance.</param>
/// <param name="ClientLoggerFactory">Factory of <see cref="MessageBrokerRemoteClientLogger"/> instances.</param>
/// <param name="ChannelEventHandlerFactory">Factory of <see cref="MessageBrokerChannelEventHandler"/> callbacks.</param>
/// <param name="StreamEventHandlerFactory">Factory of <see cref="MessageBrokerStreamEventHandler"/> callbacks.</param>
/// <param name="QueueEventHandlerFactory">Factory of <see cref="MessageBrokerQueueEventHandler"/> callbacks.</param>
/// <param name="StreamDecorator"><see cref="MessageBrokerRemoteClientStreamDecorator"/> callback.</param>
public readonly record struct MessageBrokerServerOptions(
    MessageBrokerTcpServerOptions Tcp,
    MemorySize? MinMemoryPoolSegmentLength,
    Duration? HandshakeTimeout,
    Bounds<Duration>? AcceptableMessageTimeout,
    Bounds<Duration>? AcceptablePingInterval,
    Func<MessageBrokerRemoteClient, ITimestampProvider>? TimestampsFactory,
    Func<MessageBrokerRemoteClient, ValueTaskDelaySource>? DelaySourceFactory,
    MessageBrokerServerLogger? Logger,
    Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientLogger?>? ClientLoggerFactory,
    Func<MessageBrokerChannel, MessageBrokerChannelEventHandler?>? ChannelEventHandlerFactory,
    Func<MessageBrokerStream, MessageBrokerStreamEventHandler?>? StreamEventHandlerFactory,
    Func<MessageBrokerQueue, MessageBrokerQueueEventHandler?>? QueueEventHandlerFactory,
    MessageBrokerRemoteClientStreamDecorator? StreamDecorator
)
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static MessageBrokerServerOptions Default => new MessageBrokerServerOptions();

    /// <summary>
    /// Allows to change <see cref="Tcp"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetTcpOptions(MessageBrokerTcpServerOptions value)
    {
        return new MessageBrokerServerOptions(
            value,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="MinMemoryPoolSegmentLength"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetMinMemoryPoolSegmentLength(MemorySize? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            value,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="HandshakeTimeout"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetHandshakeTimeout(Duration? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            value,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="AcceptableMessageTimeout"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetAcceptableMessageTimeout(Bounds<Duration>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            value,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="AcceptablePingInterval"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetAcceptablePingInterval(Bounds<Duration>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            value,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="TimestampsFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetTimestampsFactory(Func<MessageBrokerRemoteClient, ITimestampProvider>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            value,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="DelaySourceFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetDelaySourceFactory(Func<MessageBrokerRemoteClient, ValueTaskDelaySource>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            value,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="Logger"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetLogger(MessageBrokerServerLogger? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            value,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ClientLoggerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetClientLoggerFactory(Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientLogger?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            value,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ChannelEventHandlerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetChannelEventHandlerFactory(Func<MessageBrokerChannel, MessageBrokerChannelEventHandler?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            value,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="StreamEventHandlerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetStreamEventHandlerFactory(Func<MessageBrokerStream, MessageBrokerStreamEventHandler?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            value,
            QueueEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="QueueEventHandlerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetQueueEventHandlerFactory(Func<MessageBrokerQueue, MessageBrokerQueueEventHandler?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            value,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="StreamDecorator"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetStreamDecorator(MessageBrokerRemoteClientStreamDecorator? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelEventHandlerFactory,
            StreamEventHandlerFactory,
            QueueEventHandlerFactory,
            value );
    }
}
