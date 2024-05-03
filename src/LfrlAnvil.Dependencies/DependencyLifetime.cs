namespace LfrlAnvil.Dependencies;

/// <summary>
/// Specifies available dependency lifetimes.
/// </summary>
public enum DependencyLifetime : byte
{
    /// <summary>
    /// Represents a dependency that creates new instances every time it gets resolved.
    /// </summary>
    Transient = 0,

    /// <summary>
    /// Represents a dependency that caches a single instance in the scope that resolved it.
    /// </summary>
    Scoped = 1,

    /// <summary>
    /// Represents a dependency that caches a single instance in the scope that resolved it, which gets reused in all descendant scopes.
    /// </summary>
    ScopedSingleton = 2,

    /// <summary>
    /// Represents a dependency that caches a single instance in the root scope, which gets reused in all other scopes.
    /// </summary>
    Singleton = 3
}
