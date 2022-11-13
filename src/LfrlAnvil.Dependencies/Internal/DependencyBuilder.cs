using System;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class DependencyBuilder : IDependencyBuilder
{
    internal readonly DependencyLocatorBuilder LocatorBuilder;

    internal DependencyBuilder(DependencyLocatorBuilder locatorBuilder, Type dependencyType, DependencyLifetime lifetime)
    {
        LocatorBuilder = locatorBuilder;
        DependencyType = dependencyType;
        Lifetime = lifetime;
        SharedImplementorType = null;
        Implementor = null;
    }

    public Type DependencyType { get; }
    public DependencyLifetime Lifetime { get; private set; }
    public Type? SharedImplementorType { get; private set; }
    public IDependencyImplementorBuilder? Implementor { get; private set; }

    public IDependencyBuilder SetLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime, nameof( lifetime ) );
        Lifetime = lifetime;
        return this;
    }

    public IDependencyBuilder FromSharedImplementor(Type type)
    {
        SharedImplementorType = type;
        Implementor = null;
        return this;
    }

    public IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory)
    {
        if ( Implementor is null )
        {
            SharedImplementorType = null;
            Implementor = new DependencyImplementorBuilder( DependencyType, LocatorBuilder.DefaultDisposalStrategy );
        }

        Implementor.FromFactory( factory );
        return Implementor;
    }
}
