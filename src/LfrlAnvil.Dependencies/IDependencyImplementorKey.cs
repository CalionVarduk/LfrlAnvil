using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Dependencies;

public interface IDependencyImplementorKey : IEquatable<IDependencyImplementorKey>
{
    Type Type { get; }
    Type? KeyType { get; }
    object? Key { get; }

    [MemberNotNullWhen( true, nameof( Key ) )]
    [MemberNotNullWhen( true, nameof( KeyType ) )]
    public bool IsKeyed { get; }
}

public interface IDependencyImplementorKey<out TKey> : IDependencyImplementorKey
    where TKey : notnull
{
    new TKey Key { get; }
}
