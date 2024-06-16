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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Specifies the default value to notify the decorated event listener with when listener gets disposed,
/// but only when no events have been emitted.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public sealed class EventListenerDefaultIfEmptyDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly TEvent _defaultValue;

    /// <summary>
    /// Creates a new <see cref="EventListenerDefaultIfEmptyDecorator{TEvent}"/> instance.
    /// </summary>
    /// <param name="defaultValue">Default value.</param>
    public EventListenerDefaultIfEmptyDecorator(TEvent defaultValue)
    {
        _defaultValue = defaultValue;
    }

    /// <inheritdoc />
    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _defaultValue );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private bool _shouldEmitDefault;
        private TEvent? _defaultValue;

        internal EventListener(IEventListener<TEvent> next, TEvent defaultValue)
            : base( next )
        {
            _shouldEmitDefault = true;
            _defaultValue = defaultValue;
        }

        public override void React(TEvent @event)
        {
            _shouldEmitDefault = false;
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            if ( _shouldEmitDefault )
                Next.React( _defaultValue! );

            _defaultValue = default;
            base.OnDispose( source );
        }
    }
}
