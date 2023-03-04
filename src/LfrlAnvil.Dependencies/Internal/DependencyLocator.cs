using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyLocator : IDependencyLocator
{
    internal DependencyLocator(DependencyScope attachedScope, Dictionary<Type, DependencyResolver> resolvers)
    {
        InternalAttachedScope = attachedScope;
        Resolvers = resolvers;
    }

    public IDependencyScope AttachedScope => InternalAttachedScope;
    public Type[] ResolvableTypes => InternalAttachedScope.InternalContainer.GetResolvableTypes( Resolvers );
    internal DependencyScope InternalAttachedScope { get; }
    internal Dictionary<Type, DependencyResolver> Resolvers { get; }

    Type? IDependencyLocator.KeyType => null;
    object? IDependencyLocator.Key => null;
    bool IDependencyLocator.IsKeyed => false;

    public object? TryResolveUnsafe(Type type)
    {
        return InternalAttachedScope.InternalContainer.TryResolveDependency( this, type );
    }

    [Pure]
    public DependencyLifetime? TryGetLifetime(Type type)
    {
        return InternalAttachedScope.InternalContainer.TryGetLifetime( Resolvers, type );
    }
}

internal sealed class DependencyLocator<TKey> : DependencyLocator, IDependencyLocator<TKey>
    where TKey : notnull
{
    internal DependencyLocator(TKey key, DependencyScope attachedScope, Dictionary<Type, DependencyResolver> resolvers)
        : base( attachedScope, resolvers )
    {
        Key = key;
    }

    public TKey Key { get; }

    Type IDependencyLocator.KeyType => typeof( TKey );
    object IDependencyLocator.Key => Key;
    bool IDependencyLocator.IsKeyed => true;
}
