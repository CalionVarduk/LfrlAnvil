using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections;
using LfrlAnvil.Reactive.Queues.Composites;
using LfrlAnvil.Reactive.Queues.Internal;

namespace LfrlAnvil.Reactive.Queues;

/// <inheritdoc />
public abstract class EventQueueBase<TEvent, TPoint, TPointDelta> : IMutableEventQueue<TEvent, TPoint, TPointDelta>
{
    private readonly Heap<EnqueuedEvent<TEvent, TPoint, TPointDelta>> _events;

    /// <summary>
    /// Creates a new empty <see cref="EventQueueBase{TEvent,TPoint,TPointDelta}"/> instance
    /// with <see cref="Comparer{T}.Default"/> point comparer.
    /// </summary>
    /// <param name="startPoint">Specifies the starting point of this queue.</param>
    protected EventQueueBase(TPoint startPoint)
        : this( startPoint, Comparer<TPoint>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="EventQueueBase{TEvent,TPoint,TPointDelta}"/> instance.
    /// </summary>
    /// <param name="startPoint">Specifies the starting point of this queue.</param>
    /// <param name="comparer">Queue point comparer.</param>
    protected EventQueueBase(TPoint startPoint, IComparer<TPoint> comparer)
    {
        StartPoint = startPoint;
        CurrentPoint = startPoint;
        Comparer = comparer;
        _events = new Heap<EnqueuedEvent<TEvent, TPoint, TPointDelta>>(
            new EnqueuedEventComparer<TEvent, TPoint, TPointDelta>( Comparer ) );
    }

    /// <inheritdoc />
    public TPoint StartPoint { get; }

    /// <inheritdoc />
    public TPoint CurrentPoint { get; private set; }

    /// <inheritdoc />
    public IComparer<TPoint> Comparer { get; }

    /// <inheritdoc />
    public int Count => _events.Count;

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Pure]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetNext()
    {
        return _events.TryPeek( out var next ) ? next : null;
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta, int repetitions)
    {
        return EnqueueAt( @event, AddDelta( CurrentPoint, delta ), delta, repetitions );
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta, int repetitions)
    {
        var result = EnqueuedEvent<TEvent, TPoint, TPointDelta>.Create( @event, dequeuePoint, delta, repetitions );
        _events.Add( result );
        return result;
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> Enqueue(TEvent @event, TPointDelta delta)
    {
        return EnqueueAt( @event, AddDelta( CurrentPoint, delta ) );
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueAt(TEvent @event, TPoint dequeuePoint)
    {
        var result = EnqueuedEvent<TEvent, TPoint, TPointDelta>.CreateSingle( @event, dequeuePoint );
        _events.Add( result );
        return result;
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfinite(TEvent @event, TPointDelta delta)
    {
        return EnqueueInfiniteAt( @event, AddDelta( CurrentPoint, delta ), delta );
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> EnqueueInfiniteAt(TEvent @event, TPoint dequeuePoint, TPointDelta delta)
    {
        var result = EnqueuedEvent<TEvent, TPoint, TPointDelta>.CreateInfinite( @event, dequeuePoint, delta );
        _events.Add( result );
        return result;
    }

    /// <inheritdoc />
    public void Move(TPointDelta delta)
    {
        CurrentPoint = AddDelta( CurrentPoint, delta );
    }

    /// <inheritdoc />
    public void Clear()
    {
        _events.Clear();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Pure]
    public IEnumerator<EnqueuedEvent<TEvent, TPoint, TPointDelta>> GetEnumerator()
    {
        return _events.GetEnumerator();
    }

    /// <summary>
    /// Adds <paramref name="delta"/> to the specified <paramref name="point"/> in order to create a new point.
    /// </summary>
    /// <param name="point">Original point.</param>
    /// <param name="delta">Point delta to add to <paramref name="point"/>.</param>
    /// <returns>New point.</returns>
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
