using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyLocator : IDependencyLocator
{
    internal DependencyLocator(DependencyScope attachedScope, DependencyResolversStore resolvers)
    {
        InternalAttachedScope = attachedScope;
        Resolvers = resolvers;
    }

    public IDependencyScope AttachedScope => InternalAttachedScope;
    internal DependencyScope InternalAttachedScope { get; }
    internal DependencyResolversStore Resolvers { get; }

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
        return Resolvers.TryGetLifetime( type );
    }

    [Pure]
    public Type[] GetResolvableTypes()
    {
        return Resolvers.GetResolvableTypes();
    }
}

internal sealed class DependencyLocator<TKey> : DependencyLocator, IDependencyLocator<TKey>
    where TKey : notnull
{
    internal DependencyLocator(TKey key, DependencyScope attachedScope, DependencyResolversStore resolvers)
        : base( attachedScope, resolvers )
    {
        Key = key;
    }

    public TKey Key { get; }

    Type IDependencyLocator.KeyType => typeof( TKey );
    object IDependencyLocator.Key => Key;
    bool IDependencyLocator.IsKeyed => true;
}
