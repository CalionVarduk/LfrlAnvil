using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class SingletonDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;
    private object? _instance;

    internal SingletonDependencyResolver(ulong id, Type implementorType, Func<IDependencyScope, object> factory)
        : base( id, implementorType )
    {
        _factory = factory;
        _instance = null;
    }

    protected override object CreateInternal(DependencyScope scope)
    {
        if ( _instance is not null )
            return _instance;

        _instance = _factory( scope );

        if ( _instance is IDisposable disposable )
        {
            var rootLocator = scope.InternalContainer.InternalRootScope.InternalLocator;
            rootLocator.InternalDisposers.Add( new DependencyDisposer( disposable ) );
        }

        return _instance;
    }
}
