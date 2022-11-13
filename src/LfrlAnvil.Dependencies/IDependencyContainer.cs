namespace LfrlAnvil.Dependencies;

public interface IDependencyContainer
{
    IDependencyScope RootScope { get; }
    IDependencyScope ActiveScope { get; }
}
