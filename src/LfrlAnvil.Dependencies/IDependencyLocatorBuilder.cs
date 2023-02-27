using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyLocatorBuilder
{
    DependencyLifetime DefaultLifetime { get; }
    DependencyImplementorDisposalStrategy DefaultDisposalStrategy { get; }
    Type? KeyType { get; }
    object? Key { get; }
    bool IsKeyed { get; }

    IDependencyImplementorBuilder AddSharedImplementor(Type type);
    IDependencyBuilder Add(Type type);
    IDependencyLocatorBuilder SetDefaultLifetime(DependencyLifetime lifetime);
    IDependencyLocatorBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy);

    [Pure]
    IDependencyImplementorBuilder? TryGetSharedImplementor(Type type);

    [Pure]
    IDependencyRangeBuilder GetDependencyRange(Type type);
}

public interface IDependencyLocatorBuilder<out TKey> : IDependencyLocatorBuilder
    where TKey : notnull
{
    new TKey Key { get; }
}
