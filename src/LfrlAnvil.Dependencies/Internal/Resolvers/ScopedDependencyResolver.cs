using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;

    internal ScopedDependencyResolver(ulong id, Type implementorType, Func<IDependencyScope, object> factory)
        : base( id, implementorType )
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
        if ( result is IDisposable disposable )
            locator.InternalDisposers.Add( new DependencyDisposer( disposable ) );

        return result;
    }
}
