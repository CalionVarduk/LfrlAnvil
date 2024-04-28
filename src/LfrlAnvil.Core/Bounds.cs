using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// A lightweight representation of a generic range of values between <see cref="Min"/> and <see cref="Max"/>.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct Bounds<T> : IEquatable<Bounds<T>>
    where T : IComparable<T>
{
    /// <summary>
    /// Creates a new <see cref="Bounds{T}"/> instance.
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <exception cref="ArgumentException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public Bounds(T min, T max)
    {
        if ( min.CompareTo( max ) > 0 )
            throw new ArgumentException( ExceptionResources.MinCannotBeGreaterThanMax( min, max, nameof( Min ), nameof( Max ) ) );

        Min = min;
        Max = max;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Bounds((T Min, T Max) values)
    {
        Min = values.Min;
        Max = values.Max;
    }

    /// <summary>
    /// Minimum value is this range.
    /// </summary>
    public T Min { get; }

    /// <summary>
    /// Maximum value in this range.
    /// </summary>
    public T Max { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="Bounds{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Bounds )}({Min} : {Max})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default
            .Add( Min )
            .Add( Max )
            .Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Bounds<T> b && Equals( b );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Bounds<T> other)
    {
        return Equality.Create( Min, other.Min ).Result && Equality.Create( Max, other.Max ).Result;
    }

    /// <summary>
    /// Creates a new <see cref="Bounds{T}"/> instance with different <paramref name="min"/> value.
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <returns>New <see cref="Bounds{T}"/> instance with unchanged <see cref="Max"/>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="min"/> is greater than <see cref="Max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bounds<T> SetMin(T min)
    {
        return new Bounds<T>( min, Max );
    }

    /// <summary>
    /// Creates a new <see cref="Bounds{T}"/> instance with different <paramref name="max"/> value.
    /// </summary>
    /// <param name="max">Maximum value.</param>
    /// <returns>New <see cref="Bounds{T}"/> instance with unchanged <see cref="Min"/>.</returns>
    /// <exception cref="ArgumentException">When <see cref="Min"/> is greater than <paramref name="max"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bounds<T> SetMax(T max)
    {
        return new Bounds<T>( Min, max );
    }

    /// <summary>
    /// Checks whether or not this [<see cref="Min"/>, <see cref="Max"/>] range contains the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns><b>true</b> when this range contains the provided <paramref name="value"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(T value)
    {
        return Min.CompareTo( value ) <= 0 && Max.CompareTo( value ) >= 0;
    }

    /// <summary>
    /// Checks whether or not this exclusive (<see cref="Min"/>, <see cref="Max"/>) range contains the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns><b>true</b> when this exclusive range contains the provided <paramref name="value"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsExclusively(T value)
    {
        return Min.CompareTo( value ) < 0 && Max.CompareTo( value ) > 0;
    }

    /// <summary>
    /// Checks whether or not this [<see cref="Min"/>, <see cref="Max"/>] range contains the provided <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">Range to check.</param>
    /// <returns><b>true</b> when this range contains the provided <paramref name="other"/> range, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(Bounds<T> other)
    {
        return Min.CompareTo( other.Min ) <= 0 && Max.CompareTo( other.Max ) >= 0;
    }

    /// <summary>
    /// Checks whether or not this exclusive (<see cref="Min"/>, <see cref="Max"/>) range contains
    /// the provided <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">Range to check.</param>
    /// <returns>
    /// <b>true</b> when this exclusive range contains the provided <paramref name="other"/> range, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsExclusively(Bounds<T> other)
    {
        return Min.CompareTo( other.Min ) < 0 && Max.CompareTo( other.Max ) > 0;
    }

    /// <summary>
    /// Checks whether or not this [<see cref="Min"/>, <see cref="Max"/>] range intersects with the provided <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">Range to check.</param>
    /// <returns><b>true</b> when this range intersects with the provided <paramref name="other"/> range, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Intersects(Bounds<T> other)
    {
        return Min.CompareTo( other.Max ) <= 0 && Max.CompareTo( other.Min ) >= 0;
    }

    /// <summary>
    /// Attempts to extract an intersection between this [<see cref="Min"/>, <see cref="Max"/>] range
    /// and the provided <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">Range to check.</param>
    /// <returns>New <see cref="Bounds{T}"/> instance or null when the two ranges do not intersect.</returns>
    [Pure]
    public Bounds<T>? GetIntersection(Bounds<T> other)
    {
        var min = Min.CompareTo( other.Min ) > 0 ? Min : other.Min;
        var max = Max.CompareTo( other.Max ) < 0 ? Max : other.Max;

        if ( min.CompareTo( max ) > 0 )
            return null;

        return new Bounds<T>( (min, max) );
    }

    /// <summary>
    /// Attempts to merge this [<see cref="Min"/>, <see cref="Max"/>] range with the provided <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">Range to merge.</param>
    /// <returns>New <see cref="Bounds{T}"/> instance or null when the two ranges do not intersect.</returns>
    [Pure]
    public Bounds<T>? MergeWith(Bounds<T> other)
    {
        if ( ! Intersects( other ) )
            return null;

        var min = Min.CompareTo( other.Min ) > 0 ? other.Min : Min;
        var max = Max.CompareTo( other.Max ) < 0 ? other.Max : Max;

        return new Bounds<T>( (min, max) );
    }

    /// <summary>
    /// Attempts to split this [<see cref="Min"/>, <see cref="Max"/>] range in two at the provided <paramref name="value"/> point.
    /// </summary>
    /// <param name="value">Value to split at.</param>
    /// <returns>
    /// Pair of <see cref="Bounds{T}"/> instances when <paramref name="value"/> is exclusively contained by this range,
    /// otherwise pair with the <see cref="Pair{T1,T2}.Second"/> value equal to null.
    /// </returns>
    [Pure]
    public Pair<Bounds<T>, Bounds<T>?> SplitAt(T value)
    {
        return ContainsExclusively( value )
            ? Pair.Create( new Bounds<T>( (Min, value) ), ( Bounds<T>? )new Bounds<T>( (value, Max) ) )
            : Pair.Create( this, ( Bounds<T>? )null );
    }

    /// <summary>
    /// Attempts to split this [<see cref="Min"/>, <see cref="Max"/>] range in two by removing the provided <paramref name="other"/> range.
    /// </summary>
    /// <param name="other">Range to remove.</param>
    /// <returns>
    /// Pair of <see cref="Bounds{T}"/> instances when <paramref name="other"/> range is exclusively contained by this range,
    /// otherwise pair with both values equal to null when <paramref name="other"/> range contains this range,
    /// otherwise pair with <see cref="Pair{T1,T2}.Second"/> value equal to null.
    /// </returns>
    [Pure]
    public Pair<Bounds<T>?, Bounds<T>?> Remove(Bounds<T> other)
    {
        var minComparisonResult = Min.CompareTo( other.Min );
        var maxComparisonResult = Max.CompareTo( other.Max );

        if ( minComparisonResult >= 0 && maxComparisonResult <= 0 )
            return Pair.Create( ( Bounds<T>? )null, ( Bounds<T>? )null );

        if ( minComparisonResult > 0 || maxComparisonResult < 0 )
            return Pair.Create( ( Bounds<T>? )this, ( Bounds<T>? )null );

        if ( minComparisonResult == 0 )
            return Pair.Create( ( Bounds<T>? )new Bounds<T>( (other.Max, Max) ), ( Bounds<T>? )null );

        if ( maxComparisonResult == 0 )
            return Pair.Create( ( Bounds<T>? )new Bounds<T>( (Min, other.Min) ), ( Bounds<T>? )null );

        return Pair.Create( ( Bounds<T>? )new Bounds<T>( (Min, other.Min) ), ( Bounds<T>? )new Bounds<T>( (other.Max, Max) ) );
    }

    /// <summary>
    /// Clamps the value to this [<see cref="Min"/>, <see cref="Max"/>] range.
    /// </summary>
    /// <param name="value">Value to clamp.</param>
    /// <returns>Clamped <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Clamp(T value)
    {
        if ( Min.CompareTo( value ) >= 0 )
            return Min;

        if ( Max.CompareTo( value ) <= 0 )
            return Max;

        return value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Bounds<T> a, Bounds<T> b)
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
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Bounds<T> a, Bounds<T> b)
    {
        return ! a.Equals( b );
    }
}
