using System.Collections;
using System.Collections.Generic;

namespace LfrlSoft.NET.Common.Collections
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
                Assert.Equals( index, 0, nameof( index ) );
                return Value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
