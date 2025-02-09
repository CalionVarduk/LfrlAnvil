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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents available <see cref="System.Net.Sockets.TcpClient"/> options during <see cref="MessageBrokerServer"/> creation.
/// </summary>
/// <param name="NoDelay">Value that disables a delay when send or receive buffers are not full. Equal to <b>false</b> by default.</param>
/// <param name="SocketBufferSize">The size of send and receive buffers. Equal to <b>65535 B</b> by default.</param>
public readonly record struct MessageBrokerTcpServerOptions(bool? NoDelay, MemorySize? SocketBufferSize)
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static MessageBrokerTcpServerOptions Default => new MessageBrokerTcpServerOptions();

    /// <summary>
    /// Allows to change <see cref="NoDelay"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerTcpServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerTcpServerOptions SetNoDelay(bool? value)
    {
        return new MessageBrokerTcpServerOptions( value, SocketBufferSize );
    }

    /// <summary>
    /// Allows to change <see cref="SocketBufferSize"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerTcpServerOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MessageBrokerTcpServerOptions SetSocketBufferSize(MemorySize? value)
    {
        return new MessageBrokerTcpServerOptions( NoDelay, value );
    }
}
