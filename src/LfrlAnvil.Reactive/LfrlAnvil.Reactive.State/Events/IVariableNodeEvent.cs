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

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a type-erased event emitted by an <see cref="IVariableNode"/>.
/// </summary>
public interface IVariableNodeEvent
{
    /// <summary>
    /// Variable node that emitted this event.
    /// </summary>
    IVariableNode Variable { get; }

    /// <summary>
    /// Previous state of the <see cref="Variable"/>.
    /// </summary>
    VariableState PreviousState { get; }

    /// <summary>
    /// Current state of the <see cref="Variable"/>.
    /// </summary>
    VariableState NewState { get; }
}
