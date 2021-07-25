using System;
using System.Collections;
using System.Collections.Generic;
using LfrlSoft.NET.Common.Extensions;

namespace LfrlSoft.NET.Common.Collections.Internal
{
    internal sealed class MemoizedEnumeration<T> : IEnumerable<T>
    {
        public Lazy<IEnumerable<T>> Source { get; }

        public MemoizedEnumeration(IEnumerable<T> source)
        {
            Source = new Lazy<IEnumerable<T>>( source.Materialize );
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Source.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
