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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Computable.Automata;

/// <inheritdoc cref="IStateMachineTransition{TState,TInput,TResult}" />
public sealed class StateMachineTransition<TState, TInput, TResult> : IStateMachineTransition<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    internal StateMachineTransition(
        IStateMachineNode<TState, TInput, TResult> destination,
        IStateTransitionHandler<TState, TInput, TResult>? handler)
    {
        Destination = destination;
        Handler = handler;
    }

    /// <inheritdoc />
    public IStateMachineNode<TState, TInput, TResult> Destination { get; }

    /// <inheritdoc />
    public IStateTransitionHandler<TState, TInput, TResult>? Handler { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="StateMachineTransition{TState,TInput,TResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"=> {Destination}{(Handler is null ? string.Empty : $" ({nameof( Handler )})")}";
    }
}
