using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Computable.Automata.Exceptions;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents an instance of a deterministic finite state machine.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Resul type.</typeparam>
public interface IStateMachineInstance<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Subject of this instance. Equal to <b>this</b> unless this instance has been created with a custom subject.
    /// </summary>
    object Subject { get; }

    /// <summary>
    /// Source state machine.
    /// </summary>
    IStateMachine<TState, TInput, TResult> Machine { get; }

    /// <summary>
    /// State in which this instance currently is.
    /// </summary>
    IStateMachineNode<TState, TInput, TResult> CurrentState { get; }

    /// <summary>
    /// Attempts to transition this instance to the provided state.
    /// </summary>
    /// <param name="input">Transition identifier.</param>
    /// <param name="result"><b>out</b> parameter that returns the result returned by the transition.</param>
    /// <returns><b>true</b> when transition was successful, otherwise <b>false</b>.</returns>
    bool TryTransition(TInput input, [MaybeNullWhen( false )] out TResult result);

    /// <summary>
    /// Transitions this instance to the provided state.
    /// </summary>
    /// <param name="input">Transition identifier.</param>
    /// <returns>Result returned by the transition.</returns>
    /// <exception cref="StateMachineTransitionException">
    /// When <see cref="CurrentState"/> cannot transition with the provided <paramref name="input"/>.
    /// </exception>
    TResult Transition(TInput input);
}
