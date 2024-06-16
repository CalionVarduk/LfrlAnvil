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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

/// <summary>
/// A lightweight generic, potentially parameterized, validation message.
/// </summary>
/// <typeparam name="TResource">Resource type.</typeparam>
public readonly struct ValidationMessage<TResource>
{
    /// <summary>
    /// Creates a new <see cref="ValidationMessage{TResource}"/> instance.
    /// </summary>
    /// <param name="resource">Resource.</param>
    /// <param name="parameters">Optional range of parameters.</param>
    public ValidationMessage(TResource resource, params object?[]? parameters)
    {
        Resource = resource;
        Parameters = parameters;
    }

    /// <summary>
    /// Resource or key of this message.
    /// </summary>
    public TResource Resource { get; }

    /// <summary>
    /// Optional range of parameters.
    /// </summary>
    public object?[]? Parameters { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ValidationMessage{TResource}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Resource )}: '{Resource}', {nameof( Parameters )}: {Parameters?.Length ?? 0}";
    }
}
