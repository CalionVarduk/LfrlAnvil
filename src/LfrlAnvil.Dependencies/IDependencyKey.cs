using System;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a type-erased dependency key.
/// </summary>
public interface IDependencyKey : IEquatable<IDependencyKey>
{
    /// <summary>
    /// Dependency's type.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Keyed locator's key type or null when this key does not use keyed locators.
    /// </summary>
    Type? KeyType { get; }

    /// <summary>
    /// Keyed locator's key value or null when this key does not use keyed locators.
    /// </summary>
    object? Key { get; }

    /// <summary>
    /// Specifies whether or not this key uses keyed locators.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Key ) )]
    [MemberNotNullWhen( true, nameof( KeyType ) )]
    public bool IsKeyed { get; }
}

/// <summary>
/// Represents a generic dependency key that uses keyed locators.
/// </summary>
public interface IDependencyKey<out TKey> : IDependencyKey
    where TKey : notnull
{
    /// <summary>
    /// Keyed locator's key type.
    /// </summary>
    new Type KeyType { get; }

    /// <summary>
    /// Keyed locator's key value.
    /// </summary>
    new TKey Key { get; }
}
