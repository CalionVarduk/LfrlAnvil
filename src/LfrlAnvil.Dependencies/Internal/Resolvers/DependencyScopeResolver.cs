﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyScopeResolver : DependencyResolver
{
    internal DependencyScopeResolver(ulong id)
        : base( id, typeof( IDependencyScope ), DependencyImplementorDisposalStrategy.RenounceOwnership(), null ) { }

    internal override DependencyLifetime Lifetime => DependencyLifetime.ScopedSingleton;

    [Pure]
    protected override object CreateCore(DependencyScope scope)
    {
        return scope;
    }
}
