﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies;

public interface IDependencyLocator
{
    IDependencyScope AttachedScope { get; }
    Type? KeyType { get; }
    object? Key { get; }
    bool IsKeyed { get; }
    IEnumerable<Type> ResolvableTypes { get; }

    [Pure]
    object Resolve(Type type);

    [Pure]
    T Resolve<T>()
        where T : class;

    [Pure]
    object? TryResolve(Type type);

    [Pure]
    T? TryResolve<T>()
        where T : class;

    [Pure]
    DependencyLifetime? TryGetLifetime(Type type);
}

public interface IDependencyLocator<out TKey> : IDependencyLocator
    where TKey : notnull
{
    new TKey Key { get; }
}
