using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Decorators;

public sealed class EventListenerAggregateDecorator<TEvent> : IEventListenerDecorator<TEvent, TEvent>
{
    private readonly Optional<TEvent> _seed;
    private readonly Func<TEvent, TEvent, TEvent> _func;

    public EventListenerAggregateDecorator(Func<TEvent, TEvent, TEvent> func)
    {
        _seed = Optional<TEvent>.Empty;
        _func = func;
    }

    public EventListenerAggregateDecorator(Func<TEvent, TEvent, TEvent> func, TEvent seed)
    {
        _seed = new Optional<TEvent>( seed );
        _func = func;
    }

    [Pure]
    public IEventListener<TEvent> Decorate(IEventListener<TEvent> listener, IEventSubscriber _)
    {
        return new EventListener( listener, _func, _seed );
    }

    private sealed class EventListener : DecoratedEventListener<TEvent, TEvent>
    {
        private readonly Func<TEvent, TEvent, TEvent> _func;
        private Optional<TEvent> _result;

        internal EventListener(IEventListener<TEvent> next, Func<TEvent, TEvent, TEvent> func, Optional<TEvent> seed)
            : base( next )
        {
            _result = seed;
            _func = func;
            _result.TryForward( Next );
        }

        public override void React(TEvent @event)
        {
            _result = _result.HasValue
                ? new Optional<TEvent>( _func( _result.Event!, @event ) )
                : new Optional<TEvent>( @event );

            Next.React( _result.Event! );
        }

        public override void OnDispose(DisposalSource source)
        {
            _result = Optional<TEvent>.Empty;
            base.OnDispose( source );
        }
    }
}
