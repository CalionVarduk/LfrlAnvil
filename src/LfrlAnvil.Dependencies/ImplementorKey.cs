using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies;

public readonly struct ImplementorKey : IEquatable<ImplementorKey>
{
    private readonly int _data;

    private ImplementorKey(IDependencyKey value, int data)
    {
        Value = value;
        _data = data;
    }

    public IDependencyKey Value { get; }
    public bool IsShared => _data == -1;
    public int? RangeIndex => _data >= 0 ? _data : null;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ImplementorKey Create(IDependencyKey value, int? rangeIndex = null)
    {
        Assume.Conditional( rangeIndex is not null, () => Assume.IsGreaterThanOrEqualTo( rangeIndex!.Value, 0, nameof( rangeIndex ) ) );
        return new ImplementorKey( value, rangeIndex ?? int.MinValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ImplementorKey CreateShared(IDependencyKey value)
    {
        return new ImplementorKey( value, -1 );
    }

    [Pure]
    public override string ToString()
    {
        return IsShared ? $"{Value} (shared)" : $"{Value}{GetRangeIndexText( RangeIndex )}";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Value, _data );
    }

    public override bool Equals(object? obj)
    {
        return obj is ImplementorKey k && Equals( k );
    }

    public bool Equals(ImplementorKey other)
    {
        return Value.Equals( other.Value ) && _data == other._data;
    }

    [Pure]
    public static bool operator ==(ImplementorKey a, ImplementorKey b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(ImplementorKey a, ImplementorKey b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    private static string GetRangeIndexText(int? index)
    {
        return index is null ? string.Empty : $" (position in range: {index.Value})";
    }
}
