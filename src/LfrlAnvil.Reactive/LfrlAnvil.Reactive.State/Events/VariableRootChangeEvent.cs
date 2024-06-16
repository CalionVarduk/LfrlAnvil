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

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic value change event emitted by an <see cref="IReadOnlyVariableRoot{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Child node's key type.</typeparam>
public class VariableRootChangeEvent<TKey> : IVariableRootEvent<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Creates a new <see cref="VariableRootChangeEvent{TKey}"/> instance.
    /// </summary>
    /// <param name="root">Variable node that emitted this event.</param>
    /// <param name="nodeKey">Key of the child node that caused this event.</param>
    /// <param name="sourceEvent">Source child node event.</param>
    /// <param name="previousState">Previous state of the <see cref="Variable"/>.</param>
    public VariableRootChangeEvent(
        IReadOnlyVariableRoot<TKey> root,
        TKey nodeKey,
        IVariableNodeEvent sourceEvent,
        VariableState previousState)
    {
        Variable = root;
        NodeKey = nodeKey;
        SourceEvent = sourceEvent;
        PreviousState = previousState;
        NewState = Variable.State;
    }

    /// <inheritdoc />
    public IReadOnlyVariableRoot<TKey> Variable { get; }

    /// <inheritdoc />
    public TKey NodeKey { get; }

    /// <inheritdoc />
    public IVariableNodeEvent SourceEvent { get; }

    /// <inheritdoc />
    public VariableState PreviousState { get; }

    /// <inheritdoc />
    public VariableState NewState { get; }

    Type IVariableRootEvent.KeyType => typeof( TKey );
    IReadOnlyVariableRoot IVariableRootEvent.Variable => Variable;
    object IVariableRootEvent.NodeKey => NodeKey;
    IVariableNode IVariableNodeEvent.Variable => Variable;
}
