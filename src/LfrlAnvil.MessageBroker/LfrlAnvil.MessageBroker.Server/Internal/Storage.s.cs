// Copyright 2026 Łukasz Furlepa
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal static class Storage
{
    internal readonly record struct Context(MessageBrokerServer Server, string Path)
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Context WithSubPath(string path)
        {
            return new Context( Server, System.IO.Path.Combine( Path, path ) );
        }

        [DoesNotReturn]
        [StackTraceHidden]
        internal void Throw(Chain<string> errors)
        {
            throw Server.StorageException( Path, errors );
        }

        [DoesNotReturn]
        [StackTraceHidden]
        internal void Throw(string error)
        {
            Throw( Chain.Create( error ) );
        }

        internal bool TryIncrementMessageRefCount(int streamId, int storeKey, out StreamMessage message)
        {
            MessageBrokerStream? stream;
            using ( Server.AcquireLock() )
            {
                if ( Server.IsDisposed )
                {
                    message = default;
                    return false;
                }

                stream = Server.StreamCollection.TryGetByIdUnsafe( streamId );
            }

            if ( stream is null )
                Throw( Resources.StreamDoesNotExist( streamId ) );

            using ( stream.AcquireLock() )
            {
                if ( stream.IsDisposed )
                {
                    message = default;
                    return false;
                }

                if ( ! stream.MessageStore.TryIncrementRefCount( storeKey, out message, out var isPending ) )
                    Throw( Resources.StreamMessageDoesNotExist( stream, storeKey ) );

                if ( isPending )
                    Throw( Resources.PendingStreamMessageCannotBeReferencedByQueue( stream, storeKey ) );
            }

            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void DecrementFailedMessageRefCount(StreamMessage message, int storeKey, ulong traceId)
        {
            var stream = message.Publisher.Stream;
            if ( Server.Logger.Error is { } error )
                error.Emit(
                    MessageBrokerServerErrorEvent.Create(
                        Server,
                        traceId,
                        Server.StorageException(
                            Path,
                            Chain.Create( Resources.ListenerDoesNotExist( message.Publisher.Channel, stream, storeKey ) ) ) ) );

            using ( stream.AcquireLock() )
                stream.MessageStore.DecrementRefCount( storeKey );
        }

        [Pure]
        internal int ParseId(int prefixLength)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension( Path.AsSpan() );
            if ( fileName.Length > prefixLength
                && int.TryParse( fileName.Slice( prefixLength ), CultureInfo.InvariantCulture, out var id ) )
                return id;

            Throw( Resources.FileNameDoesNotContainValidId );
            return default;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void AssertFileExistence()
        {
            if ( ! File.Exists( Path ) )
                Throw( Resources.MissingFile );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void AssertFileLength(int expected, long actual)
        {
            if ( expected != actual )
                Throw( Resources.InvalidFileLength( expected, actual ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void AssertFileLength(int expectedMin, int expectedMax, long actual)
        {
            if ( actual < expectedMin || actual > expectedMax )
                Throw( Resources.InvalidFileLength( expectedMin, expectedMax, actual ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void AssertFileMinLength(long expected, long actual)
        {
            if ( actual < expected )
                Throw( Resources.InvalidFileMinLength( expected, actual ) );
        }

        [DoesNotReturn]
        [StackTraceHidden]
        internal void ThrowInvalidHeader()
        {
            Throw( Resources.InvalidFileHeader );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Memory<T> GetNextChunk<T>(this Memory<T> source, int length, out Memory<T> chunk)
    {
        Assume.IsGreaterThan( length, 0 );
        if ( source.Length < length )
        {
            chunk = source;
            return Memory<T>.Empty;
        }

        chunk = source.Slice( 0, length );
        return source.Slice( length );
    }

    internal readonly struct ServerMetadata
    {
        internal const string FileName = "meta.mbsr";
        internal static ReadOnlySpan<byte> Header => "LFMBSR"u8;
        internal static int Length => Header.Length + sizeof( ulong );

        internal readonly ulong TraceId;

        internal ServerMetadata(ulong traceId)
        {
            TraceId = traceId;
        }

        [Pure]
        public override string ToString()
        {
            return $"TraceId: {TraceId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ServerMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var traceId = reader.ReadInt64();

            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            return new ServerMetadata( unchecked( traceId + 1 ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var traceId = TraceId;
            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.Write( traceId );
        }
    }

    internal readonly struct ChannelMetadata
    {
        internal const string FilePrefix = "meta";
        internal const string FileExtension = "mbch";
        internal static ReadOnlySpan<byte> Header => "LFMBCH"u8;
        internal static int MinLength => Header.Length + sizeof( ulong ) + 1;

        internal readonly ulong TraceId;
        internal readonly EncodeableText Name;

        internal ChannelMetadata(ulong traceId, EncodeableText name)
        {
            TraceId = traceId;
            Name = name;
        }

        internal int Length => Header.Length + sizeof( ulong ) + Name.ByteCount;

        [Pure]
        public override string ToString()
        {
            return $"TraceId: {TraceId}, Name: {Name}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string GetFileName(int id)
        {
            return $"{FilePrefix}{id.ToString( CultureInfo.InvariantCulture )}.{FileExtension}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ChannelMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, MinLength );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var traceId = reader.MoveReadInt64();

            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var name = TextEncoding.Prepare( reader.GetSpan( source.Length - MinLength + 1 ) ).GetValueOrThrow();
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
                context.Throw( Resources.InvalidChannelNameLength( name.Value.Length ) );

            return new ChannelMetadata( unchecked( traceId + 1 ), name );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var traceId = TraceId;
            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.MoveWrite( traceId );
            Name.Encode( writer.GetSpan( Name.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct StreamMetadata
    {
        internal const string DirPrefix = "_";
        internal const string FileName = "meta.mbst";
        internal static ReadOnlySpan<byte> Header => "LFMBST"u8;
        internal static int MinLength => Header.Length + sizeof( ulong ) + 1;

        internal readonly ulong TraceId;
        internal readonly EncodeableText Name;

        internal StreamMetadata(ulong traceId, EncodeableText name)
        {
            TraceId = traceId;
            Name = name;
        }

        internal int Length => Header.Length + sizeof( ulong ) + Name.ByteCount;

        [Pure]
        public override string ToString()
        {
            return $"TraceId: {TraceId}, Name: {Name}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string GetDirName(int id)
        {
            return $"{DirPrefix}{id.ToString( CultureInfo.InvariantCulture )}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StreamMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, MinLength );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var traceId = reader.MoveReadInt64();

            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var name = TextEncoding.Prepare( reader.GetSpan( source.Length - MinLength + 1 ) ).GetValueOrThrow();
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
                context.Throw( Resources.InvalidStreamNameLength( name.Value.Length ) );

            return new StreamMetadata( unchecked( traceId + 1 ), name );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var traceId = TraceId;
            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.MoveWrite( traceId );
            Name.Encode( writer.GetSpan( Name.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct StreamMessageRangeHeader
    {
        internal const string FileName = "messages.mbms";
        internal static int Length => StreamMetadata.Header.Length + sizeof( uint ) * 3 + sizeof( ulong );

        internal readonly ulong NextMessageId;
        internal readonly NullableIndex NextPendingNodeId;
        internal readonly int MessageCount;
        internal readonly int RoutingCount;

        internal StreamMessageRangeHeader(ulong nextMessageId, NullableIndex nextPendingNodeId, int messageCount, int routingCount)
        {
            NextMessageId = nextMessageId;
            NextPendingNodeId = nextPendingNodeId;
            MessageCount = messageCount;
            RoutingCount = routingCount;
        }

        [Pure]
        public override string ToString()
        {
            return
                $"NextMessageId: {NextMessageId}, NextPendingNodeId: {NextPendingNodeId}, MessageCount: {MessageCount}, RoutingCount: {RoutingCount}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StreamMessageRangeHeader Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( StreamMetadata.Header.Length );
            if ( ! header.SequenceEqual( StreamMetadata.Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var nextMessageId = reader.MoveReadInt64();
            var nextPendingNodeId = reader.MoveReadInt32();
            var messageCount = reader.MoveReadInt32();
            var routingCount = reader.ReadInt32();

            if ( ! BitConverter.IsLittleEndian )
            {
                nextMessageId = BinaryPrimitives.ReverseEndianness( nextMessageId );
                nextPendingNodeId = BinaryPrimitives.ReverseEndianness( nextPendingNodeId );
                messageCount = BinaryPrimitives.ReverseEndianness( messageCount );
                routingCount = BinaryPrimitives.ReverseEndianness( routingCount );
            }

            var result = new StreamMessageRangeHeader(
                nextMessageId,
                NullableIndex.Create( unchecked( ( int )nextPendingNodeId ) ),
                unchecked( ( int )messageCount ),
                unchecked( ( int )routingCount ) );

            var errors = Chain<string>.Empty;
            if ( result.MessageCount < 0 )
                errors = errors.Extend( Resources.MessageCountIsNegative( result.MessageCount ) );

            if ( result.RoutingCount < 0 )
                errors = errors.Extend( Resources.RoutingCountIsNegative( result.RoutingCount ) );

            if ( errors.Count > 0 )
                context.Throw( errors );

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var nextMessageId = NextMessageId;
            var nextPendingNodeId = unchecked( ( uint )NextPendingNodeId.Value );
            var messageCount = unchecked( ( uint )MessageCount );
            var routingCount = unchecked( ( uint )RoutingCount );
            if ( ! BitConverter.IsLittleEndian )
            {
                nextMessageId = BinaryPrimitives.ReverseEndianness( nextMessageId );
                nextPendingNodeId = BinaryPrimitives.ReverseEndianness( nextPendingNodeId );
                messageCount = BinaryPrimitives.ReverseEndianness( messageCount );
                routingCount = BinaryPrimitives.ReverseEndianness( routingCount );
            }

            var writer = new BinaryContractWriter( target.Span );
            StreamMetadata.Header.CopyTo( writer.GetSpan( StreamMetadata.Header.Length ) );
            writer.Move( StreamMetadata.Header.Length );
            writer.MoveWrite( nextMessageId );
            writer.MoveWrite( nextPendingNodeId );
            writer.MoveWrite( messageCount );
            writer.Write( routingCount );
        }
    }

    internal readonly struct StreamMessageHeader
    {
        internal const int Length = sizeof( uint ) * 4 + sizeof( ulong ) * 2;

        internal readonly ulong Id;
        internal readonly int StoreKey;
        internal readonly int SenderId;
        internal readonly int ChannelId;
        internal readonly int DataLength;
        internal readonly Timestamp PushedAt;

        internal StreamMessageHeader(ulong id, int storeKey, int senderId, int channelId, int dataLength, Timestamp pushedAt)
        {
            Id = id;
            StoreKey = storeKey;
            SenderId = senderId;
            ChannelId = channelId;
            DataLength = dataLength;
            PushedAt = pushedAt;
        }

        internal bool IsSenderVirtual => SenderId < 0;
        internal bool IsDiscarded => ChannelId == 0;

        [Pure]
        public override string ToString()
        {
            return
                $"Id: {Id}, StoreKey: {StoreKey}, SenderId: {SenderId}, ChannelId: {ChannelId}, DataLength: {DataLength}, PushedAt: {PushedAt}";
        }

        internal static StreamMessageHeader Create(
            int storeKey,
            StreamMessage message,
            Dictionary<EphemeralClientKey, EphemeralClientValue> ephemeralClients)
        {
            int senderId;
            int channelId;
            int dataLength;

            if ( message.Publisher.Channel.Listeners.Count == 0 )
            {
                senderId = message.Publisher.ClientId;
                channelId = 0;
                dataLength = 0;
            }
            else
            {
                channelId = message.Publisher.Channel.Id;
                dataLength = message.Data.Length;
                if ( message.Publisher.IsClientEphemeral )
                {
                    ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(
                        ephemeralClients,
                        new EphemeralClientKey( message.Publisher.ClientId, message.Publisher.ClientName ),
                        out var exists );

                    if ( ! exists )
                        value = new EphemeralClientValue(
                            ephemeralClients.Count,
                            TextEncoding.Prepare( message.Publisher.ClientName ).GetValueOrThrow() );

                    senderId = -value.VirtualId;
                }
                else
                    senderId = message.Publisher.ClientId;
            }

            return new StreamMessageHeader( message.Id, storeKey, senderId, channelId, dataLength, message.PushedAt );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StreamMessageHeader Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var id = reader.MoveReadInt64();
            var storeKey = reader.MoveReadInt32();
            var senderId = reader.MoveReadInt32();
            var channelId = reader.MoveReadInt32();
            var pushedAtTicks = reader.MoveReadInt64();
            var dataLength = reader.ReadInt32();

            if ( ! BitConverter.IsLittleEndian )
            {
                id = BinaryPrimitives.ReverseEndianness( id );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                senderId = BinaryPrimitives.ReverseEndianness( senderId );
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
                pushedAtTicks = BinaryPrimitives.ReverseEndianness( pushedAtTicks );
                dataLength = BinaryPrimitives.ReverseEndianness( dataLength );
            }

            var result = new StreamMessageHeader(
                id,
                unchecked( ( int )storeKey ),
                unchecked( ( int )senderId ),
                unchecked( ( int )channelId ),
                unchecked( ( int )dataLength ),
                new Timestamp( unchecked( ( long )pushedAtTicks ) ) );

            var errors = Chain<string>.Empty;
            if ( result.StoreKey < 0 )
                errors = errors.Extend( Resources.StoreKeyIsNegative( result.StoreKey ) );

            if ( result.DataLength < 0 || result.DataLength > context.Server.MaxNetworkMessagePacketBytes )
                errors = errors.Extend( Resources.InvalidDataLength( context.Server, result.DataLength ) );

            if ( result.ChannelId == 0 && result.DataLength > 0 )
                errors = errors.Extend( Resources.DiscardedDataLengthIsPositive( result.DataLength ) );

            if ( errors.Count > 0 )
                context.Throw( errors );

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var id = Id;
            var storeKey = unchecked( ( uint )StoreKey );
            var senderId = unchecked( ( uint )SenderId );
            var channelId = unchecked( ( uint )ChannelId );
            var pushedAtTicks = unchecked( ( ulong )PushedAt.UnixEpochTicks );
            var dataLength = unchecked( ( uint )DataLength );

            if ( ! BitConverter.IsLittleEndian )
            {
                id = BinaryPrimitives.ReverseEndianness( id );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                senderId = BinaryPrimitives.ReverseEndianness( senderId );
                channelId = BinaryPrimitives.ReverseEndianness( channelId );
                pushedAtTicks = BinaryPrimitives.ReverseEndianness( pushedAtTicks );
                dataLength = BinaryPrimitives.ReverseEndianness( dataLength );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( id );
            writer.MoveWrite( storeKey );
            writer.MoveWrite( senderId );
            writer.MoveWrite( channelId );
            writer.MoveWrite( pushedAtTicks );
            writer.Write( dataLength );
        }
    }

    internal readonly struct StreamRoutingHeader
    {
        internal const int Length = sizeof( ulong ) + sizeof( uint );

        internal readonly ulong MessageId;
        internal readonly int DataLength;

        internal StreamRoutingHeader(ulong messageId, int dataLength)
        {
            MessageId = messageId;
            DataLength = dataLength;
        }

        internal bool IsDiscarded => DataLength == 0;

        [Pure]
        public override string ToString()
        {
            return $"MessageId: {MessageId}, DataLength: {DataLength}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static StreamRoutingHeader Parse(Context context, ReadOnlyMemory<byte> source)
        {
            const int maxDataLength = 1 << 28;
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var messageId = reader.MoveReadInt64();
            var dataLength = reader.ReadInt32();

            if ( ! BitConverter.IsLittleEndian )
            {
                messageId = BinaryPrimitives.ReverseEndianness( messageId );
                dataLength = BinaryPrimitives.ReverseEndianness( dataLength );
            }

            var result = new StreamRoutingHeader( messageId, unchecked( ( int )dataLength ) );
            if ( result.DataLength < 0 || result.DataLength > maxDataLength )
                context.Throw( Resources.InvalidRoutingDataLength( result.DataLength, maxDataLength ) );

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var messageId = MessageId;
            var dataLength = unchecked( ( uint )DataLength );
            if ( ! BitConverter.IsLittleEndian )
            {
                messageId = BinaryPrimitives.ReverseEndianness( messageId );
                dataLength = BinaryPrimitives.ReverseEndianness( dataLength );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( messageId );
            writer.Write( dataLength );
        }
    }

    internal readonly struct ClientMetadata
    {
        internal const string DirPrefix = "_";
        internal const string FileName = "meta.mbcl";
        internal static ReadOnlySpan<byte> Header => "LFMBCL"u8;
        internal static int MinLength => Header.Length + sizeof( byte ) + sizeof( ulong ) + 1;

        internal readonly ulong TraceId;
        internal readonly bool ClearBuffers;
        internal readonly EncodeableText Name;

        internal ClientMetadata(ulong traceId, bool clearBuffers, EncodeableText name)
        {
            TraceId = traceId;
            ClearBuffers = clearBuffers;
            Name = name;
        }

        internal int Length => Header.Length + sizeof( byte ) + sizeof( ulong ) + Name.ByteCount;

        [Pure]
        public override string ToString()
        {
            return $"TraceId: {TraceId}, ClearBuffers: {ClearBuffers}, Name: {Name}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string GetDirName(int id)
        {
            return $"{DirPrefix}{id.ToString( CultureInfo.InvariantCulture )}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ClientMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, MinLength );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var flags = reader.MoveReadInt8();
            var traceId = reader.MoveReadInt64();

            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var name = TextEncoding.Prepare( reader.GetSpan( source.Length - MinLength + 1 ) ).GetValueOrThrow();
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
                context.Throw( Resources.InvalidClientNameLength( name.Value.Length ) );

            return new ClientMetadata( unchecked( traceId + 1 ), (flags & 1) != 0, name );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var flags = ( byte )(ClearBuffers ? 1 : 0);
            var traceId = TraceId;
            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.MoveWrite( flags );
            writer.MoveWrite( traceId );
            Name.Encode( writer.GetSpan( Name.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct PublisherMetadata
    {
        internal const string MetaFilePrefix = "meta";
        internal const string MetaFileExtension = "mbpb";
        internal static ReadOnlySpan<byte> Header => "LFMBPB"u8;
        internal static int Length => Header.Length + sizeof( uint );

        internal readonly int StreamId;

        internal PublisherMetadata(int streamId)
        {
            StreamId = streamId;
        }

        [Pure]
        public override string ToString()
        {
            return $"StreamId: {StreamId}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string GetFileName(int channelId)
        {
            return $"{MetaFilePrefix}{channelId.ToString( CultureInfo.InvariantCulture )}.{MetaFileExtension}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static PublisherMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var streamId = reader.ReadInt32();

            if ( ! BitConverter.IsLittleEndian )
                streamId = BinaryPrimitives.ReverseEndianness( streamId );

            return new PublisherMetadata( unchecked( ( int )streamId ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var streamId = unchecked( ( uint )StreamId );
            if ( ! BitConverter.IsLittleEndian )
                streamId = BinaryPrimitives.ReverseEndianness( streamId );

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.Write( streamId );
        }
    }

    internal readonly struct ListenerMetadata
    {
        internal const string MetaFilePrefix = "meta";
        internal const string MetaFileExtension = "mbls";
        internal static ReadOnlySpan<byte> Header => "LFMBLS"u8;
        internal static int MinLength => Header.Length + sizeof( ushort ) + sizeof( uint ) * 4 + sizeof( ulong ) * 3;

        internal readonly int QueueId;
        internal readonly short PrefetchHint;
        internal readonly int MaxRetries;
        internal readonly Duration RetryDelay;
        internal readonly int MaxRedeliveries;
        internal readonly Duration MinAckTimeout;
        internal readonly int DeadLetterCapacityHint;
        internal readonly Duration MinDeadLetterRetention;
        internal readonly EncodeableText Filter;

        internal ListenerMetadata(
            int queueId,
            short prefetchHint,
            int maxRetries,
            Duration retryDelay,
            int maxRedeliveries,
            Duration minAckTimeout,
            int deadLetterCapacityHint,
            Duration minDeadLetterRetention,
            EncodeableText filter)
        {
            QueueId = queueId;
            PrefetchHint = prefetchHint;
            MaxRetries = maxRetries;
            RetryDelay = retryDelay;
            MaxRedeliveries = maxRedeliveries;
            MinAckTimeout = minAckTimeout;
            DeadLetterCapacityHint = deadLetterCapacityHint;
            MinDeadLetterRetention = minDeadLetterRetention;
            Filter = filter;
        }

        internal int Length => MinLength + Filter.ByteCount;

        [Pure]
        public override string ToString()
        {
            return
                $"QueueId: {QueueId}, PrefetchHint: {PrefetchHint}, MaxRetries: {MaxRetries}, RetryDelay: {RetryDelay}, MaxRedeliveries: {MaxRedeliveries}, MinAckTimeout: {MinAckTimeout}, DeadLetterCapacityHint: {DeadLetterCapacityHint}, MinDeadLetterRetention: {MinDeadLetterRetention}, Filter: {Filter}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string GetFileName(int channelId)
        {
            return $"{MetaFilePrefix}{channelId.ToString( CultureInfo.InvariantCulture )}.{MetaFileExtension}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static ListenerMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, MinLength );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var queueId = reader.MoveReadInt32();
            var prefetchHint = reader.MoveReadInt16();
            var maxRetries = reader.MoveReadInt32();
            var retryDelayTicks = reader.MoveReadInt64();
            var maxRedeliveries = reader.MoveReadInt32();
            var minAckTimeoutTicks = reader.MoveReadInt64();
            var deadLetterCapacityHint = reader.MoveReadInt32();
            var minDeadLetterRetentionTicks = reader.MoveReadInt64();

            if ( ! BitConverter.IsLittleEndian )
            {
                queueId = BinaryPrimitives.ReverseEndianness( queueId );
                prefetchHint = BinaryPrimitives.ReverseEndianness( prefetchHint );
                maxRetries = BinaryPrimitives.ReverseEndianness( maxRetries );
                retryDelayTicks = BinaryPrimitives.ReverseEndianness( retryDelayTicks );
                maxRedeliveries = BinaryPrimitives.ReverseEndianness( maxRedeliveries );
                minAckTimeoutTicks = BinaryPrimitives.ReverseEndianness( minAckTimeoutTicks );
                deadLetterCapacityHint = BinaryPrimitives.ReverseEndianness( deadLetterCapacityHint );
                minDeadLetterRetentionTicks = BinaryPrimitives.ReverseEndianness( minDeadLetterRetentionTicks );
            }

            var filter = TextEncoding.Prepare( reader.GetSpan( source.Length - MinLength ) ).GetValueOrThrow();

            return new ListenerMetadata(
                unchecked( ( int )queueId ),
                unchecked( ( short )prefetchHint ),
                unchecked( ( int )maxRetries ),
                Duration.FromTicks( unchecked( ( long )retryDelayTicks ) ),
                unchecked( ( int )maxRedeliveries ),
                Duration.FromTicks( unchecked( ( long )minAckTimeoutTicks ) ),
                unchecked( ( int )deadLetterCapacityHint ),
                Duration.FromTicks( unchecked( ( long )minDeadLetterRetentionTicks ) ),
                filter );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.Equals( target.Length, Length );

            var queueId = unchecked( ( uint )QueueId );
            var prefetchHint = unchecked( ( ushort )PrefetchHint );
            var maxRetries = unchecked( ( uint )MaxRetries );
            var retryDelayTicks = unchecked( ( ulong )RetryDelay.Ticks );
            var maxRedeliveries = unchecked( ( uint )MaxRedeliveries );
            var minAckTimeoutTicks = unchecked( ( ulong )MinAckTimeout.Ticks );
            var deadLetterCapacityHint = unchecked( ( uint )DeadLetterCapacityHint );
            var minDeadLetterRetentionTicks = unchecked( ( ulong )MinDeadLetterRetention.Ticks );
            if ( ! BitConverter.IsLittleEndian )
            {
                queueId = BinaryPrimitives.ReverseEndianness( queueId );
                prefetchHint = BinaryPrimitives.ReverseEndianness( prefetchHint );
                maxRetries = BinaryPrimitives.ReverseEndianness( maxRetries );
                retryDelayTicks = BinaryPrimitives.ReverseEndianness( retryDelayTicks );
                maxRedeliveries = BinaryPrimitives.ReverseEndianness( maxRedeliveries );
                minAckTimeoutTicks = BinaryPrimitives.ReverseEndianness( minAckTimeoutTicks );
                deadLetterCapacityHint = BinaryPrimitives.ReverseEndianness( deadLetterCapacityHint );
                minDeadLetterRetentionTicks = BinaryPrimitives.ReverseEndianness( minDeadLetterRetentionTicks );
            }

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.MoveWrite( queueId );
            writer.MoveWrite( prefetchHint );
            writer.MoveWrite( maxRetries );
            writer.MoveWrite( retryDelayTicks );
            writer.MoveWrite( maxRedeliveries );
            writer.MoveWrite( minAckTimeoutTicks );
            writer.MoveWrite( deadLetterCapacityHint );
            writer.MoveWrite( minDeadLetterRetentionTicks );
            Filter.Encode( writer.GetSpan( Filter.ByteCount ) ).ThrowIfError();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ListenerMetadata Sanitize(out Chain<string> warnings)
        {
            warnings = Chain<string>.Empty;
            var prefetchHint = PrefetchHint;
            var maxRetries = MaxRetries;
            var retryDelay = RetryDelay;
            var maxRedeliveries = MaxRedeliveries;
            var minAckTimeout = MinAckTimeout;
            var deadLetterCapacityHint = DeadLetterCapacityHint;
            var minDeadLetterRetention = MinDeadLetterRetention;

            if ( prefetchHint < 1 )
            {
                warnings = warnings.Extend( Resources.InvalidPrefetchHint( prefetchHint ) );
                prefetchHint = 1;
            }

            if ( maxRetries < 0 )
            {
                warnings = warnings.Extend( Resources.MaxRetriesIsNegative( maxRetries ) );
                maxRetries = 0;
            }

            if ( retryDelay < Duration.Zero )
            {
                warnings = warnings.Extend( Resources.RetryDelayIsNegative( retryDelay ) );
                retryDelay = Duration.Zero;
            }
            else if ( retryDelay > Duration.Zero && maxRetries == 0 )
            {
                warnings = warnings.Extend( Resources.DisabledRetryDelayIsNotZero( retryDelay ) );
                retryDelay = Duration.Zero;
            }

            if ( maxRedeliveries < 0 )
            {
                warnings = warnings.Extend( Resources.MaxRedeliveriesIsNegative( maxRedeliveries ) );
                maxRedeliveries = 0;
            }

            if ( minAckTimeout < Duration.Zero )
            {
                warnings = warnings.Extend( Resources.MinAckTimeoutIsNegative( minAckTimeout ) );
                minAckTimeout = Duration.Zero;
            }
            else if ( minAckTimeout == Duration.Zero && (maxRetries > 0 || maxRedeliveries > 0 || deadLetterCapacityHint > 0) )
            {
                warnings = warnings.Extend( Resources.EnabledMinAckTimeoutIsNotPositive( minAckTimeout ) );
                minAckTimeout = Duration.FromMinutes( 10 );
            }

            if ( deadLetterCapacityHint < 0 )
            {
                warnings = warnings.Extend( Resources.DeadLetterCapacityIsNegative( deadLetterCapacityHint ) );
                deadLetterCapacityHint = 0;
                minDeadLetterRetention = Duration.Zero;
            }
            else if ( deadLetterCapacityHint == 0 )
            {
                if ( minDeadLetterRetention != Duration.Zero )
                {
                    warnings = warnings.Extend( Resources.DisabledDeadLetterRetentionIsNotZero( minDeadLetterRetention ) );
                    minDeadLetterRetention = Duration.Zero;
                }
            }
            else if ( minDeadLetterRetention <= Duration.Zero )
            {
                warnings = warnings.Extend( Resources.EnabledDeadLetterRetentionIsNotPositive( minDeadLetterRetention ) );
                minDeadLetterRetention = Duration.FromHours( ChronoConstants.HoursPerStandardDay * 30 );
            }

            return new ListenerMetadata(
                QueueId,
                prefetchHint,
                maxRetries,
                retryDelay,
                maxRedeliveries,
                minAckTimeout,
                deadLetterCapacityHint,
                minDeadLetterRetention,
                Filter );
        }
    }

    internal readonly struct QueueMetadata
    {
        internal const string DirPrefix = "_";
        internal const string FileName = "meta.mbqu";
        internal static ReadOnlySpan<byte> Header => "LFMBQU"u8;
        internal static int MinLength => Header.Length + sizeof( ulong ) + 1;

        internal readonly ulong TraceId;
        internal readonly EncodeableText Name;

        internal QueueMetadata(ulong traceId, EncodeableText name)
        {
            TraceId = traceId;
            Name = name;
        }

        internal int Length => Header.Length + sizeof( ulong ) + Name.ByteCount;

        [Pure]
        public override string ToString()
        {
            return $"TraceId: {TraceId}, Name: {Name}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static string GetDirName(int id)
        {
            return $"{DirPrefix}{id.ToString( CultureInfo.InvariantCulture )}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void AssertHeader(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Header.Length );
            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static QueueMetadata Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, MinLength );

            var reader = new BinaryContractReader( source.Span );
            var header = reader.GetSpan( Header.Length );
            if ( ! header.SequenceEqual( Header ) )
                context.ThrowInvalidHeader();

            reader.Move( header.Length );
            var traceId = reader.MoveReadInt64();

            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var name = TextEncoding.Prepare( reader.GetSpan( source.Length - MinLength + 1 ) ).GetValueOrThrow();
            if ( ! Defaults.NameLengthBounds.Contains( name.Value.Length ) )
                context.Throw( Resources.InvalidQueueNameLength( name.Value.Length ) );

            return new QueueMetadata( unchecked( traceId + 1 ), name );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void SerializeHeader(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Header.Length );
            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var traceId = TraceId;
            if ( ! BitConverter.IsLittleEndian )
                traceId = BinaryPrimitives.ReverseEndianness( traceId );

            var writer = new BinaryContractWriter( target.Span );
            Header.CopyTo( writer.GetSpan( Header.Length ) );
            writer.Move( Header.Length );
            writer.MoveWrite( traceId );
            Name.Encode( writer.GetSpan( Name.ByteCount ) ).ThrowIfError();
        }
    }

    internal readonly struct QueuePendingMessage
    {
        internal const string FileName = "pending.mbpm";
        internal const int Length = sizeof( uint ) * 2;

        internal readonly int StreamId;
        internal readonly int StoreKey;

        internal QueuePendingMessage(int streamId, int storeKey)
        {
            StreamId = streamId;
            StoreKey = storeKey;
        }

        [Pure]
        public override string ToString()
        {
            return $"StreamId: {StreamId}, StoreKey: {StoreKey}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static QueuePendingMessage Parse(ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var streamId = reader.MoveReadInt32();
            var storeKey = reader.ReadInt32();

            if ( ! BitConverter.IsLittleEndian )
            {
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
            }

            return new QueuePendingMessage( unchecked( ( int )streamId ), unchecked( ( int )storeKey ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int Serialize(ReadOnlyMemory<QueueMessage> messages, Memory<byte> target)
        {
            var written = messages.Length * Length;
            Assume.IsGreaterThanOrEqualTo( target.Length, written );

            var writer = new BinaryContractWriter( target.Span );
            foreach ( ref readonly var m in messages )
            {
                var streamId = unchecked( ( uint )m.Publisher.Stream.Id );
                var storeKey = unchecked( ( uint )m.StoreKey );

                if ( ! BitConverter.IsLittleEndian )
                {
                    streamId = BinaryPrimitives.ReverseEndianness( streamId );
                    storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                }

                writer.MoveWrite( streamId );
                writer.MoveWrite( storeKey );
            }

            return written;
        }
    }

    internal readonly struct QueueUnackedMessage
    {
        internal const string FileName = "unacked.mbue";
        internal const int Length = sizeof( uint ) * 4;

        internal readonly int StreamId;
        internal readonly int StoreKey;
        internal readonly int Retry;
        internal readonly int Redelivery;

        internal QueueUnackedMessage(int streamId, int storeKey, int retry, int redelivery)
        {
            StreamId = streamId;
            StoreKey = storeKey;
            Retry = retry;
            Redelivery = redelivery;
        }

        [Pure]
        public override string ToString()
        {
            return $"StreamId: {StreamId}, StoreKey: {StoreKey}, Retry: {Retry}, Redelivery: {Redelivery}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static QueueUnackedMessage Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var streamId = reader.MoveReadInt32();
            var storeKey = reader.MoveReadInt32();
            var retry = reader.MoveReadInt32();
            var redelivery = reader.ReadInt32();

            if ( ! BitConverter.IsLittleEndian )
            {
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                retry = BinaryPrimitives.ReverseEndianness( retry );
                redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
            }

            var result = new QueueUnackedMessage(
                unchecked( ( int )streamId ),
                unchecked( ( int )storeKey ),
                unchecked( ( int )retry ),
                unchecked( ( int )redelivery ) );

            var errors = Chain<string>.Empty;
            if ( result.Retry < 0 )
                errors = errors.Extend( Resources.RetryIsNegative( result.Retry ) );

            if ( result.Redelivery < 0 )
                errors = errors.Extend( Resources.RedeliveryIsNegative( result.Retry ) );

            if ( errors.Count > 0 )
                context.Throw( errors );

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int Serialize(ReadOnlyMemory<QueueMessageStore.UnackedEntry> messages, Memory<byte> target)
        {
            var written = messages.Length * Length;
            Assume.IsGreaterThanOrEqualTo( target.Length, written );

            var writer = new BinaryContractWriter( target.Span );
            foreach ( ref readonly var m in messages )
            {
                var streamId = unchecked( ( uint )m.Message.Publisher.Stream.Id );
                var storeKey = unchecked( ( uint )m.Message.StoreKey );
                var retry = unchecked( ( uint )m.Retry );
                var redelivery = unchecked( ( uint )m.Redelivery );

                if ( ! BitConverter.IsLittleEndian )
                {
                    streamId = BinaryPrimitives.ReverseEndianness( streamId );
                    storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                    retry = BinaryPrimitives.ReverseEndianness( retry );
                    redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
                }

                writer.MoveWrite( streamId );
                writer.MoveWrite( storeKey );
                writer.MoveWrite( retry );
                writer.MoveWrite( redelivery );
            }

            return written;
        }
    }

    internal readonly struct QueueMessageRetry
    {
        internal const string FileName = "retries.mbre";
        internal const int Length = sizeof( uint ) * 4 + sizeof( ulong );

        internal readonly int StreamId;
        internal readonly int StoreKey;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly Timestamp SendAt;

        internal QueueMessageRetry(int streamId, int storeKey, int retry, int redelivery, Timestamp sendAt)
        {
            StreamId = streamId;
            StoreKey = storeKey;
            Retry = retry;
            Redelivery = redelivery;
            SendAt = sendAt;
        }

        [Pure]
        public override string ToString()
        {
            return $"StreamId: {StreamId}, StoreKey: {StoreKey}, Retry: {Retry}, Redelivery: {Redelivery}, SendAt: {SendAt}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static QueueMessageRetry Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var streamId = reader.MoveReadInt32();
            var storeKey = reader.MoveReadInt32();
            var retry = reader.MoveReadInt32();
            var redelivery = reader.MoveReadInt32();
            var sendAtTicks = reader.ReadInt64();

            if ( ! BitConverter.IsLittleEndian )
            {
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                retry = BinaryPrimitives.ReverseEndianness( retry );
                redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
                sendAtTicks = BinaryPrimitives.ReverseEndianness( sendAtTicks );
            }

            var result = new QueueMessageRetry(
                unchecked( ( int )streamId ),
                unchecked( ( int )storeKey ),
                unchecked( ( int )retry ),
                unchecked( ( int )redelivery ),
                new Timestamp( unchecked( ( long )sendAtTicks ) ) );

            var errors = Chain<string>.Empty;
            if ( result.Retry < 0 )
                errors = errors.Extend( Resources.RetryIsNegative( result.Retry ) );

            if ( result.Redelivery < 0 )
                errors = errors.Extend( Resources.RedeliveryIsNegative( result.Retry ) );

            if ( errors.Count > 0 )
                context.Throw( errors );

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int Serialize(ReadOnlyMemory<QueueRetryHeap.Entry> messages, Memory<byte> target)
        {
            var written = messages.Length * Length;
            Assume.IsGreaterThanOrEqualTo( target.Length, written );

            var writer = new BinaryContractWriter( target.Span );
            foreach ( ref readonly var m in messages )
            {
                var streamId = unchecked( ( uint )m.Message.Publisher.Stream.Id );
                var storeKey = unchecked( ( uint )m.Message.StoreKey );
                var retry = unchecked( ( uint )m.Retry );
                var redelivery = unchecked( ( uint )m.Redelivery );
                var sendAtTicks = unchecked( ( ulong )m.SendAt.UnixEpochTicks );

                if ( ! BitConverter.IsLittleEndian )
                {
                    streamId = BinaryPrimitives.ReverseEndianness( streamId );
                    storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                    retry = BinaryPrimitives.ReverseEndianness( retry );
                    redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
                    sendAtTicks = BinaryPrimitives.ReverseEndianness( sendAtTicks );
                }

                writer.MoveWrite( streamId );
                writer.MoveWrite( storeKey );
                writer.MoveWrite( retry );
                writer.MoveWrite( redelivery );
                writer.MoveWrite( sendAtTicks );
            }

            return written;
        }
    }

    internal readonly struct QueueDeadLetterMessage
    {
        internal const string FileName = "deadletter.mbdl";
        internal const int Length = sizeof( uint ) * 4 + sizeof( ulong );

        internal readonly int StreamId;
        internal readonly int StoreKey;
        internal readonly int Retry;
        internal readonly int Redelivery;
        internal readonly Timestamp ExpiresAt;

        internal QueueDeadLetterMessage(int streamId, int storeKey, int retry, int redelivery, Timestamp expiresAt)
        {
            StreamId = streamId;
            StoreKey = storeKey;
            Retry = retry;
            Redelivery = redelivery;
            ExpiresAt = expiresAt;
        }

        [Pure]
        public override string ToString()
        {
            return $"StreamId: {StreamId}, StoreKey: {StoreKey}, Retry: {Retry}, Redelivery: {Redelivery}, ExpiresAt: {ExpiresAt}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static QueueDeadLetterMessage Parse(Context context, ReadOnlyMemory<byte> source)
        {
            Assume.IsGreaterThanOrEqualTo( source.Length, Length );

            var reader = new BinaryContractReader( source.Span );
            var streamId = reader.MoveReadInt32();
            var storeKey = reader.MoveReadInt32();
            var retry = reader.MoveReadInt32();
            var redelivery = reader.MoveReadInt32();
            var expiresAtTicks = reader.ReadInt64();

            if ( ! BitConverter.IsLittleEndian )
            {
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                retry = BinaryPrimitives.ReverseEndianness( retry );
                redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
                expiresAtTicks = BinaryPrimitives.ReverseEndianness( expiresAtTicks );
            }

            var result = new QueueDeadLetterMessage(
                unchecked( ( int )streamId ),
                unchecked( ( int )storeKey ),
                unchecked( ( int )retry ),
                unchecked( ( int )redelivery ),
                new Timestamp( unchecked( ( long )expiresAtTicks ) ) );

            var errors = Chain<string>.Empty;
            if ( result.Retry < 0 )
                errors = errors.Extend( Resources.RetryIsNegative( result.Retry ) );

            if ( result.Redelivery < 0 )
                errors = errors.Extend( Resources.RedeliveryIsNegative( result.Retry ) );

            if ( errors.Count > 0 )
                context.Throw( errors );

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static int Serialize(ReadOnlyMemory<QueueMessageStore.DeadLetterEntry> messages, Memory<byte> target)
        {
            var written = messages.Length * Length;
            Assume.IsGreaterThanOrEqualTo( target.Length, written );

            var writer = new BinaryContractWriter( target.Span );
            foreach ( ref readonly var m in messages )
            {
                var streamId = unchecked( ( uint )m.Message.Publisher.Stream.Id );
                var storeKey = unchecked( ( uint )m.Message.StoreKey );
                var retry = unchecked( ( uint )m.Retry );
                var redelivery = unchecked( ( uint )m.Redelivery );
                var expiresAtTicks = unchecked( ( ulong )m.ExpiresAt.UnixEpochTicks );

                if ( ! BitConverter.IsLittleEndian )
                {
                    streamId = BinaryPrimitives.ReverseEndianness( streamId );
                    storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
                    retry = BinaryPrimitives.ReverseEndianness( retry );
                    redelivery = BinaryPrimitives.ReverseEndianness( redelivery );
                    expiresAtTicks = BinaryPrimitives.ReverseEndianness( expiresAtTicks );
                }

                writer.MoveWrite( streamId );
                writer.MoveWrite( storeKey );
                writer.MoveWrite( retry );
                writer.MoveWrite( redelivery );
                writer.MoveWrite( expiresAtTicks );
            }

            return written;
        }
    }

    internal readonly record struct EphemeralClientKey(int Id, string Name);

    internal readonly struct EphemeralClientValue
    {
        internal readonly int VirtualId;
        internal readonly EncodeableText Name;

        internal EphemeralClientValue(int virtualId, EncodeableText name)
        {
            VirtualId = virtualId;
            Name = name;
        }

        internal int Length => Header.Length + Name.ByteCount;

        [Pure]
        public override string ToString()
        {
            return $"VirtualId: {VirtualId}, Name: {Name}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Serialize(Memory<byte> target, int clientId)
        {
            Assume.IsGreaterThanOrEqualTo( target.Length, Length );

            var virtualId = unchecked( ( uint )VirtualId );
            var senderId = unchecked( ( uint )clientId );
            var nameLength = unchecked( ( uint )Name.ByteCount );
            if ( ! BitConverter.IsLittleEndian )
            {
                virtualId = BinaryPrimitives.ReverseEndianness( virtualId );
                senderId = BinaryPrimitives.ReverseEndianness( senderId );
                nameLength = BinaryPrimitives.ReverseEndianness( nameLength );
            }

            var writer = new BinaryContractWriter( target.Span );
            writer.MoveWrite( virtualId );
            writer.MoveWrite( senderId );
            writer.MoveWrite( nameLength );
            Name.Encode( writer.GetSpan( Name.ByteCount ) ).ThrowIfError();
        }

        internal readonly struct Header
        {
            internal const int Length = sizeof( uint ) * 3;

            internal readonly int VirtualId;
            internal readonly int SenderId;
            internal readonly int NameLength;

            internal Header(int virtualId, int senderId, int nameLength)
            {
                VirtualId = virtualId;
                SenderId = senderId;
                NameLength = nameLength;
            }

            [Pure]
            public override string ToString()
            {
                return $"VirtualId: {VirtualId}, SenderId: {SenderId}, NameLength: {NameLength}";
            }

            [Pure]
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            internal static Header Parse(Context context, ReadOnlyMemory<byte> source)
            {
                Assume.IsGreaterThanOrEqualTo( source.Length, Length );

                var reader = new BinaryContractReader( source.Span );
                var virtualId = reader.MoveReadInt32();
                var senderId = reader.MoveReadInt32();
                var nameLength = reader.ReadInt32();

                if ( ! BitConverter.IsLittleEndian )
                {
                    virtualId = BinaryPrimitives.ReverseEndianness( virtualId );
                    senderId = BinaryPrimitives.ReverseEndianness( senderId );
                    nameLength = BinaryPrimitives.ReverseEndianness( nameLength );
                }

                var result = new Header( unchecked( ( int )virtualId ), unchecked( ( int )senderId ), unchecked( ( int )nameLength ) );

                var errors = Chain<string>.Empty;
                if ( result.VirtualId <= 0 )
                    errors = errors.Extend( Resources.VirtualIdIsNotPositive( result.VirtualId ) );

                if ( result.SenderId <= 0 )
                    errors = errors.Extend( Resources.SenderIdIsNotPositive( result.SenderId ) );

                if ( result.NameLength <= 0 || result.NameLength > Defaults.Memory.DefaultNetworkPacketLength )
                    errors = errors.Extend( Resources.InvalidBinaryClientNameLength( result.NameLength ) );

                if ( errors.Count > 0 )
                    context.Throw( errors );

                return result;
            }
        }
    }
}
