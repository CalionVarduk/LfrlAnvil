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
using LfrlAnvil.Diagnostics;
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );
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
        internal readonly int MaxBatchPacketCount;
        internal readonly MemorySize MaxNetworkBatchPacketLength;
        internal readonly EncodeableText ClientName;

        internal HandshakeRequest(MessageBrokerClient client)
        {
            Flags = ( byte )((client.IsEphemeral ? 0 : 1)
                | (BitConverter.IsLittleEndian ? 2 : 0)
                | (client.SynchronizeExternalObjectNames ? 4 : 0)
                | (client.ClearBuffers ? 8 : 0));

            MessageTimeout = client.MessageTimeout;
            PingInterval = client.PingInterval;
            MaxBatchPacketCount = client.MaxBatchPacketCount;
            MaxNetworkBatchPacketLength = client.MaxNetworkBatchPacketLength;
            ClientName = TextEncoding.Prepare( client.Name ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.HandshakeRequest,
                sizeof( byte ) + sizeof( ushort ) + sizeof( uint ) * 3 + ( uint )ClientName.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return
                $"[{Header}] Flags = {Flags}, MessageTimeout = {MessageTimeout}, PingInterval = {PingInterval}, MaxBatchPacketCount = {MaxBatchPacketCount}, MaxNetworkBatchPacketLength = {MaxNetworkBatchPacketLength}, ClientName = ({ClientName})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );
            var payload = Header.Payload;
            var messageTimeoutMs = unchecked( ( uint )MessageTimeout.FullMilliseconds );
            var pingIntervalMs = unchecked( ( uint )PingInterval.FullMilliseconds );
            var maxBatchPacketCount = unchecked( ( ushort )MaxBatchPacketCount );
            var maxNetworkBatchPacketLength = unchecked( ( uint )MaxNetworkBatchPacketLength.Bytes );

            if ( BitConverter.IsLittleEndian )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                messageTimeoutMs = BinaryPrimitives.ReverseEndianness( messageTimeoutMs );
                pingIntervalMs = BinaryPrimitives.ReverseEndianness( pingIntervalMs );
                maxBatchPacketCount = BinaryPrimitives.ReverseEndianness( maxBatchPacketCount );
                maxNetworkBatchPacketLength = BinaryPrimitives.ReverseEndianness( maxNetworkBatchPacketLength );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( messageTimeoutMs );
            writer.MoveWrite( pingIntervalMs );
            writer.MoveWrite( maxBatchPacketCount );
            writer.MoveWrite( maxNetworkBatchPacketLength );
            ClientName.Encode( writer.GetSpan( ClientName.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct HandshakeAcceptedResponse
    {
        internal const int Length = sizeof( byte ) + sizeof( ushort ) + sizeof( uint ) * 6;
        internal readonly byte Flags;
        internal readonly int Id;
        internal readonly Duration MessageTimeout;
        internal readonly Duration PingInterval;
        internal readonly MemorySize MaxNetworkPacketLength;
        internal readonly MemorySize MaxNetworkMessagePacketLength;
        internal readonly short MaxBatchPacketCount;
        internal readonly MemorySize MaxNetworkBatchPacketLength;

        private HandshakeAcceptedResponse(
            byte flags,
            int id,
            Duration messageTimeout,
            Duration pingInterval,
            MemorySize maxNetworkPacketLength,
            MemorySize maxNetworkMessagePacketLength,
            short maxBatchPacketCount,
            MemorySize maxNetworkBatchPacketLength)
        {
            Flags = flags;
            Id = id;
            MessageTimeout = messageTimeout;
            PingInterval = pingInterval;
            MaxNetworkPacketLength = maxNetworkPacketLength;
            MaxNetworkMessagePacketLength = maxNetworkMessagePacketLength;
            MaxBatchPacketCount = maxBatchPacketCount;
            MaxNetworkBatchPacketLength = maxNetworkBatchPacketLength;
        }

        internal bool IsServerLittleEndian => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return
                $"Flags = {Flags}, Id = {Id}, MessageTimeout = {MessageTimeout}, PingInterval = {PingInterval}, MaxNetworkPacketLength = {MaxNetworkPacketLength}, MaxNetworkMessagePacketLength = {MaxNetworkMessagePacketLength}, MaxBatchPacketCount = {MaxBatchPacketCount}, MaxNetworkBatchPacketLength = {MaxNetworkBatchPacketLength}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandshakeAcceptedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var id = unchecked( ( int )reader.MoveReadInt32() );
            var messageTimeoutMs = unchecked( ( int )reader.MoveReadInt32() );
            var pingIntervalMs = unchecked( ( int )reader.MoveReadInt32() );
            var maxNetworkPacketLength = unchecked( ( int )reader.MoveReadInt32() );
            var maxNetworkMessagePacketLength = unchecked( ( int )reader.MoveReadInt32() );
            var maxBatchPacketCount = unchecked( ( short )reader.MoveReadInt16() );
            var maxNetworkBatchPacketLength = unchecked( ( int )reader.ReadInt32() );

            return new HandshakeAcceptedResponse(
                flags,
                id,
                Duration.FromMilliseconds( messageTimeoutMs ),
                Duration.FromMilliseconds( pingIntervalMs ),
                MemorySize.FromBytes( maxNetworkPacketLength ),
                MemorySize.FromBytes( maxNetworkMessagePacketLength ),
                maxBatchPacketCount,
                MemorySize.FromBytes( maxNetworkBatchPacketLength ) );
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

            if ( ! Defaults.Memory.MaxNetworkPacketLengthBounds.Contains( MaxNetworkPacketLength ) )
                result = result.Extend( Resources.MaxNetworkPacketLengthIsOutOfBounds( MaxNetworkPacketLength ) );

            var largePacketLengthBounds = Defaults.Memory.GetNetworkLargePacketLengthBounds( MaxNetworkPacketLength );
            if ( ! largePacketLengthBounds.Contains( MaxNetworkMessagePacketLength ) )
                result = result.Extend(
                    Resources.MaxNetworkMessagePacketLengthIsOutOfBounds( MaxNetworkMessagePacketLength, largePacketLengthBounds ) );

            if ( MaxBatchPacketCount > 1 )
            {
                if ( ! largePacketLengthBounds.Contains( MaxNetworkBatchPacketLength ) )
                    result = result.Extend(
                        Resources.MaxNetworkBatchPacketLengthIsOutOfBounds( MaxNetworkBatchPacketLength, largePacketLengthBounds ) );
            }
            else if ( MaxBatchPacketCount == 0 )
            {
                if ( MaxNetworkBatchPacketLength != MemorySize.Zero )
                    result = result.Extend( Resources.MaxNetworkBatchPacketLengthIsNotEqualToZero( MaxNetworkBatchPacketLength ) );
            }
            else
                result = result.Extend( Resources.MaxBatchPacketCountIsInvalid( MaxBatchPacketCount ) );

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
        internal bool AlreadyConnected => (Flags & 2) != 0;
        internal bool EphemeralServer => (Flags & 4) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static HandshakeRejectedResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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

            if ( AlreadyConnected )
                result = result.Extend( Resources.ClientIsAlreadyConnected );

            if ( EphemeralServer )
                result = result.Extend( Resources.ServerIsEphemeral );

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

    internal readonly struct BatchHeader
    {
        internal const int Length = sizeof( ushort );
        internal readonly int PacketCount;

        private BatchHeader(int packetCount)
        {
            PacketCount = packetCount;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void Serialize(Memory<byte> target, uint payload, short packetCount, bool reverseEndianness)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, PacketHeader.Length + Length );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                packetCount = BinaryPrimitives.ReverseEndianness( packetCount );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( ( byte )MessageBrokerServerEndpoint.Batch );
            writer.MoveWrite( payload );
            writer.Write( unchecked( ( ushort )packetCount ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static BatchHeader Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var packetCount = reader.ReadInt16();
            if ( reverseEndianness )
                packetCount = BinaryPrimitives.ReverseEndianness( packetCount );

            return new BatchHeader( unchecked( ( short )packetCount ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(int max)
        {
            return PacketCount <= 1 || PacketCount > max
                ? Chain.Create( Resources.BatchPacketCountIsInvalid( PacketCount, max ) )
                : Chain<string>.Empty;
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
        internal readonly int DeadLetterCapacityHint;
        internal readonly Duration MinDeadLetterRetention;
        internal readonly EncodeableText ChannelName;
        internal readonly EncodeableText QueueName;
        internal readonly EncodeableText FilterExpression;

        internal BindListenerRequest(
            string channelName,
            string? queueName,
            short prefetchHint,
            int maxRetries,
            Duration retryDelay,
            int maxRedeliveries,
            Duration minAckTimeout,
            int deadLetterCapacityHint,
            Duration minDeadLetterRetention,
            string? filterExpression,
            bool createChannelIfNotExists)
        {
            Assume.IsGreaterThan( prefetchHint, 0 );
            Assume.IsGreaterThanOrEqualTo( maxRetries, 0 );
            Assume.IsGreaterThanOrEqualTo( maxRedeliveries, 0 );
            Assume.IsGreaterThanOrEqualTo( retryDelay, Duration.Zero );
            Assume.IsGreaterThanOrEqualTo( minAckTimeout, Duration.Zero );
            Assume.IsGreaterThanOrEqualTo( deadLetterCapacityHint, 0 );
            Assume.IsGreaterThanOrEqualTo( minDeadLetterRetention, Duration.Zero );
            Flags = ( byte )(createChannelIfNotExists ? 1 : 0);
            PrefetchHint = prefetchHint;
            MaxRetries = maxRetries;
            RetryDelay = retryDelay;
            MaxRedeliveries = maxRedeliveries;
            MinAckTimeout = minAckTimeout;
            DeadLetterCapacityHint = deadLetterCapacityHint;
            MinDeadLetterRetention = minDeadLetterRetention;
            ChannelName = TextEncoding.Prepare( channelName ).GetValueOrThrow();
            QueueName = TextEncoding.Prepare( queueName ?? string.Empty ).GetValueOrThrow();
            FilterExpression = TextEncoding.Prepare( filterExpression ?? string.Empty ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerServerEndpoint.BindListenerRequest,
                sizeof( byte )
                + sizeof( ushort ) * 3
                + sizeof( uint ) * 5
                + sizeof( ulong )
                + ( uint )ChannelName.ByteCount
                + ( uint )QueueName.ByteCount
                + ( uint )FilterExpression.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return
                $"[{Header}] Flags = {Flags}, PrefetchHint = {PrefetchHint}, MaxRetries = {MaxRetries}, RetryDelay = {RetryDelay}, MaxRedeliveries = {MaxRedeliveries}, MinAckTimeout = {MinAckTimeout}, DeadLetterCapacityHint = {DeadLetterCapacityHint}, MinDeadLetterRetention = {MinDeadLetterRetention}, ChannelName = ({ChannelName}), QueueName = ({QueueName}), FilterExpression = ({FilterExpression})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var payload = Header.Payload;
            var prefetchHint = unchecked( ( ushort )PrefetchHint );
            var maxRetries = unchecked( ( uint )MaxRetries );
            var retryDelayMs = unchecked( ( uint )RetryDelay.FullMilliseconds );
            var maxRedeliveries = unchecked( ( uint )MaxRedeliveries );
            var minAckTimeoutMs = unchecked( ( uint )MinAckTimeout.FullMilliseconds );
            var deadLetterCapacityHint = unchecked( ( uint )DeadLetterCapacityHint );
            var minDeadLetterRetentionMs = unchecked( ( ulong )MinDeadLetterRetention.FullMilliseconds );
            var channelNameLength = unchecked( ( ushort )ChannelName.ByteCount );
            var queueNameLength = unchecked( ( ushort )QueueName.ByteCount );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                prefetchHint = BinaryPrimitives.ReverseEndianness( prefetchHint );
                maxRetries = BinaryPrimitives.ReverseEndianness( maxRetries );
                retryDelayMs = BinaryPrimitives.ReverseEndianness( retryDelayMs );
                maxRedeliveries = BinaryPrimitives.ReverseEndianness( maxRedeliveries );
                minAckTimeoutMs = BinaryPrimitives.ReverseEndianness( minAckTimeoutMs );
                deadLetterCapacityHint = BinaryPrimitives.ReverseEndianness( deadLetterCapacityHint );
                minDeadLetterRetentionMs = BinaryPrimitives.ReverseEndianness( minDeadLetterRetentionMs );
                channelNameLength = BinaryPrimitives.ReverseEndianness( channelNameLength );
                queueNameLength = BinaryPrimitives.ReverseEndianness( queueNameLength );
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
            writer.MoveWrite( deadLetterCapacityHint );
            writer.MoveWrite( minDeadLetterRetentionMs );
            writer.MoveWrite( channelNameLength );
            writer.MoveWrite( queueNameLength );
            ChannelName.Encode( writer.GetSpan( ChannelName.ByteCount ) ).ThrowIfError();
            writer.Move( ChannelName.ByteCount );
            QueueName.Encode( writer.GetSpan( QueueName.ByteCount ) ).ThrowIfError();
            writer.Move( QueueName.ByteCount );
            FilterExpression.Encode( writer.GetSpan( FilterExpression.ByteCount ) ).ThrowIfError();
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
        internal bool UnexpectedFilterExpression => (Flags & 8) != 0;
        internal bool InvalidFilterExpression => (Flags & 16) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static BindListenerFailureResponse Parse(ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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

            if ( UnexpectedFilterExpression )
                result = result.Extend( Resources.UnexpectedFilterExpression );

            if ( InvalidFilterExpression )
                result = result.Extend( Resources.FilterExpressionIsNotValid );

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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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

        internal PushMessageHeader(int channelId, int messageLength, bool confirm)
        {
            Assume.IsInRange( messageLength, 0, int.MaxValue - Length );
            Flags = ( byte )(confirm ? 1 : 0);
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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

    internal readonly struct DeadLetterQuery
    {
        internal const int Length = PacketHeader.Length + sizeof( uint ) * 2;
        internal readonly PacketHeader Header;
        internal readonly int QueueId;
        internal readonly int ReadCount;

        internal DeadLetterQuery(int queueId, int readCount)
        {
            QueueId = queueId;
            ReadCount = readCount;
            Header = PacketHeader.Create( MessageBrokerServerEndpoint.DeadLetterQuery, sizeof( uint ) * 2 );
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] QueueId = {QueueId}, ReadCount = {ReadCount}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, bool reverseEndianness)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var payload = Header.Payload;
            var queueId = unchecked( ( uint )QueueId );
            var readCount = unchecked( ( uint )ReadCount );
            if ( reverseEndianness )
            {
                payload = BinaryPrimitives.ReverseEndianness( payload );
                queueId = BinaryPrimitives.ReverseEndianness( queueId );
                readCount = BinaryPrimitives.ReverseEndianness( readCount );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( payload );
            writer.MoveWrite( queueId );
            writer.Write( readCount );
        }
    }

    internal readonly struct DeadLetterQueryResponse
    {
        internal const int Length = sizeof( uint ) * 2 + sizeof( ulong );
        internal readonly int TotalCount;
        internal readonly int MaxReadCount;
        internal readonly Timestamp NextExpirationAt;

        private DeadLetterQueryResponse(int totalCount, int maxReadCount, Timestamp nextExpirationAt)
        {
            TotalCount = totalCount;
            MaxReadCount = maxReadCount;
            NextExpirationAt = nextExpirationAt;
        }

        internal bool QueueExists => TotalCount >= 0;

        [Pure]
        public override string ToString()
        {
            return $"TotalCount = {TotalCount}, MaxReadCount = {MaxReadCount}, NextExpirationAt = {NextExpirationAt}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static DeadLetterQueryResponse Parse(ReadOnlyMemory<byte> source, bool reverseEndianness)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var totalCount = unchecked( ( int )reader.MoveReadInt32() );
            var maxReadCount = unchecked( ( int )reader.MoveReadInt32() );
            var nextExpirationAtTicks = unchecked( ( long )reader.ReadInt64() );
            if ( reverseEndianness )
            {
                totalCount = BinaryPrimitives.ReverseEndianness( totalCount );
                maxReadCount = BinaryPrimitives.ReverseEndianness( maxReadCount );
                nextExpirationAtTicks = BinaryPrimitives.ReverseEndianness( nextExpirationAtTicks );
            }

            return new DeadLetterQueryResponse( totalCount, maxReadCount, new Timestamp( nextExpirationAtTicks ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;
            if ( TotalCount < -1 )
                result = result.Extend( Resources.TotalCountIsNotValid( TotalCount ) );
            else if ( TotalCount == -1 )
            {
                if ( MaxReadCount != 0 )
                    result = result.Extend( Resources.MaxReadCountIsNotEqualToZero( MaxReadCount ) );

                if ( NextExpirationAt != Timestamp.Zero )
                    result = result.Extend( Resources.NextExpirationAtIsNotEqualToZero( NextExpirationAt ) );
            }
            else
            {
                if ( MaxReadCount < 0 || MaxReadCount > TotalCount )
                    result = result.Extend( Resources.MaxReadCountIsNotValid( MaxReadCount, TotalCount ) );

                if ( TotalCount == 0 && NextExpirationAt != Timestamp.Zero )
                    result = result.Extend( Resources.NextExpirationAtIsNotEqualToZero( NextExpirationAt ) );
            }

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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

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
            var retry = Retry.IntValue;
            var isRetry = Retry.BoolValue;
            var redelivery = Redelivery.IntValue;
            var isRedelivery = Redelivery.BoolValue;

            if ( AckId <= 0 )
            {
                if ( AckId < -1 )
                    result = result.Extend( Resources.AckIdIsInvalid( AckId ) );
                else
                {
                    if ( isRetry )
                        result = result.Extend( Resources.InvalidRetry( AckId ) );

                    if ( isRedelivery )
                        result = result.Extend( Resources.InvalidRedelivery( AckId ) );
                }
            }

            if ( StreamId <= 0 )
                result = result.Extend( Resources.StreamIdIsNotPositive( StreamId ) );

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
            else if ( (retry > 0 || redelivery > 0) && AckId != -1 )
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
            else if ( AckId != 0 )
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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
            bool noDeadLetter,
            Duration? explicitDelay)
        {
            Flags = ( byte )((noRetry ? 1 : 0) | (noDeadLetter ? 2 : 0) | (explicitDelay is not null ? 4 : 0));
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
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

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
}
