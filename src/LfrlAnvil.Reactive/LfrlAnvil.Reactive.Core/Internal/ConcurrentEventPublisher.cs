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

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a concurrent version of an <see cref="EventPublisher{TEvent}"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
/// <typeparam name="TPublisher">Underlying event publisher type.</typeparam>
public class ConcurrentEventPublisher<TEvent, TPublisher> : ConcurrentEventSource<TEvent, TPublisher>, IEventPublisher<TEvent>
    where TPublisher : EventPublisher<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="ConcurrentEventPublisher{TEvent,TPublisher}"/> instance.
    /// </summary>
    /// <param name="base">Underlying event publisher.</param>
    protected internal ConcurrentEventPublisher(TPublisher @base)
        : base( @base ) { }

    /// <inheritdoc />
    public void Publish(TEvent @event)
    {
        lock ( Sync )
        {
            Base.Publish( @event );
        }
    }

    void IEventPublisher.Publish(object? @event)
    {
        Publish( Argument.CastTo<TEvent>( @event ) );
    }
}
