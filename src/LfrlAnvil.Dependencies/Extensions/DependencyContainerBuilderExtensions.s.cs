﻿// Copyright 2024 Łukasz Furlepa
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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies.Extensions;

/// <summary>
/// Contains <see cref="IDependencyContainerBuilder"/> extension methods.
/// </summary>
public static class DependencyContainerBuilderExtensions
{
    /// <summary>
    /// Builds a <see cref="DependencyContainer"/> instance.
    /// </summary>
    /// <returns>Built dependency container.</returns>
    /// <exception cref="DependencyContainerBuildException">When container could not be built.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DependencyContainer Build(this DependencyContainerBuilder builder)
    {
        return builder.TryBuild().GetContainerOrThrow();
    }

    /// <summary>
    /// Builds an <see cref="IDisposableDependencyContainer"/> instance.
    /// </summary>
    /// <returns>Built dependency container.</returns>
    /// <exception cref="DependencyContainerBuildException">When container could not be built.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IDisposableDependencyContainer Build(this IDependencyContainerBuilder builder)
    {
        return builder.TryBuild().GetContainerOrThrow();
    }
}
