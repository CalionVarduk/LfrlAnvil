using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyBuilder : IDependencyBuilder
{
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
    public IDependencyImplementorKey? SharedImplementorKey => InternalSharedImplementorKey;
    internal DependencyLocatorBuilder LocatorBuilder { get; }
    internal IInternalDependencyImplementorKey? InternalSharedImplementorKey { get; set; }

    public IDependencyBuilder SetLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime, nameof( lifetime ) );
        Lifetime = lifetime;
        return this;
    }

    public IDependencyBuilder FromSharedImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null)
    {
        InternalSharedImplementorKey = DependencyImplementorOptions.CreateImplementorKey(
            LocatorBuilder.CreateImplementorKey( type ),
            configuration );

        Implementor = null;
        return this;
    }

    public IDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return GetOrCreateImplementor().FromConstructor( configuration );
    }

    public IDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return GetOrCreateImplementor().FromConstructor( info, configuration );
    }

    public IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory)
    {
        return GetOrCreateImplementor().FromFactory( factory );
    }

    private IDependencyImplementorBuilder GetOrCreateImplementor()
    {
        if ( Implementor is not null )
            return Implementor;

        InternalSharedImplementorKey = null;
        Implementor = new DependencyImplementorBuilder( LocatorBuilder, DependencyType );
        return Implementor;
    }
}
