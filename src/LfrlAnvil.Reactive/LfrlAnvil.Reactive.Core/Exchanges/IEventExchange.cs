using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Exchanges;

public interface IEventExchange
{
    bool IsDisposed { get; }

    [Pure]
    IEnumerable<Type> GetRegisteredEventTypes();

    [Pure]
    bool IsRegistered<TEvent>();

    [Pure]
    bool IsRegistered(Type eventType);

    [Pure]
    IEventStream<TEvent> GetStream<TEvent>();

    [Pure]
    IEventStream GetStream(Type eventType);

    bool TryGetStream<TEvent>([MaybeNullWhen( false )] out IEventStream<TEvent> result);
    bool TryGetStream(Type eventType, [MaybeNullWhen( false )] out IEventStream result);

    IEventSubscriber Listen<TEvent>(IEventListener<TEvent> listener);
    IEventSubscriber Listen(Type eventType, IEventListener listener);

    bool TryListen<TEvent>(IEventListener<TEvent> listener, [MaybeNullWhen( false )] out IEventSubscriber subscriber);
    bool TryListen(Type eventType, IEventListener listener, [MaybeNullWhen( false )] out IEventSubscriber subscriber);
}
