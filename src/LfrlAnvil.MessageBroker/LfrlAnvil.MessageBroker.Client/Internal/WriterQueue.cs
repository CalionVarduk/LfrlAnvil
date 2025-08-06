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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal struct WriterQueue
{
    private StackSlim<ManualResetValueTaskSource<WriterSourceResult>> _writerCache;
    private QueueSlim<Entry> _pendingWriters;

    private WriterQueue(int capacity)
    {
        _writerCache = StackSlim<ManualResetValueTaskSource<WriterSourceResult>>.Create( capacity );
        _pendingWriters = QueueSlim<Entry>.Create();
    }

    [Pure]
    internal static WriterQueue Create()
    {
        return new WriterQueue( 0 );
    }

    internal void Dispose()
    {
        foreach ( ref readonly var entry in _pendingWriters )
        {
            if ( entry.Source.Status == ValueTaskSourceStatus.Pending )
                entry.Source.SetResult( new WriterSourceResult( WriterSourceResultStatus.Disposed ) );
        }

        _pendingWriters = QueueSlim<Entry>.Create();
        _writerCache = StackSlim<ManualResetValueTaskSource<WriterSourceResult>>.Create();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ManualResetValueTaskSource<WriterSourceResult> AcquireSource(ReadOnlyMemory<byte> data)
    {
        Assume.IsGreaterThan( data.Length, 0 );
        if ( ! _writerCache.TryPop( out var source ) )
            source = new ManualResetValueTaskSource<WriterSourceResult>();

        if ( _pendingWriters.IsEmpty )
            source.SetResult( new WriterSourceResult( WriterSourceResultStatus.Ready ) );

        _pendingWriters.Enqueue( new Entry( source, data ) );
        return source;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Release(MessageBrokerClient client, ManualResetValueTaskSource<WriterSourceResult> source)
    {
        Assume.False( _pendingWriters.IsEmpty );
        Assume.Equals( source, _pendingWriters.First().Source );

        _pendingWriters.Dequeue();
        source.Reset();
        _writerCache.Push( source );
        client.EventScheduler.ResetWriteTimeout();

        if ( _pendingWriters.IsEmpty )
            return;

        var next = _pendingWriters.First();
        next.Source.SetResult( new WriterSourceResult( WriterSourceResultStatus.Ready ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReleaseBatched(
        MessageBrokerClient client,
        ManualResetValueTaskSource<WriterSourceResult> source,
        int packetCount,
        ulong traceId)
    {
        Assume.IsInRange( packetCount, 2, _pendingWriters.Count );
        Assume.Equals( source, _pendingWriters.First().Source );

        _pendingWriters.Dequeue();
        source.Reset();
        _writerCache.Push( source );
        client.EventScheduler.ResetWriteTimeout();

        var next = _pendingWriters.First();
        next.Source.SetResult( new WriterSourceResult( WriterSourceResultStatus.Batched, packetCount - 1, traceId ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReleaseBatched(
        MessageBrokerClient client,
        ManualResetValueTaskSource<WriterSourceResult> source,
        WriterSourceResult result)
    {
        if ( result.RemainingPacketCount == 1 )
            Release( client, source );
        else
            ReleaseBatched( client, source, result.RemainingPacketCount, result.BatchTraceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int GetLargestAvailableBatchCount(MessageBrokerClient client, ref long batchLength)
    {
        var packetCount = 1;
        var slice = _pendingWriters.AsMemory().Slice( 1 );
        if ( ! GetLargestAvailableBatchCount( client, slice.First.Span, ref packetCount, ref batchLength ) )
            GetLargestAvailableBatchCount( client, slice.Second.Span, ref packetCount, ref batchLength );

        return packetCount;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void CopyToBatch(Memory<byte> target, int packetCount)
    {
        var slice = _pendingWriters.AsMemory().Slice( 0, packetCount );
        CopyToBatch( ref target, slice.First.Span );
        CopyToBatch( ref target, slice.Second.Span );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool GetLargestAvailableBatchCount(
        MessageBrokerClient client,
        ReadOnlySpan<Entry> span,
        ref int packetCount,
        ref long batchLength)
    {
        foreach ( ref readonly var entry in span )
        {
            var nextBatchLength = unchecked( batchLength + entry.Data.Length );
            if ( nextBatchLength > client.MaxNetworkBatchPacketBytes )
                return true;

            batchLength = nextBatchLength;
            if ( ++packetCount == client.MaxBatchPacketCount )
                return true;
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void CopyToBatch(ref Memory<byte> target, ReadOnlySpan<Entry> span)
    {
        foreach ( ref readonly var entry in span )
        {
            entry.Data.CopyTo( target );
            target = target.Slice( entry.Data.Length );
        }
    }

    private readonly struct Entry
    {
        internal Entry(ManualResetValueTaskSource<WriterSourceResult> source, ReadOnlyMemory<byte> data)
        {
            Source = source;
            Data = data;
        }

        internal readonly ManualResetValueTaskSource<WriterSourceResult> Source;
        internal readonly ReadOnlyMemory<byte> Data;
    }
}
