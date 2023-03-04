﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Numerics;

public readonly struct IntegerFixedPartition : IEnumerable<ulong>
{
    public IntegerFixedPartition(ulong value, ulong partCount)
    {
        Value = value;
        PartCount = partCount;
        (Quotient, Remainder) = PartCount > 0 ? Math.DivRem( Value, PartCount ) : (0, 0);
    }

    public ulong Value { get; }
    public ulong PartCount { get; }
    public ulong Quotient { get; }
    public ulong Remainder { get; }

    public Enumerator GetEnumerator()
    {
        return new Enumerator( Quotient, Remainder, PartCount );
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