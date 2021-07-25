﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LfrlSoft.NET.Common.Collections
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

        public IEnumerator<T> GetEnumerator()
        {
            return _values.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
