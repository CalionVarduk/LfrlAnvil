// Copyright 2024-2026 Łukasz Furlepa
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
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class EventSubscriber<TEvent> : IEventSubscriber
{
    private readonly object _sync = new object();
    private EventSource<TEvent>? _owner;
    private InterlockedRef<IEventListener<TEvent>> _listener;

    internal EventSubscriber(EventSource<TEvent> owner, IEventListener<TEvent> listener)
    {
        _owner = owner;
        _listener = new InterlockedRef<IEventListener<TEvent>>( listener );
    }

    internal IEventListener<TEvent> Listener
    {
        get => _listener.Value;
        set => _listener.Write( value );
    }

    public bool IsDisposed
    {
        get
        {
            using ( AcquireLock() )
                return ! HasOwnerUnsafe();
        }
    }

    public void Dispose()
    {
        EventSource<TEvent> owner;
        using ( AcquireLock() )
        {
            if ( ! HasOwnerUnsafe() )
                return;

            owner = _owner!;
            RemoveOwnerUnsafe();
        }

        owner.RemoveSubscriber( this );
        Listener.OnDispose( DisposalSource.Subscriber );
    }

    internal bool MarkAsDisposed()
    {
        using ( AcquireLock() )
        {
            if ( ! HasOwnerUnsafe() )
                return false;

            RemoveOwnerUnsafe();
            return true;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool HasOwnerUnsafe()
    {
        return _owner is not null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveOwnerUnsafe()
    {
        _owner = null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }
}
