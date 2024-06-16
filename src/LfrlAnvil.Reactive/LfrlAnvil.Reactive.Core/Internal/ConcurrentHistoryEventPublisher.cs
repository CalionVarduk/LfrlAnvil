// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
