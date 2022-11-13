using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedSingletonDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;

    internal ScopedSingletonDependencyResolver(
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

        var parentScope = scope.InternalParentScope;
        while ( parentScope is not null )
        {
            if ( parentScope.InternalLocator.ScopedInstancesByResolverId.TryGetValue( Id, out result ) )
            {
                locator.ScopedInstancesByResolverId.Add( Id, result );
                return result;
            }

            parentScope = parentScope.InternalParentScope;
        }

        result = _factory( scope );

        locator.ScopedInstancesByResolverId.Add( Id, result );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}
