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

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a generic disposable event source that can be listened to, capable of recording previously published events.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IHistoryEventPublisher<TEvent> : IEventPublisher<TEvent>
{
    /// <summary>
    /// Specifies the maximum number of events this event publisher can record.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Collection of recorded previously published events.
    /// </summary>
    IReadOnlyCollection<TEvent> History { get; }

    /// <summary>
    /// Removes all recorded events.
    /// </summary>
    void ClearHistory();
}
