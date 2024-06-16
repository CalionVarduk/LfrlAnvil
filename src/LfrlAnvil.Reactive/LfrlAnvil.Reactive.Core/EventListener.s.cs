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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Internal;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Creates instances of <see cref="IEventListener{TEvent}"/> type.
/// </summary>
public static class EventListener
{
    /// <summary>
    /// Creates a new <see cref="IEventListener{TEvent}"/> instance.
    /// </summary>
    /// <param name="react">Reaction delegate.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="IEventListener{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventListener<TEvent> Create<TEvent>(Action<TEvent> react)
    {
        return new LambdaEventListener<TEvent>( react, dispose: null );
    }

    /// <summary>
    /// Creates a new <see cref="IEventListener{TEvent}"/> instance.
    /// </summary>
    /// <param name="react">Reaction delegate.</param>
    /// <param name="onDispose">Disposal delegate.</param>
    /// <typeparam name="TEvent">Event type.</typeparam>
    /// <returns>New <see cref="IEventListener{TEvent}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEventListener<TEvent> Create<TEvent>(Action<TEvent> react, Action<DisposalSource> onDispose)
    {
        return new LambdaEventListener<TEvent>( react, onDispose );
    }
}
