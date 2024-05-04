using System.Collections.Generic;
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a concurrent version of a <see cref="HistoryEventPublisher{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPublisher">Underlying history event publisher type.</typeparam>
public class ConcurrentHistoryEventPublisher<TEvent, TPublisher>
    : ConcurrentEventPublisher<TEvent, TPublisher>, IHistoryEventPublisher<TEvent>
    where TPublisher : HistoryEventPublisher<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="ConcurrentHistoryEventPublisher{TEvent,TPublisher}"/> instance.
    /// </summary>
    /// <param name="base">Underlying history event publisher.</param>
    protected internal ConcurrentHistoryEventPublisher(TPublisher @base)
        : base( @base ) { }

    /// <inheritdoc />
    public int Capacity => Base.Capacity;

    /// <inheritdoc />
    public IReadOnlyCollection<TEvent> History => new ConcurrentReadOnlyCollection<TEvent>( Base.History, Sync );

    /// <inheritdoc />
    public void ClearHistory()
    {
        lock ( Sync )
        {
            Base.ClearHistory();
        }
    }
}
