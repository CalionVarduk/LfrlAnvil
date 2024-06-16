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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Extensions;

/// <summary>
/// Contains <see cref="IDependencyImplementorBuilder"/> extension methods.
/// </summary>
public static class DependencyImplementorBuilderExtensions
{
    /// <summary>
    /// Specifies that this implementor's instances should be created
    /// by the best suited constructor of the provided type.
    /// </summary>
    /// <param name="builder">Source dependency implementor builder.</param>
    /// <param name="configuration">Optional configurator of the constructor invocation.</param>
    /// <typeparam name="T">Implementor's type.</typeparam>
    /// <returns><paramref name="builder"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDependencyImplementorBuilder FromType<T>(
        this IDependencyImplementorBuilder builder,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return builder.FromType( typeof( T ), configuration );
    }
}
