using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.Events.Internal;

namespace LfrlAnvil.Reactive.Events
{
    public static class EventListener
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventListener<TEvent> Create<TEvent>(Action<TEvent> react)
        {
            return new LambdaEventListener<TEvent>( react, dispose: null );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IEventListener<TEvent> Create<TEvent>(Action<TEvent> react, Action onDispose)
        {
            return new LambdaEventListener<TEvent>( react, onDispose );
        }
    }
}
