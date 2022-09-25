namespace LfrlAnvil.Reactive.State.Events;

public interface IVariableNodeEvent
{
    IVariableNode Variable { get; }
    VariableState PreviousState { get; }
    VariableState NewState { get; }
}
