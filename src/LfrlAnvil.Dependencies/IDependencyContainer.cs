using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyContainer
{
    IDependencyScope RootScope { get; }

    [Pure]
    IDependencyScope? TryGetScope(string name);

    [Pure]
    IDependencyScope GetScope(string name);
}
