using System.Collections.Generic;

namespace LfrlAnvil.Reactive;

public interface IHistoryEventPublisher<TEvent> : IEventPublisher<TEvent>
{
    int Capacity { get; }
    IReadOnlyCollection<TEvent> History { get; }
    void ClearHistory();
}
