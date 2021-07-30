using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlSoft.NET.Core.Extensions;

namespace LfrlSoft.NET.Core.Collections.Internal
{
    internal sealed class MemoizedEnumerable<T> : IEnumerable<T>
    {
        public Lazy<IEnumerable<T>> Source { get; }

        public MemoizedEnumerable(IEnumerable<T> source)
        {
            Source = new Lazy<IEnumerable<T>>( source.Materialize );
        }

        [Pure]
        public IEnumerator<T> GetEnumerator()
        {
            return Source.Value.GetEnumerator();
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
