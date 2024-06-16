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

using System;
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class EventSubscriber<TEvent> : IEventSubscriber
{
    private Action<EventSubscriber<TEvent>>? _disposer;
    private InterlockedBoolean _isDisposed;

    internal EventSubscriber(Action<EventSubscriber<TEvent>> disposer, IEventListener<TEvent> listener)
    {
        _disposer = disposer;
        Listener = listener;
        _isDisposed = new InterlockedBoolean( false );
    }

    internal IEventListener<TEvent> Listener { get; set; }
    public bool IsDisposed => _isDisposed.Value;

    public void Dispose()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        _disposer?.Invoke( this );
        _disposer = null;

        Listener.OnDispose( DisposalSource.Subscriber );
    }

    internal bool MarkAsDisposed()
    {
        if ( ! _isDisposed.WriteTrue() )
            return false;

        _disposer = null;
        return true;
    }
}
