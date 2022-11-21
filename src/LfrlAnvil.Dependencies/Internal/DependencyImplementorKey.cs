using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class DependencyImplementorKey : IInternalDependencyImplementorKey
{
    internal DependencyImplementorKey(Type type)
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
        return IsEqualTo( obj as DependencyImplementorKey );
    }

    [Pure]
    public bool Equals(IDependencyImplementorKey? other)
    {
        return IsEqualTo( other as DependencyImplementorKey );
    }

    [Pure]
    public DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore)
    {
        return builderStore.Global.SharedImplementors.GetValueOrDefault( Type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsEqualTo(DependencyImplementorKey? other)
    {
        return other is not null && Type == other.Type;
    }
}

internal sealed class DependencyImplementorKey<TKey> : IInternalDependencyImplementorKey, IDependencyImplementorKey<TKey>
    where TKey : notnull
{
    private static readonly IEqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;

    internal DependencyImplementorKey(Type type, TKey key)
    {
        Type = type;
        Key = key;
    }

    public Type Type { get; }
    public TKey Key { get; }
    public Type KeyType => typeof( TKey );
    public bool IsKeyed => true;

    object IDependencyImplementorKey.Key => Key;

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
        return IsEqualTo( obj as DependencyImplementorKey<TKey> );
    }

    [Pure]
    public bool Equals(IDependencyImplementorKey? other)
    {
        return IsEqualTo( other as DependencyImplementorKey<TKey> );
    }

    [Pure]
    public DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore)
    {
        var locator = builderStore.GetKeyed( Key );
        return locator?.SharedImplementors.GetValueOrDefault( Type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsEqualTo(DependencyImplementorKey<TKey>? other)
    {
        return other is not null && Type == other.Type && KeyComparer.Equals( Key, other.Key );
    }
}
