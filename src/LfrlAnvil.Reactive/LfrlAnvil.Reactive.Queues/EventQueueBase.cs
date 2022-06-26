using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections;
using LfrlAnvil.Reactive.Queues.Composites;
using LfrlAnvil.Reactive.Queues.Internal;

namespace LfrlAnvil.Reactive.Queues;

public abstract class EventQueueBase<TEvent, TPoint, TPointDelta> : IMutableEventQueue<TEvent, TPoint, TPointDelta>
{
    private readonly Heap<EnqueuedEvent<TEvent, TPoint, TPointDelta>> _events;

    protected EventQueueBase(TPoint startPoint)
        : this( startPoint, Comparer<TPoint>.Default ) { }

    protected EventQueueBase(TPoint startPoint, IComparer<TPoint> comparer)
    {
        StartPoint = startPoint;
        CurrentPoint = startPoint;
        Comparer = comparer;
        _events = new Heap<EnqueuedEvent<TEvent, TPoint, TPointDelta>>(
            new EnqueuedEventComparer<TEvent, TPoint, TPointDelta>( Comparer ) );
    }

    public TPoint StartPoint { get; }
    public TPoint CurrentPoint { get; private set; }
    public IComparer<TPoint> Comparer { get; }
    public int Count => _events.Count;

    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? Dequeue()
    {
        if ( ! _events.TryPeek( out var next ) || Comparer.Compare( CurrentPoint, next.DequeuePoint ) < 0 )
            return null;

        var repeat = TryCreateEnqueuedEventRepeat( next );

        if ( repeat is not null )
            _events.Replace( repeat.Value );
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
        _events.Add( result );
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
        _events.Add( result );
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
        _events.Add( result );
        return result;
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
