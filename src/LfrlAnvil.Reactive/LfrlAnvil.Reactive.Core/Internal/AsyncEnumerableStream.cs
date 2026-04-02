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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Reactive.Extensions;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class AsyncEnumerableStream<TEvent> : IAsyncEnumerable<AsyncEnumerableEvent<TEvent>>
{
    private readonly IEventStream<TEvent> _stream;
    private readonly int _maxBufferSize;
    private readonly bool _discardLatest;

    internal AsyncEnumerableStream(IEventStream<TEvent> stream, int maxBufferSize, bool discardLatest)
    {
        Assume.IsGreaterThanOrEqualTo( maxBufferSize, 0 );
        _maxBufferSize = maxBufferSize;
        _discardLatest = discardLatest;
        _stream = stream;
    }

    public IAsyncEnumerator<AsyncEnumerableEvent<TEvent>> GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cancellationTokenRegistration = new LazyDisposable<CancellationTokenRegistration>();
        var listener = new AsyncEnumerableEventListener<TEvent>( cancellationTokenRegistration, _maxBufferSize, _discardLatest );
        var subscriber = _stream.Listen( listener );

        if ( ! subscriber.IsDisposed )
        {
            var actualCancellationTokenRegistration = cancellationToken.UnsafeRegister(
                (_, ct) =>
                {
                    listener.Cancel( ct );
                    subscriber.Dispose();
                },
                null );

            cancellationTokenRegistration.Assign( actualCancellationTokenRegistration );
        }

        return new Enumerator( listener, subscriber );
    }

    private sealed class Enumerator : IAsyncEnumerator<AsyncEnumerableEvent<TEvent>>
    {
        private readonly AsyncEnumerableEventListener<TEvent> _listener;
        private readonly IEventSubscriber _subscriber;

        internal Enumerator(AsyncEnumerableEventListener<TEvent> listener, IEventSubscriber subscriber)
        {
            _listener = listener;
            _subscriber = subscriber;
        }

        public AsyncEnumerableEvent<TEvent> Current { get; private set; }

        public ValueTask DisposeAsync()
        {
            _listener.Discard();
            _subscriber.Dispose();
            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if ( Current.IsDisposal )
                return false;

            Current = await _listener.GetTask().ConfigureAwait( false );
            _listener.Reset();
            return true;
        }
    }
}
