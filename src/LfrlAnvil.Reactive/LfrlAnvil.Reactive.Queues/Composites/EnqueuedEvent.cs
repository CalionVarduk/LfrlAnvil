using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Queues.Composites;

public readonly struct EnqueuedEvent<TEvent, TPoint, TPointDelta>
{
    private EnqueuedEvent(TEvent @event, TPoint dequeuePoint, TPointDelta? delta, int repetitions)
    {
        Event = @event;
        DequeuePoint = dequeuePoint;
        Delta = delta;
        Repetitions = repetitions;
    }

    public TEvent Event { get; }
    public TPoint DequeuePoint { get; }
    public TPointDelta? Delta { get; }
    public int Repetitions { get; }
    public bool IsInfinite => Repetitions == 0;

    [Pure]
    public override string ToString()
    {
        var repeatText = IsInfinite ? "inf" : $"x{Repetitions}";
        var deltaText = Repetitions != 1 ? $"({Delta} dt) " : string.Empty;
        return $"{Event} at {DequeuePoint} {deltaText}[{repeatText}]";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnqueuedEvent<TEvent, TPoint, TPointDelta> CreateSingle(TEvent @event, TPoint dequeuePoint)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( @event, dequeuePoint, default, 1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnqueuedEvent<TEvent, TPoint, TPointDelta> Create(
        TEvent @event,
        TPoint dequeuePoint,
        TPointDelta delta,
        int repetitions)
    {
        Ensure.IsGreaterThan( repetitions, 0, nameof( repetitions ) );
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( @event, dequeuePoint, delta, repetitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static EnqueuedEvent<TEvent, TPoint, TPointDelta> CreateInfinite(TEvent @event, TPoint dequeuePoint, TPointDelta delta)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( @event, dequeuePoint, delta, 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> WithDequeuePoint(TPoint dequeuePoint)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, dequeuePoint, Delta, Repetitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> WithDelta(TPointDelta delta)
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, DequeuePoint, delta, Repetitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> WithRepetitions(int repetitions)
    {
        Ensure.IsGreaterThan( repetitions, 0, nameof( repetitions ) );
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, DequeuePoint, Delta, repetitions );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public EnqueuedEvent<TEvent, TPoint, TPointDelta> AsInfinite()
    {
        return new EnqueuedEvent<TEvent, TPoint, TPointDelta>( Event, DequeuePoint, Delta, 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal EnqueuedEvent<TEvent, TPoint, TPointDelta> Repeat(TPoint dequeuePoint)
    {
        return IsInfinite
            ? CreateInfinite( Event, dequeuePoint, Delta! )
            : Create( Event, dequeuePoint, Delta!, Repetitions - 1 );
    }
}