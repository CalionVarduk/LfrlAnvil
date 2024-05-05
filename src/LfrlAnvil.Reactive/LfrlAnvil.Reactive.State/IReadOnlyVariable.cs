using System;
using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased read-only variable.
/// </summary>
public interface IReadOnlyVariable : IVariableNode
{
    /// <summary>
    /// Value type.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Current value.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Initial value.
    /// </summary>
    object? InitialValue { get; }

    /// <summary>
    /// Collection of current validation errors.
    /// </summary>
    IEnumerable Errors { get; }

    /// <summary>
    /// Collection of current validation warnings.
    /// </summary>
    IEnumerable Warnings { get; }

    /// <summary>
    /// Event stream that emits events when variable's value changes.
    /// </summary>
    new IEventStream<IVariableValueChangeEvent> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<IVariableValidationEvent> OnValidate { get; }
}

/// <summary>
/// Represents a generic read-only variable.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IReadOnlyVariable<TValue> : IReadOnlyVariable
{
    /// <summary>
    /// Current value.
    /// </summary>
    new TValue Value { get; }

    /// <summary>
    /// Initial value.
    /// </summary>
    new TValue InitialValue { get; }

    /// <summary>
    /// Value equality comparer.
    /// </summary>
    IEqualityComparer<TValue> Comparer { get; }

    /// <summary>
    /// Event stream that emits events when variable's value changes.
    /// </summary>
    new IEventStream<IVariableValueChangeEvent<TValue>> OnChange { get; }
}

/// <summary>
/// Represents a generic read-only variable.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface IReadOnlyVariable<TValue, TValidationResult> : IReadOnlyVariable<TValue>
{
    /// <summary>
    /// Collection of current validation errors.
    /// </summary>
    new Chain<TValidationResult> Errors { get; }

    /// <summary>
    /// Collection of current validation warnings.
    /// </summary>
    new Chain<TValidationResult> Warnings { get; }

    /// <summary>
    /// Value validator that marks result as errors.
    /// </summary>
    IValidator<TValue, TValidationResult> ErrorsValidator { get; }

    /// <summary>
    /// Value validator that marks result as warnings.
    /// </summary>
    IValidator<TValue, TValidationResult> WarningsValidator { get; }

    /// <summary>
    /// Event stream that emits events when variable's value changes.
    /// </summary>
    new IEventStream<VariableValueChangeEvent<TValue, TValidationResult>> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<VariableValidationEvent<TValue, TValidationResult>> OnValidate { get; }
}
