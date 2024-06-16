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
/// Represents an error that occurred due to circular dependency reference detection during dependency resolution.
/// </summary>
public class CircularDependencyReferenceException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="CircularDependencyReferenceException"/> instance.
    /// </summary>
    /// <param name="dependencyType">Dependency type.</param>
    /// <param name="implementorType">Implementor type.</param>
    /// <param name="innerException">Optional inner exception.</param>
    public CircularDependencyReferenceException(
        Type dependencyType,
        Type implementorType,
        CircularDependencyReferenceException? innerException = null)
        : base( Resources.CircularDependencyReference( dependencyType, implementorType ), innerException )
    {
        DependencyType = dependencyType;
        ImplementorType = implementorType;
    }

    /// <summary>
    /// Dependency type.
    /// </summary>
    public Type DependencyType { get; }

    /// <summary>
    /// Implementor type.
    /// </summary>
    public Type ImplementorType { get; }
}
