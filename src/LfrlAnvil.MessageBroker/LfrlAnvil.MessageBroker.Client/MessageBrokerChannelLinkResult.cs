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
/// Represents the result of <see cref="MessageBrokerLinkedChannel"/> linkage.
/// </summary>
public readonly struct MessageBrokerChannelLinkResult
{
    /// <summary>
    /// Represents possible types of channel linkage result.
    /// </summary>
    public enum ResultType : byte
    {
        /// <summary>
        /// Specifies that a new channel has been successfully created and linked to the client.
        /// </summary>
        CreatedAndLinked = 0,

        /// <summary>
        /// Specifies that an existing channel has been successfully linked to the client.
        /// </summary>
        Linked = 1,

        /// <summary>
        /// Specifies that request to the server has been cancelled because the client contains local link to the channel.
        /// </summary>
        AlreadyLinked = 2
    }

    private MessageBrokerChannelLinkResult(MessageBrokerLinkedChannel channel, ResultType type)
    {
        Channel = channel;
        Type = type;
    }

    /// <summary>
    /// Linked channel.
    /// </summary>
    public MessageBrokerLinkedChannel Channel { get; }

    /// <summary>
    /// Type of this result.
    /// </summary>
    /// <remarks>See <see cref="ResultType"/> for more information.</remarks>
    public ResultType Type { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelLinkResult CreatedAndLinked(MessageBrokerLinkedChannel channel)
    {
        return new MessageBrokerChannelLinkResult( channel, ResultType.CreatedAndLinked );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelLinkResult Linked(MessageBrokerLinkedChannel channel)
    {
        return new MessageBrokerChannelLinkResult( channel, ResultType.Linked );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelLinkResult AlreadyLinked(MessageBrokerLinkedChannel channel)
    {
        return new MessageBrokerChannelLinkResult( channel, ResultType.AlreadyLinked );
    }
}
