using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Exchanges;

public interface IMutableEventExchange : IEventExchange, IDisposable
{
    [Pure]
    IEventPublisher<TEvent> GetPublisher<TEvent>();

    [Pure]
    IEventPublisher GetPublisher(Type eventType);

    bool TryGetPublisher<TEvent>([MaybeNullWhen( false )] out IEventPublisher<TEvent> result);
    bool TryGetPublisher(Type eventType, [MaybeNullWhen( false )] out IEventPublisher result);

    void Publish<TEvent>(TEvent @event);
    bool TryPublish<TEvent>(TEvent @event);
    void Publish(Type eventType, object? @event);
    bool TryPublish(Type eventType, object? @event);

    IEventPublisher<TEvent> RegisterPublisher<TEvent>();
    IEventPublisher<TEvent> RegisterPublisher<TEvent>(IEventPublisher<TEvent> publisher);
}
