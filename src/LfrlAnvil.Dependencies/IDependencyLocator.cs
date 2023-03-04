using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyLocator
{
    IDependencyScope AttachedScope { get; }
    Type? KeyType { get; }
    object? Key { get; }
    bool IsKeyed { get; }
    Type[] ResolvableTypes { get; }

    [Pure]
    object? TryResolveUnsafe(Type type);

    [Pure]
    DependencyLifetime? TryGetLifetime(Type type);
}

public interface IDependencyLocator<out TKey> : IDependencyLocator
    where TKey : notnull
{
    new TKey Key { get; }
}
