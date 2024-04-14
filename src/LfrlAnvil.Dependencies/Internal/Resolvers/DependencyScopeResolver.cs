using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyScopeResolver : DependencyResolver
{
    internal DependencyScopeResolver(ulong id)
        : base( id, typeof( IDependencyScope ), DependencyImplementorDisposalStrategy.RenounceOwnership() ) { }

    internal override DependencyLifetime Lifetime => DependencyLifetime.ScopedSingleton;

    [Pure]
    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        return scope;
    }
}
