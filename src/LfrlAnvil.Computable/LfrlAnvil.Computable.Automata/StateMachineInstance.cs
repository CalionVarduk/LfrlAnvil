using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Computable.Automata.Exceptions;

namespace LfrlAnvil.Computable.Automata;

public sealed class StateMachineInstance<TState, TInput, TResult> : IStateMachineInstance<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    internal StateMachineInstance(StateMachine<TState, TInput, TResult> machine, IStateMachineNode<TState, TInput, TResult> currentState)
    {
        Machine = machine;
        CurrentState = currentState;
        Subject = this;
    }

    internal StateMachineInstance(
        StateMachine<TState, TInput, TResult> machine,
        IStateMachineNode<TState, TInput, TResult> currentState,
        object subject)
    {
        Machine = machine;
        CurrentState = currentState;
        Subject = subject;
    }

    public object Subject { get; }
    public StateMachine<TState, TInput, TResult> Machine { get; }
    public IStateMachineNode<TState, TInput, TResult> CurrentState { get; private set; }
    IStateMachine<TState, TInput, TResult> IStateMachineInstance<TState, TInput, TResult>.Machine => Machine;

    public bool TryTransition(TInput input, [MaybeNullWhen( false )] out TResult result)
    {
        if ( ! CurrentState.Transitions.TryGetValue( input, out var transition ) )
        {
            result = default;
            return false;
        }

        var source = CurrentState;
        CurrentState = transition.Destination;

        result = transition.Handler is null
            ? Machine.DefaultResult
            : transition.Handler.Handle(
                new StateTransitionHandlerArgs<TState, TInput, TResult>( Subject, source, transition.Destination, input ) );

        return true;
    }

    public TResult Transition(TInput input)
    {
        if ( ! TryTransition( input, out var result ) )
            throw new StateMachineTransitionException( Resources.TransitionDoesNotExist( CurrentState.Value, input ), nameof( input ) );

        return result;
    }
}
