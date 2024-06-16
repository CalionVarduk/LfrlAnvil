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
/// Represents a type-erased event listener.
/// </summary>
public interface IEventListener
{
    /// <summary>
    /// Handler invoked during reaction to an event.
    /// </summary>
    /// <param name="event">Published event.</param>
    void React(object? @event);

    /// <summary>
    /// Handler invoked during owner's disposal.
    /// </summary>
    /// <param name="source"><see cref="DisposalSource"/> that caused the invocation.</param>
    void OnDispose(DisposalSource source);
}

/// <summary>
/// Represents a generic event listener.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public interface IEventListener<in TEvent> : IEventListener
{
    /// <summary>
    /// Handler invoked during reaction to an event.
    /// </summary>
    /// <param name="event">Published event.</param>
    void React(TEvent @event);
}
