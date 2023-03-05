using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Numerics;

public readonly struct IntegerFixedPartition : IReadOnlyCollection<ulong>
{
    public IntegerFixedPartition(ulong value, int partCount)
    {
        Ensure.IsGreaterThanOrEqualTo( partCount, 0, nameof( partCount ) );
        Value = value;
        Count = partCount;
        (Quotient, Remainder) = Count > 0 ? Math.DivRem( Value, unchecked( (ulong)Count ) ) : (0, 0);
    }

    public ulong Value { get; }
    public int Count { get; }
    public ulong Quotient { get; }
    public ulong Remainder { get; }

    [Pure]
    public override string ToString()
    {
        return $"{Value} into {Count} part(s)";
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator( Quotient, Remainder, unchecked( (ulong)Count ) );
    }

    IEnumerator<ulong> IEnumerable<ulong>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

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

        public ulong Current { get; private set; }
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if ( unchecked( (ulong)++_index ) >= _partCount )
            {
                Assume.Equals( _offset, 0UL, nameof( _offset ) );
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

        public void Dispose() { }

        void IEnumerator.Reset()
        {
            Current = _quotient;
            _index = -1;
            _offset = 0;
        }
    }
}
