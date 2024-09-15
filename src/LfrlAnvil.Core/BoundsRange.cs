// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

/// <summary>
/// A lightweight representation of a generic collection of ranges of values.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct BoundsRange<T> : IReadOnlyList<Bounds<T>>, IEquatable<BoundsRange<T>>
    where T : IComparable<T>
{
    /// <summary>
    /// Represents an empty collection of ranges.
    /// </summary>
    public static readonly BoundsRange<T> Empty = new BoundsRange<T>( Array.Empty<T>() );

    private readonly T[]? _values;

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from a single <see cref="Bounds{T}"/> instance.
    /// </summary>
    /// <param name="value">Single range.</param>
    public BoundsRange(Bounds<T> value)
    {
        _values = new[] { value.Min, value.Max };
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from a collection of <see cref="Bounds{T}"/> instances.
    /// </summary>
    /// <param name="range">Collection of ranges.</param>
    /// <exception cref="ArgumentException">When <paramref name="range"/> is not ordered.</exception>
    public BoundsRange(IEnumerable<Bounds<T>> range)
    {
        _values = ExtractAndValidateValues( range );
    }

    private BoundsRange(T[] values)
    {
        _values = values;
    }

    /// <summary>
    /// Gets a single <see cref="Bounds{T}"/> range at the specified 0-based position.
    /// </summary>
    /// <param name="index">0-based range position.</param>
    /// <exception cref="IndexOutOfRangeException">
    /// When <paramref name="index"/> is less than <b>0</b> or greater than or equal to <see cref="Count"/>.
    /// </exception>
    public Bounds<T> this[int index]
    {
        get
        {
            var values = InternalValues;
            return new Bounds<T>( (values[index << 1], values[(index << 1) + 1]) );
        }
    }

    /// <summary>
    /// Specifies the number of ranges in this collection.
    /// </summary>
    public int Count => InternalValues.Length >> 1;

    private T[] InternalValues => _values ?? Array.Empty<T>();

    /// <summary>
    /// Returns a string representation of this <see cref="BoundsRange{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var rangeText = string.Join( " & ", this.Select( static r => $"[{r.Min} : {r.Max}]" ) );
        return $"{nameof( BoundsRange )}({rangeText})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.AddRange( InternalValues ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is BoundsRange<T> r && Equals( r );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(BoundsRange<T> other)
    {
        return InternalValues.SequenceEqual( other.InternalValues );
    }

    /// <summary>
    /// Attempts to create a new <see cref="Bounds{T}"/> instance by extracting the minimum and maximum values from this collection.
    /// </summary>
    /// <returns>New <see cref="Bounds{T}"/> instance or null when this collection is empty.</returns>
    [Pure]
    public Bounds<T>? Flatten()
    {
        var values = InternalValues;
        if ( values.Length == 0 )
            return null;

        return new Bounds<T>( (values[0], values[^1]) );
    }

    /// <summary>
    /// Attempts to find a 0-based position of <see cref="Bounds{T}"/> from this collection
    /// that contains the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>
    /// 0-based position of <see cref="Bounds{T}"/> that contains the provided <paramref name="value"/>,
    /// otherwise the result will be negative and will be equal to the bitwise complement of an index of the nearest <see cref="Bounds{T}"/>
    /// whose <see cref="Bounds{T}.Min"/> is larger than the provided <paramref name="value"/>.
    /// </returns>
    [Pure]
    public int FindBoundsIndex(T value)
    {
        var values = InternalValues;
        var index = Array.BinarySearch( values, value );

        if ( index >= 0 )
            return index >> 1;

        index = ~index;

        if ( index >= values.Length )
            return ~(values.Length >> 1);

        return index.IsOdd() ? index >> 1 : ~(index >> 1);
    }

    /// <summary>
    /// Attempts to find <see cref="Bounds{T}"/> from this collection that contains the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>
    /// <see cref="Bounds{T}"/> from this collection that contains the provided <paramref name="value"/>
    /// or null when <paramref name="value"/> is not contained by this collection.
    /// </returns>
    [Pure]
    public Bounds<T>? FindBounds(T value)
    {
        var values = InternalValues;
        var index = Array.BinarySearch( values, value );

        if ( index >= 0 )
        {
            return index.IsEven()
                ? new Bounds<T>( (values[index], values[index + 1]) )
                : new Bounds<T>( (values[index - 1], values[index]) );
        }

        index = ~index;

        if ( index >= values.Length || index.IsEven() )
            return null;

        return new Bounds<T>( (values[index - 1], values[index]) );
    }

    /// <summary>
    /// Checks whether or not this collection contains the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns><b>true</b> when this collection contains the provided <paramref name="value"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(T value)
    {
        return FindBoundsIndex( value ) >= 0;
    }

    /// <summary>
    /// Checks whether or not this collection contains the provided <paramref name="value"/> range.
    /// </summary>
    /// <param name="value">Range to check.</param>
    /// <returns><b>true</b> when this collection contains the provided <paramref name="value"/> range, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(Bounds<T> value)
    {
        var minIndex = FindBoundsIndex( value.Min );
        return minIndex >= 0 && FindBoundsIndex( value.Max ) == minIndex;
    }

    /// <summary>
    /// Checks whether or not this collection contains the provided <paramref name="other"/> collection of ranges.
    /// </summary>
    /// <param name="other">Collection of ranges to check.</param>
    /// <returns>
    /// <b>true</b> when this collection contains the provided <paramref name="other"/> collection of ranges, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    public bool Contains(BoundsRange<T> other)
    {
        var values = InternalValues;
        var otherValues = other.InternalValues;

        if ( values.Length == 0 )
            return otherValues.Length == 0;

        if ( otherValues.Length == 0 )
            return true;

        if ( ! TryGetStartIndex( values, otherValues, out var otherMinIndex ) || otherMinIndex != 0 )
            return false;

        if ( ! TryGetStartIndex( otherValues, values, out var minIndex ) )
            return false;

        var otherEnd = GetEndIndex( values, otherValues );
        if ( otherEnd != otherValues.Length )
            return false;

        var end = GetEndIndex( otherValues, values );

        var i = minIndex;
        var j = otherMinIndex;

        while ( i < end && j < otherEnd )
        {
            var (currentMin, currentMax) = (values[i], values[i + 1]);
            var (otherCurrentMin, otherCurrentMax) = (otherValues[j], otherValues[j + 1]);

            var minToMaxComparisonResult = currentMin.CompareTo( otherCurrentMax );
            if ( minToMaxComparisonResult > 0 )
                return false;

            var maxToMinComparisonResult = currentMax.CompareTo( otherCurrentMin );
            if ( maxToMinComparisonResult < 0 )
            {
                i += 2;
                continue;
            }

            var minToMinComparisonResult = currentMin.CompareTo( otherCurrentMin );
            if ( minToMinComparisonResult > 0 )
                return false;

            var maxToMaxComparisonResult = currentMax.CompareTo( otherCurrentMax );
            if ( maxToMaxComparisonResult < 0 )
                return false;

            if ( maxToMaxComparisonResult == 0 )
                i += 2;

            j += 2;
        }

        return true;
    }

    /// <summary>
    /// Checks whether or not this collection intersects with the provided <paramref name="value"/> range.
    /// </summary>
    /// <param name="value">Range to check.</param>
    /// <returns>
    /// <b>true</b> when this collection intersects with the provided <paramref name="value"/> range, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    public bool Intersects(Bounds<T> value)
    {
        var minIndex = FindBoundsIndex( value.Min );
        if ( minIndex >= 0 )
            return true;

        var maxIndex = FindBoundsIndex( value.Max );
        if ( maxIndex >= 0 )
            return true;

        return minIndex != maxIndex;
    }

    /// <summary>
    /// Checks whether or not this collection intersects with the provided <paramref name="other"/> collection of ranges.
    /// </summary>
    /// <param name="other">Collection of ranges to check.</param>
    /// <returns>
    /// <b>true</b> when this collection intersects with the provided <paramref name="other"/> collection of ranges, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Intersects(BoundsRange<T> other)
    {
        var values = InternalValues;
        var otherValues = other.InternalValues;

        if ( values.Length == 0 || otherValues.Length == 0 )
            return false;

        if ( ! TryGetStartIndex( values, otherValues, out var otherMinIndex ) )
            return false;

        if ( ! TryGetStartIndex( otherValues, values, out var minIndex ) )
            return false;

        var otherEnd = GetEndIndex( values, otherValues );
        var end = GetEndIndex( otherValues, values );

        var i = minIndex;
        var j = otherMinIndex;

        while ( i < end && j < otherEnd )
        {
            var (currentMin, currentMax) = (values[i], values[i + 1]);
            var (otherCurrentMin, otherCurrentMax) = (otherValues[j], otherValues[j + 1]);

            var minToMaxComparisonResult = currentMin.CompareTo( otherCurrentMax );
            if ( minToMaxComparisonResult > 0 )
            {
                j += 2;
                continue;
            }

            if ( minToMaxComparisonResult == 0 )
                return true;

            var maxToMinComparisonResult = currentMax.CompareTo( otherCurrentMin );
            if ( maxToMinComparisonResult >= 0 )
                return true;

            i += 2;
        }

        return false;
    }

    /// <summary>
    /// Attempts to extract an intersection between this collection of ranges and the provided <paramref name="value"/> range.
    /// </summary>
    /// <param name="value">Range to check.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance or <see cref="Empty"/> when the ranges do not intersect.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<T> GetIntersection(Bounds<T> value)
    {
        return GetIntersection( new BoundsRange<T>( value ) );
    }

    /// <summary>
    /// Attempts to extract an intersection between this collection of ranges
    /// and the provided <paramref name="other"/> collection of ranges.
    /// </summary>
    /// <param name="other">Collection of ranges to check.</param>
    /// <returns>
    /// New <see cref="BoundsRange{T}"/> instance or <see cref="Empty"/> when the two collections of ranges do not intersect.
    /// </returns>
    [Pure]
    public BoundsRange<T> GetIntersection(BoundsRange<T> other)
    {
        var values = InternalValues;
        var otherValues = other.InternalValues;

        if ( values.Length == 0 || otherValues.Length == 0 )
            return Empty;

        if ( ! TryGetStartIndex( values, otherValues, out var otherMinIndex ) )
            return Empty;

        if ( ! TryGetStartIndex( otherValues, values, out var minIndex ) )
            return Empty;

        var otherEnd = GetEndIndex( values, otherValues );
        var end = GetEndIndex( otherValues, values );

        var i = minIndex;
        var j = otherMinIndex;

        var resultBuffer = ListSlim<T>.Create();

        while ( i < end && j < otherEnd )
        {
            var (currentMin, currentMax) = (values[i], values[i + 1]);
            var (otherCurrentMin, otherCurrentMax) = (otherValues[j], otherValues[j + 1]);

            var minToMaxComparisonResult = currentMin.CompareTo( otherCurrentMax );
            if ( minToMaxComparisonResult > 0 )
            {
                j += 2;
                continue;
            }

            if ( minToMaxComparisonResult == 0 )
            {
                j += 2;
                resultBuffer.Add( currentMin );
                resultBuffer.Add( currentMin );
                continue;
            }

            var maxToMaxComparisonResult = currentMax.CompareTo( otherCurrentMax );
            var minToMinComparisonResult = currentMin.CompareTo( otherCurrentMin );

            if ( maxToMaxComparisonResult > 0 )
            {
                j += 2;
                resultBuffer.Add( minToMinComparisonResult >= 0 ? currentMin : otherCurrentMin );
                resultBuffer.Add( otherCurrentMax );
                continue;
            }

            i += 2;

            if ( maxToMaxComparisonResult == 0 )
            {
                j += 2;
                resultBuffer.Add( minToMinComparisonResult >= 0 ? currentMin : otherCurrentMin );
                resultBuffer.Add( currentMax );
                continue;
            }

            if ( minToMinComparisonResult >= 0 )
            {
                resultBuffer.Add( currentMin );
                resultBuffer.Add( currentMax );
                continue;
            }

            var maxToMinComparisonResult = currentMax.CompareTo( otherCurrentMin );
            if ( maxToMinComparisonResult < 0 )
                continue;

            resultBuffer.Add( otherCurrentMin );
            resultBuffer.Add( currentMax );
        }

        return CreateFromBuffer( resultBuffer );
    }

    /// <summary>
    /// Merges this collection of ranges with the provided <paramref name="value"/> range.
    /// </summary>
    /// <param name="value">Range to merge.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<T> MergeWith(Bounds<T> value)
    {
        return MergeWith( new BoundsRange<T>( value ) );
    }

    /// <summary>
    /// Merges this collection of ranges with the provided <paramref name="other"/> collections of ranges.
    /// </summary>
    /// <param name="other">Collections of ranges to merge.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    public BoundsRange<T> MergeWith(BoundsRange<T> other)
    {
        var values = InternalValues;
        var otherValues = other.InternalValues;

        if ( values.Length == 0 )
            return other;

        if ( otherValues.Length == 0 )
            return this;

        if ( ! TryGetStartIndex( values, otherValues, out var otherMinIndex ) )
        {
            var resultValues = new T[values.Length + otherValues.Length];
            otherValues.CopyTo( resultValues, 0 );
            values.CopyTo( resultValues, otherValues.Length );
            return new BoundsRange<T>( resultValues );
        }

        if ( ! TryGetStartIndex( otherValues, values, out var minIndex ) )
        {
            var resultValues = new T[values.Length + otherValues.Length];
            values.CopyTo( resultValues, 0 );
            otherValues.CopyTo( resultValues, values.Length );
            return new BoundsRange<T>( resultValues );
        }

        var otherEnd = GetEndIndex( values, otherValues );
        var end = GetEndIndex( otherValues, values );

        var i = 0;
        var j = 0;

        var resultBuffer = ListSlim<T>.Create();

        while ( i < minIndex )
        {
            resultBuffer.Add( values[i] );
            resultBuffer.Add( values[i + 1] );
            i += 2;
        }

        while ( j < otherMinIndex )
        {
            resultBuffer.Add( otherValues[j] );
            resultBuffer.Add( otherValues[j + 1] );
            j += 2;
        }

        while ( i < end && j < otherEnd )
        {
            var (currentMin, currentMax) = (values[i], values[i + 1]);
            var (otherCurrentMin, otherCurrentMax) = (otherValues[j], otherValues[j + 1]);

            var nextMin = currentMin.CompareTo( otherCurrentMin ) > 0 ? otherCurrentMin : currentMin;
            var maxToMaxComparisonResult = currentMax.CompareTo( otherCurrentMax );

            if ( maxToMaxComparisonResult > 0 )
            {
                j += 2;

                if ( resultBuffer.Count > 0 && nextMin.CompareTo( resultBuffer[^1] ) <= 0 )
                {
                    resultBuffer[^1] = otherCurrentMax;
                    continue;
                }

                resultBuffer.Add( nextMin );
                resultBuffer.Add( otherCurrentMax );
                continue;
            }

            i += 2;

            if ( maxToMaxComparisonResult == 0 )
                j += 2;

            if ( resultBuffer.Count > 0 && nextMin.CompareTo( resultBuffer[^1] ) <= 0 )
            {
                resultBuffer[^1] = currentMax;
                continue;
            }

            resultBuffer.Add( nextMin );
            resultBuffer.Add( currentMax );
        }

        if ( i < values.Length )
        {
            var (min, max) = (values[i], values[i + 1]);

            if ( min.CompareTo( resultBuffer[^1] ) <= 0 )
                resultBuffer[^1] = max;
            else
            {
                resultBuffer.Add( min );
                resultBuffer.Add( max );
            }

            i += 2;

            while ( i < values.Length )
            {
                resultBuffer.Add( values[i] );
                resultBuffer.Add( values[i + 1] );
                i += 2;
            }
        }
        else if ( j < otherValues.Length )
        {
            var (min, max) = (otherValues[j], otherValues[j + 1]);

            if ( min.CompareTo( resultBuffer[^1] ) <= 0 )
                resultBuffer[^1] = max;
            else
            {
                resultBuffer.Add( min );
                resultBuffer.Add( max );
            }

            j += 2;

            while ( j < otherValues.Length )
            {
                resultBuffer.Add( otherValues[j] );
                resultBuffer.Add( otherValues[j + 1] );
                j += 2;
            }
        }

        return CreateFromBuffer( resultBuffer );
    }

    /// <summary>
    /// Removes the provided <paramref name="value"/> range from this collection of ranges.
    /// </summary>
    /// <param name="value">Range to remove.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<T> Remove(Bounds<T> value)
    {
        return Remove( new BoundsRange<T>( value ) );
    }

    /// <summary>
    /// Removes the provided <paramref name="other"/> collection of ranges from this collection of ranges.
    /// </summary>
    /// <param name="other">Collections of ranges to remove.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    public BoundsRange<T> Remove(BoundsRange<T> other)
    {
        var values = InternalValues;
        var otherValues = other.InternalValues;

        if ( values.Length == 0 || otherValues.Length == 0 )
            return this;

        if ( ! TryGetStartIndex( values, otherValues, out var otherMinIndex ) )
            return this;

        if ( ! TryGetStartIndex( otherValues, values, out var minIndex ) )
            return this;

        var otherEnd = GetEndIndex( values, otherValues );
        var end = GetEndIndex( otherValues, values );

        var i = 0;
        var j = otherMinIndex;

        var resultBuffer = ListSlim<T>.Create();

        while ( i < minIndex )
        {
            resultBuffer.Add( values[i] );
            resultBuffer.Add( values[i + 1] );
            i += 2;
        }

        while ( i < end && j < otherEnd )
        {
            var (currentMin, currentMax) = (values[i], values[i + 1]);
            var (otherCurrentMin, otherCurrentMax) = (otherValues[j], otherValues[j + 1]);

            var minToMaxComparisonResult = currentMin.CompareTo( otherCurrentMax );
            if ( minToMaxComparisonResult > 0 )
            {
                j += 2;
                continue;
            }

            var maxToMaxComparisonResult = currentMax.CompareTo( otherCurrentMax );
            if ( minToMaxComparisonResult == 0 )
            {
                j += 2;

                if ( maxToMaxComparisonResult == 0 )
                    i += 2;

                continue;
            }

            var minToMinComparisonResult = currentMin.CompareTo( otherCurrentMin );
            if ( maxToMaxComparisonResult > 0 )
            {
                j += 2;

                if ( minToMinComparisonResult < 0 )
                {
                    if ( resultBuffer.Count > 0 && otherCurrentMin.CompareTo( resultBuffer[^1] ) <= 0 )
                    {
                        if ( otherCurrentMin.CompareTo( otherCurrentMax ) == 0 )
                            continue;

                        resultBuffer[^1] = otherCurrentMin;
                        resultBuffer.Add( otherCurrentMax );
                        resultBuffer.Add( currentMax );
                        continue;
                    }

                    if ( otherCurrentMin.CompareTo( otherCurrentMax ) == 0 )
                    {
                        resultBuffer.Add( currentMin );
                        resultBuffer.Add( currentMax );
                        continue;
                    }

                    resultBuffer.Add( currentMin );
                    resultBuffer.Add( otherCurrentMin );
                    resultBuffer.Add( otherCurrentMax );
                    resultBuffer.Add( currentMax );
                    continue;
                }

                resultBuffer.Add( otherCurrentMax );
                resultBuffer.Add( currentMax );
                continue;
            }

            i += 2;

            if ( maxToMaxComparisonResult == 0 )
            {
                j += 2;

                if ( minToMinComparisonResult < 0 )
                {
                    if ( resultBuffer.Count > 0 && otherCurrentMin.CompareTo( resultBuffer[^1] ) <= 0 )
                    {
                        if ( otherCurrentMin.CompareTo( otherCurrentMax ) == 0 )
                            continue;

                        resultBuffer[^1] = otherCurrentMin;
                        continue;
                    }

                    resultBuffer.Add( currentMin );
                    resultBuffer.Add( otherCurrentMin );
                }

                continue;
            }

            if ( minToMinComparisonResult >= 0 )
                continue;

            var maxToMinComparisonResult = currentMax.CompareTo( otherCurrentMin );
            if ( maxToMinComparisonResult > 0 )
            {
                if ( resultBuffer.Count > 0 && otherCurrentMin.CompareTo( resultBuffer[^1] ) <= 0 )
                {
                    resultBuffer[^1] = otherCurrentMin;
                    continue;
                }

                resultBuffer.Add( currentMin );
                resultBuffer.Add( otherCurrentMin );
                continue;
            }

            if ( resultBuffer.Count > 0 && currentMax.CompareTo( resultBuffer[^1] ) == 0 )
                continue;

            resultBuffer.Add( currentMin );
            resultBuffer.Add( currentMax );
        }

        if ( i < values.Length )
        {
            if ( resultBuffer.Count > 0 && values[i + 1].CompareTo( resultBuffer[^1] ) == 0 )
                i += 2;

            while ( i < values.Length )
            {
                resultBuffer.Add( values[i] );
                resultBuffer.Add( values[i + 1] );
                i += 2;
            }
        }

        return CreateFromBuffer( resultBuffer );
    }

    /// <summary>
    /// Complements this collection of ranges within its own minimum and maximum range of values.
    /// </summary>
    /// <returns>New <see cref="BoundsRange{T}"/> instance or <see cref="Empty"/> when this collection of ranges is empty.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<T> Complement()
    {
        var container = Flatten();
        return container is null ? Empty : Complement( new BoundsRange<T>( container.Value ) );
    }

    /// <summary>
    /// Complements this collection of ranges within the provided <paramref name="container"/> range.
    /// </summary>
    /// <param name="container">Range to complement this collection of ranges in.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<T> Complement(Bounds<T> container)
    {
        return Complement( new BoundsRange<T>( container ) );
    }

    /// <summary>
    /// Complements this collection of ranges within the provided <paramref name="container"/> collection of ranges.
    /// </summary>
    /// <param name="container">Collection of ranges to complement this collection of ranges in.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<T> Complement(BoundsRange<T> container)
    {
        return container.Remove( this );
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance by invoking the provided <paramref name="normalizePredicate"/>
    /// on each pair of (previous-bounds-maximum, current-bounds-minimum): when the predicate returns <b>true</b>,
    /// then [previous-bounds-minimum, previous-bounds-maximum] and [current-bounds-minimum, current-bounds-maximum]
    /// will be merged together into a single [previous-minimum, current-maximum] range.
    /// </summary>
    /// <param name="normalizePredicate">Normalization predicate.</param>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    public BoundsRange<T> Normalize(Func<T, T, bool> normalizePredicate)
    {
        var values = InternalValues;
        if ( values.Length <= 2 )
            return this;

        var buffer = ListSlim<T>.Create( minCapacity: 2 );
        buffer.Add( values[0] );
        buffer.Add( values[1] );

        for ( var i = 2; i < values.Length; i += 2 )
        {
            var lastMax = buffer[^1];
            var (min, max) = (values[i], values[i + 1]);

            if ( normalizePredicate( lastMax, min ) )
            {
                buffer[^1] = max;
                continue;
            }

            buffer.Add( min );
            buffer.Add( max );
        }

        return CreateFromBuffer( buffer );
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<Bounds<T>> GetEnumerator()
    {
        var values = InternalValues;
        for ( var i = 0; i < values.Length; i += 2 )
            yield return new Bounds<T>( (values[i], values[i + 1]) );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(BoundsRange<T> a, BoundsRange<T> b)
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
    public static bool operator !=(BoundsRange<T> a, BoundsRange<T> b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T[] ExtractAndValidateValues(IEnumerable<Bounds<T>> range)
    {
        List<T>? buffer;

        using ( var enumerator = range.GetEnumerator() )
        {
            if ( ! enumerator.MoveNext() )
                return Array.Empty<T>();

            buffer = range.TryGetNonEnumeratedCount( out var count ) ? new List<T>( capacity: count << 1 ) : new List<T>();
            var next = enumerator.Current;

            buffer.Add( next.Min );
            buffer.Add( next.Max );

            while ( enumerator.MoveNext() )
            {
                next = enumerator.Current;
                var last = buffer[^1];

                if ( last.CompareTo( next.Min ) != 0 )
                {
                    buffer.Add( next.Min );
                    buffer.Add( next.Max );
                    continue;
                }

                buffer[^1] = next.Max;
            }
        }

        Ensure.IsOrdered( buffer, nameof( range ) );
        return CreateDataFromBuffer( CollectionsMarshal.AsSpan( buffer ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool TryGetStartIndex(T[] values, T[] target, out int index)
    {
        index = Array.BinarySearch( target, values[0] );
        if ( index < 0 )
        {
            index = ~index;
            if ( index >= target.Length )
                return false;
        }

        if ( index.IsOdd() )
            --index;

        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int GetEndIndex(T[] values, T[] target)
    {
        var index = Array.BinarySearch( target, values[^1] );
        if ( index < 0 )
        {
            index = ~index;
            if ( index >= target.Length )
                return index;
        }

        return index.IsEven() ? index + 2 : index + 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T[] CreateDataFromBuffer(ReadOnlySpan<T> buffer)
    {
        var result = new T[buffer.Length];
        buffer.CopyTo( result );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static BoundsRange<T> CreateFromBuffer(ListSlim<T> buffer)
    {
        return buffer.Count == 0
            ? Empty
            : new BoundsRange<T>( CreateDataFromBuffer( buffer.AsSpan() ) );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
