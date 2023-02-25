using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Dependencies;

public interface IDependencyKey : IEquatable<IDependencyKey>
{
    Type Type { get; }
    Type? KeyType { get; }
    object? Key { get; }

    [MemberNotNullWhen( true, nameof( Key ) )]
    [MemberNotNullWhen( true, nameof( KeyType ) )]
    public bool IsKeyed { get; }
}

public interface IDependencyKey<out TKey> : IDependencyKey
    where TKey : notnull
{
    new Type KeyType { get; }
    new TKey Key { get; }
}
