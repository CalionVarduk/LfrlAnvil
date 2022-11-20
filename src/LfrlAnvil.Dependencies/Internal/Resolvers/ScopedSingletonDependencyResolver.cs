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
        if ( scope.ScopedInstancesByResolverId.TryGetValue( Id, out var result ) )
            return result;

        var parentScope = scope.InternalParentScope;
        while ( parentScope is not null )
        {
            if ( parentScope.ScopedInstancesByResolverId.TryGetValue( Id, out result ) )
            {
                scope.ScopedInstancesByResolverId.Add( Id, result );
                return result;
            }

            parentScope = parentScope.InternalParentScope;
        }

        result = _factory( scope );

        scope.ScopedInstancesByResolverId.Add( Id, result );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}
