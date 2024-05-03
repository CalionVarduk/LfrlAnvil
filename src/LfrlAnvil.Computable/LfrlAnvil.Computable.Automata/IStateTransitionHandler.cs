namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a handler that is invoked when <see cref="IStateMachine{TState,TInput,TResult}"/> transitions to a different state.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Resul type.</typeparam>
public interface IStateTransitionHandler<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Implementation of this handler's invocation.
    /// </summary>
    /// <param name="args">Invocation arguments.</param>
    /// <returns>Result provided by this handler.</returns>
    TResult Handle(StateTransitionHandlerArgs<TState, TInput, TResult> args);
}
