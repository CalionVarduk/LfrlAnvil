namespace LfrlAnvil.Dependencies.Bootstrapping;

/// <summary>
/// Represents an <see cref="IDependencyContainerBuilder"/> bootstrapper that contains a set of dependency definitions.
/// </summary>
/// <typeparam name="TBuilder">Dependency container builder type.</typeparam>
public interface IDependencyContainerBootstrapper<in TBuilder>
    where TBuilder : IDependencyContainerBuilder
{
    /// <summary>
    /// Populates the provided dependency container <paramref name="builder"/>
    /// with the set of dependency definitions stored by this bootstrapper.
    /// </summary>
    /// <param name="builder">Dependency container builder to populate.</param>
    void Bootstrap(TBuilder builder);
}
