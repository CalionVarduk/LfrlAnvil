using System.Collections.Generic;
using LfrlAnvil.Async;

namespace LfrlAnvil.Reactive.Internal;

public class ConcurrentHistoryEventPublisher<TEvent, TPublisher>
    : ConcurrentEventPublisher<TEvent, TPublisher>, IHistoryEventPublisher<TEvent>
    where TPublisher : HistoryEventPublisher<TEvent>
{
    protected internal ConcurrentHistoryEventPublisher(TPublisher @base)
        : base( @base ) { }

    public int Capacity => Base.Capacity;
    public IReadOnlyCollection<TEvent> History => new ConcurrentReadOnlyCollection<TEvent>( Base.History, Sync );

    public void ClearHistory()
    {
        lock ( Sync )
        {
            Base.ClearHistory();
        }
    }
}