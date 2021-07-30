using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlSoft.NET.Core.Collections
{
    public sealed class Many<T> : IReadOnlyList<T>
    {
        private readonly T[] _values;

        public Many(params T[] values)
        {
            _values = values;
        }

        public int Count => _values.Length;
        public T this[int index] => _values[index];

        [Pure]
        public IEnumerator<T> GetEnumerator()
        {
            return _values.AsEnumerable().GetEnumerator();
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
