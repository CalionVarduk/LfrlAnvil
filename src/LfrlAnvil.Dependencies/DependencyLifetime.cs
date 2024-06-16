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

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Specifies available dependency lifetimes.
/// </summary>
public enum DependencyLifetime : byte
{
    /// <summary>
    /// Represents a dependency that creates new instances every time it gets resolved.
    /// </summary>
    Transient = 0,

    /// <summary>
    /// Represents a dependency that caches a single instance in the scope that resolved it.
    /// </summary>
    Scoped = 1,

    /// <summary>
    /// Represents a dependency that caches a single instance in the scope that resolved it, which gets reused in all descendant scopes.
    /// </summary>
    ScopedSingleton = 2,

    /// <summary>
    /// Represents a dependency that caches a single instance in the root scope, which gets reused in all other scopes.
    /// </summary>
    Singleton = 3
}
