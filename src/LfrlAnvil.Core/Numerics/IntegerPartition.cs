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

namespace LfrlAnvil.Numerics;

/// <summary>
///A lightweight representation of an enumerable partition of an <see cref="UInt64"/> into specified fractional parts.
/// </summary>
public readonly struct IntegerPartition : IReadOnlyCollection<ulong>
{
    private readonly Fraction[]? _parts;
    private readonly ulong _sumOfPartsNumerator;

    /// <summary>
    /// Creates a new <see cref="IntegerPartition"/> instance.
    /// </summary>
    /// <param name="value">Value to partition.</param>
    /// <param name="parts">Fractional parts.</param>
    /// <exception cref="ArgumentOutOfRangeException">When any fractional part is less than <b>0</b>.</exception>
    public IntegerPartition(ulong value, params Fraction[] parts)
    {
        Value = value;
        _parts = parts;

        var commonDenominator = 1UL;
        for ( var i = 0; i < _parts.Length; ++i )
        {
            Ensure.IsGreaterThanOrEqualTo( _parts[i], Fraction.Zero, "each part" );
            _parts[i] = _parts[i].Simplify();
            commonDenominator = MathUtils.Lcm( commonDenominator, _parts[i].Denominator );
        }

        _sumOfPartsNumerator = 0UL;
        for ( var i = 0; i < _parts.Length; ++i )
        {
            var numerator = checked( _parts[i].Numerator * ( long )(commonDenominator / _parts[i].Denominator) );
            _parts[i] = new Fraction( numerator, commonDenominator );
            var unsignedNumerator = unchecked( ( ulong )numerator );
            _sumOfPartsNumerator = checked( _sumOfPartsNumerator + unsignedNumerator );
        }

        var sumGcd = MathUtils.Gcd( _sumOfPartsNumerator, commonDenominator );
        Sum = checked( Value * (_sumOfPartsNumerator / sumGcd) ) / (commonDenominator / sumGcd);
        (Quotient, Remainder) = _sumOfPartsNumerator > 0 ? Math.DivRem( Sum, _sumOfPartsNumerator ) : (0, 0);
    }

    /// <summary>
    /// Value to partition.
    /// </summary>
    public ulong Value { get; }

    /// <summary>
    /// Target sum which is the result of multiplication of the value to partition by the sum of all fractional parts.
    /// Sum of all resulting partitions will be equal to this value.
    /// </summary>
    public ulong Sum { get; }

    /// <summary>
    /// Base value for each part.
    /// </summary>
    public ulong Quotient { get; }

    /// <summary>
    /// An auxiliary integer division remainder used for filling out irregularities.
    /// </summary>
    public ulong Remainder { get; }

    /// <summary>
    /// Number of parts.
    /// </summary>
    public int Count => Parts.Count;

    /// <summary>
    /// Fractional parts.
    /// </summary>
    public ReadOnlyArray<Fraction> Parts => _parts ?? ReadOnlyArray<Fraction>.Empty;

    /// <summary>
    /// Returns a string representation of this <see cref="IntegerPartition"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Partition {Value} into {Count} fraction part(s) with {Sum} sum";
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this partition.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return _parts is null ? default : new Enumerator( _parts, Quotient, Remainder, _sumOfPartsNumerator );
    }

    [Pure]
    IEnumerator<ulong> IEnumerable<ulong>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="IntegerPartition"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<ulong>
    {
        private readonly Fraction[]? _parts;
        private readonly ulong _partCount;
        private readonly ulong _quotient;
        private readonly ulong _remainder;
        private readonly ulong _sumOfPartsNumerator;
        private int _index;
        private ulong _offset;

        internal Enumerator(Fraction[] parts, ulong quotient, ulong remainder, ulong sumOfPartsNumerator)
        {
            _parts = parts;
            _partCount = unchecked( ( ulong )parts.Length );
            _quotient = quotient;
            _remainder = remainder;
            _sumOfPartsNumerator = sumOfPartsNumerator;
            Current = _quotient;
            _index = -1;
            _offset = 0;
        }

        /// <inheritdoc />
        public ulong Current { get; private set; }

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if ( unchecked( ( ulong )++_index ) >= _partCount )
            {
                Assume.Equals( _offset, 0UL );
                return false;
            }

            Assume.IsNotNull( _parts );
            var numerator = unchecked( ( ulong )_parts[_index].Numerator );
            (var q, _offset) = Math.DivRem( checked( _offset + _remainder * numerator ), _sumOfPartsNumerator );
            Current = unchecked( _quotient * numerator + q );
            return true;
        }

        /// <inheritdoc />
        public void Dispose() { }

        void IEnumerator.Reset()
        {
            Current = _quotient;
            _index = -1;
            _offset = 0;
        }
    }
}
