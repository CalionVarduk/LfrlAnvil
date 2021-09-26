using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Extensions;

namespace LfrlSoft.NET.Core
{
    public readonly struct BoundsRange<T> : IReadOnlyList<Bounds<T>>, IEquatable<BoundsRange<T>>
        where T : IComparable<T>
    {
        public static readonly BoundsRange<T> Empty = new BoundsRange<T>( Array.Empty<T>() );

        private readonly T[]? _values;

        public BoundsRange(Bounds<T> value)
        {
            _values = new[] { value.Min, value.Max };
        }

        public BoundsRange(IEnumerable<Bounds<T>> range)
        {
            _values = ExtractAndValidateValues( range );
        }

        private BoundsRange(T[] values)
        {
            _values = values;
        }

        public Bounds<T> this[int index]
        {
            get
            {
                var values = InternalValues;
                return new Bounds<T>( (values[index << 1], values[(index << 1) + 1]) );
            }
        }

        public int Count => InternalValues.Length >> 1;

        private T[] InternalValues => _values ?? Array.Empty<T>();

        [Pure]
        public override string ToString()
        {
            var rangeText = string.Join( " & ", this.Select( r => $"[{r.Min} : {r.Max}]" ) );
            return $"{nameof( BoundsRange )}({rangeText})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default.AddRange( InternalValues ).Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is BoundsRange<T> r && Equals( r );
        }

        [Pure]
        public bool Equals(BoundsRange<T> other)
        {
            return InternalValues.SequenceEqual( other.InternalValues );
        }

        [Pure]
        public Bounds<T>? Flatten()
        {
            var values = InternalValues;
            if ( values.Length == 0 )
                return null;

            return new Bounds<T>( (values[0], values[^1]) );
        }

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

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains(T value)
        {
            return FindBoundsIndex( value ) >= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains(Bounds<T> value)
        {
            var minIndex = FindBoundsIndex( value.Min );
            return minIndex >= 0 && FindBoundsIndex( value.Max ) == minIndex;
        }

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

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BoundsRange<T> GetIntersection(Bounds<T> value)
        {
            return GetIntersection( new BoundsRange<T>( value ) );
        }

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

            var resultBuffer = new List<T>();

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

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BoundsRange<T> MergeWith(Bounds<T> value)
        {
            return MergeWith( new BoundsRange<T>( value ) );
        }

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

            var resultBuffer = new List<T>();

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

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BoundsRange<T> Remove(Bounds<T> value)
        {
            return Remove( new BoundsRange<T>( value ) );
        }

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

            var resultBuffer = new List<T>();

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

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BoundsRange<T> Complement()
        {
            var container = Flatten();
            return container is null ? Empty : Complement( new BoundsRange<T>( container.Value ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BoundsRange<T> Complement(Bounds<T> container)
        {
            return Complement( new BoundsRange<T>( container ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BoundsRange<T> Complement(BoundsRange<T> container)
        {
            return container.Remove( this );
        }

        [Pure]
        public IEnumerator<Bounds<T>> GetEnumerator()
        {
            var values = InternalValues;
            for ( var i = 0; i < values.Length; i += 2 )
                yield return new Bounds<T>( (values[i], values[i + 1]) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(BoundsRange<T> a, BoundsRange<T> b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(BoundsRange<T> a, BoundsRange<T> b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

                buffer = new List<T>();
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
            return CreateDataFromBuffer( buffer );
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
        private static T[] CreateDataFromBuffer(List<T> buffer)
        {
            var result = new T[buffer.Count];
            buffer.CopyTo( result, 0 );
            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static BoundsRange<T> CreateFromBuffer(List<T> buffer)
        {
            return buffer.Count == 0
                ? Empty
                : new BoundsRange<T>( CreateDataFromBuffer( buffer ) );
        }
    }
}
