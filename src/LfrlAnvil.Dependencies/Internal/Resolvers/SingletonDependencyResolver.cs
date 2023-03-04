using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class SingletonDependencyResolver : FactoryDependencyResolver
{
    private object? _instance;

    internal SingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, factory )
    {
        _instance = null;
    }

    internal SingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, expression )
    {
        _instance = null;
    }

    protected override object CreateInternal(DependencyScope scope)
    {
        if ( _instance is not null )
            return _instance;

        Assume.IsNotNull( Factory, nameof( Factory ) );
        _instance = Factory( scope );
        ClearFactory();

        var rootScope = scope.InternalContainer.InternalRootScope;
        SetupDisposalStrategy( rootScope, _instance );
        return _instance;
    }
}
