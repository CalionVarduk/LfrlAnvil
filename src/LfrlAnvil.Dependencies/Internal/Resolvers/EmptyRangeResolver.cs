using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class EmptyRangeResolver : DependencyResolver
{
    private readonly object _instance;

    internal EmptyRangeResolver(ulong id, Type implementorType, object instance)
        : base( id, implementorType, DependencyImplementorDisposalStrategy.RenounceOwnership() )
    {
        _instance = instance;
    }

    internal override DependencyLifetime Lifetime => DependencyLifetime.Transient;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        return _instance;
    }
}
