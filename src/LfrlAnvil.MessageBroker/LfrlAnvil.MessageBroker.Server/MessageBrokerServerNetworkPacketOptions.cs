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
using LfrlAnvil.Diagnostics;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents available network packet options during <see cref="MessageBrokerServer"/> creation.
/// </summary>
/// <param name="MaxLength">
/// Max network packet length and min segment length for each client's underlying memory pool.
/// Equal to <b>16 KB</b> by default. Value will be clamped to [<b>16 KB</b>, <b>1 MB</b>] range.
/// Represents max possible length for packets not handled by either <see cref="MaxMessageLength"/> or <see cref="MaxBatchLength"/>.
/// </param>
/// <param name="MaxMessageLength">
/// Max network message packet length. Equal to <b>10 MB</b> by default.
/// Value will be clamped to [<see cref="MaxLength"/>, <b>1 GB</b>] range.
/// Represents max possible length for outbound packets of <see cref="MessageBrokerClientEndpoint.MessageNotification"/> type
/// or inbound packets of <see cref="MessageBrokerServerEndpoint.PushMessage"/> type.
/// </param>
/// <param name="MaxBatchLength">
/// Max network batch packet length. Equal to <b>10 MB</b> by default.
/// Value will be clamped to [<see cref="MaxLength"/>, <b>1 GB</b>] range.
/// Represents max possible length for packets of <b>Batch</b> type.
/// </param>
/// <param name="MaxBatchPacketCount">
/// Max number of packets in a single network batch packet. Equal to <b>100</b> by default.
/// Value will be clamped to [<b>0</b>, <b>32767</b>] range. Value equal to <b>1</b> will be changed to <b>0</b>.
/// </param>
public readonly record struct MessageBrokerServerNetworkPacketOptions(
    MemorySize? MaxLength,
    MemorySize? MaxMessageLength,
    MemorySize? MaxBatchLength,
    short? MaxBatchPacketCount
)
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static MessageBrokerServerNetworkPacketOptions Default => new MessageBrokerServerNetworkPacketOptions();

    /// <summary>
    /// Allows to change <see cref="MaxLength"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerNetworkPacketOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerNetworkPacketOptions SetMaxLength(MemorySize? value)
    {
        return new MessageBrokerServerNetworkPacketOptions( value, MaxMessageLength, MaxBatchLength, MaxBatchPacketCount );
    }

    /// <summary>
    /// Allows to change <see cref="MaxMessageLength"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerNetworkPacketOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerNetworkPacketOptions SetMaxMessageLength(MemorySize? value)
    {
        return new MessageBrokerServerNetworkPacketOptions( MaxLength, value, MaxBatchLength, MaxBatchPacketCount );
    }

    /// <summary>
    /// Allows to change <see cref="MaxBatchLength"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerNetworkPacketOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerNetworkPacketOptions SetMaxBatchLength(MemorySize? value)
    {
        return new MessageBrokerServerNetworkPacketOptions( MaxLength, MaxMessageLength, value, MaxBatchPacketCount );
    }

    /// <summary>
    /// Allows to change <see cref="MaxBatchPacketCount"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerServerNetworkPacketOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerServerNetworkPacketOptions SetMaxBatchPacketCount(short? value)
    {
        return new MessageBrokerServerNetworkPacketOptions( MaxLength, MaxMessageLength, MaxBatchLength, value );
    }
}
