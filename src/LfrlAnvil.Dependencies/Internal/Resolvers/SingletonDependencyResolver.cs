using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class SingletonDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;
    private object? _instance;

    internal SingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        _factory = factory;
        _instance = null;
    }

    protected override object CreateInternal(DependencyScope scope)
    {
        if ( _instance is not null )
            return _instance;

        _instance = _factory( scope );
        SetupDisposalStrategy( scope.InternalContainer.InternalRootScope, _instance );
        return _instance;
    }
}
