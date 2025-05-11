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
/// Represents the result of binding a client to a channel as a listener.
/// </summary>
public readonly struct MessageBrokerBindListenerResult
{
    private readonly byte _state;

    private MessageBrokerBindListenerResult(MessageBrokerListener listener, byte state)
    {
        Listener = listener;
        _state = state;
    }

    /// <summary>
    /// Listener bound to the channel.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client is already locally bound as listener to the channel.
    /// </summary>
    public bool AlreadyBound => _state == 1;

    /// <summary>
    /// Specifies whether or not a new channel has been created by the server.
    /// </summary>
    public bool ChannelCreated => (_state & 2) != 0;

    /// <summary>
    /// Specifies whether or not a new queue has been created by the server.
    /// </summary>
    public bool QueueCreated => (_state & 4) != 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerBindListenerResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        if ( _state == 1 )
            return $"{Listener} (already bound)";

        var channelCreated = ChannelCreated ? " (channel created)" : string.Empty;
        var queueCreated = QueueCreated ? " (queue created)" : string.Empty;
        return $"{Listener}{channelCreated}{queueCreated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerBindListenerResult Create(MessageBrokerListener listener, bool channelCreated, bool queueCreated)
    {
        return new MessageBrokerBindListenerResult( listener, ( byte )((channelCreated ? 2 : 0) | (queueCreated ? 4 : 0)) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerBindListenerResult CreateAlreadyBound(MessageBrokerListener listener)
    {
        return new MessageBrokerBindListenerResult( listener, 1 );
    }
}
