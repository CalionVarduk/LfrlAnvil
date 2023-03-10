using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyRangeBuilder
{
    Type DependencyType { get; }
    Action<Type, IDependencyScope>? OnResolvingCallback { get; }
    IReadOnlyList<IDependencyBuilder> Elements { get; }

    IDependencyBuilder Add();

    [Pure]
    IDependencyBuilder? TryGetLast();

    IDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback);
}
