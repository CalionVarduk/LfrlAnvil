using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedDependencyResolver : FactoryDependencyResolver
{
    internal ScopedDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, factory ) { }

    internal ScopedDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, expression ) { }

    protected override object CreateInternal(DependencyScope scope)
    {
        if ( scope.ScopedInstancesByResolverId.TryGetValue( Id, out var result ) )
            return result;

        Assume.IsNotNull( Factory, nameof( Factory ) );
        result = Factory( scope );

        scope.ScopedInstancesByResolverId.Add( Id, result );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}
