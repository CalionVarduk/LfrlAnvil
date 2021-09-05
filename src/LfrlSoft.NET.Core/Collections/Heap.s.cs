using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Collections
{
    public static class Heap
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetParentIndex(int childIndex)
        {
            return (childIndex - 1) >> 1;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetLeftChildIndex(int parentIndex)
        {
            return (parentIndex << 1) + 1;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static int GetRightChildIndex(int parentIndex)
        {
            return GetLeftChildIndex( parentIndex ) + 1;
        }
    }
}
