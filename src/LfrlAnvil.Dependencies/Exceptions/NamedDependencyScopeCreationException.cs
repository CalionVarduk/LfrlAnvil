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

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to duplicated scope name.
/// </summary>
public class NamedDependencyScopeCreationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="NamedDependencyScopeCreationException"/> instance.
    /// </summary>
    /// <param name="parentScope">Parent scope.</param>
    /// <param name="name">Duplicated name.</param>
    public NamedDependencyScopeCreationException(IDependencyScope parentScope, string name)
        : base( Resources.NamedScopeAlreadyExists( parentScope, name ) )
    {
        ParentScope = parentScope;
        Name = name;
    }

    /// <summary>
    /// Parent scope.
    /// </summary>
    public IDependencyScope ParentScope { get; }

    /// <summary>
    /// Duplicated name.
    /// </summary>
    public string Name { get; }
}
