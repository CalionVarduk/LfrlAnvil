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
        MessageBrokerChannelBinding? binding,
        MessageBrokerQueueEventType type,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId,
        Exception? exception = null)
    {
        Queue = queue;
        Binding = binding;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerQueue"/> that emitted this event.
    /// </summary>
    public MessageBrokerQueue Queue { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelBinding"/> related to this event.
    /// </summary>
    public MessageBrokerChannelBinding? Binding { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Queue"/> or the <see cref="Binding"/>.
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
            .Append( Queue.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
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
                    if ( Binding is not null )
                        builder
                            .Append( " by binding [" )
                            .Append( Binding.Client.Id.ToString( CultureInfo.InvariantCulture ) )
                            .Append( "::'" )
                            .Append( Binding.Client.Name )
                            .Append( "'] => [" )
                            .Append( Binding.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
                            .Append( "::'" )
                            .Append( Binding.Channel.Name )
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
        MessageBrokerChannelBinding? binding,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerQueueEvent( queue, binding, MessageBrokerQueueEventType.Created, contextId );
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
