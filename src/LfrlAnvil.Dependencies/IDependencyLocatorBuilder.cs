using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a type-erased <see cref="IDependencyLocator"/> builder.
/// </summary>
public interface IDependencyLocatorBuilder
{
    /// <summary>
    /// Specifies the default dependency lifetime for this locator.
    /// </summary>
    DependencyLifetime DefaultLifetime { get; }

    /// <summary>
    /// Specifies the default implementor disposal strategy for this locator.
    /// </summary>
    DependencyImplementorDisposalStrategy DefaultDisposalStrategy { get; }

    /// <summary>
    /// Key type of this locator or null when it is not keyed.
    /// </summary>
    Type? KeyType { get; }

    /// <summary>
    /// Key value of this locator or null when it is not keyed.
    /// </summary>
    object? Key { get; }

    /// <summary>
    /// Specifies whether or not this locator is keyed.
    /// </summary>
    bool IsKeyed { get; }

    /// <summary>
    /// Gets or creates a new <see cref="IDependencyImplementorBuilder"/> instance for the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Shared implementor type.</param>
    /// <returns>New <see cref="IDependencyImplementorBuilder"/> instance or an existing instance.</returns>
    /// <exception cref="InvalidTypeRegistrationException">
    /// When the provided <paramref name="type"/> is a generic type definition or contains generic parameters.
    /// </exception>
    IDependencyImplementorBuilder AddSharedImplementor(Type type);

    /// <summary>
    /// Creates a new <see cref="IDependencyBuilder"/> instance for the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Dependency type.</param>
    /// <returns>New <see cref="IDependencyBuilder"/> instance.</returns>
    /// <exception cref="InvalidTypeRegistrationException">
    /// When the provided <paramref name="type"/> is a generic type definition or contains generic parameters.
    /// </exception>
    /// <remarks>
    /// This may also create an <see cref="IDependencyRangeBuilder"/> instance if it did not exist yet
    /// for the provided <paramref name="type"/>.
    /// </remarks>
    IDependencyBuilder Add(Type type);

    /// <summary>
    /// Sets the <see cref="DefaultLifetime"/> of this instance.
    /// </summary>
    /// <param name="lifetime">Default lifetime to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyLocatorBuilder SetDefaultLifetime(DependencyLifetime lifetime);

    /// <summary>
    /// Sets the <see cref="DefaultDisposalStrategy"/> of this instance.
    /// </summary>
    /// <param name="strategy">Default strategy to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyLocatorBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    /// <summary>
    /// Attempts to get an <see cref="IDependencyImplementorBuilder"/> instance associated with the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get the shared implementor builder for.</param>
    /// <returns>
    /// <see cref="IDependencyImplementorBuilder"/> instance associated with the provided <paramref name="type"/>
    /// or null when it does not exist.
    /// </returns>
    [Pure]
    IDependencyImplementorBuilder? TryGetSharedImplementor(Type type);

    /// <summary>
    /// Gets or creates a new <see cref="IDependencyRangeBuilder"/> instance for the provided element <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Element type.</param>
    /// <returns>New <see cref="IDependencyRangeBuilder"/> instance or an existing instance.</returns>
    /// <exception cref="InvalidTypeRegistrationException">
    /// When the provided element <paramref name="type"/> is a generic type definition or contains generic parameters.
    /// </exception>
    [Pure]
    IDependencyRangeBuilder GetDependencyRange(Type type);
}

/// <summary>
/// Represents a generic keyed <see cref="IDependencyLocator"/> builder.
/// </summary>
public interface IDependencyLocatorBuilder<out TKey> : IDependencyLocatorBuilder
    where TKey : notnull
{
    /// <summary>
    /// Key value of this locator.
    /// </summary>
    new TKey Key { get; }
}
