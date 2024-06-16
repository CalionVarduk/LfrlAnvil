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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Composites;

/// <summary>
/// Represents an event with a sender.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithSender<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithSender{TEvent}"/> instance.
    /// </summary>
    /// <param name="sender">Event's sender.</param>
    /// <param name="event">Underlying event.</param>
    public WithSender(object? sender, TEvent @event)
    {
        Sender = sender;
        Event = @event;
    }

    /// <summary>
    /// Event's sender.
    /// </summary>
    public object? Sender { get; }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithSender{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Sender} => {Event}";
    }
}
