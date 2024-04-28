using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Numerics;

/// <summary>
///A lightweight representation of an enumerable partition of an <see cref="UInt64"/> into specified number of parts.
/// </summary>
public readonly struct IntegerFixedPartition : IReadOnlyCollection<ulong>
{
    /// <summary>
    /// Creates a new <see cref="IntegerFixedPartition"/> instance.
    /// </summary>
    /// <param name="value">Value to partition.</param>
    /// <param name="partCount">Number of parts.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="partCount"/> is less than <b>0</b>.</exception>
    public IntegerFixedPartition(ulong value, int partCount)
    {
        Ensure.IsGreaterThanOrEqualTo( partCount, 0 );
        Value = value;
        Count = partCount;
        (Quotient, Remainder) = Count > 0 ? Math.DivRem( Value, unchecked( ( ulong )Count ) ) : (0, 0);
    }

    /// <summary>
    /// Value to partition.
    /// </summary>
    public ulong Value { get; }

    /// <summary>
    /// Number of parts.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Base value for each part.
    /// </summary>
    public ulong Quotient { get; }

    /// <summary>
    /// An auxiliary integer division remainder used for filling out irregularities.
    /// </summary>
    public ulong Remainder { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="IntegerFixedPartition"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Partition {Value} into {Count} fixed part(s)";
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this partition.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Quotient, Remainder, unchecked( ( ulong )Count ) );
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
    /// Lightweight enumerator implementation for <see cref="IntegerFixedPartition"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<ulong>
    {
        private readonly ulong _quotient;
        private readonly ulong _remainder;
        private readonly ulong _partCount;
        private int _index;
        private ulong _offset;

        internal Enumerator(ulong quotient, ulong remainder, ulong partCount)
        {
            _quotient = quotient;
            _remainder = remainder;
            _partCount = partCount;
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

            Current = _quotient;
            _offset += _remainder;
            if ( _offset < _partCount )
                return true;

            ++Current;
            _offset -= _partCount;
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
