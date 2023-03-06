using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Numerics;

public readonly struct IntegerPartition : IReadOnlyCollection<ulong>
{
    private readonly Fraction[]? _parts;
    private readonly ulong _sumOfPartsNumerator;

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
            var numerator = checked( _parts[i].Numerator * (long)(commonDenominator / _parts[i].Denominator) );
            _parts[i] = new Fraction( numerator, commonDenominator );
            var unsignedNumerator = unchecked( (ulong)numerator );
            _sumOfPartsNumerator = checked( _sumOfPartsNumerator + unsignedNumerator );
        }

        var sumGcd = MathUtils.Gcd( _sumOfPartsNumerator, commonDenominator );
        Sum = checked( Value * (_sumOfPartsNumerator / sumGcd) ) / (commonDenominator / sumGcd);
        (Quotient, Remainder) = _sumOfPartsNumerator > 0 ? Math.DivRem( Sum, _sumOfPartsNumerator ) : (0, 0);
    }

    public ulong Value { get; }
    public ulong Sum { get; }
    public ulong Quotient { get; }
    public ulong Remainder { get; }
    public int Count => Parts.Count;
    public IReadOnlyList<Fraction> Parts => _parts ?? Array.Empty<Fraction>();

    [Pure]
    public override string ToString()
    {
        return $"Partition {Value} into {Count} fraction part(s) with {Sum} sum";
    }

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
            _partCount = unchecked( (ulong)parts.Length );
            _quotient = quotient;
            _remainder = remainder;
            _sumOfPartsNumerator = sumOfPartsNumerator;
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

            Assume.IsNotNull( _parts, nameof( _parts ) );
            var numerator = unchecked( (ulong)_parts[_index].Numerator );
            (var q, _offset) = Math.DivRem( checked( _offset + _remainder * numerator ), _sumOfPartsNumerator );
            Current = unchecked( _quotient * numerator + q );
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
