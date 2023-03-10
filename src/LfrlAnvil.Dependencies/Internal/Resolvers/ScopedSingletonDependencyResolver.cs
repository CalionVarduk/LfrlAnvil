using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedSingletonDependencyResolver : FactoryDependencyResolver
{
    internal ScopedSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, factory ) { }

    internal ScopedSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, expression ) { }

    internal override DependencyLifetime Lifetime => DependencyLifetime.ScopedSingleton;

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

        Assume.IsNotNull( Factory, nameof( Factory ) );
        result = Factory( scope );

        scope.ScopedInstancesByResolverId.Add( Id, result );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}
