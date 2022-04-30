using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Internal
{
    public sealed class MemoizedCollection<T> : IMemoizedCollection<T>
    {
        public MemoizedCollection(IEnumerable<T> source)
        {
            Source = new Lazy<IReadOnlyCollection<T>>( source.Materialize );
        }

        public Lazy<IReadOnlyCollection<T>> Source { get; }
        public int Count => Source.Value.Count;
        public bool IsMaterialized => Source.IsValueCreated;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
