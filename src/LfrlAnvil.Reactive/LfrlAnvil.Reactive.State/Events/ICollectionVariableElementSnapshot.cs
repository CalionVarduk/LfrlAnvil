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
/// Represents a type-erased snapshot of an <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
public interface ICollectionVariableElementSnapshot
{
    /// <summary>
    /// Element type.
    /// </summary>
    Type ElementType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Underlying element.
    /// </summary>
    object Element { get; }

    /// <summary>
    /// Previous state of the <see cref="Element"/>.
    /// </summary>
    CollectionVariableElementState PreviousState { get; }

    /// <summary>
    /// Current state of the <see cref="Element"/>.
    /// </summary>
    CollectionVariableElementState NewState { get; }

    /// <summary>
    /// Collection of validation errors before the change.
    /// </summary>
    IEnumerable PreviousErrors { get; }

    /// <summary>
    /// Collection of validation errors after the change.
    /// </summary>
    IEnumerable NewErrors { get; }

    /// <summary>
    /// Collection of validation warnings before the change.
    /// </summary>
    IEnumerable PreviousWarnings { get; }

    /// <summary>
    /// Collection of validation warnings after the change.
    /// </summary>
    IEnumerable NewWarnings { get; }
}

/// <summary>
/// Represents a generic snapshot of an <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
/// <typeparam name="TElement">Element type.</typeparam>
public interface ICollectionVariableElementSnapshot<out TElement> : ICollectionVariableElementSnapshot
{
    /// <summary>
    /// Underlying element.
    /// </summary>
    new TElement Element { get; }
}
