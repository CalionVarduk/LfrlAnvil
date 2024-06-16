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

namespace LfrlAnvil.Reactive.State.Extensions;

/// <summary>
/// Contains <see cref="IVariableNode"/> extension methods.
/// </summary>
public static class VariableNodeExtensions
{
    /// <summary>
    /// Checks whether or not the <paramref name="node"/> contains <see cref="VariableState.Changed"/> state.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when the <paramref name="node"/> contains expected state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsChanged(this IVariableNode node)
    {
        return (node.State & VariableState.Changed) != VariableState.Default;
    }

    /// <summary>
    /// Checks whether or not the <paramref name="node"/> contains <see cref="VariableState.Invalid"/> state.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when the <paramref name="node"/> contains expected state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInvalid(this IVariableNode node)
    {
        return (node.State & VariableState.Invalid) != VariableState.Default;
    }

    /// <summary>
    /// Checks whether or not the <paramref name="node"/> contains <see cref="VariableState.Warning"/> state.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when the <paramref name="node"/> contains expected state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsWarning(this IVariableNode node)
    {
        return (node.State & VariableState.Warning) != VariableState.Default;
    }

    /// <summary>
    /// Checks whether or not the <paramref name="node"/> contains <see cref="VariableState.ReadOnly"/> state.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when the <paramref name="node"/> contains expected state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsReadOnly(this IVariableNode node)
    {
        return (node.State & VariableState.ReadOnly) != VariableState.Default;
    }

    /// <summary>
    /// Checks whether or not the <paramref name="node"/> contains <see cref="VariableState.Disposed"/> state.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when the <paramref name="node"/> contains expected state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDisposed(this IVariableNode node)
    {
        return (node.State & VariableState.Disposed) != VariableState.Default;
    }

    /// <summary>
    /// Checks whether or not the <paramref name="node"/> contains <see cref="VariableState.Dirty"/> state.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when the <paramref name="node"/> contains expected state, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDirty(this IVariableNode node)
    {
        return (node.State & VariableState.Dirty) != VariableState.Default;
    }
}
