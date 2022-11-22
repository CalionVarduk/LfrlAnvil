using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyConstructorInvocationOptions : IDependencyConstructorInvocationOptions
{
    private readonly List<DependencyConstructorParameterResolution> _parameterResolutions;

    internal DependencyConstructorInvocationOptions(DependencyLocatorBuilder locatorBuilder)
    {
        OnCreatedCallback = null;
        _parameterResolutions = new List<DependencyConstructorParameterResolution>();
        LocatorBuilder = locatorBuilder;
    }

    public Action<object, Type, IDependencyScope>? OnCreatedCallback { get; private set; }
    public IReadOnlyCollection<DependencyConstructorParameterResolution> ParameterResolutions => _parameterResolutions;
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback)
    {
        OnCreatedCallback = callback;
        return this;
    }

    public IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory)
    {
        var resolution = DependencyConstructorParameterResolution.FromFactory( predicate, factory );
        _parameterResolutions.Add( resolution );
        return this;
    }

    public IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        var key = DependencyImplementorOptions.CreateImplementorKey(
            LocatorBuilder.CreateImplementorKey( implementorType ),
            configuration );

        var resolution = DependencyConstructorParameterResolution.FromImplementorKey( predicate, key );
        _parameterResolutions.Add( resolution );
        return this;
    }

    public IDependencyConstructorInvocationOptions ClearParameterResolutions()
    {
        _parameterResolutions.Clear();
        return this;
    }
}
