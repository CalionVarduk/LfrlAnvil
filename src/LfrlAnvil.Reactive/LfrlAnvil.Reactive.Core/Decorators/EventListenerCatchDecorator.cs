using System;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerCatchDecorator<TEvent, TException> : IEventListenerDecorator<TEvent, TEvent>
    where TException : Exception
{
    private readonly Action<TException> _onError;

    public EventListenerCatchDecorator(Action<TException> onError)
    {
        _onError = onError;
    }

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
