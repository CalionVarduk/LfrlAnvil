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
using System.Buffers.Binary;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Client.Events;
using LfrlAnvil.MessageBroker.Client.Exceptions;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal static class Protocol
{
    internal static class Endianness
    {
        internal const uint VerificationPayload = 0x0102fdfe;
    }

    internal readonly struct PacketHeader
    {
        internal const int Length = sizeof( byte ) + sizeof( uint );
        internal readonly byte EndpointCode;
        internal readonly uint Payload;

        private PacketHeader(byte endpointCode, uint payload)
        {
            EndpointCode = endpointCode;
            Payload = payload;
        }

        [Pure]
        public override string ToString()
        {
            return $"Endpoint: {EndpointCode}, Payload: {Payload}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Create(MessageBrokerServerEndpoint endpoint, uint payload)
        {
            return new PacketHeader( ( byte )endpoint, payload );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var endpointCode = reader.MoveReadInt8();
            var payload = reader.ReadInt32();
            if ( reverseEndianness )
                payload = BinaryPrimitives.ReverseEndianness( payload );

            return new PacketHeader( endpointCode, payload );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal MessageBrokerClientEndpoint GetClientEndpoint()
        {
            return ( MessageBrokerClientEndpoint )EndpointCode;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal MessageBrokerServerEndpoint GetServerEndpoint()
        {
            return ( MessageBrokerServerEndpoint )EndpointCode;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( EndpointCode );
            writer.Write( reverseEndianness ? BinaryPrimitives.ReverseEndianness( Payload ) : Payload );
        }
    }

    internal readonly struct HandshakeRequest
    {
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly Duration MessageTimeout;
        internal readonly Duration PingInterval;
        internal readonly EncodeableText ClientName;

        internal HandshakeRequest(MessageBrokerClient client)
        {
            Flags = ( byte )( /*(client.IsPersistent ? 1 : 0) |*/ (BitConverter.IsLittleEndian ? 2 : 0));
            MessageTimeout = client.MessageTimeout;
            PingInterval = client.PingInterval;
            ClientName = TextEncoding.Prepare( client.Name ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.HandshakeRequest,
                sizeof( byte ) + sizeof( uint ) * 2 + ( uint )ClientName.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return
                $"[{Header}] Flags = {Flags}, MessageTimeout = {MessageTimeout}, PingInterval = {PingInterval}, ClientName = ({ClientName})";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, Length );
            var payload = Header.Payload;
            var messageTimeoutMs = unchecked( ( uint )MessageTimeout.FullMilliseconds );
            var pingIntervalMs = unchecked( ( uint )PingInterval.FullMilliseconds );

            if ( BitConverter.IsLittleEndian )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                messageTimeoutMs = BinaryPrimitives.ReverseEndianness( messageTimeoutMs );
                pingIntervalMs = BinaryPrimitives.ReverseEndianness( pingIntervalMs );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( messageTimeoutMs );
            writer.MoveWrite( pingIntervalMs );
            ClientName.Encode( writer.GetSpan( ClientName.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct HandshakeAcceptedResponse
    {
        internal const int Length = sizeof( byte ) + sizeof( uint ) * 3;
        internal readonly byte Flags;
        internal readonly int Id;
        internal readonly Duration MessageTimeout;
        internal readonly Duration PingInterval;

        private HandshakeAcceptedResponse(byte flags, int id, Duration messageTimeout, Duration pingInterval)
        {
            Flags = flags;
            Id = id;
            MessageTimeout = messageTimeout;
            PingInterval = pingInterval;
        }

        internal bool IsServerLittleEndian => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, Id = {Id}, MessageTimeout = {MessageTimeout}, PingInterval = {PingInterval}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandshakeAcceptedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var id = unchecked( ( int )reader.MoveReadInt32() );
            var messageTimeoutMs = unchecked( ( int )reader.MoveReadInt32() );
            var pingIntervalMs = unchecked( ( int )reader.ReadInt32() );

            return new HandshakeAcceptedResponse(
                flags,
                id,
                Duration.FromMilliseconds( messageTimeoutMs ),
                Duration.FromMilliseconds( pingIntervalMs ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;

            if ( Id <= 0 )
                result = result.Extend( Resources.ClientIdIsNotPositive( Id ) );

            if ( ! Defaults.Temporal.TimeoutBounds.Contains( MessageTimeout ) )
                result = result.Extend( Resources.MessageTimeoutIsOutOfBounds( MessageTimeout ) );

            if ( ! Defaults.Temporal.PingIntervalBounds.Contains( PingInterval ) )
                result = result.Extend( Resources.PingIntervalIsOutOfBounds( PingInterval ) );

            return result;
        }
    }

    internal readonly struct HandshakeRejectedResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private HandshakeRejectedResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool InvalidNameLength => (Flags & 1) != 0;
        internal bool NameDecodingFailure => (Flags & 2) != 0;
        internal bool NameAlreadyExists => (Flags & 4) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandshakeRejectedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new HandshakeRejectedResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;

            if ( InvalidNameLength )
                result = result.Extend( Resources.ClientNameLengthOutOfBounds );

            if ( NameDecodingFailure )
                result = result.Extend( Resources.ServerFailedToDecodeClientName );

            if ( NameAlreadyExists )
                result = result.Extend( Resources.ClientNameAlreadyExists );

            return result;
        }
    }

    internal readonly struct ConfirmHandshakeResponse
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Create()
        {
            return PacketHeader.Create( MessageBrokerServerEndpoint.ConfirmHandshakeResponse, Endianness.VerificationPayload );
        }
    }

    internal readonly struct PingRequest
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Create()
        {
            return PacketHeader.Create( MessageBrokerServerEndpoint.PingRequest, Endianness.VerificationPayload );
        }
    }

    internal readonly struct LinkChannelRequest
    {
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly EncodeableText ChannelName;

        internal LinkChannelRequest(string channelName)
        {
            Flags = 0;
            ChannelName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.LinkChannelRequest,
                sizeof( byte ) + ( uint )ChannelName.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelName = ({ChannelName})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( reverseEndianness ? BinaryPrimitives.ReverseEndianness( Header.Payload ) : Header.Payload );
            writer.MoveWrite( Flags );
            ChannelName.Encode( writer.GetSpan( ChannelName.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct ChannelLinkedResponse
    {
        internal const int Length = sizeof( byte ) + sizeof( uint );
        internal readonly byte Flags;
        internal readonly int Id;

        private ChannelLinkedResponse(byte flags, int id)
        {
            Flags = flags;
            Id = id;
        }

        internal bool Created => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, Id = {Id}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ChannelLinkedResponse Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var id = unchecked( ( int )reader.ReadInt32() );
            return new ChannelLinkedResponse( flags, reverseEndianness ? BinaryPrimitives.ReverseEndianness( id ) : id );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            return Id > 0 ? Chain<string>.Empty : Chain.Create( Resources.ChannelIdIsNotPositive( Id ) );
        }
    }

    internal readonly struct LinkChannelFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private LinkChannelFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ClientAlreadyLinkedToChannel => (Flags & 1) != 0;
        internal bool LinkingCancelled => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static LinkChannelFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new LinkChannelFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(string channelName)
        {
            var result = Chain<string>.Empty;

            if ( ClientAlreadyLinkedToChannel )
                result = result.Extend( Resources.ClientAlreadyLinkedToChannel( channelName ) );

            if ( LinkingCancelled )
                result = result.Extend( Resources.ClientChannelLinkingCancelled( channelName ) );

            return result;
        }
    }

    internal readonly struct UnlinkChannelRequest
    {
        internal const int Length = PacketHeader.Length + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly int ChannelId;

        internal UnlinkChannelRequest(int channelId)
        {
            ChannelId = channelId;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.UnlinkChannelRequest, sizeof( uint ) );
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] ChannelId = {ChannelId}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var channelId = unchecked( ( uint )ChannelId );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.Write( channelId );
        }
    }

    internal readonly struct ChannelUnlinkedResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private ChannelUnlinkedResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ChannelRemoved => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ChannelUnlinkedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new ChannelUnlinkedResponse( flags );
        }
    }

    internal readonly struct UnlinkChannelFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private UnlinkChannelFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ClientNotLinked => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnlinkChannelFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new UnlinkChannelFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(MessageBrokerLinkedChannel channel)
        {
            return ClientNotLinked ? Chain.Create( Resources.ClientIsNotLinkedToChannel( channel.Id, channel.Name ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct SubscribeRequest
    {
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly EncodeableText ChannelName;

        internal SubscribeRequest(string channelName, bool createChannelIfNotExists)
        {
            Flags = ( byte )(createChannelIfNotExists ? 1 : 0);
            ChannelName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.SubscribeRequest, sizeof( byte ) + ( uint )ChannelName.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelName = ({ChannelName})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( reverseEndianness ? BinaryPrimitives.ReverseEndianness( Header.Payload ) : Header.Payload );
            writer.MoveWrite( Flags );
            ChannelName.Encode( writer.GetSpan( ChannelName.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct SubscribedResponse
    {
        internal const int Length = sizeof( byte ) + sizeof( uint );
        internal readonly byte Flags;
        internal readonly int ChannelId;

        private SubscribedResponse(byte flags, int channelId)
        {
            Flags = flags;
            ChannelId = channelId;
        }

        internal bool ChannelCreated => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, ChannelId = {ChannelId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static SubscribedResponse Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var id = unchecked( ( int )reader.ReadInt32() );
            return new SubscribedResponse( flags, reverseEndianness ? BinaryPrimitives.ReverseEndianness( id ) : id );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            return ChannelId > 0 ? Chain<string>.Empty : Chain.Create( Resources.ChannelIdIsNotPositive( ChannelId ) );
        }
    }

    internal readonly struct SubscribeFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private SubscribeFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ChannelDoesNotExist => (Flags & 1) != 0;
        internal bool ClientAlreadySubscribedToChannel => (Flags & 2) != 0;
        internal bool SubscribingCancelled => (Flags & 4) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static SubscribeFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new SubscribeFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(string channelName)
        {
            var result = Chain<string>.Empty;

            if ( ChannelDoesNotExist )
                result = result.Extend( Resources.ChannelDoesNotExist( channelName ) );

            if ( ClientAlreadySubscribedToChannel )
                result = result.Extend( Resources.ClientAlreadySubscribedToChannel( channelName ) );

            if ( SubscribingCancelled )
                result = result.Extend( Resources.ClientSubscribingCancelled( channelName ) );

            return result;
        }
    }

    internal readonly struct UnsubscribeRequest
    {
        internal const int Length = PacketHeader.Length + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly int ChannelId;

        internal UnsubscribeRequest(int channelId)
        {
            ChannelId = channelId;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.UnsubscribeRequest, sizeof( uint ) );
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] ChannelId = {ChannelId}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var channelId = unchecked( ( uint )ChannelId );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.Write( channelId );
        }
    }

    internal readonly struct UnsubscribedResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private UnsubscribedResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ChannelRemoved => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnsubscribedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new UnsubscribedResponse( flags );
        }
    }

    internal readonly struct UnsubscribeFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private UnsubscribeFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ClientNotSubscribed => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnsubscribeFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new UnsubscribeFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(MessageBrokerListener listener)
        {
            return ClientNotSubscribed
                ? Chain.Create( Resources.ClientIsNotSubscribedToChannel( listener.ChannelId, listener.ChannelName ) )
                : Chain<string>.Empty;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientRequestException RequestException(
        MessageBrokerClient client,
        PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerClientRequestException( client, header.GetServerEndpoint(), header.Payload, errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException ProtocolException(
        MessageBrokerClient client,
        PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerClientProtocolException( client, header.GetClientEndpoint(), header.Payload, errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException UnexpectedClientEndpointException(MessageBrokerClient client, PacketHeader header)
    {
        return ProtocolException( client, header, Chain.Create( Resources.UnexpectedClientEndpoint ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException EndiannessPayloadException(MessageBrokerClient client, PacketHeader header)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidEndiannessPayload( header.Payload ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException? AssertPayload(MessageBrokerClient client, PacketHeader header, uint expected)
    {
        return header.Payload != expected
            ? ProtocolException( client, header, Chain.Create( Resources.InvalidHeaderPayload( expected ) ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<int> AssertPacketLength(MessageBrokerClient client, PacketHeader header)
    {
        var result = unchecked( ( int )header.Payload );
        if ( result >= 0 )
            return result;

        return ProtocolException( client, header, Chain.Create( Resources.UnexpectedPacketLength( result ) ) );
    }
}
