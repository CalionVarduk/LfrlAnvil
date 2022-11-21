using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyConstructor
{
    ConstructorInfo? Info { get; }
    IDependencyConstructorInvocationOptions InvocationOptions { get; }
}
