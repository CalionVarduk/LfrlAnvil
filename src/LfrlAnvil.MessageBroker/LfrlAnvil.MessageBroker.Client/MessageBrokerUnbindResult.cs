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
/// Represents the result of unbinding a client from a channel.
/// </summary>
public readonly struct MessageBrokerUnbindResult
{
    private readonly byte _state;

    private MessageBrokerUnbindResult(byte state)
    {
        _state = state;
    }

    /// <summary>
    /// Specifies whether or not request to the server has been cancelled
    /// because the client does not contain a local publisher bound to the channel.
    /// </summary>
    public bool NotBound => _state == 1;

    /// <summary>
    /// Specifies whether or not the channel has been removed by the server.
    /// </summary>
    public bool ChannelRemoved => _state == 2;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerUnbindResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return _state switch
        {
            1 => "Not bound",
            2 => "Success (channel removed)",
            _ => "Success"
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnbindResult Create(bool channelRemoved)
    {
        return new MessageBrokerUnbindResult( ( byte )(channelRemoved ? 2 : 0) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerUnbindResult CreateNotBound()
    {
        return new MessageBrokerUnbindResult( 1 );
    }
}
