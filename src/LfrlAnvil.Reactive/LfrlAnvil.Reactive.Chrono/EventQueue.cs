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
using LfrlAnvil.Reactive.Queues;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a generic event queue with <see cref="Timestamp"/> point and <see cref="Duration"/> delta.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public class EventQueue<TEvent> : EventQueueBase<TEvent, Timestamp, Duration>
{
    /// <summary>
    /// Creates a new <see cref="EventQueue{TEvent}"/> instance.
    /// </summary>
    /// <param name="startPoint">Specifies the starting <see cref="Timestamp"/> of this queue.</param>
    public EventQueue(Timestamp startPoint)
        : base( startPoint ) { }

    /// <inheritdoc />
    [Pure]
    protected sealed override Timestamp AddDelta(Timestamp point, Duration delta)
    {
        return point.Add( delta );
    }
}
