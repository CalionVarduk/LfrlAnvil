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

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerSubscription"/>.
/// </summary>
public readonly struct MessageBrokerSubscriptionEvent
{
    private MessageBrokerSubscriptionEvent(
        MessageBrokerSubscription subscription,
        MessageBrokerSubscriptionEventType type,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId,
        Exception? exception = null)
    {
        Subscription = subscription;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerSubscription"/> that emitted this event.
    /// </summary>
    public MessageBrokerSubscription Subscription { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Subscription"/> or related client or channel.
    /// </remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerSubscriptionEventType"/> for more information.</remarks>
    public MessageBrokerSubscriptionEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event is related to a subscription-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == MessageBrokerRemoteClientEvent.RootContextId;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerSubscriptionEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Subscription.Client.Name.Length + Subscription.Channel.Name.Length + 96 );
        ToString( builder );
        return builder.ToString();
    }

    /// <summary>
    /// Appends a string representation of this <see cref="MessageBrokerSubscriptionEvent"/> instance
    /// to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append this event to.</param>
    public void ToString(StringBuilder builder)
    {
        builder
            .Append( '[' )
            .Append( Subscription.Client.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Subscription.Client.Name )
            .Append( "'=>" )
            .Append( Subscription.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Subscription.Channel.Name )
            .Append( "'::" )
            .Append( IsRootContext ? "<ROOT>" : ContextId.ToString( CultureInfo.InvariantCulture ) )
            .Append( "] [" )
            .Append( Type.ToString() )
            .Append( ']' );

        if ( Exception is not null )
            builder.AppendLine( " Encountered an error:" ).Append( Exception );
        else
        {
            switch ( Type )
            {
                case MessageBrokerSubscriptionEventType.Created:
                case MessageBrokerSubscriptionEventType.Unexpected:
                case MessageBrokerSubscriptionEventType.Disposing:
                case MessageBrokerSubscriptionEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionEvent Created(
        MessageBrokerSubscription subscription,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerSubscriptionEvent( subscription, MessageBrokerSubscriptionEventType.Created, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionEvent Disposing(
        MessageBrokerSubscription subscription,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerSubscriptionEvent( subscription, MessageBrokerSubscriptionEventType.Disposing, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionEvent Disposed(MessageBrokerSubscription subscription)
    {
        return new MessageBrokerSubscriptionEvent( subscription, MessageBrokerSubscriptionEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerSubscriptionEvent Unexpected(MessageBrokerSubscription subscription, Exception exception)
    {
        return new MessageBrokerSubscriptionEvent( subscription, MessageBrokerSubscriptionEventType.Unexpected, exception: exception );
    }
}
