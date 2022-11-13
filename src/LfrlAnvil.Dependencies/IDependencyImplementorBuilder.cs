using System;

namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorBuilder
{
    Type ImplementorType { get; }
    Func<IDependencyScope, object>? Factory { get; }
    DependencyImplementorDisposalStrategy DisposalStrategy { get; }
    Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
    IDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy);
    IDependencyImplementorBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
