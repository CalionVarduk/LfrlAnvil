using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections;
using LfrlAnvil.Reactive.Queues.Internal;

namespace LfrlAnvil.Reactive.Queues
{
    public abstract class ReorderableEventQueueBase<TEvent, TPoint, TPointDelta>
        : IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta>
    {
        private readonly DictionaryHeap<TEvent, EnqueuedEvent<TEvent, TPoint, TPointDelta>> _events;

        protected ReorderableEventQueueBase(TPoint startPoint)
            : this( startPoint, EqualityComparer<TEvent>.Default, Comparer<TPoint>.Default ) { }

        protected ReorderableEventQueueBase(TPoint startPoint, IEqualityComparer<TEvent> eventComparer, IComparer<TPoint> comparer)
        {
            StartPoint = startPoint;
            CurrentPoint = startPoint;
            Comparer = comparer;
            _events = new DictionaryHeap<TEvent, EnqueuedEvent<TEvent, TPoint, TPointDelta>>(
                eventComparer,
                new EnqueuedEventComparer<TEvent, TPoint, TPointDelta>( Comparer ) );
        }

        public TPoint StartPoint { get; }
        public TPoint CurrentPoint { get; private set; }
        public IComparer<TPoint> Comparer { get; }
        public IEqualityComparer<TEvent> EventComparer => _events.KeyComparer;
        public int Count => _events.Count;

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? Dequeue()
        {
            if ( ! _events.TryPeek( out var next ) || Comparer.Compare( CurrentPoint, next.DequeuePoint ) < 0 )
                return null;

            var repeat = TryCreateEnqueuedEventRepeat( next );

            if ( repeat is not null )
                _events.Replace( next.Event, repeat.Value );
            else
                _events.Pop();

            return next;
        }

        [Pure]
        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetNext()
        {
            return _events.TryPeek( out var next ) ? next : null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta, int repetitions)
        {
            return EnqueueAt( @event, AddDelta( CurrentPoint, delta ), delta, repetitions );
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta, int repetitions)
        {
            var result = EnqueuedEvent<TEvent, TPoint, TPointDelta>.Create( @event, dequeuePoint, delta, repetitions );
            _events.AddOrReplace( @event, result );
            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta)
        {
            return EnqueueAt( @event, AddDelta( CurrentPoint, delta ) );
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint)
        {
            var result = EnqueuedEvent<TEvent, TPoint, TPointDelta>.CreateSingle( @event, dequeuePoint );
            _events.AddOrReplace( @event, result );
            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfinite(TEvent @event, TPointDelta delta)
        {
            return EnqueueInfiniteAt( @event, AddDelta( CurrentPoint, delta ), delta );
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfiniteAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta)
        {
            var result = EnqueuedEvent<TEvent, TPoint, TPointDelta>.CreateInfinite( @event, dequeuePoint, delta );
            _events.AddOrReplace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDequeuePoint(TEvent @event, TPoint dequeuePoint)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithDequeuePoint( dequeuePoint );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? DelayDequeuePoint(TEvent @event, TPointDelta delta)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithDequeuePoint( AddDelta( data.DequeuePoint, delta ) );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? AdvanceDequeuePoint(TEvent @event, TPointDelta delta)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithDequeuePoint( SubtractDelta( data.DequeuePoint, delta ) );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetRepetitions(TEvent @event, int repetitions)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithRepetitions( repetitions );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseRepetitions(TEvent @event, int count)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            if ( data.IsInfinite )
                return data;

            var result = data.WithRepetitions( data.Repetitions + count );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseRepetitions(TEvent @event, int count)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            if ( data.IsInfinite )
                return data;

            var result = data.WithRepetitions( data.Repetitions - count );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? MakeInfinite(TEvent @event)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.AsInfinite();
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDelta(TEvent @event, TPointDelta delta)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithDelta( delta );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseDelta(TEvent @event, TPointDelta delta)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithDelta( AddDelta( data.Delta, delta ) );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseDelta(TEvent @event, TPointDelta delta)
        {
            if ( ! _events.TryGetValue( @event, out var data ) )
                return null;

            var result = data.WithDelta( SubtractDelta( data.Delta, delta ) );
            _events.Replace( @event, result );
            return result;
        }

        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? Remove(TEvent @event)
        {
            return _events.TryRemove( @event, out var removed ) ? removed : null;
        }

        public void Move(TPointDelta delta)
        {
            CurrentPoint = AddDelta( CurrentPoint, delta );
        }

        public void Clear()
        {
            _events.Clear();
        }

        [Pure]
        public bool Contains(TEvent @event)
        {
            return _events.ContainsKey( @event );
        }

        [Pure]
        public EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetEvent(TEvent @event)
        {
            return _events.TryGetValue( @event, out var result ) ? result : null;
        }

        [Pure]
        public IEnumerable<EnqueuedEvent<TEvent, TPoint, TPointDelta>> GetEvents(TPoint endPoint)
        {
            foreach ( var e in _events )
            {
                if ( Comparer.Compare( e.DequeuePoint, endPoint ) > 0 )
                    continue;

                yield return e;

                var next = TryCreateEnqueuedEventRepeat( e );
                while ( next is not null && Comparer.Compare( next.Value.DequeuePoint, endPoint ) <= 0 )
                {
                    yield return next.Value;

                    next = TryCreateEnqueuedEventRepeat( next.Value );
                }
            }
        }

        [Pure]
        public IEnumerator<EnqueuedEvent<TEvent, TPoint, TPointDelta>> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        [Pure]
        protected abstract TPoint AddDelta(TPoint point, TPointDelta? delta);

        [Pure]
        protected abstract TPoint SubtractDelta(TPoint point, TPointDelta? delta);

        [Pure]
        protected abstract TPointDelta AddDelta(TPointDelta? a, TPointDelta b);

        [Pure]
        protected abstract TPointDelta SubtractDelta(TPointDelta? a, TPointDelta b);

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private EnqueuedEvent<TEvent, TPoint, TPointDelta>? TryCreateEnqueuedEventRepeat(EnqueuedEvent<TEvent, TPoint, TPointDelta> source)
        {
            if ( source.Repetitions == 1 )
                return null;

            var dequeuePoint = AddDelta( source.DequeuePoint, source.Delta );
            return source.Repeat( dequeuePoint );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
