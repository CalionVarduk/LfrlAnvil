﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents an <see cref="IDisposableDependencyContainer"/> builder.
/// </summary>
public interface IDependencyContainerBuilder : IDependencyLocatorBuilder
{
    /// <summary>
    /// Represents the current configuration.
    /// </summary>
    IDependencyContainerConfigurationBuilder Configuration { get; }

    /// <summary>
    /// Sets the <see cref="IDependencyLocatorBuilder.DefaultLifetime"/> of this instance.
    /// </summary>
    /// <param name="lifetime">Default lifetime to set.</param>
    /// <returns><b>this</b>.</returns>
    new IDependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime);

    /// <summary>
    /// Sets the <see cref="IDependencyLocatorBuilder.DefaultDisposalStrategy"/> of this instance.
    /// </summary>
    /// <param name="strategy">Default strategy to set.</param>
    /// <returns><b>this</b>.</returns>
    new IDependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    /// <summary>
    /// Gets or adds a keyed <see cref="IDependencyLocatorBuilder{TKey}"/> instance.
    /// </summary>
    /// <param name="key">Locator's key.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <returns>Existing <see cref="IDependencyLocatorBuilder{TKey}"/> or an added one.</returns>
    [Pure]
    IDependencyLocatorBuilder<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull;

    /// <summary>
    /// Attempts to build an <see cref="IDisposableDependencyContainer"/> instance.
    /// </summary>
    /// <returns><see cref="DependencyContainerBuildResult{TContainer}"/> instance with the build attempt result.</returns>
    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> TryBuild();
}
