using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public readonly struct MemorySize : IEquatable<MemorySize>, IComparable<MemorySize>, IComparable
{
    public const long BytesPerKilobyte = 1024;
    public const long BytesPerMegabyte = BytesPerKilobyte * 1024;
    public const long BytesPerGigabyte = BytesPerMegabyte * 1024;
    public const long BytesPerTerabyte = BytesPerGigabyte * 1024;
    public static readonly MemorySize Empty = new MemorySize( 0 );

    public MemorySize(long bytes)
    {
        Bytes = bytes;
    }

    public long Bytes { get; }
    public double TotalKilobytes => (double)Bytes / BytesPerKilobyte;
    public double TotalMegabytes => (double)Bytes / BytesPerMegabyte;
    public double TotalGigabytes => (double)Bytes / BytesPerGigabyte;
    public double TotalTerabytes => (double)Bytes / BytesPerTerabyte;
    public long FullKilobytes => Bytes / BytesPerKilobyte;
    public long FullMegabytes => Bytes / BytesPerMegabyte;
    public long FullGigabytes => Bytes / BytesPerGigabyte;
    public int FullTerabytes => (int)(Bytes / BytesPerTerabyte);
    public int BytesInKilobyte => (int)(Bytes % BytesPerKilobyte);
    public int BytesInMegabyte => (int)(Bytes % BytesPerMegabyte);
    public int BytesInGigabyte => (int)(Bytes % BytesPerGigabyte);
    public long BytesInTerabyte => Bytes % BytesPerTerabyte;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromBytes(long value)
    {
        return new MemorySize( value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromBytes(double value)
    {
        return FromBytes( checked( (long)Math.Round( value, MidpointRounding.AwayFromZero ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromKilobytes(long value)
    {
        return FromBytes( checked( value * BytesPerKilobyte ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromKilobytes(double value)
    {
        return FromBytes( value * BytesPerKilobyte );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromMegabytes(long value)
    {
        return FromBytes( checked( value * BytesPerMegabyte ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromMegabytes(double value)
    {
        return FromBytes( value * BytesPerMegabyte );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromGigabytes(long value)
    {
        return FromBytes( checked( value * BytesPerGigabyte ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromGigabytes(double value)
    {
        return FromBytes( value * BytesPerGigabyte );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromTerabytes(long value)
    {
        return FromBytes( checked( value * BytesPerTerabyte ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromTerabytes(double value)
    {
        return FromBytes( value * BytesPerTerabyte );
    }

    [Pure]
    public override string ToString()
    {
        return $"{Bytes} B";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Bytes.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is MemorySize d && Equals( d );
    }

    [Pure]
    public bool Equals(MemorySize other)
    {
        return Bytes == other.Bytes;
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is MemorySize d ? CompareTo( d ) : 1;
    }

    [Pure]
    public int CompareTo(MemorySize other)
    {
        return Bytes.CompareTo( other.Bytes );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MemorySize Add(MemorySize other)
    {
        return new MemorySize( checked( Bytes + other.Bytes ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MemorySize Subtract(MemorySize other)
    {
        return new MemorySize( checked( Bytes - other.Bytes ) );
    }

    [Pure]
    public static MemorySize operator +(MemorySize a, MemorySize b)
    {
        return a.Add( b );
    }

    [Pure]
    public static MemorySize operator -(MemorySize a, MemorySize b)
    {
        return a.Subtract( b );
    }

    [Pure]
    public static bool operator ==(MemorySize a, MemorySize b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(MemorySize a, MemorySize b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    public static bool operator >=(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    public static bool operator <=(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    public static bool operator >(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    public static bool operator <(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) < 0;
    }
}
