using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Collections
{
    public static class Heap
    {
        [Pure]
        public static int GetParentIndex(int childIndex)
        {
            return (childIndex - 1) >> 1;
        }

        [Pure]
        public static int GetLeftChildIndex(int parentIndex)
        {
            return (parentIndex << 1) + 1;
        }

        [Pure]
        public static int GetRightChildIndex(int parentIndex)
        {
            return GetLeftChildIndex( parentIndex ) + 1;
        }
    }
}
