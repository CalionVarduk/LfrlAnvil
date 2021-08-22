using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Internal
{
    internal sealed class InvertedComparer<T> : IComparer<T>
    {
        public readonly IComparer<T> BaseComparer;

        public InvertedComparer(IComparer<T> baseComparer)
        {
            BaseComparer = baseComparer;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int Compare(T x, T y)
        {
            return -BaseComparer.Compare( x, y );
        }
    }
}
