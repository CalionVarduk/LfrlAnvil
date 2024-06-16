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
/// Represents a type-erased value change event emitted by an <see cref="IReadOnlyVariable"/>.
/// </summary>
public interface IVariableValueChangeEvent : IVariableNodeEvent
{
    /// <summary>
    /// Variable's value type.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Variable's validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariable Variable { get; }

    /// <summary>
    /// Value before the change.
    /// </summary>
    object? PreviousValue { get; }

    /// <summary>
    /// Value after the change.
    /// </summary>
    object? NewValue { get; }

    /// <summary>
    /// Specifies the source of this value change.
    /// </summary>
    VariableChangeSource Source { get; }
}

/// <summary>
/// Represents a generic value change event emitted by an <see cref="IReadOnlyVariable{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">Variable's value type.</typeparam>
public interface IVariableValueChangeEvent<TValue> : IVariableValueChangeEvent
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    new IReadOnlyVariable<TValue> Variable { get; }

    /// <summary>
    /// Value before the change.
    /// </summary>
    new TValue PreviousValue { get; }

    /// <summary>
    /// Value after the change.
    /// </summary>
    new TValue NewValue { get; }
}
