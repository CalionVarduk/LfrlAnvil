using System;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class DependencyImplementorBuilder : IDependencyImplementorBuilder
{
    internal DependencyImplementorBuilder(Type implementorType)
    {
        ImplementorType = implementorType;
        Factory = null;
        DisposalStrategy = DependencyImplementorDisposalStrategy.UseDisposableInterface();
    }

    public Type ImplementorType { get; }
    public Func<IDependencyScope, object>? Factory { get; private set; }
    public DependencyImplementorDisposalStrategy DisposalStrategy { get; private set; }

    public IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory)
    {
        Factory = factory;
        return this;
    }

    public IDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        DisposalStrategy = strategy;
        return this;
    }
}
