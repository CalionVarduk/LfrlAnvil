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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents the result of <see cref="IDependencyContainerBuilder.TryBuild()"/> invocation.
/// </summary>
/// <typeparam name="TContainer">Dependency container type.</typeparam>
public readonly struct DependencyContainerBuildResult<TContainer>
    where TContainer : class, IDisposableDependencyContainer
{
    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuildResult{TContainer}"/> instance.
    /// </summary>
    /// <param name="container">Created container instance or null when <paramref name="messages"/> contains errors.</param>
    /// <param name="messages">Build errors and warnings.</param>
    public DependencyContainerBuildResult(TContainer? container, Chain<DependencyContainerBuildMessages> messages)
    {
        Container = container;
        Messages = messages;
    }

    /// <summary>
    /// Specifies whether or not this result has a valid <see cref="Container"/>.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Container ) )]
    public bool IsOk => Container is not null;

    /// <summary>
    /// Created container instance or null when <see cref="Messages"/> contains errors.
    /// </summary>
    public TContainer? Container { get; }

    /// <summary>
    /// Build errors and warnings.
    /// </summary>
    public Chain<DependencyContainerBuildMessages> Messages { get; }

    /// <summary>
    /// Returns <see cref="Container"/> if it is not null.
    /// </summary>
    /// <returns><see cref="Container"/>.</returns>
    /// <exception cref="DependencyContainerBuildException">When <see cref="Container"/> is null.</exception>
    [Pure]
    public TContainer GetContainerOrThrow()
    {
        if ( IsOk )
            return Container;

        throw new DependencyContainerBuildException( Messages );
    }
}
