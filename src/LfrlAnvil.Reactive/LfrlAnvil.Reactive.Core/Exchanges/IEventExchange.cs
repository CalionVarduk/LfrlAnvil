using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.Exceptions;

namespace LfrlAnvil.Reactive.Exchanges;

/// <summary>
/// Represents a collection of event streams identifiable by their event types.
/// </summary>
public interface IEventExchange
{
    /// <summary>
    /// Specifies whether or not this event exchange has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Returns a collection of event types of all currently registered event streams.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<Type> GetRegisteredEventTypes();

    /// <summary>
    /// Checks whether or not an event stream for the provided <paramref name="eventType"/> exists.
    /// </summary>
    /// <param name="eventType">Event type to check.</param>
    /// <returns><b>true</b> when event stream exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool IsRegistered(Type eventType);

    /// <summary>
    /// Returns an event stream associated with the provided <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    /// <returns>Registered <see cref="IEventStream"/> instance.</returns>
    /// <exception cref="EventPublisherNotFoundException">When event stream does not exist.</exception>
    [Pure]
    IEventStream GetStream(Type eventType);

    /// <summary>
    /// Attempts to return an event stream associated with the provided <paramref name="eventType"/>.
    /// </summary>
    /// <param name="eventType">Event type.</param>
    /// <param name="result"><b>out</b> parameter that returns the registered <see cref="IEventStream"/> instance.</param>
    /// <returns><b>true</b> when event stream exists, otherwise <b>false</b>.</returns>
    bool TryGetStream(Type eventType, [MaybeNullWhen( false )] out IEventStream result);
}
