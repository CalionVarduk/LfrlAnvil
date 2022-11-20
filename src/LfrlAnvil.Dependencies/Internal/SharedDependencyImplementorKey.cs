using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class SharedDependencyImplementorKey : IInternalSharedDependencyImplementorKey
{
    internal SharedDependencyImplementorKey(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
    public Type? KeyType => null;
    public object? Key => null;
    public bool IsKeyed => false;

    [Pure]
    public override string ToString()
    {
        return Type.GetDebugString();
    }

    [Pure]
    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return IsEqualTo( obj as SharedDependencyImplementorKey );
    }

    [Pure]
    public bool Equals(ISharedDependencyImplementorKey? other)
    {
        return IsEqualTo( other as SharedDependencyImplementorKey );
    }

    [Pure]
    public DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore)
    {
        return builderStore.Global.SharedImplementors.GetValueOrDefault( Type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsEqualTo(SharedDependencyImplementorKey? other)
    {
        return other is not null && Type == other.Type;
    }
}

internal sealed class SharedDependencyImplementorKey<TKey> : IInternalSharedDependencyImplementorKey, ISharedDependencyImplementorKey<TKey>
    where TKey : notnull
{
    private static readonly IEqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;

    internal SharedDependencyImplementorKey(Type type, TKey key)
    {
        Type = type;
        Key = key;
    }

    public Type Type { get; }
    public TKey Key { get; }
    public Type KeyType => typeof( TKey );
    public bool IsKeyed => true;

    object ISharedDependencyImplementorKey.Key => Key;

    [Pure]
    public override string ToString()
    {
        return $"{Type.GetDebugString()} [{nameof( Key )}: '{Key}']";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Type, Key );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return IsEqualTo( obj as SharedDependencyImplementorKey<TKey> );
    }

    [Pure]
    public bool Equals(ISharedDependencyImplementorKey? other)
    {
        return IsEqualTo( other as SharedDependencyImplementorKey<TKey> );
    }

    [Pure]
    public DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore)
    {
        var locator = builderStore.GetKeyed( Key );
        return locator?.SharedImplementors.GetValueOrDefault( Type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsEqualTo(SharedDependencyImplementorKey<TKey>? other)
    {
        return other is not null && Type == other.Type && KeyComparer.Equals( Key, other.Key );
    }
}
