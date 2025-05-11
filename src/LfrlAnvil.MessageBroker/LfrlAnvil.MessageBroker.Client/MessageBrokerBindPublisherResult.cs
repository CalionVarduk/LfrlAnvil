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
/// Represents the result of binding a client to a channel as a publisher.
/// </summary>
public readonly struct MessageBrokerBindPublisherResult
{
    private readonly byte _state;

    private MessageBrokerBindPublisherResult(MessageBrokerPublisher publisher, byte state)
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
    /// because the client is already locally bound as publisher to the channel.
    /// </summary>
    public bool AlreadyBound => _state == 1;

    /// <summary>
    /// Specifies whether or not a new channel has been created by the server.
    /// </summary>
    public bool ChannelCreated => (_state & 2) != 0;

    /// <summary>
    /// Specifies whether or not a new stream has been created by the server.
    /// </summary>
    public bool StreamCreated => (_state & 4) != 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerBindPublisherResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        if ( _state == 1 )
            return $"{Publisher} (already bound)";

        var channelCreated = ChannelCreated ? " (channel created)" : string.Empty;
        var streamCreated = StreamCreated ? " (stream created)" : string.Empty;
        return $"{Publisher}{channelCreated}{streamCreated}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerBindPublisherResult Create(MessageBrokerPublisher publisher, bool channelCreated, bool streamCreated)
    {
        return new MessageBrokerBindPublisherResult( publisher, ( byte )((channelCreated ? 2 : 0) | (streamCreated ? 4 : 0)) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerBindPublisherResult CreateAlreadyBound(MessageBrokerPublisher publisher)
    {
        return new MessageBrokerBindPublisherResult( publisher, 1 );
    }
}
