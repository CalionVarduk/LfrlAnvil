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

using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class AsyncEnumerableEventListener<TEvent> : EventListener<TEvent>
{
    private readonly object _lock = new object();
    private readonly LazyDisposable<CancellationTokenRegistration> _cancellationTokenRegistration;
    private readonly ManualResetValueTaskSource<AsyncEnumerableEvent<TEvent>> _next;
    private QueueSlim<AsyncEnumerableEvent<TEvent>> _buffer;
    private CancellationToken? _cancellationSource;
    private readonly int _maxBufferSize;
    private readonly bool _discardLatest;
    private bool _discarded;

    internal AsyncEnumerableEventListener(
        LazyDisposable<CancellationTokenRegistration> cancellationTokenRegistration,
        int maxBufferSize,
        bool discardLatest)
    {
        Assume.IsGreaterThanOrEqualTo( maxBufferSize, 0 );
        _cancellationTokenRegistration = cancellationTokenRegistration;
        _next = new ManualResetValueTaskSource<AsyncEnumerableEvent<TEvent>>();
        _buffer = QueueSlim<AsyncEnumerableEvent<TEvent>>.Create();
        _maxBufferSize = maxBufferSize;
        _discardLatest = discardLatest;
    }

    public override void React(TEvent @event)
    {
        using ( ExclusiveLock.Enter( _lock ) )
        {
            if ( _discarded )
                return;

            var e = AsyncEnumerableEvent<TEvent>.Create( @event );
            if ( _buffer.IsEmpty && _next.TrySetResult( e ) )
                return;

            if ( _maxBufferSize == 0 )
                return;

            var bufferSize = _buffer.Count;
            if ( bufferSize >= _maxBufferSize )
            {
                if ( _discardLatest )
                    return;

                _buffer.Dequeue();
            }

            _buffer.Enqueue( e );
        }
    }

    public override void OnDispose(DisposalSource source)
    {
        _cancellationTokenRegistration.Dispose();

        using ( ExclusiveLock.Enter( _lock ) )
        {
            if ( _discarded )
                return;

            _discarded = true;
            var e = AsyncEnumerableEvent<TEvent>.CreateDisposal( source );
            if ( ! _buffer.IsEmpty || ! _next.TrySetResult( e ) )
                _buffer.Enqueue( e );
        }
    }

    [Pure]
    internal ValueTask<AsyncEnumerableEvent<TEvent>> GetTask()
    {
        return _next.GetTask();
    }

    internal void Discard()
    {
        using ( ExclusiveLock.Enter( _lock ) )
            _discarded = true;
    }

    internal void Cancel(CancellationToken cancellationToken)
    {
        using ( ExclusiveLock.Enter( _lock ) )
        {
            if ( _discarded )
                return;

            _discarded = true;
            _cancellationSource = cancellationToken;
            _next.TrySetCancelled( cancellationToken );
        }
    }

    internal void Reset()
    {
        using ( ExclusiveLock.Enter( _lock ) )
        {
            _next.Reset();
            if ( _cancellationSource is not null )
            {
                _buffer.Clear();
                _next.SetCancelled( _cancellationSource.Value );
            }
            else if ( _buffer.TryDequeue( out var e ) )
                _next.SetResult( e );
        }
    }
}
