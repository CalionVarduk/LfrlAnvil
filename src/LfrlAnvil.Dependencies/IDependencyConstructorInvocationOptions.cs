using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyConstructorInvocationOptions
{
    Action<object, Type, IDependencyScope>? OnCreatedCallback { get; }
    IReadOnlyCollection<DependencyConstructorParameterResolution> ParameterResolutions { get; }

    IDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback);

    IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory);

    IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null);

    IDependencyConstructorInvocationOptions ClearParameterResolutions();
}
