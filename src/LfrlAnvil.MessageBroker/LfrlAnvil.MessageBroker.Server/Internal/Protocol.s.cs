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
        internal MessageBrokerClientEndpoint GetClientEndpoint()
        {
            return ( MessageBrokerClientEndpoint )EndpointCode;
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
        internal bool SynchronizeExternalObjectNames => (Flags & 4) != 0;

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

    internal readonly struct Pong
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PacketHeader Create()
        {
            return PacketHeader.Create( MessageBrokerClientEndpoint.Pong, Endianness.VerificationPayload );
        }
    }

    internal readonly struct BindPublisherRequestHeader
    {
        internal const int Length = sizeof( byte ) + sizeof( ushort );
        internal readonly byte Flags;
        internal readonly int ChannelNameLength;

        private BindPublisherRequestHeader(byte flags, int channelNameLength)
        {
            Flags = flags;
            ChannelNameLength = channelNameLength;
        }

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, ChannelNameLength = {ChannelNameLength}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static BindPublisherRequestHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            Assume.Equals( flags, 0 );
            var channelNameLength = ( int )reader.ReadInt16();

            return new BindPublisherRequestHeader( flags, channelNameLength );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(int packetLength)
        {
            var maxChannelNameLength = packetLength - Length;
            return ChannelNameLength > maxChannelNameLength
                ? Chain.Create( Resources.InvalidBinaryChannelNameLength( ChannelNameLength, maxChannelNameLength ) )
                : Chain<string>.Empty;
        }
    }

    internal readonly struct PublisherBoundResponse
    {
        internal const int Payload = sizeof( byte ) + sizeof( uint ) * 2;
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int ChannelId;
        internal readonly int StreamId;

        internal PublisherBoundResponse(bool channelCreated, bool streamCreated, int channelId, int streamId)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.PublisherBoundResponse, Payload );
            Flags = ( byte )((channelCreated ? 1 : 0) | (streamCreated ? 2 : 0));
            ChannelId = channelId;
            StreamId = streamId;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelId = {ChannelId}, StreamId = {StreamId}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( unchecked( ( uint )ChannelId ) );
            writer.Write( unchecked( ( uint )StreamId ) );
        }
    }

    internal readonly struct BindPublisherFailureResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal BindPublisherFailureResponse(BindResult result)
        {
            Assume.IsInRange( result, BindResult.AlreadyBound, BindResult.ParentDisposed );
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.BindPublisherFailureResponse, Payload );
            Flags = result == BindResult.ParentDisposed ? ( byte )BindResult.ChannelDisposed : ( byte )result;
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

    internal readonly struct UnbindPublisherRequest
    {
        internal const int Length = sizeof( uint );
        internal readonly int ChannelId;

        private UnbindPublisherRequest(int channelId)
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
        internal static UnbindPublisherRequest Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var channelId = unchecked( ( int )reader.ReadInt32() );
            return new UnbindPublisherRequest( channelId );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            return ChannelId <= 0 ? Chain.Create( Resources.ChannelIdIsNotPositive( ChannelId ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct PublisherUnboundResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal PublisherUnboundResponse(bool channelRemoved, bool streamRemoved)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.PublisherUnboundResponse, Payload );
            Flags = ( byte )((channelRemoved ? 1 : 0) | (streamRemoved ? 2 : 0));
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

    internal readonly struct UnbindPublisherFailureResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal UnbindPublisherFailureResponse(UnbindResult result)
        {
            Assume.IsInRange( result, UnbindResult.NotBound, UnbindResult.BindingDisposed );
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.UnbindPublisherFailureResponse, Payload );
            Flags = ( byte )UnbindResult.NotBound;
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

    internal readonly struct BindListenerRequestHeader
    {
        internal const int Length = sizeof( byte ) + sizeof( ushort ) * 2 + sizeof( uint ) * 4;
        internal readonly byte Flags;
        internal readonly short PrefetchHint;
        internal readonly int MaxRetries;
        internal readonly Duration RetryDelay;
        internal readonly int MaxRedeliveries;
        internal readonly Duration MinAckTimeout;
        internal readonly int ChannelNameLength;

        private BindListenerRequestHeader(
            byte flags,
            short prefetchHint,
            int maxRetries,
            Duration retryDelay,
            int maxRedeliveries,
            Duration minAckTimeout,
            int channelNameLength)
        {
            Flags = flags;
            PrefetchHint = prefetchHint;
            MaxRetries = maxRetries;
            RetryDelay = retryDelay;
            MaxRedeliveries = maxRedeliveries;
            MinAckTimeout = minAckTimeout;
            ChannelNameLength = channelNameLength;
        }

        internal bool CreateChannelIfNotExists => (Flags & 1) != 0;

        [Pure]
        public override string ToString()
        {
            return
                $"Flags = {Flags}, PrefetchHint = {PrefetchHint}, MaxRetries = {MaxRetries}, RetryDelay = {RetryDelay}, MaxRedeliveries = {MaxRedeliveries}, MinAckTimeout = {MinAckTimeout}, ChannelNameLength = {ChannelNameLength}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static BindListenerRequestHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var prefetchHint = unchecked( ( short )reader.MoveReadInt16() );
            var maxRetries = unchecked( ( int )reader.MoveReadInt32() );
            var retryDelay = Duration.FromMilliseconds( unchecked( ( int )reader.MoveReadInt32() ) );
            var maxRedeliveries = unchecked( ( int )reader.MoveReadInt32() );
            var minAckTimeout = Duration.FromMilliseconds( unchecked( ( int )reader.MoveReadInt32() ) );
            var channelNameLength = ( int )reader.ReadInt16();
            return new BindListenerRequestHeader(
                flags,
                prefetchHint,
                maxRetries,
                retryDelay,
                maxRedeliveries,
                minAckTimeout,
                channelNameLength );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors(int packetLength)
        {
            var errors = Chain<string>.Empty;
            var maxChannelNameLength = packetLength - Length;

            if ( ChannelNameLength > maxChannelNameLength )
                errors = errors.Extend( Resources.InvalidBinaryChannelNameLength( ChannelNameLength, maxChannelNameLength ) );

            if ( PrefetchHint < 1 )
                errors = errors.Extend( Resources.InvalidPrefetchHint( PrefetchHint ) );

            if ( MaxRetries < 0 )
                errors = errors.Extend( Resources.MaxRetriesIsNegative( MaxRetries ) );

            if ( RetryDelay < Duration.Zero )
                errors = errors.Extend( Resources.RetryDelayIsNegative( RetryDelay ) );
            else if ( RetryDelay > Duration.Zero && MaxRetries == 0 )
                errors = errors.Extend( Resources.DisabledRetryDelayIsNotZero( RetryDelay ) );

            if ( MaxRedeliveries < 0 )
                errors = errors.Extend( Resources.MaxRedeliveriesIsNegative( MaxRedeliveries ) );

            if ( MinAckTimeout < Duration.Zero )
                errors = errors.Extend( Resources.MinAckTimeoutIsNegative( MinAckTimeout ) );
            else if ( MinAckTimeout == Duration.Zero && (MaxRetries > 0 || MaxRedeliveries > 0) )
                errors = errors.Extend( Resources.EnabledMinAckTimeoutIsNotPositive( MinAckTimeout ) );

            return errors;
        }
    }

    internal readonly struct ListenerBoundResponse
    {
        internal const int Payload = sizeof( byte ) + sizeof( uint ) * 2;
        internal readonly PacketHeader Header;
        internal readonly byte Flags;
        internal readonly int ChannelId;
        internal readonly int QueueId;

        internal ListenerBoundResponse(bool channelCreated, bool queueCreated, int channelId, int queueId)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.ListenerBoundResponse, Payload );
            Flags = ( byte )((channelCreated ? 1 : 0) | (queueCreated ? 2 : 0));
            ChannelId = channelId;
            QueueId = queueId;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Flags = {Flags}, ChannelId = {ChannelId}, QueueId = {QueueId}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.MoveWrite( Flags );
            writer.MoveWrite( unchecked( ( uint )ChannelId ) );
            writer.Write( unchecked( ( uint )QueueId ) );
        }
    }

    internal readonly struct BindListenerFailureResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal BindListenerFailureResponse(BindResult result)
        {
            Assume.IsInRange( result, BindResult.AlreadyBound, BindResult.ChannelDoesNotExist );
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.BindListenerFailureResponse, Payload );
            Flags = result == BindResult.ParentDisposed ? ( byte )BindResult.ChannelDisposed : ( byte )result;
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

    internal readonly struct UnbindListenerRequest
    {
        internal const int Length = sizeof( uint );
        internal readonly int ChannelId;

        private UnbindListenerRequest(int channelId)
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
        internal static UnbindListenerRequest Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var channelId = unchecked( ( int )reader.ReadInt32() );
            return new UnbindListenerRequest( channelId );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            return ChannelId <= 0 ? Chain.Create( Resources.ChannelIdIsNotPositive( ChannelId ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct ListenerUnboundResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal ListenerUnboundResponse(bool channelRemoved, bool queueRemoved)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.ListenerUnboundResponse, Payload );
            Flags = ( byte )((channelRemoved ? 1 : 0) | (queueRemoved ? 2 : 0));
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

    internal readonly struct UnbindListenerFailureResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal UnbindListenerFailureResponse(UnbindResult result)
        {
            Assume.IsInRange( result, UnbindResult.NotBound, UnbindResult.BindingDisposed );
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.UnbindListenerFailureResponse, Payload );
            Flags = ( byte )UnbindResult.NotBound;
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

    internal readonly struct PushMessageRoutingHeader
    {
        internal const int Length = sizeof( ushort );
        internal readonly short TargetCount;

        private PushMessageRoutingHeader(short targetCount)
        {
            TargetCount = targetCount;
        }

        [Pure]
        public override string ToString()
        {
            return $"TargetCount = {TargetCount}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PushMessageRoutingHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var targetCount = unchecked( ( short )reader.ReadInt16() );
            return new PushMessageRoutingHeader( targetCount );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            return TargetCount <= 0 ? Chain.Create( Resources.TargetCountIsNotPositive( TargetCount ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct PushMessageHeader
    {
        internal const int Length = sizeof( byte ) + sizeof( uint );
        internal readonly byte Flags;
        internal readonly int ChannelId;

        private PushMessageHeader(byte flags, int channelId)
        {
            Flags = flags;
            ChannelId = channelId;
        }

        internal bool Confirm => (Flags & 1) != 0;
        internal bool ClearBuffer => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return $"Flags = {Flags}, ChannelId = {ChannelId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PushMessageHeader Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var channelId = unchecked( ( int )reader.ReadInt32() );
            return new PushMessageHeader( flags, channelId );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            return ChannelId <= 0 ? Chain.Create( Resources.ChannelIdIsNotPositive( ChannelId ) ) : Chain<string>.Empty;
        }
    }

    internal readonly struct MessageAcceptedResponse
    {
        internal const int Payload = sizeof( ulong );
        internal readonly PacketHeader Header;
        internal readonly ulong Id;

        internal MessageAcceptedResponse(ulong id)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.MessageAcceptedResponse, Payload );
            Id = id;
        }

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Id = {Id}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.Write( Id );
        }
    }

    internal readonly struct MessageRejectedResponse
    {
        internal const int Payload = sizeof( byte );
        internal readonly PacketHeader Header;
        internal readonly byte Flags;

        internal MessageRejectedResponse(PushMessageResult result)
        {
            Assume.IsInRange( result, PushMessageResult.NotBound, PushMessageResult.BindingDisposed );
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.MessageRejectedResponse, Payload );
            Flags = result == PushMessageResult.BindingDisposed ? ( byte )PushMessageResult.StreamDisposed : ( byte )result;
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

    internal readonly struct MessageNotificationHeader
    {
        internal const int Payload = sizeof( ulong ) * 2 + sizeof( uint ) * 6;
        internal readonly PacketHeader Header;
        internal readonly int AckId;
        internal readonly int StreamId;
        internal readonly ulong MessageId;
        internal readonly Int31BoolPair Retry;
        internal readonly Int31BoolPair Redelivery;
        internal readonly int ChannelId;
        internal readonly int SenderId;
        internal readonly Timestamp PushedAt;

        internal MessageNotificationHeader(
            int ackId,
            int streamId,
            Int31BoolPair retry,
            Int31BoolPair redelivery,
            ulong messageId,
            int channelId,
            int senderId,
            Timestamp pushedAt,
            int length)
        {
            Header = PacketHeader.Create( MessageBrokerClientEndpoint.MessageNotification, unchecked( Payload + ( uint )length ) );
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
                $"[{Header}] AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = ({Retry}), Redelivery = ({Redelivery}), ChannelId = {ChannelId}, SenderId = {SenderId}, PushedAt = {PushedAt}";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, PacketHeader.Length + Payload );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.MoveWrite( unchecked( ( uint )AckId ) );
            writer.MoveWrite( unchecked( ( uint )StreamId ) );
            writer.MoveWrite( MessageId );
            writer.MoveWrite( Retry.Data );
            writer.MoveWrite( Redelivery.Data );
            writer.MoveWrite( unchecked( ( uint )ChannelId ) );
            writer.MoveWrite( unchecked( ( uint )SenderId ) );
            writer.Write( unchecked( ( ulong )PushedAt.UnixEpochTicks ) );
        }
    }

    internal readonly struct ObjectNameNotification
    {
        internal readonly PacketHeader Header;
        internal readonly MessageBrokerSystemNotificationType Type;
        internal readonly int Id;
        internal readonly EncodeableText Name;

        internal ObjectNameNotification(MessageBrokerSystemNotificationType type, int id, string name)
        {
            Assume.True( type is MessageBrokerSystemNotificationType.SenderName or MessageBrokerSystemNotificationType.StreamName );
            Type = type;
            Id = id;
            Name = TextEncoding.Prepare( name ).GetValueOrThrow();
            Header = PacketHeader.Create(
                MessageBrokerClientEndpoint.SystemNotification,
                sizeof( byte ) + sizeof( uint ) + ( uint )Name.ByteCount );
        }

        internal int Length => PacketHeader.Length + unchecked( ( int )Header.Payload );

        [Pure]
        public override string ToString()
        {
            return $"[{Header}] Type = {Type}, Id = {Id}, Name = ({Name})";
        }

        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, Length );
            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( Header.EndpointCode );
            writer.MoveWrite( Header.Payload );
            writer.MoveWrite( ( byte )Type );
            writer.MoveWrite( unchecked( ( uint )Id ) );
            Name.Encode( writer.GetSpan( Name.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct MessageNotificationAck
    {
        internal const int Length = sizeof( uint ) * 5 + sizeof( ulong );
        internal readonly int QueueId;
        internal readonly int AckId;
        internal readonly int StreamId;
        internal readonly ulong MessageId;
        internal readonly int Retry;
        internal readonly int Redelivery;

        private MessageNotificationAck(int queueId, int ackId, int streamId, ulong messageId, int retry, int redelivery)
        {
            QueueId = queueId;
            AckId = ackId;
            StreamId = streamId;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
        }

        [Pure]
        public override string ToString()
        {
            return
                $"QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MessageNotificationAck Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var queueId = unchecked( ( int )reader.MoveReadInt32() );
            var ackId = unchecked( ( int )reader.MoveReadInt32() );
            var streamId = unchecked( ( int )reader.MoveReadInt32() );
            var messageId = reader.MoveReadInt64();
            var retry = unchecked( ( int )reader.MoveReadInt32() );
            var redelivery = unchecked( ( int )reader.ReadInt32() );
            return new MessageNotificationAck( queueId, ackId, streamId, messageId, retry, redelivery );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;
            if ( QueueId <= 0 )
                result = result.Extend( Resources.QueueIdIsNotPositive( QueueId ) );

            if ( AckId <= 0 )
                result = result.Extend( Resources.AckIdIsNotPositive( AckId ) );

            if ( StreamId <= 0 )
                result = result.Extend( Resources.StreamIdIsNotPositive( StreamId ) );

            if ( Retry < 0 )
                result = result.Extend( Resources.RetryIsNegative( Retry ) );

            if ( Redelivery < 0 )
                result = result.Extend( Resources.RedeliveryIsNegative( Redelivery ) );

            return result;
        }
    }

    internal readonly struct MessageNotificationNegativeAck
    {
        internal const int Length = sizeof( byte ) + sizeof( uint ) * 6 + sizeof( ulong );
        internal readonly byte Flags;
        internal readonly int QueueId;
        internal readonly int AckId;
        internal readonly int StreamId;
        internal readonly ulong MessageId;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly Duration ExplicitDelay;

        private MessageNotificationNegativeAck(
            byte flags,
            int queueId,
            int ackId,
            int streamId,
            ulong messageId,
            int retry,
            int redelivery,
            Duration explicitDelay)
        {
            Flags = flags;
            QueueId = queueId;
            AckId = ackId;
            StreamId = streamId;
            MessageId = messageId;
            Retry = retry;
            Redelivery = redelivery;
            ExplicitDelay = explicitDelay;
        }

        internal bool NoRetry => (Flags & 1) != 0;
        internal bool HasExplicitDelay => (Flags & 2) != 0;

        [Pure]
        public override string ToString()
        {
            return
                $"Flags = {Flags}, QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}, ExplicitDelay = {ExplicitDelay}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static MessageNotificationNegativeAck Parse(ReadOnlyMemory<byte> source)
        {
            Assume.Equals( source.Length, Length );
            var reader = new BinaryContractReader( source.Span );
            var flags = reader.MoveReadInt8();
            var queueId = unchecked( ( int )reader.MoveReadInt32() );
            var ackId = unchecked( ( int )reader.MoveReadInt32() );
            var streamId = unchecked( ( int )reader.MoveReadInt32() );
            var messageId = reader.MoveReadInt64();
            var retry = unchecked( ( int )reader.MoveReadInt32() );
            var redelivery = unchecked( ( int )reader.MoveReadInt32() );
            var explicitDelay = Duration.FromMilliseconds( unchecked( ( int )reader.ReadInt32() ) );
            return new MessageNotificationNegativeAck(
                flags,
                queueId,
                ackId,
                streamId,
                messageId,
                retry,
                redelivery,
                explicitDelay );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Chain<string> StringifyErrors()
        {
            var result = Chain<string>.Empty;
            if ( QueueId <= 0 )
                result = result.Extend( Resources.QueueIdIsNotPositive( QueueId ) );

            if ( AckId <= 0 )
                result = result.Extend( Resources.AckIdIsNotPositive( AckId ) );

            if ( StreamId <= 0 )
                result = result.Extend( Resources.StreamIdIsNotPositive( StreamId ) );

            if ( Retry < 0 )
                result = result.Extend( Resources.RetryIsNegative( Retry ) );

            if ( Redelivery < 0 )
                result = result.Extend( Resources.RedeliveryIsNegative( Redelivery ) );

            if ( ExplicitDelay < Duration.Zero )
                result = result.Extend( Resources.ExplicitDelayIsNegative( ExplicitDelay ) );
            else if ( ExplicitDelay > Duration.Zero && (NoRetry || ! HasExplicitDelay) )
                result = result.Extend( Resources.DisabledExplicitDelayIsNotZero( ExplicitDelay ) );

            return result;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException ProtocolException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        Chain<string> errors)
    {
        return new MessageBrokerServerProtocolException( client, header.GetServerEndpoint(), errors );
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
    internal static MessageBrokerServerProtocolException InvalidNameLengthException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException InvalidChannelNameLengthException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidChannelNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException InvalidStreamNameLengthException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidStreamNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException InvalidQueueNameLengthException(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        int length)
    {
        return ProtocolException( client, header, Chain.Create( Resources.InvalidQueueNameLength( length ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException? AssertMinPayload(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        uint expectedMin)
    {
        return header.Payload < expectedMin
            ? ProtocolException( client, header, Chain.Create( Resources.TooShortHeaderPayload( header.Payload, expectedMin ) ) )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerServerProtocolException? AssertPayload(
        MessageBrokerRemoteClient client,
        PacketHeader header,
        uint expected)
    {
        return header.Payload != expected
            ? ProtocolException( client, header, Chain.Create( Resources.InvalidHeaderPayload( header.Payload, expected ) ) )
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
