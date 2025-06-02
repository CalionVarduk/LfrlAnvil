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
using LfrlAnvil.Chrono.Async;
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
/// <param name="ListenerDisposalTimeout">
/// Amount of time that <see cref="MessageBrokerListener"/> instances will wait during their disposal
/// for callbacks to complete before giving up. Equal to <b>15 seconds</b> by default.
/// </param>
/// <param name="SynchronizeExternalObjectNames">
/// Specifies whether or not synchronization of external object names is enabled. Equal to <b>true</b> by default.
/// </param>
/// <param name="Timestamps"><see cref="Timestamp"/> provider.</param>
/// <param name="DelaySource"><see cref="ValueTaskDelaySource"/> instance used for scheduling future events.</param>
/// <param name="Logger"><see cref="MessageBrokerClientLogger"/> instance.</param>
/// <param name="StreamDecorator"><see cref="MessageBrokerClientStreamDecorator"/> callback.</param>
public readonly record struct MessageBrokerClientOptions(
    MessageBrokerTcpClientOptions Tcp,
    MemorySize? MinMemoryPoolSegmentLength,
    Duration? ConnectionTimeout,
    Duration? DesiredMessageTimeout,
    Duration? DesiredPingInterval,
    Duration? ListenerDisposalTimeout,
    bool? SynchronizeExternalObjectNames,
    ITimestampProvider? Timestamps,
    ValueTaskDelaySource? DelaySource,
    MessageBrokerClientLogger? Logger,
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
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
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
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
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
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
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
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
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
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="ListenerDisposalTimeout"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetListenerDisposalTimeout(Duration? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            value,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="SynchronizeExternalObjectNames"/>.
    /// </summary>
    /// <param name="value">New value</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetSynchronizeExternalObjectNames(bool? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            ListenerDisposalTimeout,
            value,
            Timestamps,
            DelaySource,
            Logger,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="Timestamps"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetTimestamps(ITimestampProvider? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            value,
            DelaySource,
            Logger,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="DelaySource"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetDelaySource(ValueTaskDelaySource? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            value,
            Logger,
            StreamDecorator );
    }

    /// <summary>
    /// Allows to change <see cref="Logger"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientOptions SetLogger(MessageBrokerClientLogger? value)
    {
        return new MessageBrokerClientOptions(
            Tcp,
            MinMemoryPoolSegmentLength,
            ConnectionTimeout,
            DesiredMessageTimeout,
            DesiredPingInterval,
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
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
            ListenerDisposalTimeout,
            SynchronizeExternalObjectNames,
            Timestamps,
            DelaySource,
            Logger,
            value );
    }
}
