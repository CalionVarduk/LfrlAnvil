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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerServer"/>.
/// </summary>
public readonly struct MessageBrokerServerEvent
{
    private MessageBrokerServerEvent(MessageBrokerServer server, MessageBrokerServerEventType type, Exception? exception = null)
    {
        Server = server;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> that emitted this event.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerServerEventType"/> for more information.</remarks>
    public MessageBrokerServerEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies whether or not this event contains an <see cref="Exception"/> which represents operation cancellation.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Exception ) )]
    public bool IsCancellation => Exception is OperationCanceledException;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerServerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: 96 ).Append( '[' ).Append( Type.ToString() ).Append( ']' );
        if ( Exception is not null )
        {
            if ( ! IsCancellation )
                builder.AppendLine( " Encountered an error:" ).Append( Exception );
            else
            {
                builder.Append( " Operation cancelled" );
                if ( Exception is MessageBrokerServerDisposedException )
                    builder.Append( " (server disposed)" );
            }
        }
        else
        {
            switch ( Type )
            {
                case MessageBrokerServerEventType.Starting:
                    builder
                        .Append( " At " )
                        .Append( Server.LocalEndPoint )
                        .Append( " (HandshakeTimeout = " )
                        .Append( Server.HandshakeTimeout )
                        .Append( ", AcceptableMessageTimeout = " )
                        .Append( Server.AcceptableMessageTimeout )
                        .Append( ", AcceptablePingInterval = " )
                        .Append( Server.AcceptablePingInterval )
                        .Append( ')' );

                    break;

                case MessageBrokerServerEventType.Started:
                    builder.Append( " At " ).Append( Server.LocalEndPoint );
                    break;

                case MessageBrokerServerEventType.WaitingForClient:
                case MessageBrokerServerEventType.ClientRejected:
                case MessageBrokerServerEventType.Disposing:
                case MessageBrokerServerEventType.Disposed:
                case MessageBrokerServerEventType.Unexpected:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }

        return builder.ToString();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent Starting(MessageBrokerServer server, Exception? exception = null)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.Starting, exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent Started(MessageBrokerServer server)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.Started );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent WaitingForClient(MessageBrokerServer server, Exception? exception = null)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.WaitingForClient, exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent ClientRejected(MessageBrokerServer server, Exception exception)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.ClientRejected, exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent Disposing(MessageBrokerServer server)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.Disposing );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent Disposed(MessageBrokerServer server)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerEvent Unexpected(MessageBrokerServer server, Exception exception)
    {
        return new MessageBrokerServerEvent( server, MessageBrokerServerEventType.Unexpected, exception );
    }
}
