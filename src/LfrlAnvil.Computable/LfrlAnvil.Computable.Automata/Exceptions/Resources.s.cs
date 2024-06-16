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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata.Exceptions;

internal static class Resources
{
    internal const string InitialStateIsMissing = "Initial state is missing.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TransitionAlreadyExists<TState, TInput>(TState source, TInput input)
    {
        return $"Outgoing transition from '{source}' state for '{input}' input already exists.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TransitionDoesNotExist<TState, TInput>(TState source, TInput input)
    {
        return $"Outgoing transition from '{source}' state for '{input}' input does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StateDoesNotExist<TState>(TState state)
    {
        return $"State '{state}' does not exist.";
    }
}
