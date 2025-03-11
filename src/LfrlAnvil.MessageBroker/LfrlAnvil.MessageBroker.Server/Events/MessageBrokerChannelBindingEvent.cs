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
/// Represents an event emitted by <see cref="MessageBrokerChannelBinding"/>.
/// </summary>
public readonly struct MessageBrokerChannelBindingEvent
{
    private MessageBrokerChannelBindingEvent(
        MessageBrokerChannelBinding binding,
        MessageBrokerChannelBindingEventType type,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId,
        Exception? exception = null)
    {
        Binding = binding;
        ContextId = contextId;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerChannelBinding"/> that emitted this event.
    /// </summary>
    public MessageBrokerChannelBinding Binding { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>
    /// Can be used to find other correlating events emitted either by the <see cref="Binding"/> or related client or channel.
    /// </remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelBindingEventType"/> for more information.</remarks>
    public MessageBrokerChannelBindingEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event is related to a binding-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == MessageBrokerRemoteClientEvent.RootContextId;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelBindingEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Binding.Client.Name.Length + Binding.Channel.Name.Length + 96 );
        ToString( builder );
        return builder.ToString();
    }

    /// <summary>
    /// Appends a string representation of this <see cref="MessageBrokerChannelBindingEvent"/> instance
    /// to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="builder"><see cref="StringBuilder"/> to append this event to.</param>
    public void ToString(StringBuilder builder)
    {
        builder
            .Append( '[' )
            .Append( Binding.Client.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Binding.Client.Name )
            .Append( "'=>" )
            .Append( Binding.Channel.Id.ToString( CultureInfo.InvariantCulture ) )
            .Append( "::'" )
            .Append( Binding.Channel.Name )
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
                case MessageBrokerChannelBindingEventType.Created:
                case MessageBrokerChannelBindingEventType.Unexpected:
                case MessageBrokerChannelBindingEventType.Disposing:
                case MessageBrokerChannelBindingEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelBindingEvent Created(
        MessageBrokerChannelBinding binding,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerChannelBindingEvent( binding, MessageBrokerChannelBindingEventType.Created, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelBindingEvent Disposing(
        MessageBrokerChannelBinding binding,
        ulong contextId = MessageBrokerRemoteClientEvent.RootContextId)
    {
        return new MessageBrokerChannelBindingEvent( binding, MessageBrokerChannelBindingEventType.Disposing, contextId );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelBindingEvent Disposed(MessageBrokerChannelBinding binding)
    {
        return new MessageBrokerChannelBindingEvent( binding, MessageBrokerChannelBindingEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelBindingEvent Unexpected(MessageBrokerChannelBinding binding, Exception exception)
    {
        return new MessageBrokerChannelBindingEvent( binding, MessageBrokerChannelBindingEventType.Unexpected, exception: exception );
    }
}
