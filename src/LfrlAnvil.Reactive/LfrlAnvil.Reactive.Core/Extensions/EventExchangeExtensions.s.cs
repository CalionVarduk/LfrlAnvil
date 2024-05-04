using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Exceptions;
using LfrlAnvil.Reactive.Exchanges;

namespace LfrlAnvil.Reactive.Extensions;

/// <summary>
/// Contains <see cref="IEventExchange"/> extension methods.
/// </summary>
public static class EventExchangeExtensions
{
    /// <summary>
    /// Checks whether or not an event stream for the provided event type exists.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <typeparam name="TEvent">Event type to check.</typeparam>
    /// <returns><b>true</b> when event stream exists, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsRegistered<TEvent>(this IEventExchange exchange)
    {
        return exchange.IsRegistered( typeof( TEvent ) );
    }

    /// <summary>
    /// Returns an event stream associated with the provided event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <typeparam name="TEvent">Event type to check.</typeparam>
    /// <returns>Registered <see cref="IEventStream{TEvent}"/> instance.</returns>
    /// <exception cref="EventPublisherNotFoundException">When event stream does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventStream<TEvent> GetStream<TEvent>(this IEventExchange exchange)
    {
        return ( IEventStream<TEvent> )exchange.GetStream( typeof( TEvent ) );
    }

    /// <summary>
    /// Attempts to return an event stream associated with the provided event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <typeparam name="TEvent">Event type to check.</typeparam>
    /// <param name="result"><b>out</b> parameter that returns the registered <see cref="IEventStream"/> instance.</param>
    /// <returns><b>true</b> when event stream exists, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryGetStream<TEvent>(this IEventExchange exchange, [MaybeNullWhen( false )] out IEventStream<TEvent> result)
    {
        if ( exchange.TryGetStream( typeof( TEvent ), out var innerResult ) )
        {
            result = ( IEventStream<TEvent> )innerResult;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Attaches the provided <paramref name="listener"/> to an event stream associated with the specified event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="listener">Event listener to attach.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="IEventSubscriber"/> instance that can be used to detach the <paramref name="listener"/>.</returns>
    /// <exception cref="EventPublisherNotFoundException">When event stream does not exist.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventSubscriber Listen<TEvent>(this IEventExchange exchange, IEventListener<TEvent> listener)
    {
        return exchange.GetStream<TEvent>().Listen( listener );
    }

    /// <summary>
    /// Attaches the provided <paramref name="listener"/> to an event stream associated with the specified <paramref name="eventType"/>.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="eventType">Event type.</param>
    /// <param name="listener">Event listener to attach.</param>
    /// <returns>New <see cref="IEventSubscriber"/> instance that can be used to detach the <paramref name="listener"/>.</returns>
    /// <exception cref="EventPublisherNotFoundException">When event stream does not exist.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventSubscriber Listen(this IEventExchange exchange, Type eventType, IEventListener listener)
    {
        return exchange.GetStream( eventType ).Listen( listener );
    }

    /// <summary>
    /// Attempts to attach the provided <paramref name="listener"/> to an event stream associated with the specified event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="listener">Event listener to attach.</param>
    /// <param name="subscriber">
    /// <b>out</b> parameter that returns the new <see cref="IEventSubscriber"/> instance
    /// that can be used to detach the <paramref name="listener"/>.
    /// </param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns><b>true</b> when event stream exists, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryListen<TEvent>(
        this IEventExchange exchange,
        IEventListener<TEvent> listener,
        [MaybeNullWhen( false )] out IEventSubscriber subscriber)
    {
        if ( exchange.TryGetStream<TEvent>( out var stream ) )
        {
            subscriber = stream.Listen( listener );
            return true;
        }

        subscriber = default;
        return false;
    }

    /// <summary>
    /// Attempts to attach the provided <paramref name="listener"/> to an event stream associated
    /// with the specified <paramref name="eventType"/>.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="eventType">Event type.</param>
    /// <param name="listener">Event listener to attach.</param>
    /// <param name="subscriber">
    /// <b>out</b> parameter that returns the new <see cref="IEventSubscriber"/> instance
    /// that can be used to detach the <paramref name="listener"/>.
    /// </param>
    /// <returns><b>true</b> when event stream exists, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryListen(
        this IEventExchange exchange,
        Type eventType,
        IEventListener listener,
        [MaybeNullWhen( false )] out IEventSubscriber subscriber)
    {
        if ( exchange.TryGetStream( eventType, out var stream ) )
        {
            subscriber = stream.Listen( listener );
            return true;
        }

        subscriber = default;
        return false;
    }

    /// <summary>
    /// Returns an event publisher associated with the provided event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <typeparam name="TEvent">Event type to check.</typeparam>
    /// <returns>Registered <see cref="IEventPublisher{TEvent}"/> instance.</returns>
    /// <exception cref="EventPublisherNotFoundException">When event publisher does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventPublisher<TEvent> GetPublisher<TEvent>(this IMutableEventExchange exchange)
    {
        return ( IEventPublisher<TEvent> )exchange.GetPublisher( typeof( TEvent ) );
    }

    /// <summary>
    /// Attempts to return an event publisher associated with the provided event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <typeparam name="TEvent">Event type to check.</typeparam>
    /// <param name="result"><b>out</b> parameter that returns the registered <see cref="IEventPublisher"/> instance.</param>
    /// <returns><b>true</b> when event publisher exists, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryGetPublisher<TEvent>(
        this IMutableEventExchange exchange,
        [MaybeNullWhen( false )] out IEventPublisher<TEvent> result)
    {
        if ( exchange.TryGetPublisher( typeof( TEvent ), out var innerResult ) )
        {
            result = ( IEventPublisher<TEvent> )innerResult;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Publishes an event on an event publisher associated with the specified event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="event">Event to publish.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <exception cref="EventPublisherNotFoundException">When event publisher does not exist.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Publish<TEvent>(this IMutableEventExchange exchange, TEvent @event)
    {
        exchange.GetPublisher<TEvent>().Publish( @event );
    }

    /// <summary>
    /// Attempts to publish an event on an event publisher associated with the specified event type.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="event">Event to publish.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns><b>true</b> when event publisher exists, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryPublish<TEvent>(this IMutableEventExchange exchange, TEvent @event)
    {
        if ( ! exchange.TryGetPublisher<TEvent>( out var publisher ) )
            return false;

        publisher.Publish( @event );
        return true;
    }

    /// <summary>
    /// Publishes an event on an event publisher associated with the specified <paramref name="eventType"/>.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="eventType">Event type.</param>
    /// <param name="event">Event to publish.</param>
    /// <exception cref="EventPublisherNotFoundException">When event publisher does not exist.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Publish(this IMutableEventExchange exchange, Type eventType, object? @event)
    {
        exchange.GetPublisher( eventType ).Publish( @event );
    }

    /// <summary>
    /// Attempts to publish an event on an event publisher associated with the specified <paramref name="eventType"/>.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <param name="eventType">Event type.</param>
    /// <param name="event">Event to publish.</param>
    /// <returns><b>true</b> when event publisher exists, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryPublish(this IMutableEventExchange exchange, Type eventType, object? @event)
    {
        if ( ! exchange.TryGetPublisher( eventType, out var publisher ) )
            return false;

        publisher.Publish( @event );
        return true;
    }

    /// <summary>
    /// Creates a new <see cref="EventPublisher{TEvent}"/> instance and registers it.
    /// </summary>
    /// <param name="exchange">Source event exchange.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>Registered event publisher.</returns>
    /// <exception cref="EventPublisherAlreadyExistsException">When event publisher for the given event type already exists.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventPublisher<TEvent> RegisterPublisher<TEvent>(this IMutableEventExchange exchange)
    {
        return exchange.RegisterPublisher( new EventPublisher<TEvent>() );
    }
}
