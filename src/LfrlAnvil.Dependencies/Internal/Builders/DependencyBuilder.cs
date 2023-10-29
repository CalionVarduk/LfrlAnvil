using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyBuilder : IDependencyBuilder
{
    internal DependencyBuilder(DependencyRangeBuilder rangeBuilder)
    {
        InternalRangeBuilder = rangeBuilder;
        Lifetime = rangeBuilder.LocatorBuilder.DefaultLifetime;
        InternalSharedImplementorKey = null;
        Implementor = null;
        IsIncludedInRange = true;
    }

    public DependencyLifetime Lifetime { get; private set; }
    public IDependencyImplementorBuilder? Implementor { get; private set; }
    public bool IsIncludedInRange { get; private set; }
    public Type DependencyType => InternalRangeBuilder.DependencyType;
    public IDependencyKey? SharedImplementorKey => InternalSharedImplementorKey;
    public IDependencyRangeBuilder RangeBuilder => InternalRangeBuilder;
    internal DependencyRangeBuilder InternalRangeBuilder { get; }
    internal IInternalDependencyKey? InternalSharedImplementorKey { get; private set; }

    public IDependencyBuilder IncludeInRange(bool included = true)
    {
        IsIncludedInRange = included;
        return this;
    }

    public IDependencyBuilder SetLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime );
        Lifetime = lifetime;
        return this;
    }

    public IDependencyBuilder FromSharedImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null)
    {
        InternalSharedImplementorKey = DependencyImplementorOptions.CreateImplementorKey(
            InternalRangeBuilder.LocatorBuilder.CreateImplementorKey( type ),
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

    public IDependencyImplementorBuilder FromType(Type type, Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return GetOrCreateImplementor().FromType( type, configuration );
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
        Implementor = new DependencyImplementorBuilder( InternalRangeBuilder.LocatorBuilder, DependencyType );
        return Implementor;
    }
}
