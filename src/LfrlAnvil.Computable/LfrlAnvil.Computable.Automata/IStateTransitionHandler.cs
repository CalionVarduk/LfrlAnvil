namespace LfrlAnvil.Computable.Automata;

public interface IStateTransitionHandler<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    TResult Handle(StateTransitionHandlerArgs<TState, TInput, TResult> args);
}
