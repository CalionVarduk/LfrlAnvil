using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;

    internal ScopedDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        _factory = factory;
    }

    protected override object CreateInternal(DependencyScope scope)
    {
        var locator = scope.InternalLocator;
        if ( locator.ScopedInstancesByResolverId.TryGetValue( Id, out var result ) )
            return result;

        result = _factory( scope );

        locator.ScopedInstancesByResolverId.Add( Id, result );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}
