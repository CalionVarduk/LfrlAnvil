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
public readonly struct MessageBrokerSubscriptionResult
{
    /// <summary>
    /// Represents possible types of subscription result.
    /// </summary>
    public enum ResultType : byte
    {
        /// <summary>
        /// Specifies that a new channel has been created and subscription to that channel has been successfully created.
        /// </summary>
        SubscribedAndChannelCreated = 0,

        /// <summary>
        /// Specifies that subscription to an existing channel has been successfully created.
        /// </summary>
        Subscribed = 1,

        /// <summary>
        /// Specifies that request to the server has been cancelled because the client contains local subscription to the channel.
        /// </summary>
        AlreadySubscribed = 2
    }

    private MessageBrokerSubscriptionResult(MessageBrokerListener listener, ResultType type)
    {
        Listener = listener;
        Type = type;
    }

    /// <summary>
    /// Result listener.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Type of this result.
    /// </summary>
    /// <remarks>See <see cref="ResultType"/> for more information.</remarks>
    public ResultType Type { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionResult SubscribedAndChannelCreated(MessageBrokerListener listener)
    {
        return new MessageBrokerSubscriptionResult( listener, ResultType.SubscribedAndChannelCreated );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionResult Subscribed(MessageBrokerListener listener)
    {
        return new MessageBrokerSubscriptionResult( listener, ResultType.Subscribed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionResult AlreadySubscribed(MessageBrokerListener listener)
    {
        return new MessageBrokerSubscriptionResult( listener, ResultType.AlreadySubscribed );
    }
}
