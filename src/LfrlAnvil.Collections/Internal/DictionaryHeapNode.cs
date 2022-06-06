using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Internal
{
    internal sealed class DictionaryHeapNode<TKey, TValue>
    {
        internal DictionaryHeapNode(TKey key, TValue value, int index)
        {
            Key = key;
            Value = value;
            Index = index;
        }

        public TKey Key { get; }
        public TValue Value { get; set; }
        public int Index { get; private set; }

        [Pure]
        public override string ToString()
        {
            return $"[{Index}]: {Key} => {Value}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SwapIndexWith(DictionaryHeapNode<TKey, TValue> other)
        {
            (Index, other.Index) = (other.Index, Index);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void AssignIndexFrom(DictionaryHeapNode<TKey, TValue> other)
        {
            Index = other.Index;
        }
    }
}
