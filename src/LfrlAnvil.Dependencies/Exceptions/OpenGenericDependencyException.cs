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

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred during open generic dependency resolution.
/// </summary>
public class OpenGenericDependencyException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="OpenGenericDependencyException"/> instance.
    /// </summary>
    /// <param name="dependencyType">Dependency type.</param>
    /// <param name="message">Error message.</param>
    public OpenGenericDependencyException(Type dependencyType, string message)
        : base( message )
    {
        DependencyType = dependencyType;
    }

    /// <summary>
    /// Dependency type.
    /// </summary>
    public Type DependencyType { get; }
}
