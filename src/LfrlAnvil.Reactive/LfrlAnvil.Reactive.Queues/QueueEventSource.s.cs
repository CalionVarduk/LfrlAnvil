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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Queues;

/// <summary>
/// Creates instances of <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> type.
/// </summary>
public static class QueueEventSource
{
    /// <summary>
    /// Creates a new <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> instance from the provided <paramref name="queue"/>.
    /// </summary>
    /// <param name="queue">Underlying queue.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TPoint">Queue point type.</typeparam>
    /// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
    /// <returns>New <see cref="QueueEventSource{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static QueueEventSource<TEvent, TPoint, TPointDelta> Create<TEvent, TPoint, TPointDelta>(
        IMutableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        return new QueueEventSource<TEvent, TPoint, TPointDelta>( queue );
    }

    /// <summary>
    /// Creates a new <see cref="ReorderableQueueEventSource{TEvent,TPoint,TPointDelta}"/> instance
    /// from the provided <paramref name="queue"/>.
    /// </summary>
    /// <param name="queue">Underlying queue.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <typeparam name="TPoint">Queue point type.</typeparam>
    /// <typeparam name="TPointDelta">Queue point delta type.</typeparam>
    /// <returns>New <see cref="ReorderableQueueEventSource{TEvent,TPoint,TPointDelta}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReorderableQueueEventSource<TEvent, TPoint, TPointDelta> Create<TEvent, TPoint, TPointDelta>(
        IMutableReorderableEventQueue<TEvent, TPoint, TPointDelta> queue)
    {
        return new ReorderableQueueEventSource<TEvent, TPoint, TPointDelta>( queue );
    }
}
