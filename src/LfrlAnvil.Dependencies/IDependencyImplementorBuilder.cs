using System;

namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorBuilder
{
    Type ImplementorType { get; }
    Func<IDependencyScope, object>? Factory { get; }
    DependencyImplementorDisposalStrategy DisposalStrategy { get; }

    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
    IDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy);
}
