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
/// <param name="EventHandler"><see cref="MessageBrokerServerEvent"/> callback.</param>
/// <param name="ClientEventHandlerFactory">Factory of <see cref="MessageBrokerRemoteClientEventHandler"/> callbacks.</param>
/// <param name="StreamDecorator"><see cref="MessageBrokerRemoteClientStreamDecorator"/> callback.</param>
public readonly record struct MessageBrokerServerOptions(
    MessageBrokerTcpServerOptions Tcp,
    MemorySize? MinMemoryPoolSegmentLength,
    Duration? HandshakeTimeout,
    Bounds<Duration>? AcceptableMessageTimeout,
    Bounds<Duration>? AcceptablePingInterval,
    MessageBrokerServerEventHandler? EventHandler,
    Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientEventHandler?>? ClientEventHandlerFactory,
    Func<MessageBrokerChannel, MessageBrokerChannelEventHandler?>? ChannelEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="EventHandler"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetEventHandler(MessageBrokerServerEventHandler? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            value,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ClientEventHandlerFactory"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerOptions SetClientEventHandlerFactory(
        Func<MessageBrokerRemoteClient, MessageBrokerRemoteClientEventHandler?>? value)
    {
        return new MessageBrokerServerOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            HandshakeTimeout,
            AcceptableMessageTimeout,
            AcceptablePingInterval,
            EventHandler,
            value,
            ChannelEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
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
            EventHandler,
            ClientEventHandlerFactory,
            ChannelEventHandlerFactory,
            value );
    }
}
