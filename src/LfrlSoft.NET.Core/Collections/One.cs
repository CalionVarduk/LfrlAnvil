using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Collections
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
                Ensure.Equals( index, 0, nameof( index ) );
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
