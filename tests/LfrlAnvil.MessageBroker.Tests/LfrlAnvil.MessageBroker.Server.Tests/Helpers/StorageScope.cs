using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using LfrlAnvil.Chrono;
using LfrlAnvil.Internal;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Tests.Helpers;

internal readonly struct StorageScope : IDisposable
{
    internal StorageScope(string path)
    {
        Path = path;
    }

    internal string Path { get; }

    [Pure]
    internal static StorageScope Create()
    {
        return new StorageScope( $"_{Guid.NewGuid():N}" );
    }

    [Pure]
    internal static string[] GetServerMetadataSubpath()
    {
        return [ Storage.ServerMetadata.FileName ];
    }

    [Pure]
    internal static string[] GetChannelMetadataSubpath(int channelId, string? fileName = null)
    {
        return [ "channels", fileName ?? Storage.ChannelMetadata.GetFileName( channelId ) ];
    }

    [Pure]
    internal static string[] GetStreamMetadataSubpath(int streamId, string? directoryName = null)
    {
        return [ "streams", directoryName ?? Storage.StreamMetadata.GetDirName( streamId ), Storage.StreamMetadata.FileName ];
    }

    [Pure]
    internal static string[] GetStreamMessagesSubpath(int streamId)
    {
        return [ "streams", Storage.StreamMetadata.GetDirName( streamId ), Storage.StreamMessageRangeHeader.FileName ];
    }

    [Pure]
    internal static string[] GetClientMetadataSubpath(int clientId, string? directoryName = null)
    {
        return [ "clients", directoryName ?? Storage.ClientMetadata.GetDirName( clientId ), Storage.ClientMetadata.FileName ];
    }

    [Pure]
    internal static string[] GetPublisherMetadataSubpath(int clientId, int channelId, string? fileName = null)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "publishers",
            fileName ?? Storage.PublisherMetadata.GetFileName( channelId )
        ];
    }

    [Pure]
    internal static string[] GetListenerMetadataSubpath(int clientId, int channelId, string? fileName = null)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "listeners",
            fileName ?? Storage.ListenerMetadata.GetFileName( channelId )
        ];
    }

    [Pure]
    internal static string[] GetQueueMetadataSubpath(int clientId, int queueId, string? directorName = null)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "queues",
            directorName ?? Storage.QueueMetadata.GetDirName( queueId ),
            Storage.QueueMetadata.FileName
        ];
    }

    [Pure]
    internal static string[] GetQueuePendingMessagesSubpath(int clientId, int queueId)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "queues",
            Storage.QueueMetadata.GetDirName( queueId ),
            Storage.QueuePendingMessage.FileName
        ];
    }

    [Pure]
    internal static string[] GetQueueUnackedMessagesSubpath(int clientId, int queueId)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "queues",
            Storage.QueueMetadata.GetDirName( queueId ),
            Storage.QueueUnackedMessage.FileName
        ];
    }

    [Pure]
    internal static string[] GetQueueRetryMessagesSubpath(int clientId, int queueId)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "queues",
            Storage.QueueMetadata.GetDirName( queueId ),
            Storage.QueueMessageRetry.FileName
        ];
    }

    [Pure]
    internal static string[] GetQueueDeadLetterMessagesSubpath(int clientId, int queueId)
    {
        return
        [
            "clients",
            Storage.ClientMetadata.GetDirName( clientId ),
            "queues",
            Storage.QueueMetadata.GetDirName( queueId ),
            Storage.QueueDeadLetterMessage.FileName
        ];
    }

    internal void WriteToFile(string[] subpathSegments, byte[] data)
    {
        var path = System.IO.Path.Combine( Path, System.IO.Path.Combine( subpathSegments ) );
        var directory = new DirectoryInfo( System.IO.Path.GetDirectoryName( path )! );
        directory.Create();
        File.WriteAllBytes( path, data );
    }

    internal void CreateDirectory(string[] subpathSegments)
    {
        var path = System.IO.Path.Combine( Path, System.IO.Path.Combine( subpathSegments ) );
        var directory = new DirectoryInfo( System.IO.Path.GetDirectoryName( path )! );
        directory.Create();
    }

    [Pure]
    internal bool FileExists(string[] subpathSegments)
    {
        var path = System.IO.Path.Combine( Path, System.IO.Path.Combine( subpathSegments ) );
        return File.Exists( path );
    }

    [Pure]
    internal bool DirectoryExists(string[] subpathSegments)
    {
        var path = System.IO.Path.Combine( Path, System.IO.Path.Combine( subpathSegments ) );
        var directoryPath = System.IO.Path.GetDirectoryName( path );
        return Directory.Exists( directoryPath );
    }

    internal void WriteServerMetadata(ulong traceId = 0, ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.ServerMetadata( traceId );
        var buffer = new byte[Storage.ServerMetadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.ServerMetadata.Header );
        WriteToFile( GetServerMetadataSubpath(), buffer );
    }

    internal void WriteChannelMetadata(
        int channelId,
        string channelName,
        ulong traceId = 0,
        string? fileName = null,
        ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.ChannelMetadata( traceId, TextEncoding.Prepare( channelName ).GetValueOrThrow() );
        var buffer = new byte[metadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.ChannelMetadata.Header );
        WriteToFile( GetChannelMetadataSubpath( channelId, fileName ), buffer );
    }

    internal void WriteStreamMetadata(
        int streamId,
        string streamName,
        ulong traceId = 0,
        string? directoryName = null,
        ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.StreamMetadata( traceId, TextEncoding.Prepare( streamName ).GetValueOrThrow() );
        var buffer = new byte[metadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.StreamMetadata.Header );
        WriteToFile( GetStreamMetadataSubpath( streamId, directoryName ), buffer );
    }

    internal void WriteStreamMessages(
        int streamId,
        KeyValuePair<Storage.StreamMessageHeader, byte[]>[] messages,
        KeyValuePair<Storage.StreamRoutingHeader, byte[]>[]? routings = null,
        (Storage.EphemeralClientValue Value, int SenderId, int NameLength)[]? ephemeralClients = null,
        ulong? nextMessageId = null,
        NullableIndex? nextPendingNodeId = null,
        int? messageCount = null,
        int? routingCount = null,
        ReadOnlySpan<byte> header = default)
    {
        var messageRangeHeader = new Storage.StreamMessageRangeHeader(
            nextMessageId ?? 0,
            nextPendingNodeId ?? NullableIndex.Null,
            messageCount ?? messages.Length,
            routingCount ?? routings?.Length ?? 0 );

        var buffer = new List<byte>();
        var intermediateBuffer = new byte[Storage.StreamMessageRangeHeader.Length];
        messageRangeHeader.Serialize( intermediateBuffer );
        OverrideHeader( intermediateBuffer, header, Storage.StreamMetadata.Header );
        buffer.AddRange( intermediateBuffer );

        intermediateBuffer = new byte[Storage.StreamMessageHeader.Length];
        foreach ( var (messageHeader, data) in messages )
        {
            messageHeader.Serialize( intermediateBuffer );
            buffer.AddRange( intermediateBuffer );
            buffer.AddRange( data );
        }

        intermediateBuffer = new byte[Storage.StreamRoutingHeader.Length];
        foreach ( var (routingHeader, data) in routings ?? [ ] )
        {
            routingHeader.Serialize( intermediateBuffer );
            buffer.AddRange( intermediateBuffer );
            buffer.AddRange( data );
        }

        foreach ( var (value, senderId, nameLength) in ephemeralClients ?? [ ] )
        {
            intermediateBuffer = new byte[value.Length];
            value.Serialize( intermediateBuffer, senderId );
            if ( nameLength != value.Name.ByteCount )
            {
                var nameLengthOverride = unchecked( ( uint )nameLength );
                if ( ! BitConverter.IsLittleEndian )
                    nameLengthOverride = BinaryPrimitives.ReverseEndianness( nameLengthOverride );

                var writer = new BinaryContractWriter( intermediateBuffer.AsSpan( sizeof( uint ) * 2 ) );
                writer.Write( nameLengthOverride );
            }

            buffer.AddRange( intermediateBuffer );
        }

        WriteToFile( GetStreamMessagesSubpath( streamId ), buffer.ToArray() );
    }

    internal void WriteClientMetadata(
        int clientId,
        string clientName,
        ulong traceId = 0,
        bool clearBuffers = true,
        string? directoryName = null,
        ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.ClientMetadata( traceId, clearBuffers, TextEncoding.Prepare( clientName ).GetValueOrThrow() );
        var buffer = new byte[metadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.ClientMetadata.Header );
        WriteToFile( GetClientMetadataSubpath( clientId, directoryName ), buffer );
    }

    internal void WritePublisherMetadata(
        int clientId,
        int channelId,
        int streamId,
        string? fileName = null,
        ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.PublisherMetadata( streamId );
        var buffer = new byte[Storage.PublisherMetadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.PublisherMetadata.Header );
        WriteToFile( GetPublisherMetadataSubpath( clientId, channelId, fileName ), buffer );
    }

    internal void WriteListenerMetadata(
        int clientId,
        int channelId,
        int queueId,
        short prefetchHint = 1,
        int maxRetries = 0,
        Duration retryDelay = default,
        int maxRedeliveries = 0,
        Duration minAckTimeout = default,
        int deadLetterCapacityHint = 0,
        Duration minDeadLetterRetention = default,
        string? filter = null,
        string? fileName = null,
        ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.ListenerMetadata(
            queueId,
            prefetchHint,
            maxRetries,
            retryDelay,
            maxRedeliveries,
            minAckTimeout,
            deadLetterCapacityHint,
            minDeadLetterRetention,
            TextEncoding.Prepare( filter ?? string.Empty ).GetValueOrThrow() );

        var buffer = new byte[metadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.ListenerMetadata.Header );
        WriteToFile( GetListenerMetadataSubpath( clientId, channelId, fileName ), buffer );
    }

    internal void WriteQueueMetadata(
        int clientId,
        int queueId,
        string queueName,
        ulong traceId = 0,
        string? directoryName = null,
        ReadOnlySpan<byte> header = default)
    {
        var metadata = new Storage.QueueMetadata( traceId, TextEncoding.Prepare( queueName ).GetValueOrThrow() );
        var buffer = new byte[metadata.Length];
        metadata.Serialize( buffer );
        OverrideHeader( buffer, header, Storage.QueueMetadata.Header );
        WriteToFile( GetQueueMetadataSubpath( clientId, queueId, directoryName ), buffer );
    }

    internal void WriteQueuePendingMessages(
        int clientId,
        int queueId,
        Storage.QueuePendingMessage[] messages,
        ReadOnlySpan<byte> header = default)
    {
        var buffer = new List<byte>();
        var intermediateBuffer = new byte[Storage.QueueMetadata.Header.Length];
        Storage.QueueMetadata.SerializeHeader( intermediateBuffer );
        OverrideHeader( intermediateBuffer, header, Storage.QueueMetadata.Header );
        buffer.AddRange( intermediateBuffer );

        intermediateBuffer = new byte[Storage.QueuePendingMessage.Length];
        foreach ( var m in messages )
        {
            var writer = new BinaryContractWriter( intermediateBuffer );
            var streamId = unchecked( ( uint )m.StreamId );
            var storeKey = unchecked( ( uint )m.StoreKey );

            if ( ! BitConverter.IsLittleEndian )
            {
                streamId = BinaryPrimitives.ReverseEndianness( streamId );
                storeKey = BinaryPrimitives.ReverseEndianness( storeKey );
            }

            writer.MoveWrite( streamId );
            writer.Write( storeKey );
            buffer.AddRange( intermediateBuffer );
        }

        WriteToFile( GetQueuePendingMessagesSubpath( clientId, queueId ), buffer.ToArray() );
    }

    internal void WriteQueueUnackedMessages(
        int clientId,
        int queueId,
        Storage.QueueUnackedMessage[] messages,
        ReadOnlySpan<byte> header = default)
    {
        var buffer = new List<byte>();
        var intermediateBuffer = new byte[Storage.QueueMetadata.Header.Length];
        Storage.QueueMetadata.SerializeHeader( intermediateBuffer );
        OverrideHeader( intermediateBuffer, header, Storage.QueueMetadata.Header );
        buffer.AddRange( intermediateBuffer );

        intermediateBuffer = new byte[Storage.QueueUnackedMessage.Length];
        foreach ( var m in messages )
        {
            var writer = new BinaryContractWriter( intermediateBuffer );
            var streamId = unchecked( ( uint )m.StreamId );
            var storeKey = unchecked( ( uint )m.StoreKey );
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
            writer.Write( redelivery );
            buffer.AddRange( intermediateBuffer );
        }

        WriteToFile( GetQueueUnackedMessagesSubpath( clientId, queueId ), buffer.ToArray() );
    }

    internal void WriteQueueRetryMessages(
        int clientId,
        int queueId,
        Storage.QueueMessageRetry[] messages,
        ReadOnlySpan<byte> header = default)
    {
        var buffer = new List<byte>();
        var intermediateBuffer = new byte[Storage.QueueMetadata.Header.Length];
        Storage.QueueMetadata.SerializeHeader( intermediateBuffer );
        OverrideHeader( intermediateBuffer, header, Storage.QueueMetadata.Header );
        buffer.AddRange( intermediateBuffer );

        intermediateBuffer = new byte[Storage.QueueMessageRetry.Length];
        foreach ( var m in messages )
        {
            var writer = new BinaryContractWriter( intermediateBuffer );
            var streamId = unchecked( ( uint )m.StreamId );
            var storeKey = unchecked( ( uint )m.StoreKey );
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
            writer.Write( sendAtTicks );
            buffer.AddRange( intermediateBuffer );
        }

        WriteToFile( GetQueueRetryMessagesSubpath( clientId, queueId ), buffer.ToArray() );
    }

    internal void WriteQueueDeadLetterMessages(
        int clientId,
        int queueId,
        Storage.QueueDeadLetterMessage[] messages,
        ReadOnlySpan<byte> header = default)
    {
        var buffer = new List<byte>();
        var intermediateBuffer = new byte[Storage.QueueMetadata.Header.Length];
        Storage.QueueMetadata.SerializeHeader( intermediateBuffer );
        OverrideHeader( intermediateBuffer, header, Storage.QueueMetadata.Header );
        buffer.AddRange( intermediateBuffer );

        intermediateBuffer = new byte[Storage.QueueDeadLetterMessage.Length];
        foreach ( var m in messages )
        {
            var writer = new BinaryContractWriter( intermediateBuffer );
            var streamId = unchecked( ( uint )m.StreamId );
            var storeKey = unchecked( ( uint )m.StoreKey );
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
            writer.Write( expiresAtTicks );
            buffer.AddRange( intermediateBuffer );
        }

        WriteToFile( GetQueueDeadLetterMessagesSubpath( clientId, queueId ), buffer.ToArray() );
    }

    [Pure]
    internal static KeyValuePair<Storage.StreamMessageHeader, byte[]> PrepareStreamMessage(
        ulong id,
        int storeKey,
        int senderId,
        int channelId,
        byte[] data,
        int? dataLength = null,
        Timestamp? pushedAt = null)
    {
        return KeyValuePair.Create(
            new Storage.StreamMessageHeader(
                id,
                storeKey,
                senderId,
                channelId,
                dataLength ?? data.Length,
                pushedAt ?? TimestampProvider.Shared.GetNow() ),
            data );
    }

    [Pure]
    internal static KeyValuePair<Storage.StreamMessageHeader, byte[]> PrepareDiscardedStreamMessage(
        ulong id,
        int storeKey,
        int senderId,
        int? channelId = null,
        int? dataLength = null,
        Timestamp? pushedAt = null)
    {
        return KeyValuePair.Create(
            new Storage.StreamMessageHeader(
                id,
                storeKey,
                senderId,
                channelId ?? 0,
                dataLength ?? 0,
                pushedAt ?? TimestampProvider.Shared.GetNow() ),
            Array.Empty<byte>() );
    }

    [Pure]
    internal static KeyValuePair<Storage.StreamRoutingHeader, byte[]> PrepareStreamMessageRouting(
        ulong messageId,
        IReadOnlySet<int> targetClientIds,
        int? dataLength = null)
    {
        var data = Array.Empty<byte>();
        if ( targetClientIds.Count > 0 )
        {
            var maxId = targetClientIds.Max();
            data = new byte[(maxId + 7) / 8];
            Array.Fill( data, ( byte )0 );
            foreach ( var id in targetClientIds )
            {
                var index = id - 1;
                ref var current = ref data[index / 8];
                current |= ( byte )(1 << (index & 7));
            }
        }

        return KeyValuePair.Create( new Storage.StreamRoutingHeader( messageId, dataLength ?? data.Length ), data );
    }

    [Pure]
    internal static (Storage.EphemeralClientValue Value, int SenderId, int NameLength) PrepareEphemeralClient(
        int senderId,
        int virtualId,
        string name,
        int? nameLength = null)
    {
        var value = new Storage.EphemeralClientValue( virtualId, TextEncoding.Prepare( name ).GetValueOrThrow() );
        return (value, senderId, nameLength ?? value.Name.ByteCount);
    }

    [Pure]
    internal static Storage.QueuePendingMessage PrepareQueuePendingMessage(int streamId, int storeKey)
    {
        return new Storage.QueuePendingMessage( streamId, storeKey );
    }

    [Pure]
    internal static Storage.QueueUnackedMessage PrepareQueueUnackedMessage(int streamId, int storeKey, int retry, int redelivery)
    {
        return new Storage.QueueUnackedMessage( streamId, storeKey, retry, redelivery );
    }

    [Pure]
    internal static Storage.QueueMessageRetry PrepareQueueRetryMessage(
        int streamId,
        int storeKey,
        int retry,
        int redelivery,
        Timestamp? sendAt = null)
    {
        return new Storage.QueueMessageRetry( streamId, storeKey, retry, redelivery, sendAt ?? TimestampProvider.Shared.GetNow() );
    }

    [Pure]
    internal static Storage.QueueDeadLetterMessage PrepareQueueDeadLetterMessage(
        int streamId,
        int storeKey,
        int retry,
        int redelivery,
        Timestamp? expiresAt = null)
    {
        return new Storage.QueueDeadLetterMessage( streamId, storeKey, retry, redelivery, expiresAt ?? TimestampProvider.Shared.GetNow() );
    }

    public void Dispose()
    {
        if ( Directory.Exists( Path ) )
            Directory.Delete( Path, recursive: true );
    }

    private static void OverrideHeader(byte[] buffer, ReadOnlySpan<byte> header, ReadOnlySpan<byte> defaultHeader)
    {
        if ( header.Length == 0 )
            return;

        if ( header.Length > defaultHeader.Length )
            header = header.Slice( 0, defaultHeader.Length );

        header.CopyTo( buffer );
    }
}
