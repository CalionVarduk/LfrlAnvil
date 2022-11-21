using System;
using System.Collections.Generic;
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

    public IDependencyConstructorInvocationOptions AddParameterResolution(
        Func<ParameterInfo, bool> predicate,
        Action<IDependencyConstructorParameterResolutionOptions> configuration)
    {
        var options = new DependencyConstructorParameterResolutionOptions( LocatorBuilder, predicate );
        configuration( options );
        _parameterResolutions.Add( options.CreateResolution() );
        return this;
    }

    public IDependencyConstructorInvocationOptions ClearParameterResolutions()
    {
        _parameterResolutions.Clear();
        return this;
    }
}
