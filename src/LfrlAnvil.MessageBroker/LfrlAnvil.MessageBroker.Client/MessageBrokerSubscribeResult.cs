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
/// Represents the result of <see cref="MessageBrokerListener"/> creation.
/// </summary>
public readonly struct MessageBrokerSubscribeResult
{
    private readonly byte _state;

    private MessageBrokerSubscribeResult(MessageBrokerListener listener, byte state)
    {
        Listener = listener;
        _state = state;
    }

    /// <summary>
    /// Listener subscribed to the channel.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client already contains a local listener subscribed to the channel.
    /// </summary>
    public bool AlreadySubscribed => _state == 1;

    /// <summary>
    /// Specifies whether or not a new channel has been created by the server.
    /// </summary>
    public bool ChannelCreated => _state == 2;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerSubscribeResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return _state switch
        {
            1 => $"{Listener} (already subscribed)",
            2 => $"{Listener} (channel created)",
            _ => Listener.ToString()
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscribeResult Create(MessageBrokerListener listener, bool channelCreated)
    {
        return new MessageBrokerSubscribeResult( listener, ( byte )(channelCreated ? 2 : 0) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscribeResult CreateAlreadySubscribed(MessageBrokerListener listener)
    {
        return new MessageBrokerSubscribeResult( listener, 1 );
    }
}
