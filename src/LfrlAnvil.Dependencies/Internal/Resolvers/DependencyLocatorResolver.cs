﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyLocatorResolver : DependencyResolver
{
    internal DependencyLocatorResolver(ulong id)
        : base( id, typeof( IDependencyLocator ), DependencyImplementorDisposalStrategy.RenounceOwnership(), null ) { }

    [Pure]
    protected override object CreateInternal(DependencyScope scope)
    {
        return scope.InternalLocator;
    }
}
