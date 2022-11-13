using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class TransientDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;

    internal TransientDependencyResolver(ulong id, Type implementorType, Func<IDependencyScope, object> factory)
        : base( id, implementorType )
    {
        _factory = factory;
    }

    protected override object CreateInternal(DependencyScope scope)
    {
        var result = _factory( scope );

        if ( result is IDisposable disposable )
            scope.InternalLocator.InternalDisposers.Add( new DependencyDisposer( disposable ) );

        return result;
    }
}
