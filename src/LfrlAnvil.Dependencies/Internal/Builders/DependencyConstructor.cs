using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyConstructor : IDependencyConstructor
{
    private readonly Type? _type;

    internal DependencyConstructor(DependencyLocatorBuilder locatorBuilder, ConstructorInfo? info)
    {
        _type = null;
        Info = info;
        InternalInvocationOptions = new DependencyConstructorInvocationOptions( locatorBuilder );
    }

    internal DependencyConstructor(DependencyLocatorBuilder locatorBuilder, Type type)
    {
        _type = type;
        Info = null;
        InternalInvocationOptions = new DependencyConstructorInvocationOptions( locatorBuilder );
    }

    public ConstructorInfo? Info { get; }
    public Type? Type => Info?.DeclaringType ?? _type;
    public IDependencyConstructorInvocationOptions InvocationOptions => InternalInvocationOptions;
    internal DependencyConstructorInvocationOptions InternalInvocationOptions { get; }
}
