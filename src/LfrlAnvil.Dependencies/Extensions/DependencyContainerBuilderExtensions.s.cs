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
