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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueueEvent
{
    private MessageBrokerQueueEvent(
        MessageBrokerQueue queue,
        MessageBrokerSubscription? subscription,
        MessageBrokerQueueEventType type,
        ulong? messageId = null,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId,
        Exception? exception = null)
    {
        Queue = queue;
        Subscription = subscription;
        MessageId = messageId;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerQueue"/> that emitted this event.
    /// </summary>
    public MessageBrokerQueue Queue { get; }

    /// <summary>
    /// <see cref="MessageBrokerSubscription"/> related to this event.
    /// </summary>
    public MessageBrokerSubscription? Subscription { get; }

    /// <summary>
    /// Id of a message related to this event.
    /// </summary>
    public ulong? MessageId { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Queue"/> or the <see cref="Subscription"/>.
    /// </remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerQueueEventType"/> for more information.</remarks>
    public MessageBrokerQueueEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event is related to a queue-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == MessageBrokerRemoteClientEvent.RootContextId;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Queue.Name.Length + 96 );
        ToString( builder );
        return builder.ToString();
    }

    /// <summary>
    /// Appends a string representation of this <see cref="MessageBrokerQueueEvent"/> instance
    /// to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append this event to.</param>
    public void ToString(StringBuilder builder)
    {
        builder
            .Append( '[' )
            .Append( Queue.Client.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Queue.Client.Name )
            .Append( "'::'" )
            .Append( Queue.Name )
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
                case MessageBrokerQueueEventType.Created:
                {
                    if ( Subscription is not null )
                        builder
                            .Append( " by subscription to [" )
                            .Append( Subscription.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
                            .Append( "::'" )
                            .Append( Subscription.Channel.Name )
                            .Append( "']" );

                    break;
                }

                case MessageBrokerQueueEventType.MessageEnqueued:
                {
                    if ( MessageId is not null )
                        builder.Append( " MessageId = " ).Append( MessageId.Value.ToString( CultureInfo.InvariantCulture ) );

                    if ( Subscription is not null )
                        builder
                            .Append( " due to subscription to [" )
                            .Append( Subscription.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
                            .Append( "::'" )
                            .Append( Subscription.Channel.Name )
                            .Append( "']" );

                    break;
                }

                case MessageBrokerQueueEventType.MessageDequeued:
                {
                    if ( MessageId is not null )
                        builder.Append( " MessageId = " ).Append( MessageId.Value.ToString( CultureInfo.InvariantCulture ) );

                    if ( Subscription is not null )
                        builder
                            .Append( " due to subscription to [" )
                            .Append( Subscription.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
                            .Append( "::'" )
                            .Append( Subscription.Channel.Name )
                            .Append( "']" );

                    break;
                }

                case MessageBrokerQueueEventType.Unexpected:
                case MessageBrokerQueueEventType.Disposing:
                case MessageBrokerQueueEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEvent Created(
        MessageBrokerQueue queue,
        MessageBrokerSubscription subscription,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerQueueEvent( queue, subscription, MessageBrokerQueueEventType.Created, contextId: contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEvent MessageEnqueued(
        MessageBrokerQueue queue,
        MessageBrokerSubscription subscription,
        ulong messageId)
    {
        return new MessageBrokerQueueEvent( queue, subscription, MessageBrokerQueueEventType.MessageEnqueued, messageId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEvent MessageDequeued(
        MessageBrokerQueue queue,
        MessageBrokerSubscription subscription,
        ulong messageId)
    {
        return new MessageBrokerQueueEvent( queue, subscription, MessageBrokerQueueEventType.MessageDequeued, messageId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEvent Disposing(MessageBrokerQueue queue)
    {
        return new MessageBrokerQueueEvent( queue, null, MessageBrokerQueueEventType.Disposing );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEvent Disposed(MessageBrokerQueue queue)
    {
        return new MessageBrokerQueueEvent( queue, null, MessageBrokerQueueEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueEvent Unexpected(MessageBrokerQueue queue, Exception exception)
    {
        return new MessageBrokerQueueEvent( queue, null, MessageBrokerQueueEventType.Unexpected, exception: exception );
    }
}
