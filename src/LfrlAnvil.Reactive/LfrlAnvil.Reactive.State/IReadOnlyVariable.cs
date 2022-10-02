using System;
using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public interface IReadOnlyVariable : IVariableNode
{
    Type ValueType { get; }
    Type ValidationResultType { get; }
    object? Value { get; }
    object? InitialValue { get; }
    IEnumerable Errors { get; }
    IEnumerable Warnings { get; }
    new IEventStream<IVariableValueChangeEvent> OnChange { get; }
    new IEventStream<IVariableValidationEvent> OnValidate { get; }
}

public interface IReadOnlyVariable<TValue> : IReadOnlyVariable
{
    new TValue Value { get; }
    new TValue InitialValue { get; }
    IEqualityComparer<TValue> Comparer { get; }
    new IEventStream<IVariableValueChangeEvent<TValue>> OnChange { get; }
}

public interface IReadOnlyVariable<TValue, TValidationResult> : IReadOnlyVariable<TValue>
{
    new Chain<TValidationResult> Errors { get; }
    new Chain<TValidationResult> Warnings { get; }
    IValidator<TValue, TValidationResult> ErrorsValidator { get; }
    IValidator<TValue, TValidationResult> WarningsValidator { get; }
    new IEventStream<VariableValueChangeEvent<TValue, TValidationResult>> OnChange { get; }
    new IEventStream<VariableValidationEvent<TValue, TValidationResult>> OnValidate { get; }
}
