using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct ImplementorKey : IEquatable<ImplementorKey>
{
    public ImplementorKey(IDependencyKey value, bool isShared)
    {
        Value = value;
        IsShared = isShared;
    }

    public IDependencyKey Value { get; }
    public bool IsShared { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ImplementorKey Create(IDependencyKey value)
    {
        return new ImplementorKey( value, isShared: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ImplementorKey CreateShared(IDependencyKey value)
    {
        return new ImplementorKey( value, isShared: true );
    }

    [Pure]
    public override string ToString()
    {
        return IsShared ? $"{Value} (shared)" : Value.ToString() ?? string.Empty;
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Value, IsShared );
    }

    public override bool Equals(object? obj)
    {
        return obj is ImplementorKey k && Equals( k );
    }

    public bool Equals(ImplementorKey other)
    {
        return Value.Equals( other.Value ) && IsShared == other.IsShared;
    }
}
