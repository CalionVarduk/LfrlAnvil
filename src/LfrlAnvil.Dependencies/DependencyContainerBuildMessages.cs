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

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a collection of errors and warnings associated with the specified <see cref="ImplementorKey"/>.
/// </summary>
public readonly struct DependencyContainerBuildMessages
{
    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuildMessages"/> instance.
    /// </summary>
    /// <param name="implementorKey"><see cref="Dependencies.ImplementorKey"/> associated with given errors and warnings.</param>
    /// <param name="errors">Collection of errors.</param>
    /// <param name="warnings">Collection of warnings.</param>
    public DependencyContainerBuildMessages(ImplementorKey implementorKey, Chain<string> errors, Chain<string> warnings)
    {
        ImplementorKey = implementorKey;
        Errors = errors;
        Warnings = warnings;
    }

    /// <summary>
    /// <see cref="Dependencies.ImplementorKey"/> instance.
    /// </summary>
    public ImplementorKey ImplementorKey { get; }

    /// <summary>
    /// Errors associated with <see cref="ImplementorKey"/>.
    /// </summary>
    public Chain<string> Errors { get; }

    /// <summary>
    /// Warnings associated with <see cref="ImplementorKey"/>.
    /// </summary>
    public Chain<string> Warnings { get; }

    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuildMessages"/> instance.
    /// </summary>
    /// <param name="implementorKey"><see cref="Dependencies.ImplementorKey"/> associated with given errors.</param>
    /// <param name="messages">Collection of errors.</param>
    /// <returns>New <see cref="DependencyContainerBuildMessages"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyContainerBuildMessages CreateErrors(ImplementorKey implementorKey, Chain<string> messages)
    {
        return new DependencyContainerBuildMessages( implementorKey, messages, Chain<string>.Empty );
    }

    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuildMessages"/> instance.
    /// </summary>
    /// <param name="implementorKey"><see cref="Dependencies.ImplementorKey"/> associated with given warnings.</param>
    /// <param name="messages">Collection of warnings.</param>
    /// <returns>New <see cref="DependencyContainerBuildMessages"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyContainerBuildMessages CreateWarnings(ImplementorKey implementorKey, Chain<string> messages)
    {
        return new DependencyContainerBuildMessages( implementorKey, Chain<string>.Empty, messages );
    }
}
