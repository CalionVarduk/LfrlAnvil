// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic validation event emitted by an <see cref="IReadOnlyVariable{TValue,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TValue">Value type.</typeparam>
/// <typeparam name="TValidationResult">Variable's validation result type.</typeparam>
public class VariableValidationEvent<TValue, TValidationResult> : IVariableValidationEvent<TValidationResult>
{
    /// <summary>
    /// Creates a new <see cref="VariableValidationEvent{TValue,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousErrors">Collection of validation errors before the change.</param>
    /// <param name="previousWarnings">Collection of validation warnings before the change.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="associatedChange">
    /// <see cref="VariableValueChangeEvent{TEvent,TValidationResult}"/> instance associated with this validation event.
    /// </param>
    public VariableValidationEvent(
        IReadOnlyVariable<TValue, TValidationResult> variable,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        VariableState previousState,
        VariableValueChangeEvent<TValue, TValidationResult>? associatedChange)
    {
        AssociatedChange = associatedChange;
        Variable = variable;
        PreviousState = previousState;
        NewState = variable.State;
        PreviousErrors = previousErrors;
        NewErrors = Variable.Errors;
        PreviousWarnings = previousWarnings;
        NewWarnings = Variable.Warnings;
    }

    /// <inheritdoc cref="IVariableValidationEvent.AssociatedChange" />
    public VariableValueChangeEvent<TValue, TValidationResult>? AssociatedChange { get; }

    /// <inheritdoc cref="IVariableValidationEvent.Variable" />
    public IReadOnlyVariable<TValue, TValidationResult> Variable { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> PreviousErrors { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> NewErrors { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> PreviousWarnings { get; }

    /// <inheritdoc />
    public Chain<TValidationResult> NewWarnings { get; }

    Type IVariableValidationEvent.ValueType => typeof( TValue );
    Type IVariableValidationEvent.ValidationResultType => typeof( TValidationResult );
    IVariableValueChangeEvent? IVariableValidationEvent.AssociatedChange => AssociatedChange;
    IReadOnlyVariable IVariableValidationEvent.Variable => Variable;
    IVariableNode IVariableNodeEvent.Variable => Variable;
    IEnumerable IVariableValidationEvent.PreviousErrors => PreviousErrors;
    IEnumerable IVariableValidationEvent.NewErrors => NewErrors;
    IEnumerable IVariableValidationEvent.PreviousWarnings => PreviousWarnings;
    IEnumerable IVariableValidationEvent.NewWarnings => NewWarnings;
}
