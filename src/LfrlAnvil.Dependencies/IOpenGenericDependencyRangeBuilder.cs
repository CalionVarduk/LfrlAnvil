// Copyright 2026 Łukasz Furlepa
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
/// Represents an open generic dependency range resolution builder.
/// </summary>
public interface IOpenGenericDependencyRangeBuilder
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
    /// Collection of all <see cref="IOpenGenericDependencyBuilder"/> instances associated with this range.
    /// </summary>
    IReadOnlyList<IOpenGenericDependencyBuilder> Elements { get; }

    /// <summary>
    /// Collection of all closed type <see cref="IDependencyRangeBuilder"/> instances associated with this range.
    /// </summary>
    IReadOnlyList<IDependencyRangeBuilder> ClosedBuilders { get; }

    /// <summary>
    /// Creates a new <see cref="IOpenGenericDependencyBuilder"/> instance and registers it in this range.
    /// </summary>
    /// <returns>New <see cref="IOpenGenericDependencyBuilder"/> instance.</returns>
    IOpenGenericDependencyBuilder Add();

    /// <summary>
    /// Gets the last <see cref="IOpenGenericDependencyBuilder"/> instance registered in this range.
    /// </summary>
    /// <returns>
    /// <see cref="IOpenGenericDependencyBuilder"/> instance of the last registered element or null when this range is empty.
    /// </returns>
    [Pure]
    IOpenGenericDependencyBuilder? TryGetLast();

    /// <summary>
    /// Sets the <see cref="OnResolvingCallback"/> for this instance.
    /// </summary>
    /// <param name="callback">Delegate to set.</param>
    /// <returns><b>this</b>.</returns>
    IOpenGenericDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
