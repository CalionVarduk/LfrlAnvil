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
/// Represents an event emitted by <see cref="MessageBrokerChannel"/>.
/// </summary>
public readonly struct MessageBrokerChannelEvent
{
    private MessageBrokerChannelEvent(
        MessageBrokerChannel channel,
        MessageBrokerRemoteClient? client,
        MessageBrokerChannelEventType type,
        ulong contextId = 0,
        Exception? exception = null)
    {
        Channel = channel;
        Client = client;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> that emitted this event.
    /// </summary>
    public MessageBrokerChannel Channel { get; }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> related to this event.
    /// </summary>
    public MessageBrokerRemoteClient? Client { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Channel"/> or the <see cref="Client"/>.
    /// </remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelEventType"/> for more information.</remarks>
    public MessageBrokerChannelEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event is related to a channel-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Channel.Name.Length + 96 );
        ToString( builder );
        return builder.ToString();
    }

    /// <summary>
    /// Appends a string representation of this <see cref="MessageBrokerChannelEvent"/> instance
    /// to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append this event to.</param>
    public void ToString(StringBuilder builder)
    {
        builder
            .Append( '[' )
            .Append( Channel.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Channel.Name )
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
                case MessageBrokerChannelEventType.Created:
                {
                    if ( Client is not null )
                        builder
                            .Append( " by client [" )
                            .Append( Client.Id.ToString( CultureInfo.InvariantCulture ) )
                            .Append( "::'" )
                            .Append( Client.Name )
                            .Append( "']" );

                    break;
                }

                case MessageBrokerChannelEventType.Unexpected:
                case MessageBrokerChannelEventType.Disposing:
                case MessageBrokerChannelEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelEvent Created(
        MessageBrokerChannel channel,
        MessageBrokerRemoteClient? client,
        ulong contextId = 0)
    {
        return new MessageBrokerChannelEvent( channel, client, MessageBrokerChannelEventType.Created, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelEvent Disposing(MessageBrokerChannel channel)
    {
        return new MessageBrokerChannelEvent( channel, null, MessageBrokerChannelEventType.Disposing );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelEvent Disposed(MessageBrokerChannel channel)
    {
        return new MessageBrokerChannelEvent( channel, null, MessageBrokerChannelEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelEvent Unexpected(MessageBrokerChannel channel, Exception exception)
    {
        return new MessageBrokerChannelEvent( channel, null, MessageBrokerChannelEventType.Unexpected, exception: exception );
    }
}
