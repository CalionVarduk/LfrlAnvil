using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections;
using LfrlAnvil.Reactive.Queues.Composites;
using LfrlAnvil.Reactive.Queues.Internal;

namespace LfrlAnvil.Reactive.Queues;

/// <inheritdoc cref="IMutableReorderableEventQueue{TEvent,TPoint,TPointDelta}" />
public abstract class ReorderableEventQueueBase<TEvent, TPoint, TPointDelta>
    : IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta>
    where TEvent : notnull
{
    private readonly DictionaryHeap<TEvent, EnqueuedEvent<TEvent, TPoint, TPointDelta>> _events;

    /// <summary>
    /// Creates a new empty <see cref="ReorderableEventQueueBase{TEvent,TPoint,TPointDelta}"/> instance
    /// with <see cref="Comparer{T}.Default"/> point comparer and <see cref="EqualityComparer{T}.Default"/> event comparer.
    /// </summary>
    /// <param name="startPoint">Specifies the starting point of this queue.</param>
    protected ReorderableEventQueueBase(TPoint startPoint)
        : this( startPoint, EqualityComparer<TEvent>.Default, Comparer<TPoint>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="ReorderableEventQueueBase{TEvent,TPoint,TPointDelta}"/> instance.
    /// </summary>
    /// <param name="startPoint">Specifies the starting point of this queue.</param>
    /// <param name="eventComparer">Event equality comparer.</param>
    /// <param name="comparer">Queue point comparer.</param>
    protected ReorderableEventQueueBase(TPoint startPoint, IEqualityComparer<TEvent> eventComparer, IComparer<TPoint> comparer)
    {
        StartPoint = startPoint;
        CurrentPoint = startPoint;
        Comparer = comparer;
        _events = new DictionaryHeap<TEvent, EnqueuedEvent<TEvent, TPoint, TPointDelta>>(
            eventComparer,
            new EnqueuedEventComparer<TEvent, TPoint, TPointDelta>( Comparer ) );
    }

    /// <inheritdoc />
    public TPoint StartPoint { get; }

    /// <inheritdoc />
    public TPoint CurrentPoint { get; private set; }

    /// <inheritdoc />
    public IComparer<TPoint> Comparer { get; }

    /// <inheritdoc />
    public IEqualityComparer<TEvent> EventComparer => _events.KeyComparer;

    /// <inheritdoc />
    public int Count => _events.Count;

    /// <inheritdoc />
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
        _events.AddOrReplace( @event, result );
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
        _events.AddOrReplace( @event, result );
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
        _events.AddOrReplace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDequeuePoint(TEvent @event, TPoint dequeuePoint)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithDequeuePoint( dequeuePoint );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? DelayDequeuePoint(TEvent @event, TPointDelta delta)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithDequeuePoint( AddDelta( data.DequeuePoint, delta ) );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? AdvanceDequeuePoint(TEvent @event, TPointDelta delta)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithDequeuePoint( SubtractDelta( data.DequeuePoint, delta ) );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetRepetitions(TEvent @event, int repetitions)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithRepetitions( repetitions );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? MakeInfinite(TEvent @event)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.AsInfinite();
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? SetDelta(TEvent @event, TPointDelta delta)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithDelta( delta );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? IncreaseDelta(TEvent @event, TPointDelta delta)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithDelta( Add( data.Delta, delta ) );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? DecreaseDelta(TEvent @event, TPointDelta delta)
    {
        if ( ! _events.TryGetValue( @event, out var data ) )
            return null;

        var result = data.WithDelta( Subtract( data.Delta, delta ) );
        _events.Replace( @event, result );
        return result;
    }

    /// <inheritdoc />
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? Remove(TEvent @event)
    {
        return _events.TryRemove( @event, out var removed ) ? removed : null;
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
    public bool Contains(TEvent @event)
    {
        return _events.ContainsKey( @event );
    }

    /// <inheritdoc />
    [Pure]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta>? GetEvent(TEvent @event)
    {
        return _events.TryGetValue( @event, out var result ) ? result : null;
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

    /// <summary>
    /// Subtracts <paramref name="delta"/> from the specified <paramref name="point"/> in order to create a new point.
    /// </summary>
    /// <param name="point">Original point.</param>
    /// <param name="delta">Point delta to subtract from <paramref name="point"/>.</param>
    /// <returns>New point.</returns>
    [Pure]
    protected abstract TPoint SubtractDelta(TPoint point, TPointDelta? delta);

    /// <summary>
    /// Adds <paramref name="a"/> and <paramref name="b"/> together in order to create a new point delta.
    /// </summary>
    /// <param name="a">First point delta.</param>
    /// <param name="b">Second point delta.</param>
    /// <returns>New point delta.</returns>
    [Pure]
    protected abstract TPointDelta Add(TPointDelta? a, TPointDelta b);

    /// <summary>
    /// Subtracts <paramref name="b"/> from <paramref name="a"/> in order to create a new point delta.
    /// </summary>
    /// <param name="a">First point delta.</param>
    /// <param name="b">Second point delta.</param>
    /// <returns>New point delta.</returns>
    [Pure]
    protected abstract TPointDelta Subtract(TPointDelta? a, TPointDelta b);

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
