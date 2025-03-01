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
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/>.
/// </summary>
public readonly struct MessageBrokerClientEvent
{
    internal const int RootContextId = 0;

    private MessageBrokerClientEvent(
        MessageBrokerClient client,
        MessageBrokerClientEventType type,
        ulong contextId = RootContextId,
        byte endpointCode = 0,
        uint payload = 0,
        object? data = null)
    {
        Client = client;
        ContextId = contextId;
        Payload = payload;
        EndpointCode = endpointCode;
        Type = type;
        Data = data;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> that emitted this event.
    /// </summary>
    public MessageBrokerClient Client { get; }

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
    /// <remarks>See <see cref="MessageBrokerClientEventType"/> for more information.</remarks>
    public MessageBrokerClientEventType Type { get; }

    /// <summary>
    /// Additional data associated with this event.
    /// </summary>
    public object? Data { get; }

    /// <summary>
    /// Error associated with this event.
    /// </summary>
    public Exception? Exception => Data as Exception;

    /// <summary>
    /// Specifies the length of sent or received network packet, with which this event is associated.
    /// </summary>
    public int PacketLength
    {
        get
        {
            switch ( Type )
            {
                case MessageBrokerClientEventType.MessageReceived:
                case MessageBrokerClientEventType.MessageAccepted:
                case MessageBrokerClientEventType.MessageRejected:
                    return Protocol.PacketHeader.Length
                        + (GetClientEndpoint() == MessageBrokerClientEndpoint.PingResponse ? 0 : unchecked( ( int )Payload ));
                case MessageBrokerClientEventType.SendingMessage:
                case MessageBrokerClientEventType.MessageSent:
                    return Protocol.PacketHeader.Length
                        + (GetServerEndpoint() < MessageBrokerServerEndpoint.HandshakeRequest ? 0 : unchecked( ( int )Payload ));
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
    [MemberNotNullWhen( true, nameof( Data ) )]
    [MemberNotNullWhen( true, nameof( Exception ) )]
    public bool IsCancellation => Exception is OperationCanceledException;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var builder = new StringBuilder( capacity: Client.Name.Length + 96 )
            .Append( "['" )
            .Append( Client.Name )
            .Append( "'::" )
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

        var exc = Exception;
        if ( exc is not null )
        {
            if ( ! IsCancellation || exc is MessageBrokerClientResponseTimeoutException )
                builder.AppendLine( " Encountered an error:" ).Append( exc );
            else
            {
                builder.Append( " Operation cancelled" );
                if ( exc is MessageBrokerClientDisposedException )
                    builder.Append( " (client disposed)" );
            }
        }
        else
        {
            switch ( Type )
            {
                case MessageBrokerClientEventType.Connecting:
                    builder.Append( " To server at " ).Append( Client.RemoteEndPoint );
                    break;

                case MessageBrokerClientEventType.Connected:
                {
                    var localEndPoint = Client.LocalEndPoint;
                    if ( localEndPoint is not null )
                        builder.Append( " From " ).Append( localEndPoint );

                    break;
                }

                case MessageBrokerClientEventType.MessageReceived:
                    if ( IsRootContext && GetClientEndpoint() != MessageBrokerClientEndpoint.HandshakeAcceptedResponse )
                        builder.AppendSpace().Append( GetClientEndpoint().ToString() );
                    else
                        builder.AppendSpace().Append( "Begin handling " ).Append( GetClientEndpoint().ToString() );

                    break;

                case MessageBrokerClientEventType.MessageAccepted:
                    builder.AppendSpace().Append( GetClientEndpoint().ToString() );
                    if ( GetClientEndpoint() == MessageBrokerClientEndpoint.HandshakeAcceptedResponse )
                    {
                        builder
                            .Append( " (Id = " )
                            .Append( Client.Id )
                            .Append( ", IsServerLittleEndian = " )
                            .Append( Client.IsServerLittleEndian )
                            .Append( ", MessageTimeout = " )
                            .Append( Client.MessageTimeout )
                            .Append( ", PingInterval = " )
                            .Append( Client.PingInterval )
                            .Append( ')' );
                    }
                    else if ( GetClientEndpoint() == MessageBrokerClientEndpoint.ChannelLinkedResponse
                        && Data is MessageBrokerLinkedChannel channel )
                        builder.Append( " (Id = " ).Append( channel.Id ).Append( ')' );

                    break;

                case MessageBrokerClientEventType.SendingMessage:
                    builder.AppendSpace().Append( GetServerEndpoint().ToString() );
                    if ( GetServerEndpoint() == MessageBrokerServerEndpoint.LinkChannelRequest && Data is string channelName )
                        builder.Append( " (ChannelName = '" ).Append( channelName ).Append( "')" );

                    break;
                case MessageBrokerClientEventType.MessageSent:
                    builder.AppendSpace().Append( GetServerEndpoint().ToString() );
                    break;

                case MessageBrokerClientEventType.Unexpected:
                case MessageBrokerClientEventType.WaitingForMessage:
                case MessageBrokerClientEventType.MessageRejected:
                case MessageBrokerClientEventType.Disposing:
                case MessageBrokerClientEventType.Disposed:
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
    internal static MessageBrokerClientEvent Connecting(MessageBrokerClient client, Exception? exception = null)
    {
        return new MessageBrokerClientEvent( client, MessageBrokerClientEventType.Connecting, data: exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent Connected(MessageBrokerClient client)
    {
        return new MessageBrokerClientEvent( client, MessageBrokerClientEventType.Connected );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent WaitingForMessage(MessageBrokerClient client, Exception? exception = null)
    {
        return new MessageBrokerClientEvent( client, MessageBrokerClientEventType.WaitingForMessage, data: exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent MessageReceived(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        ulong contextId = RootContextId,
        Exception? exception = null)
    {
        return new MessageBrokerClientEvent(
            client,
            MessageBrokerClientEventType.MessageReceived,
            contextId,
            header.EndpointCode,
            header.Payload,
            exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent MessageAccepted(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        ulong contextId = RootContextId,
        object? data = null)
    {
        return new MessageBrokerClientEvent(
            client,
            MessageBrokerClientEventType.MessageAccepted,
            contextId,
            header.EndpointCode,
            header.Payload,
            data );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent MessageRejected(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        Exception exception,
        ulong contextId = RootContextId)
    {
        return new MessageBrokerClientEvent(
            client,
            MessageBrokerClientEventType.MessageRejected,
            contextId,
            header.EndpointCode,
            header.Payload,
            exception );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent SendingMessage(
        MessageBrokerClient client,
        Protocol.PacketHeader header,
        ulong contextId,
        object? data = null)
    {
        return new MessageBrokerClientEvent(
            client,
            MessageBrokerClientEventType.SendingMessage,
            contextId,
            header.EndpointCode,
            header.Payload,
            data );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent MessageSent(MessageBrokerClient client, Protocol.PacketHeader header, ulong contextId)
    {
        return new MessageBrokerClientEvent(
            client,
            MessageBrokerClientEventType.MessageSent,
            contextId,
            header.EndpointCode,
            header.Payload );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent Disposing(MessageBrokerClient client)
    {
        return new MessageBrokerClientEvent( client, MessageBrokerClientEventType.Disposing );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent Disposed(MessageBrokerClient client)
    {
        return new MessageBrokerClientEvent( client, MessageBrokerClientEventType.Disposed );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientEvent Unexpected(MessageBrokerClient client, Exception exception)
    {
        return new MessageBrokerClientEvent( client, MessageBrokerClientEventType.Unexpected, data: exception );
    }
}
