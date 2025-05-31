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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct WriterQueue
{
    private StackSlim<ManualResetValueTaskSource<bool>> _writerCache;
    private QueueSlim<ManualResetValueTaskSource<bool>> _pendingWriters;

    private WriterQueue(int capacity)
    {
        _writerCache = StackSlim<ManualResetValueTaskSource<bool>>.Create( capacity );
        _pendingWriters = QueueSlim<ManualResetValueTaskSource<bool>>.Create();
    }

    [Pure]
    internal static WriterQueue Create()
    {
        return new WriterQueue( 0 );
    }

    internal void Dispose()
    {
        foreach ( var source in _pendingWriters )
        {
            if ( source.Status == ValueTaskSourceStatus.Pending )
                source.SetResult( default );
        }

        _pendingWriters = QueueSlim<ManualResetValueTaskSource<bool>>.Create();
        _writerCache = StackSlim<ManualResetValueTaskSource<bool>>.Create();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ManualResetValueTaskSource<bool> AcquireSource()
    {
        if ( ! _writerCache.TryPop( out var result ) )
            result = new ManualResetValueTaskSource<bool>();

        if ( _pendingWriters.IsEmpty )
            result.SetResult( true );

        _pendingWriters.Enqueue( result );
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Release(MessageBrokerRemoteClient client, ManualResetValueTaskSource<bool> source)
    {
        Assume.IsNotNull( source );
        Assume.Equals( source, _pendingWriters.First() );

        _pendingWriters.Dequeue();
        source.Reset();
        _writerCache.Push( source );

        client.EventScheduler.ResetWriteTimeout();
        if ( _pendingWriters.IsEmpty )
            return;

        var next = _pendingWriters.First();
        next.SetResult( true );
    }
}
