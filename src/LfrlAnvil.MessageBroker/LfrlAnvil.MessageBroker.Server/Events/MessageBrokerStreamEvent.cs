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
/// Represents an event emitted by <see cref="MessageBrokerStream"/>.
/// </summary>
public readonly struct MessageBrokerStreamEvent
{
    private MessageBrokerStreamEvent(
        MessageBrokerStream stream,
        MessageBrokerChannelBinding? binding,
        MessageBrokerStreamEventType type,
        ulong? messageId = null,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId,
        Exception? exception = null)
    {
        Stream = stream;
        Binding = binding;
        MessageId = messageId;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerStream"/> that emitted this event.
    /// </summary>
    public MessageBrokerStream Stream { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelBinding"/> related to this event.
    /// </summary>
    public MessageBrokerChannelBinding? Binding { get; }

    /// <summary>
    /// Id of a message related to this event.
    /// </summary>
    public ulong? MessageId { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Stream"/> or the <see cref="Binding"/>.
    /// </remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerStreamEventType"/> for more information.</remarks>
    public MessageBrokerStreamEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event is related to a stream-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == MessageBrokerRemoteClientEvent.RootContextId;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerStreamEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Stream.Name.Length + 96 );
        ToString( builder );
        return builder.ToString();
    }

    /// <summary>
    /// Appends a string representation of this <see cref="MessageBrokerStreamEvent"/> instance
    /// to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append this event to.</param>
    public void ToString(StringBuilder builder)
    {
        builder
            .Append( '[' )
            .Append( Stream.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Stream.Name )
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
                case MessageBrokerStreamEventType.Created:
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

                case MessageBrokerStreamEventType.MessageEnqueued:
                {
                    if ( MessageId is not null )
                        builder.Append( " MessageId = " ).Append( MessageId.Value.ToString( CultureInfo.InvariantCulture ) );

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

                case MessageBrokerStreamEventType.MessageDequeued:
                {
                    if ( MessageId is not null )
                        builder.Append( " MessageId = " ).Append( MessageId.Value.ToString( CultureInfo.InvariantCulture ) );

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

                case MessageBrokerStreamEventType.Unexpected:
                case MessageBrokerStreamEventType.Disposing:
                case MessageBrokerStreamEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEvent Created(
        MessageBrokerStream stream,
        MessageBrokerChannelBinding binding,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerStreamEvent( stream, binding, MessageBrokerStreamEventType.Created, contextId: contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEvent MessageEnqueued(
        MessageBrokerStream stream,
        MessageBrokerChannelBinding binding,
        ulong messageId,
        ulong contextId)
    {
        return new MessageBrokerStreamEvent( stream, binding, MessageBrokerStreamEventType.MessageEnqueued, messageId, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEvent MessageDequeued(
        MessageBrokerStream stream,
        MessageBrokerChannelBinding binding,
        ulong messageId)
    {
        return new MessageBrokerStreamEvent( stream, binding, MessageBrokerStreamEventType.MessageDequeued, messageId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEvent Disposing(MessageBrokerStream stream)
    {
        return new MessageBrokerStreamEvent( stream, null, MessageBrokerStreamEventType.Disposing );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEvent Disposed(MessageBrokerStream stream)
    {
        return new MessageBrokerStreamEvent( stream, null, MessageBrokerStreamEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerStreamEvent Unexpected(MessageBrokerStream stream, Exception exception)
    {
        return new MessageBrokerStreamEvent( stream, null, MessageBrokerStreamEventType.Unexpected, exception: exception );
    }
}
