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
