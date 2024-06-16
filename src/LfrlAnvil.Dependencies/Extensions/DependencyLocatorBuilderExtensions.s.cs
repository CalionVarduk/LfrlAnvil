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

using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Extensions;

/// <summary>
/// Contains <see cref="IDependencyLocatorBuilder"/> extension methods.
/// </summary>
public static class DependencyLocatorBuilderExtensions
{
    /// <summary>
    /// Gets or creates a new <see cref="IDependencyImplementorBuilder"/> instance for the provided type.
    /// </summary>
    /// <param name="builder">Source dependency locator builder.</param>
    /// <typeparam name="T">Shared implementor type.</typeparam>
    /// <returns>New <see cref="IDependencyImplementorBuilder"/> instance or an existing instance.</returns>
    /// <exception cref="InvalidTypeRegistrationException">
    /// When the provided type is a generic type definition or contains generic parameters.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyImplementorBuilder AddSharedImplementor<T>(this IDependencyLocatorBuilder builder)
    {
        return builder.AddSharedImplementor( typeof( T ) );
    }

    /// <summary>
    /// Creates a new <see cref="IDependencyBuilder"/> instance for the provided type.
    /// </summary>
    /// <param name="builder">Source dependency locator builder.</param>
    /// <typeparam name="T">Dependency type.</typeparam>
    /// <returns>New <see cref="IDependencyBuilder"/> instance.</returns>
    /// <exception cref="InvalidTypeRegistrationException">
    /// When the provided type is a generic type definition or contains generic parameters.
    /// </exception>
    /// <remarks>
    /// This may also create an <see cref="IDependencyRangeBuilder"/> instance if it did not exist yet for the provided type.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyBuilder Add<T>(this IDependencyLocatorBuilder builder)
    {
        return builder.Add( typeof( T ) );
    }

    /// <summary>
    /// Gets or creates a new <see cref="IDependencyRangeBuilder"/> instance for the provided element type.
    /// </summary>
    /// <param name="builder">Source dependency locator builder.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="IDependencyRangeBuilder"/> instance or an existing instance.</returns>
    /// <exception cref="InvalidTypeRegistrationException">
    /// When the provided element type is a generic type definition or contains generic parameters.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyRangeBuilder GetDependencyRange<T>(this IDependencyLocatorBuilder builder)
    {
        return builder.GetDependencyRange( typeof( T ) );
    }
}
