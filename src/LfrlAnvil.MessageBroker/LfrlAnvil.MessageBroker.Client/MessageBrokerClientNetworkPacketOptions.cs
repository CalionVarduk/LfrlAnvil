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

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents available network packet options during <see cref="MessageBrokerClient"/> creation.
/// </summary>
/// <param name="DesiredMaxBatchLength">
/// Desired max network batch packet length. Equal to <b>10 MB</b> by default.
/// Value will be clamped to [<b>16 KB</b>, <b>1 GB</b>] range. Actual length will be negotiated with the server during handshake.
/// Represents max possible length for packets of <b>Batch</b> type.
/// </param>
/// <param name="DesiredMaxBatchPacketCount">
/// Desired max number of packets in a single network batch packet. Equal to <b>30</b> by default.
/// Value will be clamped to [<b>0</b>, <b>32767</b>] range. Value equal to <b>1</b> will be changed to <b>0</b>.
/// Actual count will be negotiated with the server during handshake.
/// </param>
public readonly record struct MessageBrokerClientNetworkPacketOptions(MemorySize? DesiredMaxBatchLength, short? DesiredMaxBatchPacketCount)
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static MessageBrokerClientNetworkPacketOptions Default => new MessageBrokerClientNetworkPacketOptions();

    /// <summary>
    /// Allows to change <see cref="DesiredMaxBatchLength"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientNetworkPacketOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientNetworkPacketOptions SetDesiredMaxBatchLength(MemorySize? value)
    {
        return new MessageBrokerClientNetworkPacketOptions( value, DesiredMaxBatchPacketCount );
    }

    /// <summary>
    /// Allows to change <see cref="DesiredMaxBatchPacketCount"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerClientNetworkPacketOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerClientNetworkPacketOptions SetDesiredMaxBatchPacketCount(short? value)
    {
        return new MessageBrokerClientNetworkPacketOptions( DesiredMaxBatchLength, value );
    }
}
