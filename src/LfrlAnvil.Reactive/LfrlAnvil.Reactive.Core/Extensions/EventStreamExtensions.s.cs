using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Internal;
using LfrlAnvil.Reactive.Composites;
using LfrlAnvil.Reactive.Decorators;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive.Extensions;

public static class EventStreamExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Where<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
    {
        var decorator = new EventListenerWhereDecorator<TEvent>( predicate );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> WhereNotNull<TEvent>(this IEventStream<TEvent?> source)
        where TEvent : class
    {
        return source.Where( e => e is not null )!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> WhereNotNull<TEvent>(this IEventStream<TEvent?> source)
        where TEvent : struct
    {
        return source.Where( e => e.HasValue ).Select( e => e!.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> WhereNotNull<TEvent>(this IEventStream<TEvent?> source, IEqualityComparer<TEvent> comparer)
    {
        if ( ! Generic<TEvent>.IsReferenceType && ! Generic<TEvent>.IsNullableType )
            return source!;

        return source.Where( e => ! comparer.Equals( e!, default! ) )!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> Select<TSourceEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, TNextEvent> selector)
    {
        var decorator = new EventListenerSelectDecorator<TSourceEvent, TNextEvent>( selector );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> SelectMany<TSourceEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, IEnumerable<TNextEvent>> selector)
    {
        var decorator = new EventListenerSelectManyDecorator<TSourceEvent, TNextEvent>( selector );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<Pair<TSourceEvent, TInnerEvent>> Flatten<TSourceEvent, TInnerEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, IEnumerable<TInnerEvent>> selector)
    {
        return source.Flatten( selector, Pair.Create );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> Flatten<TSourceEvent, TInnerEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, IEnumerable<TInnerEvent>> selector,
        Func<TSourceEvent, TInnerEvent, TNextEvent> resultMapper)
    {
        return source.SelectMany( p => selector( p ).Select( c => resultMapper( p, c ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Flatten<TEvent>(this IEventStream<IEnumerable<TEvent>> source)
    {
        return source.SelectMany( x => x );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> Zip<TSourceEvent, TTargetEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        IEventStream<TTargetEvent> target,
        Func<TSourceEvent, TTargetEvent, TNextEvent> selector)
    {
        var decorator = new EventListenerZipDecorator<TSourceEvent, TTargetEvent, TNextEvent>( target, selector );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<(TSourceEvent First, TTargetEvent Second)> Zip<TSourceEvent, TTargetEvent>(
        this IEventStream<TSourceEvent> source,
        IEventStream<TTargetEvent> target)
    {
        return source.Zip( target, (a, b) => (a, b) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<EventGrouping<TKey, TEvent>> GroupBy<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector)
        where TKey : notnull
    {
        return source.GroupBy( keySelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<EventGrouping<TKey, TEvent>> GroupBy<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector,
        IEqualityComparer<TKey> equalityComparer)
        where TKey : notnull
    {
        var decorator = new EventListenerGroupByDecorator<TEvent, TKey>( keySelector, equalityComparer );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithIndex<TEvent>> WithIndex<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerWithIndexDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ForEach<TEvent>(this IEventStream<TEvent> source, Action<TEvent> action)
    {
        var decorator = new EventListenerForEachDecorator<TEvent>( action );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Ignore<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerIgnoreDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Aggregate<TEvent>(this IEventStream<TEvent> source, Func<TEvent, TEvent, TEvent> func)
    {
        var decorator = new EventListenerAggregateDecorator<TEvent>( func );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Aggregate<TEvent>(
        this IEventStream<TEvent> source,
        Func<TEvent, TEvent, TEvent> func,
        TEvent seed)
    {
        var decorator = new EventListenerAggregateDecorator<TEvent>( func, seed );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> First<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerFirstDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Last<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerLastDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Single<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerSingleDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DefaultIfEmpty<TEvent>(this IEventStream<TEvent> source, TEvent defaultValue)
    {
        var decorator = new EventListenerDefaultIfEmptyDecorator<TEvent>( defaultValue );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent?> DefaultIfEmpty<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerDefaultIfEmptyDecorator<TEvent?>( default );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ElementAt<TEvent>(this IEventStream<TEvent> source, int index)
    {
        var decorator = new EventListenerElementAtDecorator<TEvent>( index );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<ReadOnlyMemory<TEvent>> Buffer<TEvent>(this IEventStream<TEvent> source, int bufferLength)
    {
        var decorator = new EventListenerBufferDecorator<TEvent>( bufferLength );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<ReadOnlyMemory<TEvent>> BufferUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerBufferUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Distinct<TEvent>(this IEventStream<TEvent> source)
    {
        return source.DistinctBy( e => e );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Distinct<TEvent>(this IEventStream<TEvent> source, IEqualityComparer<TEvent> equalityComparer)
    {
        return source.DistinctBy( e => e, equalityComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctBy<TEvent, TKey>(this IEventStream<TEvent> source, Func<TEvent, TKey> keySelector)
    {
        return source.DistinctBy( keySelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctBy<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector,
        IEqualityComparer<TKey> equalityComparer)
    {
        var decorator = new EventListenerDistinctDecorator<TEvent, TKey>( keySelector, equalityComparer );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntilChanged<TEvent>(this IEventStream<TEvent> source)
    {
        return source.DistinctByUntilChanged( e => e );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntilChanged<TEvent>(
        this IEventStream<TEvent> source,
        IEqualityComparer<TEvent> equalityComparer)
    {
        return source.DistinctByUntilChanged( e => e, equalityComparer );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctByUntilChanged<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector)
    {
        return source.DistinctByUntilChanged( keySelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctByUntilChanged<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector,
        IEqualityComparer<TKey> equalityComparer)
    {
        var decorator = new EventListenerDistinctUntilChangedDecorator<TEvent, TKey>( keySelector, equalityComparer );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        return source.DistinctByUntil( e => e, target );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEqualityComparer<TEvent> equalityComparer,
        IEventStream<TTargetEvent> target)
    {
        return source.DistinctByUntil( e => e, equalityComparer, target );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctByUntil<TEvent, TKey, TTargetEvent>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector,
        IEventStream<TTargetEvent> target)
    {
        return source.DistinctByUntil( keySelector, EqualityComparer<TKey>.Default, target );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctByUntil<TEvent, TKey, TTargetEvent>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector,
        IEqualityComparer<TKey> equalityComparer,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerDistinctUntilDecorator<TEvent, TKey, TTargetEvent>( keySelector, equalityComparer, target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Skip<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerSkipDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SkipLast<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerSkipLastDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SkipUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerSkipUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SkipWhile<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
    {
        var decorator = new EventListenerSkipWhileDecorator<TEvent>( predicate );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Take<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerTakeDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> TakeLast<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerTakeLastDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> TakeUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerTakeUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> TakeWhile<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
    {
        var decorator = new EventListenerTakeWhileDecorator<TEvent>( predicate );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Prepend<TEvent>(this IEventStream<TEvent> source, IEnumerable<TEvent> values)
    {
        var decorator = new EventListenerPrependDecorator<TEvent>( values );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Prepend<TEvent>(this IEventStream<TEvent> source, params TEvent[] values)
    {
        return source.Prepend( values.AsEnumerable() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Append<TEvent>(this IEventStream<TEvent> source, IEnumerable<TEvent> values)
    {
        var decorator = new EventListenerAppendDecorator<TEvent>( values );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Append<TEvent>(this IEventStream<TEvent> source, params TEvent[] values)
    {
        return source.Append( values.AsEnumerable() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Lock<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerLockDecorator<TEvent>( null );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Lock<TEvent>(this IEventStream<TEvent> source, object sync)
    {
        var decorator = new EventListenerLockDecorator<TEvent>( sync );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TResult ShareLockWith<TEvent, TTargetEvent, TResult>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target,
        Func<IEventStream<TEvent>, IEventStream<TTargetEvent>, TResult> resultSelector)
    {
        return source.ShareLockWith( target, resultSelector, new object() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TResult ShareLockWith<TEvent, TTargetEvent, TResult>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target,
        Func<IEventStream<TEvent>, IEventStream<TTargetEvent>, TResult> resultSelector,
        object sync)
    {
        return resultSelector( source.Lock( sync ), target.Lock( sync ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> AuditUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerAuditUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DebounceUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerDebounceUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SampleWhen<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerSampleWhenDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ThrottleUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerThrottleUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> ContinueWith<TEvent, TNextEvent>(
        this IEventStream<TEvent> source,
        Func<TEvent, IEventStream<TNextEvent>> continuationFactory)
    {
        var decorator = new EventListenerContinueWithDecorator<TEvent, TNextEvent>( continuationFactory );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> MergeAll<TEvent>(
        this IEventStream<IEventStream<TEvent>> source,
        int maxConcurrency = int.MaxValue)
    {
        var decorator = new EventListenerMergeAllDecorator<TEvent>( maxConcurrency );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ConcatAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        return source.MergeAll( maxConcurrency: 1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SwitchAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        var decorator = new EventListenerSwitchAllDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ExhaustAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        var decorator = new EventListenerExhaustAllDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> LockAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        var decorator = new EventListenerLockAllDecorator<TEvent>( null );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> LockAll<TEvent>(this IEventStream<IEventStream<TEvent>> source, object sync)
    {
        var decorator = new EventListenerLockAllDecorator<TEvent>( sync );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> ShareLockWithAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        return source.ShareLockWithAll( new object() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> ShareLockWithAll<TEvent>(
        this IEventStream<IEventStream<TEvent>> source,
        object sync)
    {
        return source.Lock( sync ).LockAll( sync );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> UseSynchronizationContext<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerUseSynchronizationContextDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Catch<TEvent, TException>(this IEventStream<TEvent> source, Action<TException> onError)
        where TException : Exception
    {
        var decorator = new EventListenerCatchDecorator<TEvent, TException>( onError );
        return source.Decorate( decorator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventSource<TEvent> ToConcurrent<TEvent>(this EventSource<TEvent> source)
    {
        return new ConcurrentEventSource<TEvent, EventSource<TEvent>>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventPublisher<TEvent> ToConcurrent<TEvent>(this EventPublisher<TEvent> source)
    {
        return new ConcurrentEventPublisher<TEvent, EventPublisher<TEvent>>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IHistoryEventPublisher<TEvent> ToConcurrent<TEvent>(this HistoryEventPublisher<TEvent> source)
    {
        return new ConcurrentHistoryEventPublisher<TEvent, HistoryEventPublisher<TEvent>>( source );
    }

    public static Task<TEvent?> ToTask<TEvent>(this IEventStream<TEvent> source, CancellationToken cancellationToken)
    {
        var completionSource = new TaskCompletionSource<TEvent?>();
        if ( cancellationToken.IsCancellationRequested )
        {
            completionSource.SetCanceled();
            return completionSource.Task;
        }

        var cancellationTokenRegistration = new LazyDisposable<CancellationTokenRegistration>();
        var listener = new TaskCompletionEventListener<TEvent>( completionSource, cancellationTokenRegistration );
        var subscriber = source.Listen( listener );

        if ( ! subscriber.IsDisposed )
        {
            var actualCancellationTokenRegistration = cancellationToken.Register(
                () =>
                {
                    listener.MarkAsCancelled();
                    subscriber.Dispose();
                } );

            cancellationTokenRegistration.Assign( actualCancellationTokenRegistration );
        }

        return completionSource.Task;
    }
}
