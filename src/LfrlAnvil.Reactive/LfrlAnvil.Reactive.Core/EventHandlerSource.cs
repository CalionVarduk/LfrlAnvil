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
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a generic disposable event source that can be listened to created from an <see cref="EventHandler{TEventArgs}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class EventHandlerSource<TEvent> : EventSource<WithSender<TEvent>>
{
    internal Action<EventHandler<TEvent>>? Teardown;

    /// <summary>
    /// Creates a new <see cref="EventHandlerSource{TEvent}"/> instance.
    /// </summary>
    /// <param name="setup">Delegate that handles initialization of this event source.</param>
    /// <param name="teardown">Delegate that handles disposal of this event source.</param>
    public EventHandlerSource(Action<EventHandler<TEvent>> setup, Action<EventHandler<TEvent>> teardown)
    {
        setup( Handle );
        Teardown = teardown;
    }

    internal EventHandlerSource()
    {
        Teardown = null;
    }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        Assume.IsNotNull( Teardown );
        base.OnDispose();
        Teardown( Handle );
        Teardown = null;
    }

    internal void Handle(object? sender, TEvent args)
    {
        NotifyListeners( new WithSender<TEvent>( sender, args ) );
    }
}
