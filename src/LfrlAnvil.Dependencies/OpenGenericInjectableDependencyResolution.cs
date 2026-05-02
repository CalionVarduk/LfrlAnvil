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
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a custom parameter or member resolution for an open generic.
/// </summary>
/// <typeparam name="T">Dependency type.</typeparam>
public readonly struct OpenGenericInjectableDependencyResolution<T>
    where T : class, ICustomAttributeProvider
{
    private OpenGenericInjectableDependencyResolution(Func<T, bool> predicate, IDependencyKey implementorKey)
    {
        Predicate = predicate;
        ImplementorKey = implementorKey;
    }

    /// <summary>
    /// Predicate used for locating the desired parameter or member, denoted by the predicate returning <b>true</b>.
    /// </summary>
    public Func<T, bool> Predicate { get; }

    /// <summary>
    /// Custom implementor key.
    /// </summary>
    public IDependencyKey ImplementorKey { get; }

    /// <summary>
    /// Creates a new <see cref="OpenGenericInjectableDependencyResolution{T}"/> instance with custom implementor type.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired parameter or member, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="implementorKey">Custom implementor key.</param>
    /// <returns>New <see cref="OpenGenericInjectableDependencyResolution{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OpenGenericInjectableDependencyResolution<T> FromImplementorKey(Func<T, bool> predicate, IDependencyKey implementorKey)
    {
        return new OpenGenericInjectableDependencyResolution<T>( predicate, implementorKey );
    }
}
