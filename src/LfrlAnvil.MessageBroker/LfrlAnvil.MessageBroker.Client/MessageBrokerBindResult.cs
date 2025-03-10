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
/// Represents the result of binding a client to a channel.
/// </summary>
public readonly struct MessageBrokerBindResult
{
    private readonly byte _state;

    private MessageBrokerBindResult(MessageBrokerPublisher publisher, byte state)
    {
        Publisher = publisher;
        _state = state;
    }

    /// <summary>
    /// Publisher bound to the channel.
    /// </summary>
    public MessageBrokerPublisher Publisher { get; }

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client already contains a local publisher bound to the channel.
    /// </summary>
    public bool AlreadyBound => _state == 1;

    /// <summary>
    /// Specifies whether or not a new channel has been created by the server.
    /// </summary>
    public bool ChannelCreated => _state == 2;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerBindResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return _state switch
        {
            1 => $"{Publisher} (already bound)",
            2 => $"{Publisher} (channel created)",
            _ => Publisher.ToString()
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerBindResult Create(MessageBrokerPublisher publisher, bool channelCreated)
    {
        return new MessageBrokerBindResult( publisher, ( byte )(channelCreated ? 2 : 0) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerBindResult CreateAlreadyBound(MessageBrokerPublisher publisher)
    {
        return new MessageBrokerBindResult( publisher, 1 );
    }
}
