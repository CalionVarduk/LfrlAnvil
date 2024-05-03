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
