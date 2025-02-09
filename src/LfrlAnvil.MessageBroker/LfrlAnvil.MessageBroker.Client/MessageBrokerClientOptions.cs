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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Client.Events;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents available <see cref="MessageBrokerClient"/> creation options.
/// </summary>
/// <param name="Tcp">Available <see cref="System.Net.Sockets.TcpClient"/> options.</param>
/// <param name="MinMemoryPoolSegmentLength">
/// Minimum segment length of an underlying <see cref="MemoryPool{T}"/>. Equal to <b>16 KB</b> by default.
/// </param>
/// <param name="ConnectionTimeout">Connect to server timeout. Equal to <b>15 seconds</b> by default.</param>
/// <param name="DesiredMessageTimeout">
/// Desired send or receive message timeout. Equal to <b>15 seconds</b> by default.
/// Actual timeout will be negotiated with the server during handshake.
/// </param>
/// <param name="DesiredPingInterval">
/// Desired send ping interval. Equal to <b>15 seconds</b> by default. Actual interval will be negotiated with the server during handshake.
/// </param>
/// <param name="EventHandler"><see cref="MessageBrokerClientEvent"/> callback.</param>
/// <param name="StreamDecorator"><see cref="MessageBrokerClientStreamDecorator"/> callback.</param>
public readonly record struct MessageBrokerClientOptions(
    MessageBrokerTcpClientOptions Tcp,
    MemorySize? MinMemoryPoolSegmentLength,
    Duration? ConnectionTimeout,
    Duration? DesiredMessageTimeout,
    Duration? DesiredPingInterval,
    MessageBrokerClientEventHandler? EventHandler,
    MessageBrokerClientStreamDecorator? StreamDecorator
)
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static MessageBrokerClientOptions Default => new MessageBrokerClientOptions();

    /// <summary>
    /// Allows to change <see cref="Tcp"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetTcpOptions(MessageBrokerTcpClientOptions value)
    {
        return new MessageBrokerClientOptions(
            value,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            EventHandler,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="MinMemoryPoolSegmentLength"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetMinMemoryPoolSegmentLength(MemorySize? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            value,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            EventHandler,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ConnectionTimeout"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetConnectionTimeout(Duration? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            value,
            DesiredMessageTimeout,
            DesiredPingInterval,
            EventHandler,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="DesiredMessageTimeout"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetDesiredMessageTimeout(Duration? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            value,
            DesiredPingInterval,
            EventHandler,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="DesiredPingInterval"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetDesiredPingInterval(Duration? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            value,
            EventHandler,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="EventHandler"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetEventHandler(MessageBrokerClientEventHandler? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            value,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="StreamDecorator"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetStreamDecorator(MessageBrokerClientStreamDecorator? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            EventHandler,
            value );
    }
}
