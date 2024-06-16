﻿// Copyright 2024 Łukasz Furlepa
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
/// Represents a handler that is invoked when <see cref="IStateMachine{TState,TInput,TResult}"/> transitions to a different state.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TInput">Input type.</typeparam>
/// <typeparam name="TResult">Resul type.</typeparam>
public interface IStateTransitionHandler<TState, TInput, TResult>
    where TState : notnull
    where TInput : notnull
{
    /// <summary>
    /// Implementation of this handler's invocation.
    /// </summary>
    /// <param name="args">Invocation arguments.</param>
    /// <returns>Result provided by this handler.</returns>
    TResult Handle(StateTransitionHandlerArgs<TState, TInput, TResult> args);
}
