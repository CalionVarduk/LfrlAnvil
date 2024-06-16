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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.Reactive.Chrono.Composites;

/// <summary>
/// Represents an event with <see cref="Timestamp"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithTimestamp<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithTimestamp{TEvent}"/> instance.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="timestamp"><see cref="LfrlAnvil.Chrono.Timestamp"/> associated with this event.</param>
    public WithTimestamp(TEvent @event, Timestamp timestamp)
    {
        Event = @event;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Underlying event.
    /// </summary>
    public TEvent Event { get; }

    /// <summary>
    /// <see cref="LfrlAnvil.Chrono.Timestamp"/> associated with this event.
    /// </summary>
    public Timestamp Timestamp { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithTimestamp{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Timestamp}] {Event}";
    }
}
