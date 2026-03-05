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
/// Represents the result of unbinding a client from a channel as a listener.
/// </summary>
public readonly struct MessageBrokerUnbindListenerResult
{
    private readonly byte _state;

    private MessageBrokerUnbindListenerResult(byte state)
    {
        _state = state;
    }

    /// <summary>
    /// Specifies whether request to the server has been cancelled
    /// because the client is not locally bound as a listener to the channel.
    /// </summary>
    public bool NotBound => _state == 1;

    /// <summary>
    /// Specifies whether the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved => (_state & 2) != 0;

    /// <summary>
    /// Specifies whether the queue has been removed by the server.
    /// </summary>
    public bool QueueRemoved => (_state & 4) != 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerUnbindListenerResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        if ( _state == 1 )
            return "Not bound";

        var channelRemoved = ChannelRemoved ? " (channel removed)" : string.Empty;
        var queueRemoved = QueueRemoved ? " (queue removed)" : string.Empty;
        return $"Success{channelRemoved}{queueRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnbindListenerResult Create(bool channelRemoved, bool queueRemoved)
    {
        return new MessageBrokerUnbindListenerResult( ( byte )((channelRemoved ? 2 : 0) | (queueRemoved ? 4 : 0)) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnbindListenerResult CreateNotBound()
    {
        return new MessageBrokerUnbindListenerResult( 1 );
    }
}
