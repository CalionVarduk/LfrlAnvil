using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// Represents a size in memory.
/// </summary>
public readonly struct MemorySize : IEquatable<MemorySize>, IComparable<MemorySize>, IComparable
{
    /// <summary>
    /// Constant that defines how many bytes are in a single kilobyte (KB).
    /// </summary>
    public const long BytesPerKilobyte = 1024;

    /// <summary>
    /// Constant that defines how many bytes are in a single megabyte (MB).
    /// </summary>
    public const long BytesPerMegabyte = BytesPerKilobyte * 1024;

    /// <summary>
    /// Constant that defines how many bytes are in a single gigabyte (GB).
    /// </summary>
    public const long BytesPerGigabyte = BytesPerMegabyte * 1024;

    /// <summary>
    /// Constant that defines how many bytes are in a single terabyte (TB).
    /// </summary>
    public const long BytesPerTerabyte = BytesPerGigabyte * 1024;

    /// <summary>
    /// Represents 0 bytes.
    /// </summary>
    public static readonly MemorySize Zero = new MemorySize( 0 );

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> instance.
    /// </summary>
    /// <param name="bytes">Total amount of bytes.</param>
    public MemorySize(long bytes)
    {
        Bytes = bytes;
    }

    /// <summary>
    /// Total amount of bytes.
    /// </summary>
    public long Bytes { get; }

    /// <summary>
    /// Total amount of kilobytes (KB).
    /// </summary>
    public double TotalKilobytes => ( double )Bytes / BytesPerKilobyte;

    /// <summary>
    /// Total amount of megabytes (MB).
    /// </summary>
    public double TotalMegabytes => ( double )Bytes / BytesPerMegabyte;

    /// <summary>
    /// Total amount of gigabytes (GB).
    /// </summary>
    public double TotalGigabytes => ( double )Bytes / BytesPerGigabyte;

    /// <summary>
    /// Total amount of terabytes (TB).
    /// </summary>
    public double TotalTerabytes => ( double )Bytes / BytesPerTerabyte;

    /// <summary>
    /// Total amount of full kilobytes (KB).
    /// </summary>
    public long FullKilobytes => Bytes / BytesPerKilobyte;

    /// <summary>
    /// Total amount of full megabytes (MB).
    /// </summary>
    public long FullMegabytes => Bytes / BytesPerMegabyte;

    /// <summary>
    /// Total amount of full gigabytes (GB).
    /// </summary>
    public long FullGigabytes => Bytes / BytesPerGigabyte;

    /// <summary>
    /// Total amount of full terabytes (TB).
    /// </summary>
    public int FullTerabytes => ( int )(Bytes / BytesPerTerabyte);

    /// <summary>
    /// Amount of bytes left after subtraction of <see cref="FullKilobytes"/>.
    /// </summary>
    public int BytesInKilobyte => ( int )(Bytes % BytesPerKilobyte);

    /// <summary>
    /// Amount of bytes left after subtraction of <see cref="FullMegabytes"/>.
    /// </summary>
    public int BytesInMegabyte => ( int )(Bytes % BytesPerMegabyte);

    /// <summary>
    /// Amount of bytes left after subtraction of <see cref="FullGigabytes"/>.
    /// </summary>
    public int BytesInGigabyte => ( int )(Bytes % BytesPerGigabyte);

    /// <summary>
    /// Amount of bytes left after subtraction of <see cref="FullTerabytes"/>.
    /// </summary>
    public long BytesInTerabyte => Bytes % BytesPerTerabyte;

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of bytes.
    /// </summary>
    /// <param name="value">Total amount of bytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromBytes(long value)
    {
        return new MemorySize( value );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of bytes.
    /// </summary>
    /// <param name="value">Total amount of bytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    /// <remarks>Uses <see cref="MidpointRounding.AwayFromZero"/> rounding.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromBytes(double value)
    {
        return FromBytes( checked( ( long )Math.Round( value, MidpointRounding.AwayFromZero ) ) );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of kilobytes (KB).
    /// </summary>
    /// <param name="value">Total amount of kilobytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromKilobytes(long value)
    {
        return FromBytes( checked( value * BytesPerKilobyte ) );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of kilobytes (KB).
    /// </summary>
    /// <param name="value">Total amount of kilobytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    /// <remarks>Uses <see cref="MidpointRounding.AwayFromZero"/> rounding.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromKilobytes(double value)
    {
        return FromBytes( value * BytesPerKilobyte );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of megabytes (MB).
    /// </summary>
    /// <param name="value">Total amount of megabytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromMegabytes(long value)
    {
        return FromBytes( checked( value * BytesPerMegabyte ) );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of megabytes (MB).
    /// </summary>
    /// <param name="value">Total amount of megabytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    /// <remarks>Uses <see cref="MidpointRounding.AwayFromZero"/> rounding.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromMegabytes(double value)
    {
        return FromBytes( value * BytesPerMegabyte );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of gigabytes (GB).
    /// </summary>
    /// <param name="value">Total amount of gigabytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromGigabytes(long value)
    {
        return FromBytes( checked( value * BytesPerGigabyte ) );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of gigabytes (GB).
    /// </summary>
    /// <param name="value">Total amount of gigabytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    /// <remarks>Uses <see cref="MidpointRounding.AwayFromZero"/> rounding.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromGigabytes(double value)
    {
        return FromBytes( value * BytesPerGigabyte );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of terabytes (TB).
    /// </summary>
    /// <param name="value">Total amount of terabytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromTerabytes(long value)
    {
        return FromBytes( checked( value * BytesPerTerabyte ) );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> from total amount of terabytes (TB).
    /// </summary>
    /// <param name="value">Total amount of terabytes.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    /// <remarks>Uses <see cref="MidpointRounding.AwayFromZero"/> rounding.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MemorySize FromTerabytes(double value)
    {
        return FromBytes( value * BytesPerTerabyte );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MemorySize"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Bytes} B";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Bytes.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is MemorySize d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(MemorySize other)
    {
        return Bytes == other.Bytes;
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is MemorySize d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(MemorySize other)
    {
        return Bytes.CompareTo( other.Bytes );
    }

    /// <summary>
    /// Create a new <see cref="MemorySize"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MemorySize Add(MemorySize other)
    {
        return new MemorySize( checked( Bytes + other.Bytes ) );
    }

    /// <summary>
    /// Create a new <see cref="MemorySize"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MemorySize Subtract(MemorySize other)
    {
        return new MemorySize( checked( Bytes - other.Bytes ) );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    public static MemorySize operator +(MemorySize a, MemorySize b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="MemorySize"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="MemorySize"/> instance.</returns>
    [Pure]
    public static MemorySize operator -(MemorySize a, MemorySize b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(MemorySize a, MemorySize b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(MemorySize a, MemorySize b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >=(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <=(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <(MemorySize a, MemorySize b)
    {
        return a.CompareTo( b ) < 0;
    }
}
