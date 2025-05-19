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

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents the result of pushing a message to a channel.
/// </summary>
public readonly struct MessageBrokerPushResult
{
    private readonly byte _state;

    private MessageBrokerPushResult(byte state, ulong? id)
    {
        _state = state;
        Id = id;
    }

    /// <summary>
    /// Identifier of the message accepted by the server.
    /// </summary>
    /// <remarks>
    /// Id will not be <b>null</b> only when the <see cref="Confirm"/> option is equal to <b>true</b>
    /// and the message has been successfully received by the server.
    /// </remarks>
    public ulong? Id { get; }

    /// <summary>
    /// Specifies whether or not the client requested confirmation from the server that it received the message.
    /// </summary>
    public bool Confirm => (_state & 1) != 0;

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client does not contain a local publisher bound to the channel.
    /// </summary>
    public bool NotBound => (_state & 2) == 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerPushResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return _state <= 1 ? "Not bound" : $"Id = {Id?.ToString() ?? "<NULL>"}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPushResult Create(ulong id)
    {
        return new MessageBrokerPushResult( 3, id );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPushResult CreateNotBound(bool confirm)
    {
        return new MessageBrokerPushResult( ( byte )(confirm ? 1 : 0), null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerPushResult CreateUnconfirmed()
    {
        return new MessageBrokerPushResult( 2, null );
    }
}
