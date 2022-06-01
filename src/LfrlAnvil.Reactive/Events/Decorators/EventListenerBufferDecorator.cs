using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Events.Decorators
{
    public sealed class EventListenerBufferDecorator<TEvent> : IEventListenerDecorator<TEvent, ReadOnlyMemory<TEvent>>
    {
        private readonly int _bufferLength;

        public EventListenerBufferDecorator(int bufferLength)
        {
            if ( bufferLength < 1 )
                throw new ArgumentOutOfRangeException( nameof( bufferLength ), Resources.MustBeGreaterThanZero( nameof( bufferLength ) ) );

            _bufferLength = bufferLength;
        }

        [Pure]
        public IEventListener<TEvent> Decorate(IEventListener<ReadOnlyMemory<TEvent>> listener, IEventSubscriber _)
        {
            return new EventListener( listener, _bufferLength );
        }

        private sealed class EventListener : DecoratedEventListener<TEvent, ReadOnlyMemory<TEvent>>
        {
            private int _count;
            private readonly TEvent[] _buffer;

            internal EventListener(IEventListener<ReadOnlyMemory<TEvent>> next, int bufferLength)
                : base( next )
            {
                _count = 0;
                _buffer = new TEvent[bufferLength];
            }

            public override void React(TEvent @event)
            {
                _buffer[_count] = @event;

                if ( ++_count < _buffer.Length )
                    return;

                Next.React( _buffer.AsMemory() );

                _count = 0;
                Array.Clear( _buffer, 0, _buffer.Length );
            }

            public override void OnDispose(DisposalSource source)
            {
                if ( _count > 0 )
                {
                    Next.React( _buffer.AsMemory( 0, _count ) );

                    _count = 0;
                    Array.Clear( _buffer, 0, _buffer.Length );
                }

                base.OnDispose( source );
            }
        }
    }
}
