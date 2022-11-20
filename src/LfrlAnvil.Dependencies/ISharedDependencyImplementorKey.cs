using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Dependencies;

public interface ISharedDependencyImplementorKey : IEquatable<ISharedDependencyImplementorKey>
{
    Type Type { get; }
    Type? KeyType { get; }
    object? Key { get; }

    [MemberNotNullWhen( true, nameof( Key ) )]
    [MemberNotNullWhen( true, nameof( KeyType ) )]
    public bool IsKeyed { get; }
}

public interface ISharedDependencyImplementorKey<out TKey> : ISharedDependencyImplementorKey
    where TKey : notnull
{
    new TKey Key { get; }
}
