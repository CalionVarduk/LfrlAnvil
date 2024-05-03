using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency container.
/// </summary>
public interface IDependencyContainer
{
    /// <summary>
    /// Specifies the root scope of this container.
    /// </summary>
    IDependencyScope RootScope { get; }

    /// <summary>
    /// Attempts to return the named scope.
    /// </summary>
    /// <param name="name">Scope's name.</param>
    /// <returns>Named <see cref="IDependencyScope"/> instance or null when named scope does not exist.</returns>
    [Pure]
    IDependencyScope? TryGetScope(string name);

    /// <summary>
    /// Returns the named scope.
    /// </summary>
    /// <param name="name">Scope's name.</param>
    /// <returns>Named <see cref="IDependencyScope"/> instance.</returns>
    /// <exception cref="DependencyScopeNotFoundException">When named scope does not exist.</exception>
    [Pure]
    IDependencyScope GetScope(string name);
}
