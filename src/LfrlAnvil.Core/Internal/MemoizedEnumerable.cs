using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Internal
{
    public sealed class MemoizedEnumerable<T> : IEnumerable<T>
    {
        public Lazy<IReadOnlyCollection<T>> Source { get; }

        public MemoizedEnumerable(IEnumerable<T> source)
        {
            Source = new Lazy<IReadOnlyCollection<T>>( source.Materialize );
        }

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
