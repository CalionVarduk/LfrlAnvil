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

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a type-erased dependency key.
/// </summary>
public interface IDependencyKey : IEquatable<IDependencyKey>
{
    /// <summary>
    /// Dependency's type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Keyed locator's key type or null when this key does not use keyed locators.
    /// </summary>
    Type? KeyType { get; }

    /// <summary>
    /// Keyed locator's key value or null when this key does not use keyed locators.
    /// </summary>
    object? Key { get; }

    /// <summary>
    /// Specifies whether or not this key uses keyed locators.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Key ) )]
    [MemberNotNullWhen( true, nameof( KeyType ) )]
    public bool IsKeyed { get; }
}

/// <summary>
/// Represents a generic dependency key that uses keyed locators.
/// </summary>
public interface IDependencyKey<out TKey> : IDependencyKey
    where TKey : notnull
{
    /// <summary>
    /// Keyed locator's key type.
    /// </summary>
    new Type KeyType { get; }

    /// <summary>
    /// Keyed locator's key value.
    /// </summary>
    new TKey Key { get; }
}
