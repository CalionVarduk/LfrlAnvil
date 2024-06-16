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
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State.Events;

/// <inheritdoc cref="ICollectionVariableRootChangeEvent{TKey,TElement}" />
public class CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult> : ICollectionVariableRootChangeEvent<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Creates a new <see cref="CollectionVariableRootChangeEvent{TKey,TElement,TValidationResult}"/> instance
    /// with null <see cref="ICollectionVariableRootChangeEvent.SourceEvent"/>.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="addedElements">Collection of elements added due to this event.</param>
    /// <param name="removedElements">Collection of elements removed due to this event.</param>
    /// <param name="restoredElements">Collection of elements restored due to this event.</param>
    /// <param name="source">Specifies the source of this value change.</param>
    public CollectionVariableRootChangeEvent(
        IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        IReadOnlyList<TElement> addedElements,
        IReadOnlyList<TElement> removedElements,
        IReadOnlyList<TElement> restoredElements,
        VariableChangeSource source)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = Variable.State;
        AddedElements = addedElements;
        RemovedElements = removedElements;
        RestoredElements = restoredElements;
        Source = source;
        SourceEvent = null;
    }

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRootChangeEvent{TKey,TElement,TValidationResult}"/> instance
    /// with <see cref="VariableChangeSource.ChildNode"/> <see cref="Source"/>.
    /// </summary>
    /// <param name="variable">Variable node that emitted this event.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    /// <param name="addedElements">Collection of elements added due to this event.</param>
    /// <param name="removedElements">Collection of elements removed due to this event.</param>
    /// <param name="restoredElements">Collection of elements restored due to this event.</param>
    /// <param name="sourceEvent">Source child node event.</param>
    public CollectionVariableRootChangeEvent(
        IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> variable,
        VariableState previousState,
        IReadOnlyList<TElement> addedElements,
        IReadOnlyList<TElement> removedElements,
        IReadOnlyList<TElement> restoredElements,
        IVariableNodeEvent sourceEvent)
    {
        Variable = variable;
        PreviousState = previousState;
        NewState = Variable.State;
        AddedElements = addedElements;
        RemovedElements = removedElements;
        RestoredElements = restoredElements;
        Source = VariableChangeSource.ChildNode;
        SourceEvent = sourceEvent;
    }

    /// <inheritdoc cref="ICollectionVariableRootChangeEvent{TKey,TElement}.Variable" />
    public IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> Variable { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    /// <inheritdoc />
    public IReadOnlyList<TElement> AddedElements { get; }

    /// <inheritdoc />
    public IReadOnlyList<TElement> RemovedElements { get; }

    /// <inheritdoc />
    public IReadOnlyList<TElement> RestoredElements { get; }

    /// <inheritdoc />
    public VariableChangeSource Source { get; }

    /// <inheritdoc />
    public IVariableNodeEvent? SourceEvent { get; }

    IReadOnlyCollectionVariableRoot<TKey, TElement> ICollectionVariableRootChangeEvent<TKey, TElement>.Variable => Variable;

    Type ICollectionVariableRootChangeEvent.KeyType => typeof( TKey );
    Type ICollectionVariableRootChangeEvent.ElementType => typeof( TElement );
    Type ICollectionVariableRootChangeEvent.ValidationResultType => typeof( TValidationResult );
    IReadOnlyCollectionVariableRoot ICollectionVariableRootChangeEvent.Variable => Variable;
    IReadOnlyList<IVariableNode> ICollectionVariableRootChangeEvent.AddedElements => AddedElements;
    IReadOnlyList<IVariableNode> ICollectionVariableRootChangeEvent.RemovedElements => RemovedElements;
    IReadOnlyList<IVariableNode> ICollectionVariableRootChangeEvent.RestoredElements => RestoredElements;

    IVariableNode IVariableNodeEvent.Variable => Variable;
}
