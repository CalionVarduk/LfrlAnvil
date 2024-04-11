using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyContainerResolver : DependencyResolver
{
    internal DependencyContainerResolver(ulong id)
        : base( id, typeof( IDependencyContainer ), DependencyImplementorDisposalStrategy.RenounceOwnership(), null ) { }

    internal override DependencyLifetime Lifetime => DependencyLifetime.Singleton;

    [Pure]
    protected override object CreateCore(DependencyScope scope)
    {
        return scope.InternalContainer;
    }
}
