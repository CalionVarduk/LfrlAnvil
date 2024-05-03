using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Creates instances of <see cref="IStateTransitionHandler{TState,TInput,TResult}"/> type.
/// </summary>
public static class StateTransitionHandler
{
    /// <summary>
    /// Creates a new <see cref="IStateTransitionHandler{TState,TInput,TResult}"/> instance.
    /// </summary>
    /// <param name="func">Handler's delegate.</param>
    /// <typeparam name="TState">State type.</typeparam>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IStateTransitionHandler{TState,TInput,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IStateTransitionHandler<TState, TInput, TResult> Create<TState, TInput, TResult>(
        Func<StateTransitionHandlerArgs<TState, TInput, TResult>, TResult> func)
        where TState : notnull
        where TInput : notnull
    {
        return new Lambda<TState, TInput, TResult>( func );
    }

    private sealed class Lambda<TState, TInput, TResult> : IStateTransitionHandler<TState, TInput, TResult>
        where TState : notnull
        where TInput : notnull
    {
        private readonly Func<StateTransitionHandlerArgs<TState, TInput, TResult>, TResult> _func;

        internal Lambda(Func<StateTransitionHandlerArgs<TState, TInput, TResult>, TResult> func)
        {
            _func = func;
        }

        public TResult Handle(StateTransitionHandlerArgs<TState, TInput, TResult> args)
        {
            return _func( args );
        }
    }
}
