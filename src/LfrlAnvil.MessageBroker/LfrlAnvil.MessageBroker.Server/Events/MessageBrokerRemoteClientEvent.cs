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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/>.
/// </summary>
public readonly struct MessageBrokerRemoteClientEvent
{
    internal const int RootContextId = 0;

    private MessageBrokerRemoteClientEvent(
        MessageBrokerRemoteClient client,
        MessageBrokerRemoteClientEventType type,
        ulong contextId = RootContextId,
        byte endpointCode = 0,
        uint payload = 0,
        Exception? exception = null)
    {
        Client = client;
        ContextId = contextId;
        Payload = payload;
        EndpointCode = endpointCode;
        Type = type;
        Exception = exception;
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> that emitted this event.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Id of an internal context with which this event is associated.
    /// </summary>
    /// <remarks>Can be used to find other correlating events.</remarks>
    public ulong ContextId { get; }

    /// <summary>
    /// Packet's header payload. Equal to <b>0</b> for events unrelated to sending or receiving network packets.
    /// </summary>
    public uint Payload { get; }

    /// <summary>
    /// Client or server endpoint. Equal to <b>0</b> for events unrelated to sending or receiving network packets.
    /// </summary>
    public byte EndpointCode { get; }

    /// <summary>
    /// Specifies the type of this event.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerRemoteClientEventType"/> for more information.</remarks>
    public MessageBrokerRemoteClientEventType Type { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Specifies the length of sent or received network packet, with which this event is associated.
    /// </summary>
    public int PacketLength
    {
        get
        {
            switch ( Type )
            {
                case MessageBrokerRemoteClientEventType.MessageReceived:
                case MessageBrokerRemoteClientEventType.MessageAccepted:
                case MessageBrokerRemoteClientEventType.MessageRejected:
                    return Protocol.PacketHeader.Length
                        + (GetServerEndpoint() < MessageBrokerServerEndpoint.HandshakeRequest ? 0 : unchecked( ( int )Payload ));
                case MessageBrokerRemoteClientEventType.SendingMessage:
                case MessageBrokerRemoteClientEventType.MessageSent:
                    return Protocol.PacketHeader.Length
                        + (GetClientEndpoint() == MessageBrokerClientEndpoint.PingResponse ? 0 : unchecked( ( int )Payload ));
                default:
                    return 0;
            }
        }
    }

    /// <summary>
    /// Specifies whether or not this event is related to a client-wide operation.
    /// </summary>
    public bool IsRootContext => ContextId == RootContextId;

    /// <summary>
    /// Specifies whether or not this event contains an <see cref="Exception"/> which represents operation cancellation.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Exception ) )]
    public bool IsCancellation => Exception is OperationCanceledException;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Client.Name.Length + 96 )
            .Append( '[' )
            .Append( Client.Id.ToString( CultureInfo.InvariantCulture ) );

        if ( Client.Name.Length > 0 )
            builder.Append( "::'" ).Append( Client.Name ).Append( '\'' );

        builder
            .Append( "::" )
            .Append( IsRootContext ? "<ROOT>" : ContextId.ToString( CultureInfo.InvariantCulture ) )
            .Append( "] [" )
            .Append( Type.ToString() )
            .Append( ']' );

        var packetLength = PacketLength;
        if ( packetLength > 0 )
            builder
                .AppendSpace()
                .Append( "[PacketLength: " )
                .Append( packetLength.ToString( CultureInfo.InvariantCulture ) )
                .Append( ']' );

        if ( Exception is not null )
        {
            if ( ! IsCancellation )
                builder.AppendLine( " Encountered an error:" ).Append( Exception );
            else
            {
                builder.Append( " Operation cancelled" );
                if ( Exception is MessageBrokerRemoteClientDisposedException )
                    builder.Append( " (client disposed)" );
                else if ( Exception is MessageBrokerServerDisposedException )
                    builder.Append( " (server disposed)" );
            }
        }
        else
        {
            switch ( Type )
            {
                case MessageBrokerRemoteClientEventType.Created:
                {
                    var remoteEndPoint = Client.RemoteEndPoint;
                    if ( remoteEndPoint is not null )
                        builder.Append( " From " ).Append( remoteEndPoint );

                    break;
                }
                case MessageBrokerRemoteClientEventType.MessageReceived:
                    if ( IsRootContext && GetServerEndpoint() != MessageBrokerServerEndpoint.HandshakeRequest )
                        builder.AppendSpace().Append( GetServerEndpoint() );
                    else
                        builder.Append( " Begin handling " ).Append( GetServerEndpoint() );

                    break;

                case MessageBrokerRemoteClientEventType.MessageAccepted:
                    builder.AppendSpace().Append( GetServerEndpoint() );
                    if ( GetServerEndpoint() == MessageBrokerServerEndpoint.HandshakeRequest )
                    {
                        builder
                            .Append( " (IsLittleEndian = " )
                            .Append( Client.IsLittleEndian )
                            .Append( ", MessageTimeout = " )
                            .Append( Client.MessageTimeout )
                            .Append( ", PingInterval = " )
                            .Append( Client.PingInterval )
                            .Append( ')' );
                    }

                    break;

                case MessageBrokerRemoteClientEventType.SendingMessage:
                case MessageBrokerRemoteClientEventType.MessageSent:
                    builder.AppendSpace().Append( GetClientEndpoint() );
                    break;

                case MessageBrokerRemoteClientEventType.Unexpected:
                case MessageBrokerRemoteClientEventType.WaitingForMessage:
                case MessageBrokerRemoteClientEventType.MessageRejected:
                case MessageBrokerRemoteClientEventType.Disposing:
                case MessageBrokerRemoteClientEventType.Disposed:
                    break;

                default:
                    builder.Append( " <UNKNOWN>" );
                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns <see cref="EndpointCode"/> as <see cref="MessageBrokerServerEndpoint"/>.
    /// </summary>
    /// <returns><see cref="EndpointCode"/> as <see cref="MessageBrokerServerEndpoint"/>.</returns>
    [Pure]
    public MessageBrokerServerEndpoint GetServerEndpoint()
    {
        return ( MessageBrokerServerEndpoint )EndpointCode;
    }

    /// <summary>
    /// Returns <see cref="EndpointCode"/> as <see cref="MessageBrokerClientEndpoint"/>.
    /// </summary>
    /// <returns><see cref="EndpointCode"/> as <see cref="MessageBrokerClientEndpoint"/>.</returns>
    [Pure]
    public MessageBrokerClientEndpoint GetClientEndpoint()
    {
        return ( MessageBrokerClientEndpoint )EndpointCode;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent Created(MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientEvent( client, MessageBrokerRemoteClientEventType.Created );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent WaitingForMessage(MessageBrokerRemoteClient client, Exception? exception = null)
    {
        return new MessageBrokerRemoteClientEvent( client, MessageBrokerRemoteClientEventType.WaitingForMessage, exception: exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent MessageReceived(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        ulong contextId = RootContextId,
        Exception? exception = null)
    {
        return new MessageBrokerRemoteClientEvent(
            client,
            MessageBrokerRemoteClientEventType.MessageReceived,
            contextId,
            header.EndpointCode,
            header.Payload,
            exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent MessageAccepted(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        ulong contextId = RootContextId)
    {
        return new MessageBrokerRemoteClientEvent(
            client,
            MessageBrokerRemoteClientEventType.MessageAccepted,
            contextId,
            header.EndpointCode,
            header.Payload );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent MessageRejected(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        Exception exception,
        ulong contextId = RootContextId)
    {
        return new MessageBrokerRemoteClientEvent(
            client,
            MessageBrokerRemoteClientEventType.MessageRejected,
            contextId,
            header.EndpointCode,
            header.Payload,
            exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent SendingMessage(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        ulong contextId,
        Exception? exception = null)
    {
        return new MessageBrokerRemoteClientEvent(
            client,
            MessageBrokerRemoteClientEventType.SendingMessage,
            contextId,
            header.EndpointCode,
            header.Payload,
            exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent MessageSent(
        MessageBrokerRemoteClient client,
        Protocol.PacketHeader header,
        ulong contextId)
    {
        return new MessageBrokerRemoteClientEvent(
            client,
            MessageBrokerRemoteClientEventType.MessageSent,
            contextId,
            header.EndpointCode,
            header.Payload );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent Disposing(MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientEvent( client, MessageBrokerRemoteClientEventType.Disposing );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent Disposed(MessageBrokerRemoteClient client)
    {
        return new MessageBrokerRemoteClientEvent( client, MessageBrokerRemoteClientEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientEvent Unexpected(MessageBrokerRemoteClient client, Exception exception)
    {
        return new MessageBrokerRemoteClientEvent( client, MessageBrokerRemoteClientEventType.Unexpected, exception: exception );
    }
}
