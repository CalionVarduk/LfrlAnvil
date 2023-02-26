using System;
using System.Reflection;

namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorBuilder
{
    Type ImplementorType { get; }
    IDependencyConstructor? Constructor { get; }
    Func<IDependencyScope, object>? Factory { get; }
    DependencyImplementorDisposalStrategy DisposalStrategy { get; }
    Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    IDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null);

    IDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null);

    IDependencyImplementorBuilder FromType(Type type, Action<IDependencyConstructorInvocationOptions>? configuration = null);

    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
    IDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy);
    IDependencyImplementorBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
