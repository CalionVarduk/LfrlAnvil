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
/// Represents an event emitted by <see cref="MessageBrokerChannelPublisherBinding"/>.
/// </summary>
public readonly struct MessageBrokerChannelPublisherBindingEvent
{
    private MessageBrokerChannelPublisherBindingEvent(
        MessageBrokerChannelPublisherBinding publisher,
        MessageBrokerChannelPublisherBindingEventType type,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId,
        Exception? exception = null)
    {
        Publisher = publisher;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that emitted this event.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Publisher"/> or related client or channel.
    /// </remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelPublisherBindingEventType"/> for more information.</remarks>
    public MessageBrokerChannelPublisherBindingEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event is related to a publisher-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == MessageBrokerRemoteClientEvent.RootContextId;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelPublisherBindingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Publisher.Client.Name.Length + Publisher.Channel.Name.Length + 96 );
        ToString( builder );
        return builder.ToString();
    }

    /// <summary>
    /// Appends a string representation of this <see cref="MessageBrokerChannelPublisherBindingEvent"/> instance
    /// to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append this event to.</param>
    public void ToString(StringBuilder builder)
    {
        builder
            .Append( '[' )
            .Append( Publisher.Client.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Publisher.Client.Name )
            .Append( "'=>" )
            .Append( Publisher.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Publisher.Channel.Name )
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
                case MessageBrokerChannelPublisherBindingEventType.Created:
                case MessageBrokerChannelPublisherBindingEventType.Unexpected:
                case MessageBrokerChannelPublisherBindingEventType.Disposing:
                case MessageBrokerChannelPublisherBindingEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBindingEvent Created(
        MessageBrokerChannelPublisherBinding publisher,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerChannelPublisherBindingEvent( publisher, MessageBrokerChannelPublisherBindingEventType.Created, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBindingEvent Disposing(
        MessageBrokerChannelPublisherBinding publisher,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerChannelPublisherBindingEvent(
            publisher,
            MessageBrokerChannelPublisherBindingEventType.Disposing,
            contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBindingEvent Disposed(MessageBrokerChannelPublisherBinding publisher)
    {
        return new MessageBrokerChannelPublisherBindingEvent( publisher, MessageBrokerChannelPublisherBindingEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelPublisherBindingEvent Unexpected(
        MessageBrokerChannelPublisherBinding publisher,
        Exception exception)
    {
        return new MessageBrokerChannelPublisherBindingEvent(
            publisher,
            MessageBrokerChannelPublisherBindingEventType.Unexpected,
            exception: exception );
    }
}
