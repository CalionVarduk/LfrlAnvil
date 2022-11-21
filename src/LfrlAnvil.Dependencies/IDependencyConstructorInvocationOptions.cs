using System;
using System.Collections.Generic;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyConstructorInvocationOptions
{
    Action<object, Type, IDependencyScope>? OnCreatedCallback { get; }
    IReadOnlyCollection<DependencyConstructorParameterResolution> ParameterResolutions { get; }

    IDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback);

    IDependencyConstructorInvocationOptions AddParameterResolution(
        Func<ParameterInfo, bool> predicate,
        Action<IDependencyConstructorParameterResolutionOptions> configuration);

    IDependencyConstructorInvocationOptions ClearParameterResolutions();
}
