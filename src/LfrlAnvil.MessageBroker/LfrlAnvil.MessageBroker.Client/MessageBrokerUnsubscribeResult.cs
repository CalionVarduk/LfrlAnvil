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
/// Represents the result of unsubscribing a client from a channel.
/// </summary>
public readonly struct MessageBrokerUnsubscribeResult
{
    private readonly byte _state;

    private MessageBrokerUnsubscribeResult(byte state)
    {
        _state = state;
    }

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client does not contain a local listener subscribed to the channel.
    /// </summary>
    public bool NotSubscribed => _state == 1;

    /// <summary>
    /// Specifies whether or not the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved => (_state & 2) != 0;

    /// <summary>
    /// Specifies whether or not the queue has been removed by the server.
    /// </summary>
    public bool QueueRemoved => (_state & 4) != 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerUnsubscribeResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        if ( _state == 1 )
            return "Not subscribed";

        var channelRemoved = ChannelRemoved ? " (channel removed)" : string.Empty;
        var queueRemoved = QueueRemoved ? " (queue removed)" : string.Empty;
        return $"Success{channelRemoved}{queueRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnsubscribeResult Create(bool channelRemoved, bool queueRemoved)
    {
        return new MessageBrokerUnsubscribeResult( ( byte )((channelRemoved ? 2 : 0) | (queueRemoved ? 4 : 0)) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnsubscribeResult CreateNotSubscribed()
    {
        return new MessageBrokerUnsubscribeResult( 1 );
    }
}
