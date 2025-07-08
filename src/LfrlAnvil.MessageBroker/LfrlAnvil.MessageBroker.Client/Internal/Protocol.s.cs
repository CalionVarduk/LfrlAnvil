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
            Flags = ( byte )( /*(client.IsPersistent ? 1 : 0) |*/
                (BitConverter.IsLittleEndian ? 2 : 0) | (client.SynchronizeExternalObjectNames ? 4 : 0));

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

    internal readonly struct Ping
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Create()
        {
            return PacketHeader.Create( MessageBrokerServerEndpoint.Ping, Endianness.VerificationPayload );
        }
    }

    internal readonly struct BindPublisherRequest
    {
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly EncodeableText ChannelName;
        internal readonly EncodeableText StreamName;

        internal BindPublisherRequest(string channelName, string? streamName)
        {
            Flags = 0;
            ChannelName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
            StreamName = TextEncoding.Prepare( streamName ?? string.Empty ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.BindPublisherRequest,
                sizeof( byte ) + sizeof( ushort ) + ( uint )ChannelName.ByteCount + ( uint )StreamName.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelName = ({ChannelName}), StreamName = ({StreamName})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var channelNameLength = unchecked( ( ushort )ChannelName.ByteCount );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                channelNameLength = BinaryPrimitives.ReverseEndianness( channelNameLength );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( channelNameLength );
            ChannelName.Encode( writer.GetSpan( ChannelName.ByteCount ) ).ThrowIfError();
            writer.Move( ChannelName.ByteCount );
            StreamName.Encode( writer.GetSpan( StreamName.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct PublisherBoundResponse
    {
        internal const int Length = sizeof( byte ) + sizeof( uint ) * 2;
        internal readonly byte Flags;
        internal readonly int ChannelId;
        internal readonly int StreamId;

        private PublisherBoundResponse(byte flags, int channelId, int streamId)
        {
            Flags = flags;
            ChannelId = channelId;
            StreamId = streamId;
        }

        internal bool ChannelCreated => (Flags & 1) != 0;
        internal bool StreamCreated => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, ChannelId = {ChannelId}, StreamId = {StreamId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PublisherBoundResponse Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var channelId = unchecked( ( int )reader.MoveReadInt32() );
            var streamId = unchecked( ( int )reader.ReadInt32() );
            if ( reverseEndianness )
            {
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
            }

            return new PublisherBoundResponse( flags, channelId, streamId );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;
            if ( ChannelId <= 0 )
                result = result.Extend( Resources.ChannelIdIsNotPositive( ChannelId ) );

            if ( StreamId <= 0 )
                result = result.Extend( Resources.StreamIdIsNotPositive( StreamId ) );

            return result;
        }
    }

    internal readonly struct BindPublisherFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private BindPublisherFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool AlreadyBound => (Flags & 1) != 0;
        internal bool Cancelled => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static BindPublisherFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new BindPublisherFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(string channelName)
        {
            var result = Chain<string>.Empty;

            if ( AlreadyBound )
                result = result.Extend( Resources.PublisherAlreadyBound( channelName ) );

            if ( Cancelled )
                result = result.Extend( Resources.BindPublisherCancelled( channelName ) );

            return result;
        }
    }

    internal readonly struct UnbindPublisherRequest
    {
        internal const int Length = PacketHeader.Length + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly int ChannelId;

        internal UnbindPublisherRequest(int channelId)
        {
            ChannelId = channelId;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.UnbindPublisherRequest, sizeof( uint ) );
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

    internal readonly struct PublisherUnboundResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private PublisherUnboundResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ChannelRemoved => (Flags & 1) != 0;
        internal bool StreamRemoved => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PublisherUnboundResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new PublisherUnboundResponse( flags );
        }
    }

    internal readonly struct UnbindPublisherFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private UnbindPublisherFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool NotBound => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnbindPublisherFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new UnbindPublisherFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(MessageBrokerPublisher channel)
        {
            return NotBound ? Chain.Create( Resources.PublisherNotBound( channel.ChannelId, channel.ChannelName ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct BindListenerRequest
    {
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly short PrefetchHint;
        internal readonly int MaxRetries;
        internal readonly Duration RetryDelay;
        internal readonly int MaxRedeliveries;
        internal readonly Duration MinAckTimeout;
        internal readonly EncodeableText ChannelName;
        internal readonly EncodeableText QueueName;

        internal BindListenerRequest(
            string channelName,
            string? queueName,
            short prefetchHint,
            int maxRetries,
            Duration retryDelay,
            int maxRedeliveries,
            Duration minAckTimeout,
            bool createChannelIfNotExists)
        {
            Assume.IsGreaterThan( prefetchHint, 0 );
            Assume.IsGreaterThanOrEqualTo( maxRetries, 0 );
            Assume.IsGreaterThanOrEqualTo( maxRedeliveries, 0 );
            Assume.IsGreaterThanOrEqualTo( retryDelay, Duration.Zero );
            Assume.IsGreaterThanOrEqualTo( minAckTimeout, Duration.Zero );
            Flags = ( byte )(createChannelIfNotExists ? 1 : 0);
            PrefetchHint = prefetchHint;
            MaxRetries = maxRetries;
            RetryDelay = retryDelay;
            MaxRedeliveries = maxRedeliveries;
            MinAckTimeout = minAckTimeout;
            ChannelName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
            QueueName = TextEncoding.Prepare( queueName ?? string.Empty ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.BindListenerRequest,
                sizeof( byte ) + sizeof( ushort ) * 2 + sizeof( uint ) * 4 + ( uint )ChannelName.ByteCount + ( uint )QueueName.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return
                $"[{Header}] Flags = {Flags}, PrefetchHint = {PrefetchHint}, MaxRetries = {MaxRetries}, RetryDelay = {RetryDelay}, MaxRedeliveries = {MaxRedeliveries}, MinAckTimeout = {MinAckTimeout}, ChannelName = ({ChannelName}), QueueName = ({QueueName})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var prefetchHint = unchecked( ( ushort )PrefetchHint );
            var maxRetries = unchecked( ( uint )MaxRetries );
            var retryDelayMs = unchecked( ( uint )RetryDelay.FullMilliseconds );
            var maxRedeliveries = unchecked( ( uint )MaxRedeliveries );
            var minAckTimeoutMs = unchecked( ( uint )MinAckTimeout.FullMilliseconds );
            var channelNameLength = unchecked( ( ushort )ChannelName.ByteCount );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                prefetchHint = BinaryPrimitives.ReverseEndianness( prefetchHint );
                maxRetries = BinaryPrimitives.ReverseEndianness( maxRetries );
                retryDelayMs = BinaryPrimitives.ReverseEndianness( retryDelayMs );
                maxRedeliveries = BinaryPrimitives.ReverseEndianness( maxRedeliveries );
                minAckTimeoutMs = BinaryPrimitives.ReverseEndianness( minAckTimeoutMs );
                channelNameLength = BinaryPrimitives.ReverseEndianness( channelNameLength );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( prefetchHint );
            writer.MoveWrite( maxRetries );
            writer.MoveWrite( retryDelayMs );
            writer.MoveWrite( maxRedeliveries );
            writer.MoveWrite( minAckTimeoutMs );
            writer.MoveWrite( channelNameLength );
            ChannelName.Encode( writer.GetSpan( ChannelName.ByteCount ) ).ThrowIfError();
            writer.Move( ChannelName.ByteCount );
            QueueName.Encode( writer.GetSpan( QueueName.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct ListenerBoundResponse
    {
        internal const int Length = sizeof( byte ) + sizeof( uint ) * 2;
        internal readonly byte Flags;
        internal readonly int ChannelId;
        internal readonly int QueueId;

        private ListenerBoundResponse(byte flags, int channelId, int queueId)
        {
            Flags = flags;
            ChannelId = channelId;
            QueueId = queueId;
        }

        internal bool ChannelCreated => (Flags & 1) != 0;
        internal bool QueueCreated => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, ChannelId = {ChannelId}, QueueId = {QueueId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ListenerBoundResponse Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var channelId = unchecked( ( int )reader.MoveReadInt32() );
            var queueId = unchecked( ( int )reader.ReadInt32() );

            if ( reverseEndianness )
            {
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
                queueId = BinaryPrimitives.ReverseEndianness( queueId );
            }

            return new ListenerBoundResponse( flags, channelId, queueId );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;

            if ( ChannelId <= 0 )
                result = result.Extend( Resources.ChannelIdIsNotPositive( ChannelId ) );

            if ( QueueId <= 0 )
                result = result.Extend( Resources.QueueIdIsNotPositive( QueueId ) );

            return result;
        }
    }

    internal readonly struct BindListenerFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private BindListenerFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool AlreadyBound => (Flags & 1) != 0;
        internal bool Cancelled => (Flags & 2) != 0;
        internal bool ChannelDoesNotExist => (Flags & 4) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static BindListenerFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new BindListenerFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(string channelName)
        {
            var result = Chain<string>.Empty;

            if ( ChannelDoesNotExist )
                result = result.Extend( Resources.ChannelDoesNotExist( channelName ) );

            if ( AlreadyBound )
                result = result.Extend( Resources.ListenerAlreadyBound( channelName ) );

            if ( Cancelled )
                result = result.Extend( Resources.BindListenerCancelled( channelName ) );

            return result;
        }
    }

    internal readonly struct UnbindListenerRequest
    {
        internal const int Length = PacketHeader.Length + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly int ChannelId;

        internal UnbindListenerRequest(int channelId)
        {
            ChannelId = channelId;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.UnbindListenerRequest, sizeof( uint ) );
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

    internal readonly struct ListenerUnboundResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private ListenerUnboundResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool ChannelRemoved => (Flags & 1) != 0;
        internal bool QueueRemoved => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ListenerUnboundResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new ListenerUnboundResponse( flags );
        }
    }

    internal readonly struct UnbindListenerFailureResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private UnbindListenerFailureResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool NotBound => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static UnbindListenerFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new UnbindListenerFailureResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(MessageBrokerListener listener)
        {
            return NotBound
                ? Chain.Create( Resources.ListenerNotBound( listener.ChannelId, listener.ChannelName ) )
                : Chain<string>.Empty;
        }
    }

    internal readonly struct PushMessageRoutingHeader
    {
        internal const int Length = PacketHeader.Length + sizeof( ushort );
        internal readonly PacketHeader Header;
        internal readonly short TargetCount;

        internal PushMessageRoutingHeader(short targetCount, int length)
        {
            Assume.IsGreaterThan( targetCount, 0 );
            Assume.IsGreaterThan( length, 0 );
            TargetCount = targetCount;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.PushMessageRouting, unchecked( sizeof( ushort ) + ( uint )length ) );
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] TargetCount = {TargetCount}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var targetCount = unchecked( ( ushort )TargetCount );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                targetCount = BinaryPrimitives.ReverseEndianness( targetCount );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.Write( targetCount );
        }
    }

    internal readonly struct PushMessageHeader
    {
        internal const int Length = PacketHeader.Length + sizeof( byte ) + sizeof( uint );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int ChannelId;

        internal PushMessageHeader(int channelId, int messageLength, bool confirm, bool clearOnDispose)
        {
            Assume.IsInRange( messageLength, 0, int.MaxValue - Length );
            Flags = ( byte )((confirm ? 1 : 0) | (clearOnDispose ? 2 : 0));
            ChannelId = channelId;
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.PushMessage,
                unchecked( sizeof( byte ) + sizeof( uint ) + ( uint )messageLength ) );
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelId = {ChannelId}";
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
            writer.MoveWrite( Flags );
            writer.Write( channelId );
        }
    }

    internal readonly struct MessageAcceptedResponse
    {
        internal const int Length = sizeof( ulong );
        internal readonly ulong Id;

        private MessageAcceptedResponse(ulong id)
        {
            Id = id;
        }

        [Pure]
        public override string ToString()
        {
            return $"Id = {Id}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MessageAcceptedResponse Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var id = reader.ReadInt64();
            if ( reverseEndianness )
                id = BinaryPrimitives.ReverseEndianness( id );

            return new MessageAcceptedResponse( id );
        }
    }

    internal readonly struct MessageRejectedResponse
    {
        internal const int Length = sizeof( byte );
        internal readonly byte Flags;

        private MessageRejectedResponse(byte flags)
        {
            Flags = flags;
        }

        internal bool NotBound => (Flags & 1) != 0;
        internal bool Cancelled => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MessageRejectedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.ReadInt8();
            return new MessageRejectedResponse( flags );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(MessageBrokerPublisher publisher)
        {
            var result = Chain<string>.Empty;

            if ( NotBound )
                result = result.Extend( Resources.PublisherNotBound( publisher.ChannelId, publisher.ChannelName ) );

            if ( Cancelled )
                result = result.Extend( Resources.MessageCancelled( publisher.StreamId, publisher.StreamName ) );

            return result;
        }
    }

    internal readonly struct MessageNotificationHeader
    {
        internal const int Length = sizeof( ulong ) * 2 + sizeof( uint ) * 6;
        internal readonly int AckId;
        internal readonly int StreamId;
        internal readonly ulong MessageId;
        internal readonly Int31BoolPair Retry;
        internal readonly Int31BoolPair Redelivery;
        internal readonly int ChannelId;
        internal readonly int SenderId;
        internal readonly Timestamp PushedAt;

        private MessageNotificationHeader(
            int ackId,
            int streamId,
            ulong messageId,
            Int31BoolPair retry,
            Int31BoolPair redelivery,
            int channelId,
            int senderId,
            Timestamp pushedAt)
        {
            AckId = ackId;
            StreamId = streamId;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            ChannelId = channelId;
            SenderId = senderId;
            PushedAt = pushedAt;
        }

        [Pure]
        public override string ToString()
        {
            return
                $"AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = ({Retry}), Redelivery = ({Redelivery}), ChannelId = {ChannelId}, SenderId = {SenderId}, PushedAt = {PushedAt}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MessageNotificationHeader Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var ackId = unchecked( ( int )reader.MoveReadInt32() );
            var streamId = unchecked( ( int )reader.MoveReadInt32() );
            var messageId = reader.MoveReadInt64();
            var retryData = reader.MoveReadInt32();
            var redeliveryData = reader.MoveReadInt32();
            var channelId = unchecked( ( int )reader.MoveReadInt32() );
            var senderId = unchecked( ( int )reader.MoveReadInt32() );
            var pushedAtTicks = unchecked( ( long )reader.ReadInt64() );

            if ( reverseEndianness )
            {
                ackId = BinaryPrimitives.ReverseEndianness( ackId );
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                messageId = BinaryPrimitives.ReverseEndianness( messageId );
                retryData = BinaryPrimitives.ReverseEndianness( retryData );
                redeliveryData = BinaryPrimitives.ReverseEndianness( redeliveryData );
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
                senderId = BinaryPrimitives.ReverseEndianness( senderId );
                pushedAtTicks = BinaryPrimitives.ReverseEndianness( pushedAtTicks );
            }

            return new MessageNotificationHeader(
                ackId,
                streamId,
                messageId,
                retryData,
                redeliveryData,
                channelId,
                senderId,
                new Timestamp( pushedAtTicks ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;
            if ( AckId < 0 )
                result = result.Extend( Resources.AckIdIsNegative( AckId ) );

            if ( StreamId <= 0 )
                result = result.Extend( Resources.StreamIdIsNotPositive( StreamId ) );

            var retry = Retry.IntValue;
            var isRetry = Retry.BoolValue;
            var redelivery = Redelivery.IntValue;
            var isRedelivery = Redelivery.BoolValue;

            if ( isRetry )
            {
                if ( isRedelivery )
                {
                    result = result.Extend( Resources.MessageCannotBeBothRetryAndRedelivery );
                    if ( retry == 0 )
                        result = result.Extend( Resources.RetryIsNotPositive( retry ) );

                    if ( redelivery == 0 )
                        result = result.Extend( Resources.RedeliveryIsNotPositive( redelivery ) );
                }
                else if ( retry == 0 )
                    result = result.Extend( Resources.RetryIsNotPositive( retry ) );
            }
            else if ( isRedelivery )
            {
                if ( redelivery == 0 )
                    result = result.Extend( Resources.RedeliveryIsNotPositive( redelivery ) );
            }
            else if ( retry > 0 || redelivery > 0 )
                result = result.Extend( Resources.MissingNonZeroMessageResendAttemptMarker( retry, redelivery ) );

            if ( ChannelId <= 0 )
                result = result.Extend( Resources.ChannelIdIsNotPositive( ChannelId ) );

            if ( SenderId <= 0 )
                result = result.Extend( Resources.SenderIdIsNotPositive( SenderId ) );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyListenerErrors(MessageBrokerListener listener)
        {
            var result = Chain<string>.Empty;
            if ( listener.AreAcksEnabled )
            {
                if ( AckId == 0 )
                    result = result.Extend( Resources.ListenerExpectsAckId );
            }
            else if ( AckId > 0 )
                result = result.Extend( Resources.ListenerDoesNotExpectAckId );

            if ( Retry.IntValue > listener.MaxRetries )
                result = result.Extend( Resources.MaxRetriesExceeded( listener, Retry.IntValue ) );

            if ( Redelivery.IntValue > listener.MaxRedeliveries )
                result = result.Extend( Resources.MaxRedeliveriesExceeded( listener, Redelivery.IntValue ) );

            return result;
        }
    }

    internal readonly struct SystemNotificationHeader
    {
        internal const int Length = sizeof( byte );
        internal readonly MessageBrokerSystemNotificationType Type;

        private SystemNotificationHeader(MessageBrokerSystemNotificationType type)
        {
            Type = type;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static SystemNotificationHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var type = reader.ReadInt8();
            return new SystemNotificationHeader( ( MessageBrokerSystemNotificationType )type );
        }
    }

    internal readonly struct ObjectNameNotificationHeader
    {
        internal const int Length = sizeof( uint );
        internal readonly int Id;

        private ObjectNameNotificationHeader(int id)
        {
            Id = id;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ObjectNameNotificationHeader Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var id = unchecked( ( int )reader.ReadInt32() );
            return new ObjectNameNotificationHeader( reverseEndianness ? BinaryPrimitives.ReverseEndianness( id ) : id );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifySenderErrors(int clientId)
        {
            var result = Chain<string>.Empty;
            if ( Id <= 0 )
                result = result.Extend( Resources.SenderIdIsNotPositive( Id ) );

            if ( Id == clientId )
                result = result.Extend( Resources.SenderIdEqualsClientId( Id ) );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyStreamErrors()
        {
            return Id <= 0 ? Chain.Create( Resources.StreamIdIsNotPositive( Id ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct MessageNotificationAck
    {
        internal const int Length = PacketHeader.Length + sizeof( uint ) * 5 + sizeof( ulong );
        internal readonly PacketHeader Header;
        internal readonly int QueueId;
        internal readonly int AckId;
        internal readonly int StreamId;
        internal readonly ulong MessageId;
        internal readonly int Retry;
        internal readonly int Redelivery;

        internal MessageNotificationAck(
            int queueId,
            int ackId,
            int streamId,
            ulong messageId,
            int retry,
            int redelivery)
        {
            QueueId = queueId;
            AckId = ackId;
            StreamId = streamId;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.MessageNotificationAck, sizeof( uint ) * 5 + sizeof( ulong ) );
        }

        [Pure]
        public override string ToString()
        {
            return
                $"[{Header}] QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var queueId = unchecked( ( uint )QueueId );
            var ackId = unchecked( ( uint )AckId );
            var streamId = unchecked( ( uint )StreamId );
            var messageId = MessageId;
            var retry = unchecked( ( uint )Retry );
            var redelivery = unchecked( ( uint )Redelivery );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                queueId = BinaryPrimitives.ReverseEndianness( queueId );
                ackId = BinaryPrimitives.ReverseEndianness( ackId );
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                messageId = BinaryPrimitives.ReverseEndianness( messageId );
                retry = BinaryPrimitives.ReverseEndianness( retry );
                redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( queueId );
            writer.MoveWrite( ackId );
            writer.MoveWrite( streamId );
            writer.MoveWrite( messageId );
            writer.MoveWrite( retry );
            writer.Write( redelivery );
        }
    }

    internal readonly struct MessageNotificationNegativeAck
    {
        internal const int Length = PacketHeader.Length + sizeof( byte ) + sizeof( uint ) * 6 + sizeof( ulong );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int QueueId;
        internal readonly int AckId;
        internal readonly int StreamId;
        internal readonly ulong MessageId;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly Duration ExplicitDelay;

        internal MessageNotificationNegativeAck(
            int queueId,
            int ackId,
            int streamId,
            ulong messageId,
            int retry,
            int redelivery,
            bool noRetry,
            Duration? explicitDelay)
        {
            Flags = ( byte )((noRetry ? 1 : 0) | (explicitDelay is not null ? 2 : 0));
            QueueId = queueId;
            AckId = ackId;
            StreamId = streamId;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            ExplicitDelay = explicitDelay ?? Duration.Zero;
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.MessageNotificationNack,
                sizeof( byte ) + sizeof( uint ) * 6 + sizeof( ulong ) );
        }

        [Pure]
        public override string ToString()
        {
            return
                $"[{Header}] Flags = {Flags}, QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}, ExplicitDelay = {ExplicitDelay}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.Equals( target.Length, Length );

            var payload = Header.Payload;
            var queueId = unchecked( ( uint )QueueId );
            var ackId = unchecked( ( uint )AckId );
            var streamId = unchecked( ( uint )StreamId );
            var messageId = MessageId;
            var retry = unchecked( ( uint )Retry );
            var redelivery = unchecked( ( uint )Redelivery );
            var explicitDelayMs = unchecked( ( uint )ExplicitDelay.FullMilliseconds );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                queueId = BinaryPrimitives.ReverseEndianness( queueId );
                ackId = BinaryPrimitives.ReverseEndianness( ackId );
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                messageId = BinaryPrimitives.ReverseEndianness( messageId );
                retry = BinaryPrimitives.ReverseEndianness( retry );
                redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
                explicitDelayMs = BinaryPrimitives.ReverseEndianness( explicitDelayMs );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( queueId );
            writer.MoveWrite( ackId );
            writer.MoveWrite( streamId );
            writer.MoveWrite( messageId );
            writer.MoveWrite( retry );
            writer.MoveWrite( redelivery );
            writer.Write( explicitDelayMs );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientRequestException RequestException(
        MessageBrokerClient client,
        PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerClientRequestException( client, header.GetServerEndpoint(), errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException ProtocolException(
        MessageBrokerClient client,
        PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerClientProtocolException( client, header.GetClientEndpoint(), errors );
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
    internal static MessageBrokerClientProtocolException InvalidSenderNameLengthException(
        MessageBrokerClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidSenderNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException InvalidStreamNameLengthException(
        MessageBrokerClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidStreamNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException? AssertMinPayload(
        MessageBrokerClient client,
        PacketHeader header,
        uint expectedMin)
    {
        return header.Payload < expectedMin
            ? ProtocolException( client, header, Chain.Create( Resources.TooShortHeaderPayload( header.Payload, expectedMin ) ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProtocolException? AssertPayload(MessageBrokerClient client, PacketHeader header, uint expected)
    {
        return header.Payload != expected
            ? ProtocolException( client, header, Chain.Create( Resources.InvalidHeaderPayload( header.Payload, expected ) ) )
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
