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
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased read-only collection variable.
/// </summary>
public interface IReadOnlyCollectionVariable : IVariableNode
{
    /// <summary>
    /// Key type.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Element type.
    /// </summary>
    Type ElementType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Current collection of elements.
    /// </summary>
    ICollectionVariableElements Elements { get; }

    /// <summary>
    /// Initial collection of elements.
    /// </summary>
    IEnumerable InitialElements { get; }

    /// <summary>
    /// Collection of current validation errors.
    /// </summary>
    IEnumerable Errors { get; }

    /// <summary>
    /// Collection of current validation warnings.
    /// </summary>
    IEnumerable Warnings { get; }

    /// <summary>
    /// Event stream that emits events when variable's elements change.
    /// </summary>
    new IEventStream<ICollectionVariableChangeEvent> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<ICollectionVariableValidationEvent> OnValidate { get; }
}

/// <summary>
/// Represents a generic read-only collection variable.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public interface IReadOnlyCollectionVariable<TKey, TElement> : IReadOnlyCollectionVariable
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Element's key selector.
    /// </summary>
    Func<TElement, TKey> KeySelector { get; }

    /// <summary>
    /// Current collection of elements.
    /// </summary>
    new ICollectionVariableElements<TKey, TElement> Elements { get; }

    /// <summary>
    /// Initial collection of elements.
    /// </summary>
    new IReadOnlyDictionary<TKey, TElement> InitialElements { get; }

    /// <summary>
    /// Event stream that emits events when variable's elements change.
    /// </summary>
    new IEventStream<ICollectionVariableChangeEvent<TKey, TElement>> OnChange { get; }
}

/// <summary>
/// Represents a generic read-only collection variable.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> : IReadOnlyCollectionVariable<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
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
    /// Current collection of elements.
    /// </summary>
    new ICollectionVariableElements<TKey, TElement, TValidationResult> Elements { get; }

    /// <summary>
    /// Collection of elements validator that marks result as errors.
    /// </summary>
    IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult> ErrorsValidator { get; }

    /// <summary>
    /// Collection of elements validator that marks result as warnings.
    /// </summary>
    IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult> WarningsValidator { get; }

    /// <summary>
    /// Event stream that emits events when variable's elements change.
    /// </summary>
    new IEventStream<CollectionVariableChangeEvent<TKey, TElement, TValidationResult>> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<CollectionVariableValidationEvent<TKey, TElement, TValidationResult>> OnValidate { get; }
}
