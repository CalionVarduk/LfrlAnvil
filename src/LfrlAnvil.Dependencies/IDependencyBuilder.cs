using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyBuilder
{
    Type DependencyType { get; }
    DependencyLifetime Lifetime { get; }
    IDependencyKey? SharedImplementorKey { get; }
    IDependencyImplementorBuilder? Implementor { get; }
    bool IsIncludedInRange { get; }

    IDependencyBuilder IncludeInRange(bool included = true);

    IDependencyBuilder SetLifetime(DependencyLifetime lifetime);

    IDependencyBuilder FromSharedImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null);

    IDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null);

    IDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null);

    IDependencyImplementorBuilder FromType(Type type, Action<IDependencyConstructorInvocationOptions>? configuration = null);

    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
}
