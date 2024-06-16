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

using System.Collections.Generic;

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a single node of <see cref="IStateMachine{TState,TInput,TResult}"/>.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IStateMachineNode<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Value of this state.
    /// </summary>
    TState Value { get; }

    /// <summary>
    /// Type of this state.
    /// </summary>
    StateMachineNodeType Type { get; }

    /// <summary>
    /// Dictionary of available transitions from this state.
    /// </summary>
    IReadOnlyDictionary<TInput, IStateMachineTransition<TState, TInput, TResult>> Transitions { get; }
}
