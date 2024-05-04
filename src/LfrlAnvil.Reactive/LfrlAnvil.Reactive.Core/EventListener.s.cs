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
