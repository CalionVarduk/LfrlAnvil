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
/// Represents an event with <see cref="Timestamp"/> and <see cref="Interval"/>.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct WithInterval<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="WithInterval{TEvent}"/> instance.
    /// </summary>
    /// <param name="event">Underlying event.</param>
    /// <param name="timestamp"><see cref="LfrlAnvil.Chrono.Timestamp"/> associated with this event.</param>
    /// <param name="interval">Time elapsed since the last event.</param>
    public WithInterval(TEvent @event, Timestamp timestamp, Duration interval)
    {
        Event = @event;
        Timestamp = timestamp;
        Interval = interval;
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
    /// Time elapsed since the last event.
    /// </summary>
    public Duration Interval { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="WithInterval{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Timestamp} ({Interval} dt)] {Event}";
    }
}
