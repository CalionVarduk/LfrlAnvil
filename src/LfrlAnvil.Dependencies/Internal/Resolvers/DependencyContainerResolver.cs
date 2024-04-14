using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyContainerResolver : DependencyResolver
{
    internal DependencyContainerResolver(ulong id)
        : base( id, typeof( IDependencyContainer ), DependencyImplementorDisposalStrategy.RenounceOwnership() ) { }

    internal override DependencyLifetime Lifetime => DependencyLifetime.Singleton;

    [Pure]
    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        return scope.InternalContainer;
    }
}
