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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
///  Represents a dependency scope.
/// </summary>
public interface IDependencyScope
{
    /// <summary>
    /// Specifies whether or not this scope is a root scope.
    /// </summary>
    [MemberNotNullWhen( false, nameof( ParentScope ) )]
    bool IsRoot { get; }

    /// <summary>
    /// Optional name of this scope.
    /// If it is not null then this scope can be retrieved by invoking the <see cref="IDependencyContainer.GetScope(String)"/> method.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Specifies the <see cref="Environment.CurrentManagedThreadId"/> of the <see cref="Thread"/> that created this scope.
    /// </summary>
    int OriginalThreadId { get; }

    /// <summary>
    /// Depth of this scope. Equal to <b>0</b> when this scope is a root scope.
    /// </summary>
    int Level { get; }

    /// <summary>
    /// Specifies whether or not this scope has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// <see cref="IDependencyContainer"/> associated with this scope.
    /// </summary>
    IDependencyContainer Container { get; }

    /// <summary>
    /// Parent scope of this scope or null when this scope is a root scope.
    /// </summary>
    IDependencyScope? ParentScope { get; }

    /// <summary>
    /// Non-keyed dependency locator associated with this scope.
    /// </summary>
    IDependencyLocator Locator { get; }

    /// <summary>
    /// Gets the keyed locator associated with this scope by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Locator's key.</param>
    /// <typeparam name="TKey">Locator's key type.</typeparam>
    /// <returns>
    /// <see cref="IDependencyLocator{TKey}"/> instance associated with this scope and the provided <paramref name="key"/>.
    /// </returns>
    [Pure]
    IDependencyLocator<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull;

    /// <summary>
    /// Creates a new <see cref="IChildDependencyScope"/> from this scope.
    /// </summary>
    /// <param name="name">Optional child scope's name.</param>
    /// <returns>New <see cref="IChildDependencyScope"/> instance.</returns>
    /// <exception cref="NamedDependencyScopeCreationException">When the scope is named and that name already exists.</exception>
    [Pure]
    IChildDependencyScope BeginScope(string? name = null);

    /// <summary>
    /// Retrieves all child scopes created by this scope.
    /// </summary>
    /// <returns>Collection of child scopes created by this scope.</returns>
    [Pure]
    IDependencyScope[] GetChildren();
}
