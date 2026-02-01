// Copyright 2025-2026 Łukasz Furlepa
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal readonly struct ServerStorage
{
    private const string LockName = ".mblock";
    private const string ClientsDir = "clients";
    private const string ChannelsDir = "channels";
    private const string StreamsDir = "streams";

    private readonly AsyncMutex? _mutex;

    private ServerStorage(string? rootDir, AsyncMutex? mutex)
    {
        ServerRootDir = rootDir;
        _mutex = mutex;
    }

    internal readonly string? ServerRootDir;

    [Pure]
    internal static ServerStorage Create(string? rootPath)
    {
        return ! string.IsNullOrWhiteSpace( rootPath )
            ? new ServerStorage( Path.GetFullPath( rootPath ), new AsyncMutex() )
            : new ServerStorage( null, null );
    }

    [Pure]
    internal Client CreateForClient(int id, bool isEphemeral)
    {
        return ServerRootDir is not null && ! isEphemeral
            ? new Client( Path.Combine( ServerRootDir, ClientsDir, Storage.ClientMetadata.GetDirName( id ) ), new AsyncMutex() )
            : new Client( null, null );
    }

    [Pure]
    internal Stream CreateForStream()
    {
        return new Stream( ServerRootDir is not null ? new AsyncMutex() : null );
    }

    [Pure]
    internal Channel CreateForChannel()
    {
        return new Channel( ServerRootDir is not null ? new AsyncMutex() : null );
    }

    internal async ValueTask<FileStream?> LockAsync(MessageBrokerServer server)
    {
        if ( ServerRootDir is null )
            return null;

        Assume.IsNotNull( _mutex );
        var root = new DirectoryInfo( ServerRootDir );
        var filePath = Path.Combine( root.FullName, LockName );

        using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
        {
            if ( server.State >= MessageBrokerServerState.Disposing )
                return null;

            root.Create();

            // TODO: tests
            // - attempt to create second server which uses the same directory (fails)
            return new FileStream(
                filePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 0,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose );
        }
    }

    internal async ValueTask SaveMetadataAsync(MessageBrokerServer server, ulong traceId)
    {
        if ( ServerRootDir is null )
            return;

        Assume.IsNotNull( _mutex );
        var root = new DirectoryInfo( ServerRootDir );
        var filePath = Path.Combine( root.FullName, Storage.ServerMetadata.FileName );

        using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
        {
            if ( server.State >= MessageBrokerServerState.Disposing )
                return;

            root.Create();
            root.CreateSubdirectory( ClientsDir );
            root.CreateSubdirectory( ChannelsDir );
            root.CreateSubdirectory( StreamsDir );

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                var metadata = new Storage.ServerMetadata( traceId );
                poolToken = server.MemoryPool.Rent( Storage.ServerMetadata.Length, clear: true, out var data );
                metadata.Serialize( data );
                await using var file = OpenWrite( filePath );
                await file.WriteAsync( data ).ConfigureAwait( false );
            }
            finally
            {
                var exc = poolToken.Return();
                if ( exc is not null && server.Logger.Error is { } error )
                    error.Emit( MessageBrokerServerErrorEvent.Create( server, traceId, exc ) );
            }
        }
    }

    internal async ValueTask<Storage.ServerMetadata?> LoadMetadataAsync(MessageBrokerServer server)
    {
        if ( ServerRootDir is null )
            return null;

        Assume.IsNotNull( _mutex );
        var root = new DirectoryInfo( ServerRootDir );
        var filePath = Path.Combine( root.FullName, Storage.ServerMetadata.FileName );

        using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
        {
            if ( server.State >= MessageBrokerServerState.Disposing )
                return null;

            root.Create();
            root.CreateSubdirectory( ClientsDir );
            root.CreateSubdirectory( ChannelsDir );
            root.CreateSubdirectory( StreamsDir );

            var poolToken = MemoryPoolToken<byte>.Empty;
            try
            {
                // TODO: tests
                // - first storage
                // - existing storage reload (check trace id persistence)
                // - existing storage reload with invalid file length
                // - existing storage reload with invalid file header

                Storage.ServerMetadata metadata;
                poolToken = server.MemoryPool.Rent( Storage.ServerMetadata.Length, clear: true, out var data );
                await using var file = OpenReadWrite( filePath );
                var fileLength = file.Length;

                if ( fileLength == 0 )
                {
                    metadata = new Storage.ServerMetadata( 0 );
                    metadata.Serialize( data );
                    await file.WriteAsync( data ).ConfigureAwait( false );
                }
                else
                {
                    var context = new Storage.Context( server, filePath );
                    context.AssertFileLength( Storage.ServerMetadata.Length, fileLength );
                    await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                    metadata = Storage.ServerMetadata.Parse( context, data );
                }

                return metadata;
            }
            finally
            {
                poolToken.Return()?.Rethrow();
            }
        }
    }

    internal async IAsyncEnumerable<KeyValuePair<int, Storage.ChannelMetadata>> LoadChannelsAsync(MessageBrokerServer server, ulong traceId)
    {
        if ( ServerRootDir is null )
            yield break;

        Assume.IsNotNull( _mutex );
        var dir = Path.Combine( ServerRootDir, ChannelsDir );

        using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
        {
            if ( server.State >= MessageBrokerServerState.Disposing )
                yield break;

            var filePaths = Directory.EnumerateFiles(
                dir,
                $"{Storage.ChannelMetadata.FilePrefix}*.{Storage.ChannelMetadata.FileExtension}",
                SearchOption.TopDirectoryOnly );

            foreach ( var filePath in filePaths )
            {
                // TODO: tests
                // - id parse failure
                // - invalid file length
                // - invalid file header
                // - name with 0 length
                var context = new Storage.Context( server, filePath );
                var id = context.ParseId( prefixLength: Storage.ChannelMetadata.FilePrefix.Length );

                await using var file = OpenRead( context.Path );
                var fileLength = file.Length;
                context.AssertFileLength( Storage.ChannelMetadata.MinLength, Defaults.Memory.DefaultNetworkPacketLength, fileLength );

                Storage.ChannelMetadata metadata;
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    poolToken = server.MemoryPool.Rent( ( int )fileLength, clear: true, out var data );
                    await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                    metadata = Storage.ChannelMetadata.Parse( context, data );
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && server.Logger.Error is { } error )
                        error.Emit( MessageBrokerServerErrorEvent.Create( server, traceId, exc ) );
                }

                yield return KeyValuePair.Create( id, metadata );
            }
        }
    }

    internal async IAsyncEnumerable<KeyValuePair<int, Storage.StreamMetadata>> LoadStreamsAsync(MessageBrokerServer server, ulong traceId)
    {
        if ( ServerRootDir is null )
            yield break;

        Assume.IsNotNull( _mutex );
        var dir = Path.Combine( ServerRootDir, StreamsDir );

        using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
        {
            if ( server.State >= MessageBrokerServerState.Disposing )
                yield break;

            var directories = Directory.EnumerateDirectories( dir, $"{Storage.StreamMetadata.DirPrefix}*", SearchOption.TopDirectoryOnly );
            foreach ( var directory in directories )
            {
                // TODO: tests
                // - id parse failure
                // - lack of meta file
                // - invalid file length
                // - invalid file header
                // - name with 0 length
                var context = new Storage.Context( server, directory );
                var id = context.ParseId( prefixLength: Storage.StreamMetadata.DirPrefix.Length );

                context = context.WithSubPath( Storage.StreamMetadata.FileName );
                context.AssertFileExistence();

                await using var file = OpenRead( context.Path );
                var fileLength = file.Length;
                context.AssertFileLength( Storage.StreamMetadata.MinLength, Defaults.Memory.DefaultNetworkPacketLength, fileLength );

                Storage.StreamMetadata metadata;
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    poolToken = server.MemoryPool.Rent( ( int )fileLength, clear: true, out var data );
                    await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                    metadata = Storage.StreamMetadata.Parse( context, data );
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && server.Logger.Error is { } error )
                        error.Emit( MessageBrokerServerErrorEvent.Create( server, traceId, exc ) );
                }

                yield return KeyValuePair.Create( id, metadata );
            }
        }
    }

    internal async IAsyncEnumerable<KeyValuePair<int, Storage.ClientMetadata>> LoadClientsAsync(MessageBrokerServer server, ulong traceId)
    {
        if ( ServerRootDir is null )
            yield break;

        Assume.IsNotNull( _mutex );
        var dir = Path.Combine( ServerRootDir, ClientsDir );

        using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
        {
            if ( server.State >= MessageBrokerServerState.Disposing )
                yield break;

            var directories = Directory.EnumerateDirectories( dir, $"{Storage.ClientMetadata.DirPrefix}*", SearchOption.TopDirectoryOnly );
            foreach ( var directory in directories )
            {
                // TODO: tests
                // - id parse failure
                // - lack of meta file
                // - invalid file length
                // - invalid file header
                // - name with 0 length
                var context = new Storage.Context( server, directory );
                var id = context.ParseId( prefixLength: Storage.ClientMetadata.DirPrefix.Length );

                context = context.WithSubPath( Storage.ClientMetadata.FileName );
                context.AssertFileExistence();

                await using var file = OpenRead( context.Path );
                var fileLength = file.Length;
                context.AssertFileLength( Storage.ClientMetadata.MinLength, Defaults.Memory.DefaultNetworkPacketLength, fileLength );

                Storage.ClientMetadata metadata;
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    poolToken = server.MemoryPool.Rent( ( int )fileLength, clear: true, out var data );
                    await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                    metadata = Storage.ClientMetadata.Parse( context, data );
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && server.Logger.Error is { } error )
                        error.Emit( MessageBrokerServerErrorEvent.Create( server, traceId, exc ) );
                }

                yield return KeyValuePair.Create( id, metadata );
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private string GetChannelMetafilePath(int id)
    {
        Assume.IsNotNull( ServerRootDir );
        return Path.Combine( ServerRootDir, ChannelsDir, Storage.ChannelMetadata.GetFileName( id ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private string GetStreamRootDir(int id)
    {
        Assume.IsNotNull( ServerRootDir );
        return Path.Combine( ServerRootDir, StreamsDir, Storage.StreamMetadata.GetDirName( id ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static FileStream OpenWrite(string path, int bufferSize = 0)
    {
        return new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static FileStream OpenRead(string path, int bufferSize = 0)
    {
        return new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None,
            bufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static FileStream OpenReadWrite(string path, int bufferSize = 0)
    {
        return new FileStream(
            path,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan );
    }

    internal readonly struct Client
    {
        private const string PublishersDir = "publishers";
        private const string ListenersDir = "listeners";
        private const string QueuesDir = "queues";

        private readonly AsyncMutex? _mutex;

        internal Client(string? rootDir, AsyncMutex? mutex)
        {
            ClientRootDir = rootDir;
            _mutex = mutex;
        }

        internal readonly string? ClientRootDir;

        [Pure]
        internal Queue CreateForQueue()
        {
            return new Queue( ClientRootDir is not null ? new AsyncMutex() : null );
        }

        internal async ValueTask SaveMetadataAsync(MessageBrokerRemoteClient client, ulong traceId, bool skipDisposed = false)
        {
            if ( ClientRootDir is null )
                return;

            Assume.IsNotNull( _mutex );
            var root = new DirectoryInfo( ClientRootDir );
            var filePath = Path.Combine( root.FullName, Storage.ClientMetadata.FileName );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( skipDisposed && client.State >= MessageBrokerRemoteClientState.Disposing )
                    return;

                root.Create();
                root.CreateSubdirectory( PublishersDir );
                root.CreateSubdirectory( ListenersDir );
                root.CreateSubdirectory( QueuesDir );

                var name = TextEncoding.Prepare( client.Name ).GetValueOrThrow();
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    var metadata = new Storage.ClientMetadata( traceId, client.ClearBuffers, name );
                    poolToken = client.MemoryPool.Rent( metadata.Length, metadata.ClearBuffers, out var data );
                    metadata.Serialize( data );
                    await using var file = OpenWrite( filePath );
                    await file.WriteAsync( data ).ConfigureAwait( false );
                }
                finally
                {
                    poolToken.Return( client, traceId );
                }
            }
        }

        internal async ValueTask SaveMetadataAsync(MessageBrokerChannelPublisherBinding publisher, ulong traceId)
        {
            if ( ClientRootDir is null || publisher.IsEphemeral )
                return;

            Assume.IsNotNull( _mutex );
            var client = publisher.Client;
            var filePath = GetPublisherMetafilePath( publisher.Channel.Id );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( publisher.State >= MessageBrokerChannelPublisherBindingState.Disposing )
                    return;

                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    var metadata = new Storage.PublisherMetadata( publisher.Stream.Id );
                    poolToken = client.MemoryPool.Rent( Storage.PublisherMetadata.Length, client.ClearBuffers, out var data );
                    metadata.Serialize( data );
                    await using var file = OpenWrite( filePath );
                    await file.WriteAsync( data ).ConfigureAwait( false );
                }
                finally
                {
                    poolToken.Return( client, traceId );
                }
            }
        }

        internal async ValueTask DeleteAsync()
        {
            if ( ClientRootDir is null )
                return;

            Assume.IsNotNull( _mutex );
            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( Directory.Exists( ClientRootDir ) )
                    Directory.Delete( ClientRootDir, recursive: true );
            }
        }

        internal async ValueTask DeleteAsync(MessageBrokerChannelPublisherBinding publisher)
        {
            // TODO: tests
            // - unbind non-ephemeral publisher

            if ( ClientRootDir is null )
                return;

            Assume.IsNotNull( _mutex );
            var filePath = GetPublisherMetafilePath( publisher.Channel.Id );
            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( File.Exists( filePath ) )
                    File.Delete( filePath );
            }
        }

        internal async ValueTask SaveMetadataAsync(MessageBrokerChannelListenerBinding listener, ulong traceId)
        {
            if ( ClientRootDir is null || listener.IsEphemeral )
                return;

            Assume.IsNotNull( _mutex );
            var client = listener.Client;
            var filePath = GetListenerMetafilePath( listener.Channel.Id );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( listener.State >= MessageBrokerChannelListenerBindingState.Disposing )
                    return;

                var filter = TextEncoding.Prepare( listener.FilterExpression ?? string.Empty ).GetValueOrThrow();
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    var metadata = new Storage.ListenerMetadata(
                        listener.Queue.Id,
                        unchecked( ( short )listener.PrefetchHint ),
                        listener.MaxRetries,
                        listener.RetryDelay,
                        listener.MaxRedeliveries,
                        listener.MinAckTimeout,
                        listener.DeadLetterCapacityHint,
                        listener.MinDeadLetterRetention,
                        filter );

                    poolToken = client.MemoryPool.Rent( metadata.Length, client.ClearBuffers, out var data );
                    metadata.Serialize( data );
                    await using var file = OpenWrite( filePath );
                    await file.WriteAsync( data ).ConfigureAwait( false );
                }
                finally
                {
                    poolToken.Return( client, traceId );
                }
            }
        }

        internal async ValueTask DeleteAsync(MessageBrokerChannelListenerBinding listener)
        {
            // TODO: tests
            // - unbind non-ephemeral listener

            if ( ClientRootDir is null )
                return;

            Assume.IsNotNull( _mutex );
            var filePath = GetListenerMetafilePath( listener.Channel.Id );
            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( File.Exists( filePath ) )
                    File.Delete( filePath );
            }
        }

        internal async IAsyncEnumerable<KeyValuePair<int, Storage.QueueMetadata>> LoadQueuesAsync(
            MessageBrokerRemoteClient client,
            ulong traceId)
        {
            if ( ClientRootDir is null )
                yield break;

            Assume.IsNotNull( _mutex );
            var dir = Path.Combine( ClientRootDir, QueuesDir );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( client.State >= MessageBrokerRemoteClientState.Deactivating )
                    yield break;

                var directories = Directory.EnumerateDirectories(
                    dir,
                    $"{Storage.QueueMetadata.DirPrefix}*",
                    SearchOption.TopDirectoryOnly );

                foreach ( var directory in directories )
                {
                    // TODO: tests
                    // - id parse failure
                    // - lack of meta file
                    // - invalid file length
                    // - invalid file header
                    // - name with 0 length
                    var context = new Storage.Context( client.Server, directory );
                    var id = context.ParseId( prefixLength: Storage.QueueMetadata.DirPrefix.Length );

                    context = context.WithSubPath( Storage.QueueMetadata.FileName );
                    context.AssertFileExistence();

                    await using var file = OpenRead( context.Path );
                    var fileLength = file.Length;
                    context.AssertFileLength( Storage.QueueMetadata.MinLength, Defaults.Memory.DefaultNetworkPacketLength, fileLength );

                    Storage.QueueMetadata metadata;
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = client.Server.MemoryPool.Rent( ( int )fileLength, clear: true, out var data );
                        await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                        metadata = Storage.QueueMetadata.Parse( context, data );
                    }
                    finally
                    {
                        poolToken.Return( client, traceId );
                    }

                    yield return KeyValuePair.Create( id, metadata );
                }
            }
        }

        internal async IAsyncEnumerable<KeyValuePair<int, Storage.PublisherMetadata>> LoadPublishersAsync(
            MessageBrokerRemoteClient client,
            ulong traceId)
        {
            if ( ClientRootDir is null )
                yield break;

            Assume.IsNotNull( _mutex );
            var dir = Path.Combine( ClientRootDir, PublishersDir );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( client.State >= MessageBrokerRemoteClientState.Deactivating )
                    yield break;

                var filePaths = Directory.EnumerateFiles(
                    dir,
                    $"{Storage.PublisherMetadata.MetaFilePrefix}*.{Storage.PublisherMetadata.MetaFileExtension}",
                    SearchOption.TopDirectoryOnly );

                foreach ( var filePath in filePaths )
                {
                    // TODO: tests
                    // - id parse failure
                    // - invalid file length
                    // - invalid file header
                    var context = new Storage.Context( client.Server, filePath );
                    var channelId = context.ParseId( prefixLength: Storage.PublisherMetadata.MetaFilePrefix.Length );

                    await using var file = OpenRead( context.Path );
                    var fileLength = file.Length;
                    context.AssertFileLength( Storage.PublisherMetadata.Length, fileLength );

                    Storage.PublisherMetadata metadata;
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = client.Server.MemoryPool.Rent( ( int )fileLength, clear: true, out var data );
                        await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                        metadata = Storage.PublisherMetadata.Parse( context, data );
                    }
                    finally
                    {
                        poolToken.Return( client, traceId );
                    }

                    yield return KeyValuePair.Create( channelId, metadata );
                }
            }
        }

        internal async IAsyncEnumerable<KeyValuePair<int, Storage.ListenerMetadata>> LoadListenersAsync(
            MessageBrokerRemoteClient client,
            ulong traceId)
        {
            if ( ClientRootDir is null )
                yield break;

            Assume.IsNotNull( _mutex );
            var dir = Path.Combine( ClientRootDir, ListenersDir );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( client.State >= MessageBrokerRemoteClientState.Deactivating )
                    yield break;

                var filePaths = Directory.EnumerateFiles(
                    dir,
                    $"{Storage.ListenerMetadata.MetaFilePrefix}*.{Storage.ListenerMetadata.MetaFileExtension}",
                    SearchOption.TopDirectoryOnly );

                foreach ( var filePath in filePaths )
                {
                    // TODO: tests
                    // - id parse failure
                    // - invalid file length
                    // - invalid file header
                    var context = new Storage.Context( client.Server, filePath );
                    var channelId = context.ParseId( prefixLength: Storage.ListenerMetadata.MetaFilePrefix.Length );

                    await using var file = OpenRead( context.Path );
                    var fileLength = file.Length;
                    context.AssertFileLength(
                        Storage.ListenerMetadata.MinLength,
                        ( int )Defaults.Memory.MaxNetworkPacketLengthBounds.Max.Bytes,
                        fileLength );

                    Storage.ListenerMetadata metadata;
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = client.Server.MemoryPool.Rent( ( int )fileLength, clear: true, out var data );
                        await file.ReadExactlyAsync( data ).ConfigureAwait( false );
                        metadata = Storage.ListenerMetadata.Parse( context, data );
                    }
                    finally
                    {
                        poolToken.Return( client, traceId );
                    }

                    yield return KeyValuePair.Create( channelId, metadata );
                }
            }
        }

        // TODO: change from non-transient to transient will have to delete all files, but only after the initial load
        // so that all stored data is recovered one last time

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string GetPublisherMetafilePath(int channelId)
        {
            Assume.IsNotNull( ClientRootDir );
            return Path.Combine( ClientRootDir, PublishersDir, Storage.PublisherMetadata.GetFileName( channelId ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string GetListenerMetafilePath(int channelId)
        {
            Assume.IsNotNull( ClientRootDir );
            return Path.Combine( ClientRootDir, ListenersDir, Storage.ListenerMetadata.GetFileName( channelId ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string GetQueueRootDir(int id)
        {
            Assume.IsNotNull( ClientRootDir );
            return Path.Combine( ClientRootDir, QueuesDir, Storage.QueueMetadata.GetDirName( id ) );
        }

        internal readonly struct Queue
        {
            private readonly AsyncMutex? _mutex;

            internal Queue(AsyncMutex? mutex)
            {
                _mutex = mutex;
            }

            internal async ValueTask SaveMetadataAsync(MessageBrokerQueue queue, ulong traceId, bool skipDisposed = false)
            {
                if ( _mutex is null )
                    return;

                var root = new DirectoryInfo( queue.Client.Storage.GetQueueRootDir( queue.Id ) );
                var filePath = Path.Combine( root.FullName, Storage.QueueMetadata.FileName );

                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    if ( skipDisposed && queue.State >= MessageBrokerQueueState.Disposing )
                        return;

                    root.Create();

                    var name = TextEncoding.Prepare( queue.Name ).GetValueOrThrow();
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        var metadata = new Storage.QueueMetadata( traceId, name );
                        poolToken = queue.Client.MemoryPool.Rent( metadata.Length, queue.Client.ClearBuffers, out var data );
                        metadata.Serialize( data );
                        await using var file = OpenWrite( filePath );
                        await file.WriteAsync( data ).ConfigureAwait( false );
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Logger.Error is { } error )
                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                    }
                }
            }

            internal async ValueTask SaveAsync(MessageBrokerQueue queue, ListSlim<QueueMessage> messages, ulong traceId)
            {
                if ( _mutex is null || messages.IsEmpty )
                    return;

                var rootDir = queue.Client.Storage.GetQueueRootDir( queue.Id );
                var filePath = Path.Combine( rootDir, Storage.QueuePendingMessage.FileName );

                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        const int chunkSize = 1000;

                        await using var file = OpenWrite( filePath );
                        poolToken = queue.Client.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( chunkSize * Storage.QueuePendingMessage.Length ),
                            queue.Client.ClearBuffers,
                            out var data );

                        Storage.QueueMetadata.SerializeHeader( data );
                        await file.WriteAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );

                        var remaining = messages.AsMemory();
                        while ( remaining.Length > 0 )
                        {
                            remaining = remaining.GetNextChunk( chunkSize, out var chunk );
                            var written = Storage.QueuePendingMessage.Serialize( chunk, data );
                            await file.WriteAsync( data.Slice( 0, written ) ).ConfigureAwait( false );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Logger.Error is { } error )
                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                    }
                }
            }

            internal async ValueTask SaveAsync(MessageBrokerQueue queue, ListSlim<QueueMessageStore.UnackedEntry> entries, ulong traceId)
            {
                if ( _mutex is null || entries.IsEmpty )
                    return;

                var rootDir = queue.Client.Storage.GetQueueRootDir( queue.Id );
                var filePath = Path.Combine( rootDir, Storage.QueueUnackedMessage.FileName );

                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        const int chunkSize = 500;

                        await using var file = OpenWrite( filePath );
                        poolToken = queue.Client.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( chunkSize * Storage.QueueUnackedMessage.Length ),
                            queue.Client.ClearBuffers,
                            out var data );

                        Storage.QueueMetadata.SerializeHeader( data );
                        await file.WriteAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );

                        var remaining = entries.AsMemory();
                        while ( remaining.Length > 0 )
                        {
                            remaining = remaining.GetNextChunk( chunkSize, out var chunk );
                            var written = Storage.QueueUnackedMessage.Serialize( chunk, data );
                            await file.WriteAsync( data.Slice( 0, written ) ).ConfigureAwait( false );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Logger.Error is { } error )
                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                    }
                }
            }

            internal async ValueTask SaveAsync(MessageBrokerQueue queue, ListSlim<QueueRetryHeap.Entry> entries, ulong traceId)
            {
                if ( _mutex is null || entries.IsEmpty )
                    return;

                var rootDir = queue.Client.Storage.GetQueueRootDir( queue.Id );
                var filePath = Path.Combine( rootDir, Storage.QueueMessageRetry.FileName );

                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        const int chunkSize = 500;

                        await using var file = OpenWrite( filePath );
                        poolToken = queue.Client.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( chunkSize * Storage.QueueMessageRetry.Length ),
                            queue.Client.ClearBuffers,
                            out var data );

                        Storage.QueueMetadata.SerializeHeader( data );
                        await file.WriteAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );

                        var remaining = entries.AsMemory();
                        while ( remaining.Length > 0 )
                        {
                            remaining = remaining.GetNextChunk( chunkSize, out var chunk );
                            var written = Storage.QueueMessageRetry.Serialize( chunk, data );
                            await file.WriteAsync( data.Slice( 0, written ) ).ConfigureAwait( false );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Logger.Error is { } error )
                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                    }
                }
            }

            internal async ValueTask SaveAsync(MessageBrokerQueue queue, ListSlim<QueueMessageStore.DeadLetterEntry> entries, ulong traceId)
            {
                if ( _mutex is null || entries.IsEmpty )
                    return;

                var rootDir = queue.Client.Storage.GetQueueRootDir( queue.Id );
                var filePath = Path.Combine( rootDir, Storage.QueueDeadLetterMessage.FileName );

                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        const int chunkSize = 500;

                        await using var file = OpenWrite( filePath );
                        poolToken = queue.Client.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( chunkSize * Storage.QueueDeadLetterMessage.Length ),
                            queue.Client.ClearBuffers,
                            out var data );

                        Storage.QueueMetadata.SerializeHeader( data );
                        await file.WriteAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );

                        var remaining = entries.AsMemory();
                        while ( remaining.Length > 0 )
                        {
                            remaining = remaining.GetNextChunk( chunkSize, out var chunk );
                            var written = Storage.QueueDeadLetterMessage.Serialize( chunk, data );
                            await file.WriteAsync( data.Slice( 0, written ) ).ConfigureAwait( false );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Logger.Error is { } error )
                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                    }
                }
            }

            internal async ValueTask LoadMessagesAsync(MessageBrokerQueue queue, ulong serverTraceId)
            {
                if ( _mutex is null )
                    return;

                var rootDir = queue.Client.Storage.GetQueueRootDir( queue.Id );
                var filePath = Path.Combine( rootDir, Storage.QueuePendingMessage.FileName );

                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    if ( queue.State >= MessageBrokerQueueState.Deactivating )
                        return;

                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = queue.Client.Server.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( Storage.QueuePendingMessage.Length ),
                            clear: true,
                            out var data );

                        // TODO: tests
                        // - file doesn't exist
                        // - invalid file length
                        // - invalid file header
                        // - stream doesn't exist
                        // - stream message doesn't exist
                        // - stream message is marked as pending
                        // - listener for message's publisher's channel does not exist (not a failure, only emit)
                        var context = new Storage.Context( queue.Client.Server, filePath );
                        context.AssertFileExistence();

                        long read = Storage.QueueMetadata.Header.Length;
                        await using var file = OpenRead( filePath, bufferSize: ( int )MemorySize.BytesPerKilobyte );
                        var fileLength = file.Length;

                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );
                        Storage.QueueMetadata.AssertHeader( context, data );

                        while ( read < fileLength )
                        {
                            read += Storage.QueuePendingMessage.Length;
                            context.AssertFileMinLength( read, fileLength );
                            await file.ReadExactlyAsync( data.Slice( 0, Storage.QueuePendingMessage.Length ) ).ConfigureAwait( false );
                            var message = Storage.QueuePendingMessage.Parse( data );

                            if ( ! context.TryIncrementMessageRefCount( message.StreamId, message.StoreKey, out var streamMessage ) )
                                return;

                            var enqueued = false;
                            using ( queue.AcquireLock() )
                            {
                                if ( queue.IsDisposed )
                                    return;

                                if ( queue.ListenersByChannelId.TryGet( streamMessage.Publisher.Channel.Id, out var listener ) )
                                {
                                    queue.MessageStore.Enqueue( streamMessage.Publisher, listener, message.StoreKey );
                                    enqueued = true;
                                }
                            }

                            if ( ! enqueued )
                                context.DecrementFailedMessageRefCount( streamMessage, message.StoreKey, serverTraceId );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Client.Server.Logger.Error is { } error )
                            error.Emit( MessageBrokerServerErrorEvent.Create( queue.Client.Server, serverTraceId, exc ) );
                    }

                    if ( queue.State >= MessageBrokerQueueState.Deactivating )
                        return;

                    filePath = Path.Combine( rootDir, Storage.QueueUnackedMessage.FileName );
                    poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = queue.Client.Server.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( Storage.QueueUnackedMessage.Length ),
                            clear: true,
                            out var data );

                        // TODO: tests
                        // - file doesn't exist
                        // - invalid file length
                        // - invalid file header
                        // - retry & redelivery are negative
                        // - stream doesn't exist
                        // - stream message doesn't exist
                        // - stream message is marked as pending
                        // - listener for message's publisher's channel does not exist (not a failure, only emit)
                        //   this could be a single shared test for all message types
                        var context = new Storage.Context( queue.Client.Server, filePath );
                        context.AssertFileExistence();

                        long read = Storage.QueueMetadata.Header.Length;
                        await using var file = OpenRead( filePath, bufferSize: ( int )MemorySize.BytesPerKilobyte );
                        var fileLength = file.Length;

                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );
                        Storage.QueueMetadata.AssertHeader( context, data );

                        var expiresAt = queue.Client.GetTimestamp();
                        while ( read < fileLength )
                        {
                            read += Storage.QueueUnackedMessage.Length;
                            context.AssertFileMinLength( read, fileLength );
                            await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueUnackedMessage.Length ) ).ConfigureAwait( false );
                            var message = Storage.QueueUnackedMessage.Parse( context, data );

                            if ( ! context.TryIncrementMessageRefCount( message.StreamId, message.StoreKey, out var streamMessage ) )
                                return;

                            var enqueued = false;
                            using ( queue.AcquireLock() )
                            {
                                if ( queue.IsDisposed )
                                    return;

                                if ( queue.ListenersByChannelId.TryGet( streamMessage.Publisher.Channel.Id, out var listener ) )
                                {
                                    queue.MessageStore.Unacked.Add(
                                        new QueueMessageStore.UnackedEntry(
                                            new QueueMessage( streamMessage.Publisher, listener, message.StoreKey ),
                                            streamMessage.Id,
                                            message.Retry,
                                            message.Redelivery,
                                            expiresAt ) );

                                    listener.IncrementInactivePrefetchCounter();
                                    enqueued = true;
                                }
                            }

                            if ( ! enqueued )
                                context.DecrementFailedMessageRefCount( streamMessage, message.StoreKey, serverTraceId );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Client.Server.Logger.Error is { } error )
                            error.Emit( MessageBrokerServerErrorEvent.Create( queue.Client.Server, serverTraceId, exc ) );
                    }

                    if ( queue.State >= MessageBrokerQueueState.Deactivating )
                        return;

                    filePath = Path.Combine( rootDir, Storage.QueueMessageRetry.FileName );
                    poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = queue.Client.Server.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( Storage.QueueMessageRetry.Length ),
                            clear: true,
                            out var data );

                        // TODO: tests
                        // - file doesn't exist
                        // - invalid file length
                        // - invalid file header
                        // - retry & redelivery are negative
                        // - stream doesn't exist
                        // - stream message doesn't exist
                        // - stream message is marked as pending
                        // - listener for message's publisher's channel does not exist (not a failure, only emit)
                        var context = new Storage.Context( queue.Client.Server, filePath );
                        context.AssertFileExistence();

                        long read = Storage.QueueMetadata.Header.Length;
                        await using var file = OpenRead( filePath, bufferSize: ( int )MemorySize.BytesPerKilobyte );
                        var fileLength = file.Length;

                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );
                        Storage.QueueMetadata.AssertHeader( context, data );

                        while ( read < fileLength )
                        {
                            read += Storage.QueueMessageRetry.Length;
                            context.AssertFileMinLength( read, fileLength );

                            await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueMessageRetry.Length ) ).ConfigureAwait( false );
                            var message = Storage.QueueMessageRetry.Parse( context, data );

                            if ( ! context.TryIncrementMessageRefCount( message.StreamId, message.StoreKey, out var streamMessage ) )
                                return;

                            var enqueued = false;
                            using ( queue.AcquireLock() )
                            {
                                if ( queue.IsDisposed )
                                    return;

                                if ( queue.ListenersByChannelId.TryGet( streamMessage.Publisher.Channel.Id, out var listener ) )
                                {
                                    queue.MessageStore.Retries.Add(
                                        new QueueMessage( streamMessage.Publisher, listener, message.StoreKey ),
                                        message.Retry,
                                        message.Redelivery,
                                        message.SendAt );

                                    enqueued = true;
                                }
                            }

                            if ( ! enqueued )
                                context.DecrementFailedMessageRefCount( streamMessage, message.StoreKey, serverTraceId );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Client.Server.Logger.Error is { } error )
                            error.Emit( MessageBrokerServerErrorEvent.Create( queue.Client.Server, serverTraceId, exc ) );
                    }

                    if ( queue.State >= MessageBrokerQueueState.Deactivating )
                        return;

                    filePath = Path.Combine( rootDir, Storage.QueueDeadLetterMessage.FileName );
                    poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        poolToken = queue.Client.Server.MemoryPool.Rent(
                            Storage.QueueMetadata.Header.Length.Max( Storage.QueueDeadLetterMessage.Length ),
                            clear: true,
                            out var data );

                        // TODO: tests
                        // - file doesn't exist
                        // - invalid file length
                        // - invalid file header
                        // - retry & redelivery are negative
                        // - stream doesn't exist
                        // - stream message doesn't exist
                        // - stream message is marked as pending
                        // - listener for message's publisher's channel does not exist (not a failure, only emit)
                        var context = new Storage.Context( queue.Client.Server, filePath );
                        context.AssertFileExistence();

                        long read = Storage.QueueMetadata.Header.Length;
                        await using var file = OpenRead( filePath, bufferSize: ( int )MemorySize.BytesPerKilobyte );
                        var fileLength = file.Length;

                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueMetadata.Header.Length ) ).ConfigureAwait( false );
                        Storage.QueueMetadata.AssertHeader( context, data );

                        while ( read < fileLength )
                        {
                            read += Storage.QueueDeadLetterMessage.Length;
                            context.AssertFileMinLength( read, fileLength );
                            await file.ReadExactlyAsync( data.Slice( 0, Storage.QueueDeadLetterMessage.Length ) ).ConfigureAwait( false );
                            var message = Storage.QueueDeadLetterMessage.Parse( context, data );

                            if ( ! context.TryIncrementMessageRefCount( message.StreamId, message.StoreKey, out var streamMessage ) )
                                return;

                            var enqueued = false;
                            using ( queue.AcquireLock() )
                            {
                                if ( queue.IsDisposed )
                                    return;

                                if ( queue.ListenersByChannelId.TryGet( streamMessage.Publisher.Channel.Id, out var listener ) )
                                {
                                    queue.MessageStore.AddToDeadLetter(
                                        new QueueMessage( streamMessage.Publisher, listener, message.StoreKey ),
                                        message.Retry,
                                        message.Redelivery,
                                        message.ExpiresAt );

                                    listener.IncrementInactiveDeadLetterCounter();
                                    enqueued = true;
                                }
                            }

                            if ( ! enqueued )
                                context.DecrementFailedMessageRefCount( streamMessage, message.StoreKey, serverTraceId );
                        }
                    }
                    finally
                    {
                        var exc = poolToken.Return();
                        if ( exc is not null && queue.Client.Server.Logger.Error is { } error )
                            error.Emit( MessageBrokerServerErrorEvent.Create( queue.Client.Server, serverTraceId, exc ) );
                    }
                }
            }

            internal async ValueTask DeleteAsync(MessageBrokerQueue queue)
            {
                if ( _mutex is null )
                    return;

                // TODO: tests
                // unbind last non-ephemeral listener from the queue

                var rootDir = queue.Client.Storage.GetQueueRootDir( queue.Id );
                using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
                {
                    if ( Directory.Exists( rootDir ) )
                        Directory.Delete( rootDir, recursive: true );
                }
            }
        }
    }

    internal readonly struct Channel
    {
        private readonly AsyncMutex? _mutex;

        internal Channel(AsyncMutex? mutex)
        {
            _mutex = mutex;
        }

        internal async ValueTask SaveMetadataAsync(MessageBrokerChannel channel, ulong traceId, bool skipDisposed = false)
        {
            if ( _mutex is null )
                return;

            var filePath = channel.Server.Storage.GetChannelMetafilePath( channel.Id );
            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( skipDisposed && channel.State >= MessageBrokerChannelState.Disposing )
                    return;

                var name = TextEncoding.Prepare( channel.Name ).GetValueOrThrow();
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    var metadata = new Storage.ChannelMetadata( traceId, name );
                    poolToken = channel.Server.MemoryPool.Rent( metadata.Length, clear: true, out var data );
                    metadata.Serialize( data );
                    await using var file = OpenWrite( filePath );
                    await file.WriteAsync( data ).ConfigureAwait( false );
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && channel.Logger.Error is { } error )
                        error.Emit( MessageBrokerChannelErrorEvent.Create( channel, traceId, exc ) );
                }
            }
        }

        internal async ValueTask DeleteAsync(MessageBrokerChannel channel)
        {
            if ( _mutex is null )
                return;

            // TODO: tests
            // - unbind last non-ephemeral publisher from the channel

            Assume.IsNotNull( _mutex );
            var filePath = channel.Server.Storage.GetChannelMetafilePath( channel.Id );
            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( File.Exists( filePath ) )
                    File.Delete( filePath );
            }
        }
    }

    internal readonly struct Stream
    {
        private readonly AsyncMutex? _mutex;

        internal Stream(AsyncMutex? mutex)
        {
            _mutex = mutex;
        }

        internal async ValueTask SaveMetadataAsync(MessageBrokerStream stream, ulong traceId, bool skipDisposed = false)
        {
            if ( _mutex is null )
                return;

            var root = new DirectoryInfo( stream.Server.Storage.GetStreamRootDir( stream.Id ) );
            var fileName = Path.Combine( root.FullName, Storage.StreamMetadata.FileName );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( skipDisposed && stream.State >= MessageBrokerStreamState.Disposing )
                    return;

                root.Create();
                var name = TextEncoding.Prepare( stream.Name ).GetValueOrThrow();
                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    var metadata = new Storage.StreamMetadata( traceId, name );
                    poolToken = stream.Server.MemoryPool.Rent( metadata.Length, clear: true, out var data );
                    metadata.Serialize( data );
                    await using var file = OpenWrite( fileName );
                    await file.WriteAsync( data ).ConfigureAwait( false );
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && stream.Logger.Error is { } error )
                        error.Emit( MessageBrokerStreamErrorEvent.Create( stream, traceId, exc ) );
                }
            }
        }

        internal async ValueTask SaveAsync(
            MessageBrokerStream stream,
            NullableIndex nextPendingNodeId,
            ulong nextMessageId,
            ListSlim<KeyValuePair<int, StreamMessage>> messages,
            ListSlim<KeyValuePair<ulong, ReadOnlyMemory<byte>>> routings,
            ulong traceId)
        {
            if ( _mutex is null )
                return;

            var rootDir = stream.Server.Storage.GetStreamRootDir( stream.Id );
            var filePath = Path.Combine( rootDir, Storage.StreamMessageRangeHeader.FileName );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                var ephemeralClients = new Dictionary<Storage.EphemeralClientKey, Storage.EphemeralClientValue>();
                var header = new Storage.StreamMessageRangeHeader( nextMessageId, nextPendingNodeId, messages.Count, routings.Count );

                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    poolToken = stream.Server.MemoryPool.Rent(
                        Storage.StreamMessageRangeHeader.Length
                            .Max( Storage.StreamMessageHeader.Length )
                            .Max( Storage.StreamRoutingHeader.Length ),
                        clear: true,
                        out var data );

                    header.Serialize( data );

                    await using var file = OpenWrite( filePath, bufferSize: ( int )MemorySize.BytesPerMegabyte );
                    await file.WriteAsync( data.Slice( 0, Storage.StreamMessageRangeHeader.Length ) ).ConfigureAwait( false );
                    HashSet<ulong>? discardedMessageIds = null;

                    for ( var i = 0; i < messages.Count; ++i )
                    {
                        // TODO: tests
                        // - channel no longer has any listeners => message discarded, also test with discarded routing (one test)
                        // - message's publisher's client is ephemeral

                        var message = messages[i];
                        var messageHeader = Storage.StreamMessageHeader.Create( message.Key, message.Value, ephemeralClients );
                        messageHeader.Serialize( data );

                        await file.WriteAsync( data.Slice( 0, Storage.StreamMessageHeader.Length ) ).ConfigureAwait( false );
                        if ( messageHeader.IsDiscarded )
                        {
                            discardedMessageIds ??= [ ];
                            discardedMessageIds.Add( messageHeader.Id );
                            continue;
                        }

                        if ( messageHeader.DataLength > 0 )
                            await file.WriteAsync( message.Value.Data ).ConfigureAwait( false );
                    }

                    for ( var i = 0; i < routings.Count; ++i )
                    {
                        var routing = routings[i];
                        var length = discardedMessageIds is null || ! discardedMessageIds.Contains( routing.Key )
                            ? routing.Value.Length
                            : 0;

                        var routingHeader = new Storage.StreamRoutingHeader( routing.Key, length );
                        routingHeader.Serialize( data );

                        await file.WriteAsync( data.Slice( 0, Storage.StreamRoutingHeader.Length ) ).ConfigureAwait( false );
                        if ( length > 0 )
                            await file.WriteAsync( routing.Value ).ConfigureAwait( false );
                    }

                    foreach ( var (key, value) in ephemeralClients )
                    {
                        var length = value.Length;
                        if ( length > data.Length )
                            poolToken.IncreaseLength( length, out data );

                        value.Serialize( data, key.Id );
                        await file.WriteAsync( data.Slice( 0, length ) ).ConfigureAwait( false );
                    }
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && stream.Logger.Error is { } error )
                        error.Emit( MessageBrokerStreamErrorEvent.Create( stream, traceId, exc ) );
                }
            }
        }

        internal async ValueTask LoadMessagesAsync(MessageBrokerStream stream, ulong serverTraceId)
        {
            if ( _mutex is null )
                return;

            var rootDir = stream.Server.Storage.GetStreamRootDir( stream.Id );
            var filePath = Path.Combine( rootDir, Storage.StreamMessageRangeHeader.FileName );

            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( stream.State >= MessageBrokerStreamState.Disposing )
                    return;

                var poolToken = MemoryPoolToken<byte>.Empty;
                try
                {
                    poolToken = stream.Server.MemoryPool.Rent(
                        Storage.StreamMessageRangeHeader.Length
                            .Max( Storage.StreamMessageHeader.Length )
                            .Max( Storage.StreamRoutingHeader.Length )
                            .Max( Storage.EphemeralClientValue.Header.Length ),
                        clear: true,
                        out var data );

                    // TODO: tests
                    // - file doesn't exist
                    // - invalid file length
                    // - invalid file header
                    // - message & routing count are negative
                    // - message store key & data length are negative
                    // - message is discarded (channel id = 0) and data length is positive
                    // - message is discarded and data length is 0 => success, skip (single test with discarded routing as well)
                    // - message is related to ephemeral publisher from an existing client => success, create discarded publisher
                    // - message is related to publisher from ephemeral client (virtual id) => success, create ephemeral publisher
                    // - message channel & stream don't exist (single test)
                    // - message routing data length is negative
                    // - message for routing doesn't exist
                    // - ephemeral (virtual) publisher virtual id & sender id & name length are negative
                    // - ephemeral publisher name is empty
                    // - ephemeral publisher does not exist for related message
                    // - message store key is duplicated
                    // - next pending message key does not exist
                    // - next pending message is null => success
                    var context = new Storage.Context( stream.Server, filePath );
                    context.AssertFileExistence();

                    long read = Storage.StreamMessageRangeHeader.Length;
                    await using var file = OpenRead( filePath, bufferSize: ( int )MemorySize.BytesPerMegabyte );
                    var fileLength = file.Length;

                    context.AssertFileMinLength( read, fileLength );
                    await file.ReadExactlyAsync( data.Slice( 0, Storage.StreamMessageRangeHeader.Length ) ).ConfigureAwait( false );
                    var header = Storage.StreamMessageRangeHeader.Parse( context, data );

                    var messageBuilders = ListSlim<StreamMessage.Builder>.Create( header.MessageCount );
                    var ephemeralPublishersByClientChannelIdPair = new Dictionary<Pair<int, int>, MessageBrokerChannelPublisherBinding>();
                    var ephemeralPublishersByVirtualId = new Dictionary<int, EphemeralPublisher.Builder>();

                    for ( var i = 0; i < header.MessageCount; ++i )
                    {
                        read += Storage.StreamMessageHeader.Length;
                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.StreamMessageHeader.Length ) ).ConfigureAwait( false );
                        var messageHeader = Storage.StreamMessageHeader.Parse( context, data );

                        read += messageHeader.DataLength;
                        if ( messageHeader.IsDiscarded )
                        {
                            Assume.Equals( messageHeader.DataLength, 0 );
                            continue;
                        }

                        MessageBrokerChannel? channel;
                        MessageBrokerRemoteClient? client;
                        using ( stream.Server.AcquireLock() )
                        {
                            if ( stream.Server.IsDisposed )
                                return;

                            channel = stream.Server.ChannelCollection.TryGetByIdUnsafe( messageHeader.ChannelId );
                            client = messageHeader.IsSenderVirtual
                                ? null
                                : stream.Server.RemoteClientCollection.TryGetByIdUnsafe( messageHeader.SenderId );
                        }

                        var errors = Chain<string>.Empty;
                        if ( channel is null )
                            errors = errors.Extend( Resources.ChannelDoesNotExist( messageHeader.ChannelId ) );

                        if ( client is null && ! messageHeader.IsSenderVirtual )
                            errors = errors.Extend( Resources.ClientDoesNotExist( messageHeader.SenderId ) );

                        if ( errors.Count > 0 )
                            context.Throw( errors );

                        Assume.IsNotNull( channel );
                        MessageBrokerChannelPublisherBinding? publisher = null;
                        if ( client is not null )
                        {
                            using ( client.AcquireLock() )
                            {
                                if ( client.IsInactive )
                                    return;

                                publisher = client.PublishersByChannelId.TryGet( channel.Id, out var p ) ? p : null;
                            }

                            publisher ??= GetOrAddEphemeralPublisher( ephemeralPublishersByClientChannelIdPair, client, channel, stream );
                        }

                        context.AssertFileMinLength( read, fileLength );
                        var memoryPool = client?.GetMemoryPool( messageHeader.DataLength ) ?? stream.Server.MemoryPool;
                        var messagePoolToken = memoryPool.Rent(
                            messageHeader.DataLength,
                            clear: client?.ClearBuffers ?? true,
                            out var messageData );

                        await file.ReadExactlyAsync( messageData ).ConfigureAwait( false );
                        messageBuilders.Add(
                            new StreamMessage.Builder(
                                messageHeader.StoreKey,
                                publisher,
                                channel,
                                messageHeader.SenderId,
                                messageHeader.Id,
                                messageHeader.PushedAt,
                                messagePoolToken,
                                messageData ) );
                    }

                    var nextRoutedMessageIndex = 0;
                    for ( var i = 0; i < header.RoutingCount; ++i )
                    {
                        read += Storage.StreamRoutingHeader.Length;
                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.StreamRoutingHeader.Length ) ).ConfigureAwait( false );
                        var routingHeader = Storage.StreamRoutingHeader.Parse( context, data );
                        if ( routingHeader.IsDiscarded )
                            continue;

                        var messageFound = false;
                        MessageBrokerRemoteClient? client = null;
                        while ( nextRoutedMessageIndex < messageBuilders.Count )
                        {
                            var message = messageBuilders[nextRoutedMessageIndex++];
                            if ( message.Id == routingHeader.MessageId )
                            {
                                client = (message.Publisher as MessageBrokerChannelPublisherBinding)?.Client;
                                messageFound = true;
                                break;
                            }
                        }

                        if ( ! messageFound )
                            context.Throw( Resources.MessageDoesNotExist( routingHeader.MessageId ) );

                        read += routingHeader.DataLength;
                        context.AssertFileMinLength( read, fileLength );
                        var memoryPool = client?.GetMemoryPool( routingHeader.DataLength ) ?? stream.Server.MemoryPool;
                        var routingPoolToken = memoryPool.Rent(
                            routingHeader.DataLength,
                            clear: client?.ClearBuffers ?? true,
                            out var routingData );

                        await file.ReadExactlyAsync( routingData ).ConfigureAwait( false );
                        using ( stream.AcquireLock() )
                        {
                            if ( stream.IsDisposed )
                                return;

                            stream.MessageStore.LoadRouting( routingPoolToken, routingData, routingHeader.MessageId );
                        }
                    }

                    while ( fileLength > read )
                    {
                        read += Storage.EphemeralClientValue.Header.Length;
                        context.AssertFileMinLength( read, fileLength );
                        await file.ReadExactlyAsync( data.Slice( 0, Storage.EphemeralClientValue.Header.Length ) ).ConfigureAwait( false );
                        var clientHeader = Storage.EphemeralClientValue.Header.Parse( context, data );

                        read += clientHeader.NameLength;
                        context.AssertFileMinLength( read, fileLength );
                        if ( clientHeader.NameLength > data.Length )
                            poolToken.IncreaseLength( clientHeader.NameLength, out data );

                        await file.ReadExactlyAsync( data.Slice( 0, clientHeader.NameLength ) ).ConfigureAwait( false );
                        var name = TextEncoding.Parse( data.Slice( 0, clientHeader.NameLength ) ).GetValueOrThrow();
                        Assume.IsNotNull( name );

                        if ( ! Defaults.NameLengthBounds.Contains( name.Length ) )
                            context.Throw( Resources.InvalidClientNameLength( name.Length ) );

                        ephemeralPublishersByVirtualId[clientHeader.VirtualId] =
                            new EphemeralPublisher.Builder( clientHeader.SenderId, name, stream );
                    }

                    for ( var i = 0; i < messageBuilders.Count; ++i )
                    {
                        var builder = messageBuilders[i];
                        if ( ! builder.TryBuild( ephemeralPublishersByVirtualId, out var message ) )
                            context.Throw( Resources.EphemeralSenderDoesNotExist( -builder.SenderId ) );

                        using ( stream.AcquireLock() )
                        {
                            if ( stream.IsDisposed )
                                return;

                            if ( ! stream.MessageStore.LoadMessage( builder.StoreKey, message ) )
                                context.Throw( Resources.RecreatedMessageDuplicate( builder.StoreKey ) );
                        }
                    }

                    using ( stream.AcquireLock() )
                    {
                        if ( stream.IsDisposed )
                            return;

                        if ( ! stream.MessageStore.Initialize( header.NextMessageId, header.NextPendingNodeId ) )
                            context.Throw( Resources.NextPendingMessageDoesNotExist( header.NextPendingNodeId.Value ) );
                    }
                }
                finally
                {
                    var exc = poolToken.Return();
                    if ( exc is not null && stream.Server.Logger.Error is { } error )
                        error.Emit( MessageBrokerServerErrorEvent.Create( stream.Server, serverTraceId, exc ) );
                }
            }
        }

        internal async ValueTask DeleteAsync(MessageBrokerStream stream)
        {
            if ( _mutex is null )
                return;

            // TODO: tests
            // - unbind last non-ephemeral publisher from the stream

            var rootDir = stream.Server.Storage.GetStreamRootDir( stream.Id );
            using ( await _mutex.EnterAsync().ConfigureAwait( false ) )
            {
                if ( Directory.Exists( rootDir ) )
                    Directory.Delete( rootDir, recursive: true );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static MessageBrokerChannelPublisherBinding GetOrAddEphemeralPublisher(
            Dictionary<Pair<int, int>, MessageBrokerChannelPublisherBinding> publishersByClientChannelIdPair,
            MessageBrokerRemoteClient client,
            MessageBrokerChannel channel,
            MessageBrokerStream stream)
        {
            ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
                publishersByClientChannelIdPair,
                Pair.Create( client.Id, channel.Id ),
                out var exists )!;

            if ( ! exists )
                current = MessageBrokerChannelPublisherBinding.CreateDisposed( client, channel, stream );

            return current;
        }
    }
}
