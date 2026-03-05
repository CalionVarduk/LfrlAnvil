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
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct WriterQueue
{
    private StackSlim<TaskSource> _writerCache;
    private QueueSlim<TaskSource> _pendingWriters;

    private WriterQueue(int capacity)
    {
        _writerCache = StackSlim<TaskSource>.Create( capacity );
        _pendingWriters = QueueSlim<TaskSource>.Create();
    }

    [Pure]
    internal static WriterQueue Create()
    {
        return new WriterQueue( 0 );
    }

    internal void Dispose(ref Chain<Exception> exceptions)
    {
        foreach ( ref readonly var source in _pendingWriters )
        {
            try
            {
                if ( source.Status == ValueTaskSourceStatus.Pending )
                    source.SetResult( new WriterSourceResult( WriterSourceResultStatus.Disposed ) );
            }
            catch ( Exception exc )
            {
                exceptions = exceptions.Extend( exc );
            }
        }

        try
        {
            _pendingWriters = QueueSlim<TaskSource>.Create();
            _writerCache = StackSlim<TaskSource>.Create();
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TaskSource AcquireSource()
    {
        if ( ! _writerCache.TryPop( out var source ) )
            source = new TaskSource();

        if ( _pendingWriters.IsEmpty )
            source.SetResult( new WriterSourceResult( WriterSourceResultStatus.Ready ) );

        _pendingWriters.Enqueue( source );
        return source;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal TaskSource AcquireSource(ReadOnlyMemory<byte> data, bool clearBuffer)
    {
        Assume.IsGreaterThan( data.Length, 0 );
        var source = AcquireSource();
        source.Activate( data, clearBuffer );
        return source;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Release(MessageBrokerRemoteClient client, TaskSource source)
    {
        Assume.False( _pendingWriters.IsEmpty );
        Assume.Equals( source, _pendingWriters.First() );

        _pendingWriters.Dequeue();
        source.Reset();
        _writerCache.Push( source );
        client.EventScheduler.ResetWriteTimeout();

        if ( _pendingWriters.IsEmpty )
            return;

        var next = _pendingWriters.First();
        next.SetResult( new WriterSourceResult( WriterSourceResultStatus.Ready ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReleaseBatched(MessageBrokerRemoteClient client, TaskSource source, int packetCount, ulong traceId)
    {
        Assume.IsInRange( packetCount, 2, _pendingWriters.Count );
        Assume.Equals( source, _pendingWriters.First() );

        _pendingWriters.Dequeue();
        source.Reset();
        _writerCache.Push( source );
        client.EventScheduler.ResetWriteTimeout();

        var next = _pendingWriters.First();
        next.SetResult( new WriterSourceResult( WriterSourceResultStatus.Batched, packetCount - 1, traceId ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReleaseBatched(MessageBrokerRemoteClient client, TaskSource source, WriterSourceResult result)
    {
        if ( result.RemainingPacketCount == 1 )
            Release( client, source );
        else
            ReleaseBatched( client, source, result.RemainingPacketCount, result.BatchTraceId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal int GetLargestAvailableBatchCount(MessageBrokerRemoteClient client, ref long batchLength, ref bool clearBuffer)
    {
        var packetCount = 1;
        var slice = _pendingWriters.AsMemory().Slice( 1 );
        if ( ! GetLargestAvailableBatchCount( client, slice.First.Span, ref packetCount, ref batchLength, ref clearBuffer ) )
            GetLargestAvailableBatchCount( client, slice.Second.Span, ref packetCount, ref batchLength, ref clearBuffer );

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
        MessageBrokerRemoteClient client,
        ReadOnlySpan<TaskSource> span,
        ref int packetCount,
        ref long batchLength,
        ref bool clearBuffer)
    {
        var maxBatchPacketCount = client.GetMaxBatchPacketCountOption();
        var maxNetworkBatchPacketBytes = client.GetMaxNetworkBatchPacketBytesOption();
        foreach ( ref readonly var source in span )
        {
            if ( ! source.IsActive )
                return true;

            var nextBatchLength = unchecked( batchLength + source.Data.Length );
            if ( nextBatchLength > maxNetworkBatchPacketBytes )
                return true;

            batchLength = nextBatchLength;
            clearBuffer |= source.ClearBuffer;
            if ( ++packetCount == maxBatchPacketCount )
                return true;
        }

        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void CopyToBatch(ref Memory<byte> target, ReadOnlySpan<TaskSource> span)
    {
        foreach ( ref readonly var source in span )
        {
            source.Data.CopyTo( target );
            target = target.Slice( source.Data.Length );
        }
    }

    internal sealed class TaskSource : IValueTaskSource<WriterSourceResult>
    {
        private ManualResetValueTaskSourceCore<WriterSourceResult> _core;

        internal TaskSource()
        {
            _core = new ManualResetValueTaskSourceCore<WriterSourceResult> { RunContinuationsAsynchronously = true };
            Data = ReadOnlyMemory<byte>.Empty;
            ClearBuffer = false;
        }

        internal ReadOnlyMemory<byte> Data { get; private set; }
        internal bool ClearBuffer { get; private set; }
        internal ValueTaskSourceStatus Status => _core.GetStatus( _core.Version );
        internal bool IsActive => Data.Length > 0;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Activate(ReadOnlyMemory<byte> data, bool clearBuffer)
        {
            Assume.False( IsActive );
            Assume.IsGreaterThan( data.Length, 0 );
            Data = data;
            ClearBuffer = clearBuffer;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetResult(WriterSourceResult result)
        {
            _core.SetResult( result );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Reset()
        {
            _core.Reset();
            Data = ReadOnlyMemory<byte>.Empty;
            ClearBuffer = false;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ValueTask<WriterSourceResult> GetTask()
        {
            return new ValueTask<WriterSourceResult>( this, _core.Version );
        }

        WriterSourceResult IValueTaskSource<WriterSourceResult>.GetResult(short token)
        {
            return _core.GetResult( token );
        }

        ValueTaskSourceStatus IValueTaskSource<WriterSourceResult>.GetStatus(short token)
        {
            return _core.GetStatus( token );
        }

        void IValueTaskSource<WriterSourceResult>.OnCompleted(
            Action<object?> continuation,
            object? state,
            short token,
            ValueTaskSourceOnCompletedFlags flags)
        {
            _core.OnCompleted( continuation, state, token, flags );
        }
    }
}
