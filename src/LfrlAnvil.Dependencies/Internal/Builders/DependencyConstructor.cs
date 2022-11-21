using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyConstructor : IDependencyConstructor
{
    internal DependencyConstructor(DependencyLocatorBuilder locatorBuilder, ConstructorInfo? info)
    {
        Info = info;
        InternalInvocationOptions = new DependencyConstructorInvocationOptions( locatorBuilder );
    }

    public ConstructorInfo? Info { get; }
    public IDependencyConstructorInvocationOptions InvocationOptions => InternalInvocationOptions;
    internal DependencyConstructorInvocationOptions InternalInvocationOptions { get; }
}
