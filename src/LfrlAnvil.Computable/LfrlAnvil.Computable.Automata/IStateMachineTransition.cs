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

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents a transition to the specified <see cref="IStateMachineNode{TState,TInput,TResult}"/>.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IStateMachineTransition<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Destination state.
    /// </summary>
    IStateMachineNode<TState, TInput, TResult> Destination { get; }

    /// <summary>
    /// Optional handler invoked during the transition to <see cref="Destination"/>.
    /// </summary>
    IStateTransitionHandler<TState, TInput, TResult>? Handler { get; }
}
