﻿// Copyright 2024 Łukasz Furlepa
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
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic variable change event emitted by an <see cref="IReadOnlyCollectionVariableRoot{TKey,TElement,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public class CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>
    : ICollectionVariableRootValidationEvent<TValidationResult>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Creates a new <see cref="CollectionVariableRootValidationEvent{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="previousErrors">Collection of validation errors before the change.</param>
    /// <param name="previousWarnings">Collection of validation warnings before the change.</param>
    /// <param name="associatedChange">
    /// <see cref="ICollectionVariableRootChangeEvent"/> instance associated with this validation event.
    /// </param>
    /// <param name="sourceEvent">Source child node event.</param>
    public CollectionVariableRootValidationEvent(
        IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> previousWarnings,
        CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>? associatedChange,
        IVariableNodeEvent? sourceEvent)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = Variable.State;
        PreviousErrors = previousErrors;
        NewErrors = Variable.Errors;
        PreviousWarnings = previousWarnings;
        NewWarnings = Variable.Warnings;
        AssociatedChange = associatedChange;
        SourceEvent = sourceEvent;
    }

    /// <inheritdoc cref="ICollectionVariableRootValidationEvent.AssociatedChange" />
    public CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>? AssociatedChange { get; }

    /// <inheritdoc cref="ICollectionVariableRootValidationEvent.Variable" />
    public IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> Variable { get; }

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

    /// <inheritdoc />
    public IVariableNodeEvent? SourceEvent { get; }

    Type ICollectionVariableRootValidationEvent.KeyType => typeof( TKey );
    Type ICollectionVariableRootValidationEvent.ElementType => typeof( TElement );
    Type ICollectionVariableRootValidationEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyCollectionVariableRoot ICollectionVariableRootValidationEvent.Variable => Variable;
    ICollectionVariableRootChangeEvent? ICollectionVariableRootValidationEvent.AssociatedChange => AssociatedChange;
    IEnumerable ICollectionVariableRootValidationEvent.PreviousErrors => PreviousErrors;
    IEnumerable ICollectionVariableRootValidationEvent.NewErrors => NewErrors;
    IEnumerable ICollectionVariableRootValidationEvent.PreviousWarnings => PreviousWarnings;
    IEnumerable ICollectionVariableRootValidationEvent.NewWarnings => NewWarnings;

    IVariableNode IVariableNodeEvent.Variable => Variable;
}
