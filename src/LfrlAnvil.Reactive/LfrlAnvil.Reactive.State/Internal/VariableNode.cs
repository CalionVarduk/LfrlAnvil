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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State.Internal;

/// <inheritdoc cref="IVariableNode" />
public abstract class VariableNode : IVariableNode
{
    /// <summary>
    /// Creates a new <see cref="VariableNode"/> instance.
    /// </summary>
    protected VariableNode()
    {
        Parent = null;
    }

    /// <inheritdoc />
    public IVariableNode? Parent { get; private set; }

    /// <inheritdoc />
    public abstract VariableState State { get; }

    /// <inheritdoc />
    public abstract IEventStream<IVariableNodeEvent> OnChange { get; }

    /// <inheritdoc />
    public abstract IEventStream<IVariableNodeEvent> OnValidate { get; }

    /// <inheritdoc />
    [Pure]
    public abstract IEnumerable<IVariableNode> GetChildren();

    /// <summary>
    /// Sets this variable as <see cref="Parent"/> of the <paramref name="other"/> variable.
    /// </summary>
    /// <param name="other">Child variable.</param>
    /// <exception cref="ArgumentException">
    /// When child variable's already has a different parent or child variable and this variable are the same.
    /// </exception>
    protected void SetAsParentOf(VariableNode other)
    {
        if ( ! ReferenceEquals( other.Parent, this ) )
        {
            Ensure.IsNull( other.Parent );
            Ensure.NotRefEquals( this, other );
            other.Parent = this;
        }
    }

    /// <summary>
    /// Calculates new <see cref="VariableState"/>.
    /// </summary>
    /// <param name="current">Current state.</param>
    /// <param name="value">State to set.</param>
    /// <param name="enabled">
    /// Specifies whether the <paramref name="value"/> should be included (when set to <b>true</b>) or excluded from the result.
    /// </param>
    /// <returns>Calculated <see cref="VariableState"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static VariableState CreateState(VariableState current, VariableState value, bool enabled)
    {
        return enabled ? current | value : current & ~value;
    }
}
