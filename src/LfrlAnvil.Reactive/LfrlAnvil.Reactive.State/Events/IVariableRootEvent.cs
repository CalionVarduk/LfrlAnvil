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
/// Represents a type-erased event emitted by an <see cref="IReadOnlyVariableRoot"/>.
/// </summary>
public interface IVariableRootEvent : IVariableNodeEvent
{
    /// <summary>
    /// Child node's key type.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariableRoot Variable { get; }

    /// <summary>
    /// Key of the child node that caused this event.
    /// </summary>
    object NodeKey { get; }

    /// <summary>
    /// Source child node event.
    /// </summary>
    IVariableNodeEvent SourceEvent { get; }
}

/// <summary>
/// Represents a generic event emitted by an <see cref="IReadOnlyVariableRoot{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">Child node's key type.</typeparam>
public interface IVariableRootEvent<TKey> : IVariableRootEvent
    where TKey : notnull
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariableRoot<TKey> Variable { get; }

    /// <summary>
    /// Key of the child node that caused this event.
    /// </summary>
    new TKey NodeKey { get; }
}
