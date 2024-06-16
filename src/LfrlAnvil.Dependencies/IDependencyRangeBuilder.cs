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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency range resolution builder.
/// </summary>
public interface IDependencyRangeBuilder
{
    /// <summary>
    /// Element type.
    /// </summary>
    Type DependencyType { get; }

    /// <summary>
    /// Specifies an optional callback that gets invoked right before the dependency instance is resolved.
    /// The first argument denotes the type of a dependency to resolve and
    /// the second argument is the scope that is resolving the dependency.
    /// </summary>
    Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    /// <summary>
    /// Collection of all <see cref="IDependencyBuilder"/> instances associated with this range.
    /// </summary>
    IReadOnlyList<IDependencyBuilder> Elements { get; }

    /// <summary>
    /// Creates a new <see cref="IDependencyBuilder"/> instance and registers it in this range.
    /// </summary>
    /// <returns>New <see cref="IDependencyBuilder"/> instance.</returns>
    IDependencyBuilder Add();

    /// <summary>
    /// Gets the last <see cref="IDependencyBuilder"/> instance registered in this range.
    /// </summary>
    /// <returns><see cref="IDependencyBuilder"/> instance of the last registered element or null when this range is empty.</returns>
    [Pure]
    IDependencyBuilder? TryGetLast();

    /// <summary>
    /// Sets the <see cref="OnResolvingCallback"/> for this instance.
    /// </summary>
    /// <param name="callback">Delegate to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
