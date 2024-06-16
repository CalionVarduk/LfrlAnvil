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

/// <summary>
/// Contains <see cref="IEventStream{TEvent}"/> extension methods.
/// </summary>
public static class EventStreamExtensions
{
    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWhereDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="predicate">Predicate used for filtering events. Events that return <b>false</b> will be skipped.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Where<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
    {
        var decorator = new EventListenerWhereDecorator<TEvent>( predicate );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWhereDecorator{TEvent}"/> that filters out null events.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> WhereNotNull<TEvent>(this IEventStream<TEvent?> source)
        where TEvent : class
    {
        return source.Where( static e => e is not null )!;
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWhereDecorator{TEvent}"/> that filters out null events.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> WhereNotNull<TEvent>(this IEventStream<TEvent?> source)
        where TEvent : struct
    {
        return source.Where( static e => e.HasValue ).Select( static e => e!.Value );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWhereDecorator{TEvent}"/> that filters out null events.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="comparer">Custom equality comparer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> WhereNotNull<TEvent>(this IEventStream<TEvent?> source, IEqualityComparer<TEvent> comparer)
    {
        if ( typeof( TEvent ).IsValueType && ! Generic<TEvent>.IsNullableType )
            return source!;

        return source.Where( e => ! comparer.Equals( e, default ) )!;
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSelectDecorator{TSourceEvent,TNextEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="selector">Next event selector.</param>
    /// <typeparam name="TSourceEvent">Source event type.</typeparam>
    /// <typeparam name="TNextEvent">Next event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> Select<TSourceEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, TNextEvent> selector)
    {
        var decorator = new EventListenerSelectDecorator<TSourceEvent, TNextEvent>( selector );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSelectManyDecorator{TSourceEvent,TNextEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="selector">Next event collection selector.</param>
    /// <typeparam name="TSourceEvent">Source event type.</typeparam>
    /// <typeparam name="TNextEvent">Next event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> SelectMany<TSourceEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, IEnumerable<TNextEvent>> selector)
    {
        var decorator = new EventListenerSelectManyDecorator<TSourceEvent, TNextEvent>( selector );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSelectManyDecorator{TSourceEvent,TNextEvent}"/>
    /// that emits pairs of (source, inner) events.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="selector">Inner event collection selector.</param>
    /// <typeparam name="TSourceEvent">Source event type.</typeparam>
    /// <typeparam name="TInnerEvent">Inner event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<Pair<TSourceEvent, TInnerEvent>> Flatten<TSourceEvent, TInnerEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, IEnumerable<TInnerEvent>> selector)
    {
        return source.Flatten( selector, Pair.Create );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSelectManyDecorator{TSourceEvent,TNextEvent}"/>
    /// that emits pairs of (source, inner) events mapped to the desired event type.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="selector">Inner event collection selector.</param>
    /// <param name="resultMapper">Next event selector..</param>
    /// <typeparam name="TSourceEvent">Source event type.</typeparam>
    /// <typeparam name="TInnerEvent">Inner event type.</typeparam>
    /// <typeparam name="TNextEvent">Next event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> Flatten<TSourceEvent, TInnerEvent, TNextEvent>(
        this IEventStream<TSourceEvent> source,
        Func<TSourceEvent, IEnumerable<TInnerEvent>> selector,
        Func<TSourceEvent, TInnerEvent, TNextEvent> resultMapper)
    {
        return source.SelectMany( p => selector( p ).Select( c => resultMapper( p, c ) ) );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSelectManyDecorator{TSourceEvent,TNextEvent}"/>
    /// that emits all nested elements.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Flatten<TEvent>(this IEventStream<IEnumerable<TEvent>> source)
    {
        return source.SelectMany( static x => x );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerZipDecorator{TEvent,TTargetEvent,TNextEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream.</param>
    /// <param name="selector">Next event selector.</param>
    /// <typeparam name="TSourceEvent">Source event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <typeparam name="TNextEvent">Next event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
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

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerZipDecorator{TEvent,TTargetEvent,TNextEvent}"/>
    /// that emits pairs of (source, target) events.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream.</param>
    /// <typeparam name="TSourceEvent">Source event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<(TSourceEvent First, TTargetEvent Second)> Zip<TSourceEvent, TTargetEvent>(
        this IEventStream<TSourceEvent> source,
        IEventStream<TTargetEvent> target)
    {
        return source.Zip( target, static (a, b) => (a, b) );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerGroupByDecorator{TEvent,TKey}"/>
    /// with <see cref="EqualityComparer{T}.Default"/> event key equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event's key selector.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<EventGrouping<TKey, TEvent>> GroupBy<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector)
        where TKey : notnull
    {
        return source.GroupBy( keySelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerGroupByDecorator{TEvent,TKey}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event's key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
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

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerWithIndexDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<WithIndex<TEvent>> WithIndex<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerWithIndexDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerForEachDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="action">Delegate to invoke on each event.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ForEach<TEvent>(this IEventStream<TEvent> source, Action<TEvent> action)
    {
        var decorator = new EventListenerForEachDecorator<TEvent>( action );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerIgnoreDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Ignore<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerIgnoreDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerAggregateDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="func">Aggregator delegate.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Aggregate<TEvent>(this IEventStream<TEvent> source, Func<TEvent, TEvent, TEvent> func)
    {
        var decorator = new EventListenerAggregateDecorator<TEvent>( func );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerAggregateDecorator{TEvent}"/> with an initial event to publish immediately.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="func">Aggregator delegate.</param>
    /// <param name="seed">Initial event to publish immediately.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
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

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerFirstDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> First<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerFirstDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerLastDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Last<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerLastDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSingleDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Single<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerSingleDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDefaultIfEmptyDecorator{TEvent}"/> with a custom default value.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DefaultIfEmpty<TEvent>(this IEventStream<TEvent> source, TEvent defaultValue)
    {
        var decorator = new EventListenerDefaultIfEmptyDecorator<TEvent>( defaultValue );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDefaultIfEmptyDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent?> DefaultIfEmpty<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerDefaultIfEmptyDecorator<TEvent?>( default );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerElementAtDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="index">0-based position of the desired event.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ElementAt<TEvent>(this IEventStream<TEvent> source, int index)
    {
        var decorator = new EventListenerElementAtDecorator<TEvent>( index );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerBufferDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="bufferLength">Size of the underlying buffer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="bufferLength"/> is less than <b>1</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<ReadOnlyMemory<TEvent>> Buffer<TEvent>(this IEventStream<TEvent> source, int bufferLength)
    {
        var decorator = new EventListenerBufferDecorator<TEvent>( bufferLength );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerBufferUntilDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before emitting the underlying buffer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<ReadOnlyMemory<TEvent>> BufferUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerBufferUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctDecorator{TEvent,TKey}"/> where event is the key,
    /// with <see cref="EqualityComparer{T}.Default"/> event equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Distinct<TEvent>(this IEventStream<TEvent> source)
    {
        return source.DistinctBy( static e => e );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctDecorator{TEvent,TKey}"/> where event is the key.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Distinct<TEvent>(this IEventStream<TEvent> source, IEqualityComparer<TEvent> equalityComparer)
    {
        return source.DistinctBy( static e => e, equalityComparer );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctDecorator{TEvent,TKey}"/>
    /// with <see cref="EqualityComparer{T}.Default"/> key equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event key selector.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctBy<TEvent, TKey>(this IEventStream<TEvent> source, Func<TEvent, TKey> keySelector)
    {
        return source.DistinctBy( keySelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctDecorator{TEvent,TKey}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
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

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilChangedDecorator{TEvent,TKey}"/> where event is the key,
    /// with <see cref="EqualityComparer{T}.Default"/> event equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntilChanged<TEvent>(this IEventStream<TEvent> source)
    {
        return source.DistinctByUntilChanged( static e => e );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilChangedDecorator{TEvent,TKey}"/> where event is the key.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntilChanged<TEvent>(
        this IEventStream<TEvent> source,
        IEqualityComparer<TEvent> equalityComparer)
    {
        return source.DistinctByUntilChanged( static e => e, equalityComparer );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilChangedDecorator{TEvent,TKey}"/>
    /// with <see cref="EqualityComparer{T}.Default"/> key equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event key selector.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctByUntilChanged<TEvent, TKey>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector)
    {
        return source.DistinctByUntilChanged( keySelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilChangedDecorator{TEvent,TKey}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
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

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilDecorator{TEvent,TKey,TTargetEvent}"/> where event is the key,
    /// with <see cref="EqualityComparer{T}.Default"/> key equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream whose events cause the underlying distinct keys tracker to be reset.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        return source.DistinctByUntil( static e => e, target );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilDecorator{TEvent,TKey,TTargetEvent}"/> where event is the key.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <param name="target">Target event stream whose events cause the underlying distinct keys tracker to be reset.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEqualityComparer<TEvent> equalityComparer,
        IEventStream<TTargetEvent> target)
    {
        return source.DistinctByUntil( static e => e, equalityComparer, target );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilDecorator{TEvent,TKey,TTargetEvent}"/>
    /// with <see cref="EqualityComparer{T}.Default"/> key equality comparer.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="target">Target event stream whose events cause the underlying distinct keys tracker to be reset.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DistinctByUntil<TEvent, TKey, TTargetEvent>(
        this IEventStream<TEvent> source,
        Func<TEvent, TKey> keySelector,
        IEventStream<TTargetEvent> target)
    {
        return source.DistinctByUntil( keySelector, EqualityComparer<TKey>.Default, target );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDistinctUntilDecorator{TEvent,TKey,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="keySelector">Event key selector.</param>
    /// <param name="equalityComparer">Key equality comparer.</param>
    /// <param name="target">Target event stream whose events cause the underlying distinct keys tracker to be reset.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TKey">Event's key type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
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

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSkipDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="count">Number of events at the beginning of the sequence to skip.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Skip<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerSkipDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSkipLastDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="count">Number of events at the end of the sequence to skip.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SkipLast<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerSkipLastDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSkipUntilDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before starting to notify the decorated event listener.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SkipUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerSkipUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSkipWhileDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="predicate">Predicate that skips events until the first event that passes it (returns <b>true</b>).</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SkipWhile<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
    {
        var decorator = new EventListenerSkipWhileDecorator<TEvent>( predicate );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerTakeDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="count">Number of events at the beginning of the sequence to take.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Take<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerTakeDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerTakeLastDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="count">Number of events at the end of the sequence to take.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> TakeLast<TEvent>(this IEventStream<TEvent> source, int count)
    {
        var decorator = new EventListenerTakeLastDecorator<TEvent>( count );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerTakeUntilDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before disposing the subscriber.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> TakeUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerTakeUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerTakeWhileDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="predicate">Predicate that takes events until the first event that fails it (returns <b>false</b>).</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> TakeWhile<TEvent>(this IEventStream<TEvent> source, Func<TEvent, bool> predicate)
    {
        var decorator = new EventListenerTakeWhileDecorator<TEvent>( predicate );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerPrependDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="values">Collection of values to prepend.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Prepend<TEvent>(this IEventStream<TEvent> source, IEnumerable<TEvent> values)
    {
        var decorator = new EventListenerPrependDecorator<TEvent>( values );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerPrependDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="values">Collection of values to prepend.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Prepend<TEvent>(this IEventStream<TEvent> source, params TEvent[] values)
    {
        return source.Prepend( values.AsEnumerable() );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerAppendDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="values">Collection of values to append.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Append<TEvent>(this IEventStream<TEvent> source, IEnumerable<TEvent> values)
    {
        var decorator = new EventListenerAppendDecorator<TEvent>( values );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerAppendDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="values">Collection of values to append.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Append<TEvent>(this IEventStream<TEvent> source, params TEvent[] values)
    {
        return source.Append( values.AsEnumerable() );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerConcurrentDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Concurrent<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerConcurrentDecorator<TEvent>( null );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerConcurrentDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="sync">Synchronization object.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Concurrent<TEvent>(this IEventStream<TEvent> source, object sync)
    {
        var decorator = new EventListenerConcurrentDecorator<TEvent>( sync );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates both event streams with <see cref="EventListenerConcurrentDecorator{TEvent}"/>, with the same synchronization object.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream.</param>
    /// <param name="resultSelector">Result selector.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Selected result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TResult ShareConcurrencyWith<TEvent, TTargetEvent, TResult>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target,
        Func<IEventStream<TEvent>, IEventStream<TTargetEvent>, TResult> resultSelector)
    {
        return source.ShareConcurrencyWith( target, resultSelector, new object() );
    }

    /// <summary>
    /// Decorates both event streams with <see cref="EventListenerConcurrentDecorator{TEvent}"/>, with the same synchronization object.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream.</param>
    /// <param name="resultSelector">Result selector.</param>
    /// <param name="sync">Synchronization object.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Selected result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TResult ShareConcurrencyWith<TEvent, TTargetEvent, TResult>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target,
        Func<IEventStream<TEvent>, IEventStream<TTargetEvent>, TResult> resultSelector,
        object sync)
    {
        return resultSelector( source.Concurrent( sync ), target.Concurrent( sync ) );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerAuditUntilDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before emitting the last emitted event.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> AuditUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerAuditUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerDebounceUntilDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before emitting the stored event.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> DebounceUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerDebounceUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSampleWhenDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before emitting the last stored event.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SampleWhen<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerSampleWhenDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerThrottleUntilDecorator{TEvent,TTargetEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="target">Target event stream to wait for before emitting any subsequent events.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TTargetEvent">Target event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ThrottleUntil<TEvent, TTargetEvent>(
        this IEventStream<TEvent> source,
        IEventStream<TTargetEvent> target)
    {
        var decorator = new EventListenerThrottleUntilDecorator<TEvent, TTargetEvent>( target );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerContinueWithDecorator{TEvent,TNextEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="continuationFactory">Delegate that creates the continuation event stream based on the last emitted event.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TNextEvent">Next event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TNextEvent> ContinueWith<TEvent, TNextEvent>(
        this IEventStream<TEvent> source,
        Func<TEvent, IEventStream<TNextEvent>> continuationFactory)
    {
        var decorator = new EventListenerContinueWithDecorator<TEvent, TNextEvent>( continuationFactory );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerMergeAllDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="maxConcurrency">
    /// Maximum number of concurrently active inner event streams. Equal to <see cref="Int32.MaxValue"/> by default.
    /// </param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxConcurrency"/> is less than <b>1</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> MergeAll<TEvent>(
        this IEventStream<IEventStream<TEvent>> source,
        int maxConcurrency = int.MaxValue)
    {
        var decorator = new EventListenerMergeAllDecorator<TEvent>( maxConcurrency );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerMergeAllDecorator{TEvent}"/>
    /// with maximum number of concurrently active inner event streams equal to <b>1</b>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ConcatAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        return source.MergeAll( maxConcurrency: 1 );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerSwitchAllDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> SwitchAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        var decorator = new EventListenerSwitchAllDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerExhaustAllDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> ExhaustAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        var decorator = new EventListenerExhaustAllDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerConcurrentAllDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> ConcurrentAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        var decorator = new EventListenerConcurrentAllDecorator<TEvent>( null );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerConcurrentAllDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="sync">Shared synchronization object.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> ConcurrentAll<TEvent>(this IEventStream<IEventStream<TEvent>> source, object sync)
    {
        var decorator = new EventListenerConcurrentAllDecorator<TEvent>( sync );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerConcurrentDecorator{TEvent}"/>
    /// and <see cref="EventListenerConcurrentAllDecorator{TEvent}"/>, with the same synchronization object.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> ShareConcurrencyWithAll<TEvent>(this IEventStream<IEventStream<TEvent>> source)
    {
        return source.ShareConcurrencyWithAll( new object() );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerConcurrentDecorator{TEvent}"/>
    /// and <see cref="EventListenerConcurrentAllDecorator{TEvent}"/>, with the same synchronization object.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="sync">Shared synchronization object.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<IEventStream<TEvent>> ShareConcurrencyWithAll<TEvent>(
        this IEventStream<IEventStream<TEvent>> source,
        object sync)
    {
        return source.Concurrent( sync ).ConcurrentAll( sync );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerUseSynchronizationContextDecorator{TEvent}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="InvalidOperationException">When <see cref="SynchronizationContext.Current"/> is null.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> UseSynchronizationContext<TEvent>(this IEventStream<TEvent> source)
    {
        var decorator = new EventListenerUseSynchronizationContextDecorator<TEvent>();
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Decorates the event stream with <see cref="EventListenerCatchDecorator{TEvent,TException}"/>.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="onError">Delegate to invoke once an exception is thrown by the decorated event listener.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TException">Exception type.</typeparam>
    /// <returns>Decorated <see cref="IEventStream{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> Catch<TEvent, TException>(this IEventStream<TEvent> source, Action<TException> onError)
        where TException : Exception
    {
        var decorator = new EventListenerCatchDecorator<TEvent, TException>( onError );
        return source.Decorate( decorator );
    }

    /// <summary>
    /// Creates a new <see cref="ConcurrentEventSource{TEvent,TSource}"/> with the provided underlying event source.
    /// </summary>
    /// <param name="source">Underlying event source.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="ConcurrentEventSource{TEvent,TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventSource<TEvent> ToConcurrent<TEvent>(this EventSource<TEvent> source)
    {
        return new ConcurrentEventSource<TEvent, EventSource<TEvent>>( source );
    }

    /// <summary>
    /// Creates a new <see cref="ConcurrentEventPublisher{TEvent,TSource}"/> with the provided underlying event publisher.
    /// </summary>
    /// <param name="source">Underlying event publisher.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="ConcurrentEventPublisher{TEvent,TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventPublisher<TEvent> ToConcurrent<TEvent>(this EventPublisher<TEvent> source)
    {
        return new ConcurrentEventPublisher<TEvent, EventPublisher<TEvent>>( source );
    }

    /// <summary>
    /// Creates a new <see cref="ConcurrentHistoryEventPublisher{TEvent,TSource}"/> with the provided underlying history event publisher.
    /// </summary>
    /// <param name="source">Underlying history event publisher.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="ConcurrentHistoryEventPublisher{TEvent,TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IHistoryEventPublisher<TEvent> ToConcurrent<TEvent>(this HistoryEventPublisher<TEvent> source)
    {
        return new ConcurrentHistoryEventPublisher<TEvent, HistoryEventPublisher<TEvent>>( source );
    }

    /// <summary>
    /// Creates a new <see cref="Task{TResult}"/> instance from the provided event <paramref name="source"/>, that completes
    /// when the event source subscriber is disposed, with the last emitted event as its result.
    /// </summary>
    /// <param name="source">Source event stream.</param>
    /// <param name="cancellationToken">Task's cancellation token.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="Task{TResult}"/> instance.</returns>
    public static Task<TEvent?> ToTask<TEvent>(this IEventStream<TEvent> source, CancellationToken cancellationToken)
    {
        var completionSource = new TaskCompletionSource<TEvent?>( TaskCreationOptions.RunContinuationsAsynchronously );
        if ( cancellationToken.IsCancellationRequested )
        {
            completionSource.SetCanceled( cancellationToken );
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
