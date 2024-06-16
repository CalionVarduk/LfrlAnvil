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

namespace LfrlAnvil.Reactive.Decorators;

/// <summary>
/// Catches exceptions thrown by decorated event listener reactions and emits them by invoking the provided delegate.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TException">Exception type.</typeparam>
public sealed class EventListenerCatchDecorator<TEvent, TException> : IEventListenerDecorator<TEvent, TEvent>
    where TException : Exception
{
    private readonly Action<TException> _onError;

    /// <summary>
    /// Creates a new <see cref="EventListenerCatchDecorator{TEvent,TException}"/> instance.
    /// </summary>
    /// <param name="onError">Delegate to invoke once an exception is thrown by the decorated event listener.</param>
    public EventListenerCatchDecorator(Action<TException> onError)
    {
        _onError = onError;
    }

    /// <inheritdoc />
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber subscriber)
    {
        return new EventListener( listener, _onError );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Action<TException> _onError;

        internal EventListener(IEventListener<TEvent> next, Action<TException> onError)
            : base( next )
        {
            _onError = onError;
        }

        public override void React(TEvent @event)
        {
            try
            {
                Next.React( @event );
            }
            catch ( TException exc )
            {
                _onError( exc );
            }
        }

        public override void OnDispose(DisposalSource source)
        {
            try
            {
                base.OnDispose( source );
            }
            catch ( TException exc )
            {
                _onError( exc );
            }
        }
    }
}
