using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyBuilder : IDependencyBuilder
{
    internal readonly DependencyLocatorBuilder LocatorBuilder;

    internal DependencyBuilder(DependencyLocatorBuilder locatorBuilder, Type dependencyType, DependencyLifetime lifetime)
    {
        LocatorBuilder = locatorBuilder;
        DependencyType = dependencyType;
        Lifetime = lifetime;
        InternalSharedImplementorKey = null;
        Implementor = null;
    }

    public Type DependencyType { get; }
    public DependencyLifetime Lifetime { get; private set; }
    public IDependencyImplementorBuilder? Implementor { get; private set; }
    public ISharedDependencyImplementorKey? SharedImplementorKey => InternalSharedImplementorKey;
    internal IInternalSharedDependencyImplementorKey? InternalSharedImplementorKey { get; set; }

    public IDependencyBuilder SetLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime, nameof( lifetime ) );
        Lifetime = lifetime;
        return this;
    }

    public IDependencyBuilder FromSharedImplementor(Type type, Action<ISharedDependencyImplementorOptions>? configuration = null)
    {
        InternalSharedImplementorKey = CreateSharedImplementorKey( type, configuration );
        Implementor = null;
        return this;
    }

    public IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory)
    {
        if ( Implementor is null )
        {
            InternalSharedImplementorKey = null;
            Implementor = new DependencyImplementorBuilder( DependencyType, LocatorBuilder.DefaultDisposalStrategy );
        }

        Implementor.FromFactory( factory );
        return Implementor;
    }

    [Pure]
    private IInternalSharedDependencyImplementorKey CreateSharedImplementorKey(
        Type type,
        Action<ISharedDependencyImplementorOptions>? configuration)
    {
        var defaultKey = LocatorBuilder.CreateImplementorKey( type );
        if ( configuration is null )
            return defaultKey;

        var options = new SharedDependencyImplementorOptions( defaultKey );
        configuration( options );

        var sharedImplementorKey = options.Key as IInternalSharedDependencyImplementorKey;
        Ensure.IsNotNull( sharedImplementorKey, nameof( sharedImplementorKey ) );
        return sharedImplementorKey;
    }
}
