using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil
{
    public sealed class One<T> : IReadOnlyList<T>
    {
        public One(T value)
        {
            Value = value;
        }

        public T Value { get; }
        public int Count => 1;

        public T this[int index]
        {
            get
            {
                if ( index != 0 )
                    throw new IndexOutOfRangeException( ExceptionResources.ExpectedIndexToBeZero );

                return Value;
            }
        }

        [Pure]
        public IEnumerator<T> GetEnumerator()
        {
            yield return Value;
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
