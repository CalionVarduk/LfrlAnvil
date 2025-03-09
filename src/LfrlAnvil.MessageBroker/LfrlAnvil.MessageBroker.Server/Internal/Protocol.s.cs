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
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

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
        internal static PacketHeader Create(MessageBrokerClientEndpoint endpoint, uint payload)
        {
            return new PacketHeader( ( byte )endpoint, payload );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var endpointCode = reader.MoveReadInt8();
            var payload = reader.ReadInt32();
            return new PacketHeader( endpointCode, payload );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal MessageBrokerServerEndpoint GetServerEndpoint()
        {
            return ( MessageBrokerServerEndpoint )EndpointCode;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal PacketHeader ReverseEndianness()
        {
            return new PacketHeader( EndpointCode, BinaryPrimitives.ReverseEndianness( Payload ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, Length );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( EndpointCode );
            writer.Write( Payload );
        }
    }

    internal readonly struct HandshakeRequestHeader
    {
        internal const int Length = sizeof( byte ) + sizeof( uint ) * 2;
        internal readonly byte Flags;
        internal readonly Duration MessageTimeout;
        internal readonly Duration PingInterval;

        private HandshakeRequestHeader(byte flags, Duration messageTimeout, Duration pingInterval)
        {
            Flags = flags;
            MessageTimeout = messageTimeout;
            PingInterval = pingInterval;
        }

        internal bool IsClientLittleEndian => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, MessageTimeout = {MessageTimeout}, PingInterval = {PingInterval}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandshakeRequestHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var messageTimeoutMs = unchecked( ( int )reader.MoveReadInt32() );
            var pingIntervalMs = unchecked( ( int )reader.MoveReadInt32() );

            if ( BitConverter.IsLittleEndian )
            {
                messageTimeoutMs = BinaryPrimitives.ReverseEndianness( messageTimeoutMs );
                pingIntervalMs = BinaryPrimitives.ReverseEndianness( pingIntervalMs );
            }

            return new HandshakeRequestHeader(
                flags,
                Duration.FromMilliseconds( messageTimeoutMs ),
                Duration.FromMilliseconds( pingIntervalMs ) );
        }
    }

    internal readonly struct HandshakeAcceptedResponse
    {
        internal const int Payload = sizeof( byte ) + sizeof( uint ) * 3;
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int Id;
        internal readonly Duration MessageTimeout;
        internal readonly Duration PingInterval;

        internal HandshakeAcceptedResponse(MessageBrokerRemoteClient client)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.HandshakeAcceptedResponse, Payload );
            Flags = ( byte )(BitConverter.IsLittleEndian ? 1 : 0);
            Id = client.Id;
            MessageTimeout = client.MessageTimeout;
            PingInterval = client.PingInterval;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, Id = {Id}, MessageTimeout = {MessageTimeout}, PingInterval = {PingInterval}";
        }

        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );

            var payload = Header.Payload;
            var id = unchecked( ( uint )Id );
            var messageTimeoutMs = unchecked( ( uint )MessageTimeout.FullMilliseconds );
            var pingIntervalMs = unchecked( ( uint )PingInterval.FullMilliseconds );

            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                id = BinaryPrimitives.ReverseEndianness( id );
                messageTimeoutMs = BinaryPrimitives.ReverseEndianness( messageTimeoutMs );
                pingIntervalMs = BinaryPrimitives.ReverseEndianness( pingIntervalMs );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( id );
            writer.MoveWrite( messageTimeoutMs );
            writer.Write( pingIntervalMs );
        }
    }

    internal readonly struct HandshakeRejectedResponse
    {
        [Flags]
        internal enum Reasons : byte
        {
            None = 0,
            InvalidNameLength = 1,
            NameDecodingFailure = 2,
            NameAlreadyExists = 4
        }

        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal HandshakeRejectedResponse(Reasons reasons)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.HandshakeRejectedResponse, Payload );
            Flags = ( byte )reasons;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( reverseEndianness ? BinaryPrimitives.ReverseEndianness( Header.Payload ) : Header.Payload );
            writer.Write( Flags );
        }
    }

    internal readonly struct PingResponse
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Create()
        {
            return PacketHeader.Create( MessageBrokerClientEndpoint.PingResponse, Endianness.VerificationPayload );
        }
    }

    internal readonly struct LinkChannelRequestHeader
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private LinkChannelRequestHeader(byte flags)
        {
            Flags = flags;
        }

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static LinkChannelRequestHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            Assume.Equals( flags, 0 );

            return new LinkChannelRequestHeader( flags );
        }
    }

    internal readonly struct ChannelLinkedResponse
    {
        internal const int Payload = sizeof( byte ) + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int Id;

        internal ChannelLinkedResponse(MessageBrokerChannel channel, bool created)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.ChannelLinkedResponse, Payload );
            Flags = ( byte )(created ? 1 : 0);
            Id = channel.Id;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, Id = {Id}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.MoveWrite( Flags );
            writer.Write( unchecked( ( uint )Id ) );
        }
    }

    internal readonly struct LinkChannelFailureResponse
    {
        [Flags]
        internal enum Reasons : byte
        {
            None = 0,
            AlreadyLinked = 1,
            LinkingCancelled = 2
        }

        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal LinkChannelFailureResponse(Reasons reasons)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.LinkChannelFailureResponse, Payload );
            Flags = ( byte )reasons;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Flags );
        }
    }

    internal readonly struct UnlinkChannelRequest
    {
        internal const int Length = sizeof( uint );
        internal readonly int ChannelId;

        private UnlinkChannelRequest(int channelId)
        {
            ChannelId = channelId;
        }

        [Pure]
        public override string ToString()
        {
            return $"ChannelId = {ChannelId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnlinkChannelRequest Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var channelId = unchecked( ( int )reader.ReadInt32() );
            return new UnlinkChannelRequest( channelId );
        }
    }

    internal readonly struct ChannelUnlinkedResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal ChannelUnlinkedResponse(bool channelRemoved)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.ChannelUnlinkedResponse, Payload );
            Flags = ( byte )(channelRemoved ? 1 : 0);
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Flags );
        }
    }

    internal readonly struct UnlinkChannelFailureResponse
    {
        [Flags]
        internal enum Reasons : byte
        {
            None = 0,
            ClientNotLinked = 1
        }

        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal UnlinkChannelFailureResponse(Reasons reasons)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.UnlinkChannelFailureResponse, Payload );
            Flags = ( byte )reasons;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Flags );
        }
    }

    internal readonly struct SubscribeRequestHeader
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private SubscribeRequestHeader(byte flags)
        {
            Flags = flags;
        }

        internal bool CreateChannelIfNotExists => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static SubscribeRequestHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new SubscribeRequestHeader( flags );
        }
    }

    internal readonly struct SubscribedResponse
    {
        internal const int Payload = sizeof( byte ) + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int ChannelId;

        internal SubscribedResponse(bool channelCreated, int channelId)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.SubscribedResponse, Payload );
            Flags = ( byte )(channelCreated ? 1 : 0);
            ChannelId = channelId;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelId = {ChannelId}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.MoveWrite( Flags );
            writer.Write( unchecked( ( uint )ChannelId ) );
        }
    }

    internal readonly struct SubscribeFailureResponse
    {
        [Flags]
        internal enum Reasons : byte
        {
            None = 0,
            ChannelDoesNotExist = 1,
            AlreadySubscribed = 2,
            SubscribingCancelled = 4
        }

        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal SubscribeFailureResponse(Reasons reasons)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.SubscribeFailureResponse, Payload );
            Flags = ( byte )reasons;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Flags );
        }
    }

    internal readonly struct UnsubscribeRequest
    {
        internal const int Length = sizeof( uint );
        internal readonly int ChannelId;

        private UnsubscribeRequest(int channelId)
        {
            ChannelId = channelId;
        }

        [Pure]
        public override string ToString()
        {
            return $"ChannelId = {ChannelId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnsubscribeRequest Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var channelId = unchecked( ( int )reader.ReadInt32() );
            return new UnsubscribeRequest( channelId );
        }
    }

    internal readonly struct UnsubscribedResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal UnsubscribedResponse(bool channelRemoved)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.UnsubscribedResponse, Payload );
            Flags = ( byte )(channelRemoved ? 1 : 0);
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Flags );
        }
    }

    internal readonly struct UnsubscribeFailureResponse
    {
        [Flags]
        internal enum Reasons : byte
        {
            None = 0,
            ClientNotSubscribed = 1
        }

        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal UnsubscribeFailureResponse(Reasons reasons)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.UnsubscribeFailureResponse, Payload );
            Flags = ( byte )reasons;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Flags );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException ProtocolException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerServerProtocolException( client, header.GetServerEndpoint(), header.Payload, errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException UnexpectedServerEndpointException(
        MessageBrokerRemoteClient client,
        PacketHeader header)
    {
        return ProtocolException( client, header, Chain.Create( Resources.UnexpectedServerEndpoint ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException EndiannessPayloadException(MessageBrokerRemoteClient client, PacketHeader header)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidEndiannessPayload( header.Payload ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException InvalidPacketLengthException(MessageBrokerRemoteClient client, PacketHeader header)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidPacketLength ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException InvalidNameLengthException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException? AssertPayload(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        uint expected)
    {
        return header.Payload != expected
            ? ProtocolException( client, header, Chain.Create( Resources.InvalidHeaderPayload( expected ) ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Result<int> AssertPacketLength(MessageBrokerRemoteClient client, PacketHeader header)
    {
        var result = unchecked( ( int )header.Payload );
        if ( result >= 0 )
            return result;

        return ProtocolException( client, header, Chain.Create( Resources.UnexpectedPacketLength( result ) ) );
    }
}
