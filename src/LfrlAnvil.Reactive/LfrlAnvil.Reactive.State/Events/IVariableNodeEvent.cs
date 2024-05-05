namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased event emitted by an <see cref="IVariableNode"/>.
/// </summary>
public interface IVariableNodeEvent
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    IVariableNode Variable { get; }

    /// <summary>
    /// Previous state of the <see cref="Variable"/>.
    /// </summary>
    VariableState PreviousState { get; }

    /// <summary>
    /// Current state of the <see cref="Variable"/>.
    /// </summary>
    VariableState NewState { get; }
}
