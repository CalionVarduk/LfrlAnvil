namespace LfrlAnvil.Dependencies.Bootstrapping;

public interface IDependencyContainerBootstrapper<in TBuilder>
    where TBuilder : IDependencyContainerBuilder
{
    void Bootstrap(TBuilder builder);
}
