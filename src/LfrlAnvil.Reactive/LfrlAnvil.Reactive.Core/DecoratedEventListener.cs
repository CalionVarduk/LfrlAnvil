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

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a generic decorated event listener.
/// </summary>
/// <typeparam name="TSourceEvent">Source event type.</typeparam>
/// <typeparam name="TNextEvent">Next event type.</typeparam>
public abstract class DecoratedEventListener<TSourceEvent, TNextEvent> : EventListener<TSourceEvent>
{
    /// <summary>
    /// Creates a new <see cref="DecoratedEventListener{TSourceEvent,TNextEvent}"/> instance.
    /// </summary>
    /// <param name="next">Decorating event listener.</param>
    protected DecoratedEventListener(IEventListener<TNextEvent> next)
    {
        Next = next;
    }

    /// <summary>
    /// Decorating event listener.
    /// </summary>
    protected IEventListener<TNextEvent> Next { get; }

    /// <inheritdoc />
    public override void OnDispose(DisposalSource source)
    {
        Next.OnDispose( source );
    }
}
