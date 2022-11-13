using System;

namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorBuilder
{
    Type ImplementorType { get; }
    Func<IDependencyScope, object>? Factory { get; }

    IDependencyImplementorBuilder FromFactory(Func<IDependencyScope, object> factory);
}
