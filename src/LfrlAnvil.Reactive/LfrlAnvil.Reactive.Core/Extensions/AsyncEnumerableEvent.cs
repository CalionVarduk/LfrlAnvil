// Copyright 2026 Łukasz Furlepa
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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Extensions;

/// <summary>
/// Represents an event emitted by an event source, converted to an async enumerable element.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public readonly struct AsyncEnumerableEvent<TEvent>
{
    private readonly TEvent? _event;
    private readonly DisposalSource? _disposalSource;

    private AsyncEnumerableEvent(TEvent? @event, DisposalSource? disposalSource)
    {
        _event = @event;
        _disposalSource = disposalSource;
    }

    internal bool IsDisposal => _disposalSource is not null;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static AsyncEnumerableEvent<TEvent> Create(TEvent @event)
    {
        return new AsyncEnumerableEvent<TEvent>( @event, null );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static AsyncEnumerableEvent<TEvent> CreateDisposal(DisposalSource disposalSource)
    {
        return new AsyncEnumerableEvent<TEvent>( default, disposalSource );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="AsyncEnumerableEvent{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return _disposalSource is null ? $"Event({_event})" : $"Disposal({_disposalSource.Value})";
    }

    /// <summary>
    /// Attempts to extract the underlying emitted event.
    /// </summary>
    /// <param name="event"><b>out</b> parameter that returns the underlying emitted event.</param>
    /// <returns><b>true</b> when the event exists, otherwise <b>false</b>.</returns>
    /// <remarks>Entries returning <b>false</b> represent subscription disposal.</remarks>
    public bool TryGetEvent([MaybeNullWhen( false )] out TEvent @event)
    {
        if ( _disposalSource is null )
        {
            @event = _event!;
            return true;
        }

        @event = default;
        return false;
    }

    /// <summary>
    /// Attempts to extract the underlying emitted event.
    /// </summary>
    /// <param name="event"><b>out</b> parameter that returns the underlying emitted event.</param>
    /// <param name="disposalSource"><b>out</b> parameter that returns information about subscription disposal source.</param>
    /// <returns><b>true</b> when the event exists, otherwise <b>false</b>.</returns>
    public bool TryGetEvent([MaybeNullWhen( false )] out TEvent @event, [NotNullWhen( false )] out DisposalSource? disposalSource)
    {
        disposalSource = _disposalSource;
        if ( disposalSource is null )
        {
            @event = _event!;
            return true;
        }

        @event = default;
        return false;
    }
}
