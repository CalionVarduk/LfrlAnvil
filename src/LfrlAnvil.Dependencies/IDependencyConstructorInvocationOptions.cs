using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyConstructorInvocationOptions
{
    Action<object, Type, IDependencyScope>? OnCreatedCallback { get; }
    IReadOnlyList<InjectableDependencyResolution<ParameterInfo>> ParameterResolutions { get; }
    IReadOnlyList<InjectableDependencyResolution<MemberInfo>> MemberResolutions { get; }

    IDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback);

    IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory);

    IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null);

    IDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory);

    IDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null);

    IDependencyConstructorInvocationOptions ClearParameterResolutions();
    IDependencyConstructorInvocationOptions ClearMemberResolutions();
}
