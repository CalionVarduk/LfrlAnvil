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
/// Represents the result of unbinding a client from a channel as a publisher.
/// </summary>
public readonly struct MessageBrokerUnbindPublisherResult
{
    private readonly byte _state;

    private MessageBrokerUnbindPublisherResult(byte state)
    {
        _state = state;
    }

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client is not locally bound as a publisher to the channel.
    /// </summary>
    public bool NotBound => _state == 1;

    /// <summary>
    /// Specifies whether or not the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved => (_state & 2) != 0;

    /// <summary>
    /// Specifies whether or not the stream has been removed by the server.
    /// </summary>
    public bool StreamRemoved => (_state & 4) != 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerUnbindPublisherResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        if ( _state == 1 )
            return "Not bound";

        var channelRemoved = ChannelRemoved ? " (channel removed)" : string.Empty;
        var streamRemoved = StreamRemoved ? " (stream removed)" : string.Empty;
        return $"Success{channelRemoved}{streamRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnbindPublisherResult Create(bool channelRemoved, bool streamRemoved)
    {
        return new MessageBrokerUnbindPublisherResult( ( byte )((channelRemoved ? 2 : 0) | (streamRemoved ? 4 : 0)) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnbindPublisherResult CreateNotBound()
    {
        return new MessageBrokerUnbindPublisherResult( 1 );
    }
}
