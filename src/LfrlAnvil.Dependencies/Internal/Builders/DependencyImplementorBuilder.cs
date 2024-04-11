using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyImplementorBuilder : IDependencyImplementorBuilder
{
    internal DependencyImplementorBuilder(DependencyLocatorBuilder locatorBuilder, Type implementorType)
    {
        LocatorBuilder = locatorBuilder;
        ImplementorType = implementorType;
        DisposalStrategy = locatorBuilder.DefaultDisposalStrategy;
        Factory = null;
        OnResolvingCallback = null;
        InternalConstructor = null;
    }

    public Type ImplementorType { get; }
    public Func<IDependencyScope, object>? Factory { get; private set; }
    public DependencyImplementorDisposalStrategy DisposalStrategy { get; private set; }
    public Action<Type, IDependencyScope>? OnResolvingCallback { get; private set; }
    public IDependencyConstructor? Constructor => InternalConstructor;

    internal DependencyLocatorBuilder LocatorBuilder { get; }
    internal DependencyConstructor? InternalConstructor { get; private set; }

    public IDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return FromConstructorCore( null, configuration );
    }

    public IDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return FromConstructorCore( info, configuration );
    }

    public IDependencyImplementorBuilder FromType(Type type, Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        Factory = null;
        InternalConstructor = new DependencyConstructor( LocatorBuilder, type );
        configuration?.Invoke( InternalConstructor.InternalInvocationOptions );
        return this;
    }

    public IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory)
    {
        Factory = factory;
        InternalConstructor = null;
        return this;
    }

    public IDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        DisposalStrategy = strategy;
        return this;
    }

    public IDependencyImplementorBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback)
    {
        OnResolvingCallback = callback;
        return this;
    }

    private DependencyImplementorBuilder FromConstructorCore(
        ConstructorInfo? info,
        Action<IDependencyConstructorInvocationOptions>? configuration)
    {
        Factory = null;
        InternalConstructor = new DependencyConstructor( LocatorBuilder, info );
        configuration?.Invoke( InternalConstructor.InternalInvocationOptions );
        return this;
    }
}
