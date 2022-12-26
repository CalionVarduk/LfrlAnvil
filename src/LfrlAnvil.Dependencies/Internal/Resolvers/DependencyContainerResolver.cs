﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class DependencyContainerResolver : DependencyResolver
{
    internal DependencyContainerResolver(ulong id)
        : base( id, typeof( IDependencyContainer ), DependencyImplementorDisposalStrategy.RenounceOwnership(), null ) { }

    [Pure]
    protected override object CreateInternal(DependencyScope scope)
    {
        return scope.InternalContainer;
    }
}