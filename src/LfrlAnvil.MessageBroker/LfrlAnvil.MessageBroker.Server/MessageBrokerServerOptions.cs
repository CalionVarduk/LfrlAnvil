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
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents available <see cref="MessageBrokerServer"/> creation options.
/// </summary>
/// <param name="Tcp">Available <see cref="System.Net.Sockets.TcpClient"/> options.</param>
/// <param name="NetworkPacket">Available network packet options.</param>
/// <param name="HandshakeTimeout">
/// Handshake timeout for newly connected clients. Equal to <b>15 seconds</b> by default. Sub-millisecond components will be trimmed.
/// Value will be clamped to [<b>1 ms</b>, <b>2147483647 ms</b>] range.
/// </param>
/// <param name="AcceptableMessageTimeout">
/// Range of acceptable send or receive message timeout values. Equal to [<b>1 ms</b>, <b>2147483647 ms</b>] by default.
/// Sub-millisecond components will be trimmed. Values will be clamped to [<b>1 ms</b>, <b>2147483647 ms</b>] range.
/// </param>
/// <param name="AcceptablePingInterval">
/// Range of acceptable send ping interval values. Equal to [<b>1 ms</b>, <b>24 hours</b>] by default.
/// Sub-millisecond components will be trimmed. Values will be clamped to [<b>1 ms</b>, <b>24 hours</b>] range.
/// </param>
/// <param name="ExpressionFactory">Factory of parsed expressions for listener message filter predicates.</param>
/// <param name="TimestampsFactory">Factory of <see cref="Timestamp"/> providers.</param>
/// <param name="DelaySourceFactory">Factory of <see cref="ValueTaskDelaySource"/> instances used for scheduling future events.</param>
/// <param name="Logger"><see cref="MessageBrokerServerLogger"/> instance.</param>
/// <param name="ClientLoggerFactory">Factory of <see cref="MessageBrokerRemoteClientLogger"/> instances.</param>
/// <param name="ChannelLoggerFactory">Factory of <see cref="MessageBrokerChannelLogger"/> instances.</param>
/// <param name="StreamLoggerFactory">Factory of <see cref="MessageBrokerStreamLogger"/> instances.</param>
/// <param name="QueueLoggerFactory">Factory of <see cref="MessageBrokerQueueLogger"/> instances.</param>
/// <param name="StreamDecorator"><see cref="MessageBrokerRemoteClientStreamDecorator"/> callback.</param>
public readonly record struct MessageBrokerServerOptions(
    MessageBrokerServerTcpOptions Tcp,
    MessageBrokerServerNetworkPacketOptions NetworkPacket,
    Duration? HandshakeTimeout,
    Bounds<Duration>? AcceptableMessageTimeout,
    Bounds<Duration>? AcceptablePingInterval,
    IParsedExpressionFactory? ExpressionFactory,
    Func<MessageBrokerRemoteClient, ITimestampProvider>? TimestampsFactory,
    Func<MessageBrokerRemoteClient, ValueTaskDelaySource>? DelaySourceFactory,
    MessageBrokerServerLogger? Logger,
    Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientLogger?>? ClientLoggerFactory,
    Func<MessageBrokerChannel, MessageBrokerChannelLogger?>? ChannelLoggerFactory,
    Func<MessageBrokerStream, MessageBrokerStreamLogger?>? StreamLoggerFactory,
    Func<MessageBrokerQueue, MessageBrokerQueueLogger?>? QueueLoggerFactory,
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
    public MessageBrokerServerOptions SetTcpOptions(MessageBrokerServerTcpOptions value)
    {
        return new MessageBrokerServerOptions(
            value,
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="NetworkPacket"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetNetworkPacketOptions(MessageBrokerServerNetworkPacketOptions value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            value,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            value,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            value,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            value,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ExpressionFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetExpressionFactory(IParsedExpressionFactory? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            value,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            value,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            value,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            value,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            value,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ChannelLoggerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetChannelLoggerFactory(Func<MessageBrokerChannel, MessageBrokerChannelLogger?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            value,
            StreamLoggerFactory,
            QueueLoggerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="StreamLoggerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetStreamLoggerFactory(Func<MessageBrokerStream, MessageBrokerStreamLogger?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            value,
            QueueLoggerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="QueueLoggerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetQueueLoggerFactory(Func<MessageBrokerQueue, MessageBrokerQueueLogger?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
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
            NetworkPacket,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            ExpressionFactory,
            TimestampsFactory,
            DelaySourceFactory,
            Logger,
            ClientLoggerFactory,
            ChannelLoggerFactory,
            StreamLoggerFactory,
            QueueLoggerFactory,
            value );
    }
}
