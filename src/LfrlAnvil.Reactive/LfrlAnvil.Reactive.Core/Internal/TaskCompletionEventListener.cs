// Copyright 2024 Łukasz Furlepa
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

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class TaskCompletionEventListener<TEvent> : EventListener<TEvent>
{
    private readonly TaskCompletionSource<TEvent?> _completionSource;
    private readonly LazyDisposable<CancellationTokenRegistration> _cancellationTokenRegistration;
    private TEvent? _value;
    private bool _cancelled;

    internal TaskCompletionEventListener(
        TaskCompletionSource<TEvent?> completionSource,
        LazyDisposable<CancellationTokenRegistration> cancellationTokenRegistration)
    {
        _completionSource = completionSource;
        _cancellationTokenRegistration = cancellationTokenRegistration;
        _value = default;
        _cancelled = false;
    }

    public override void React(TEvent @event)
    {
        _value = @event;
    }

    public override void OnDispose(DisposalSource source)
    {
        _cancellationTokenRegistration.Dispose();

        var lastValue = _value;
        _value = default;

        if ( _cancelled )
        {
            _completionSource.SetCanceled();
            return;
        }

        _completionSource.SetResult( lastValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsCancelled()
    {
        _cancelled = true;
    }
}
