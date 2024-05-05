using System;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased value change event emitted by an <see cref="IReadOnlyVariable"/>.
/// </summary>
public interface IVariableValueChangeEvent : IVariableNodeEvent
{
    /// <summary>
    /// Variable's value type.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Variable's validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariable Variable { get; }

    /// <summary>
    /// Value before the change.
    /// </summary>
    object? PreviousValue { get; }

    /// <summary>
    /// Value after the change.
    /// </summary>
    object? NewValue { get; }

    /// <summary>
    /// Specifies the source of this value change.
    /// </summary>
    VariableChangeSource Source { get; }
}

/// <summary>
/// Represents a generic value change event emitted by an <see cref="IReadOnlyVariable{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">Variable's value type.</typeparam>
public interface IVariableValueChangeEvent<TValue> : IVariableValueChangeEvent
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariable<TValue> Variable { get; }

    /// <summary>
    /// Value before the change.
    /// </summary>
    new TValue PreviousValue { get; }

    /// <summary>
    /// Value after the change.
    /// </summary>
    new TValue NewValue { get; }
}
