using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions
{
    public static class ListExtensions
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SwapItems<T>(this IList<T> list, int index1, int index2)
        {
            (list[index2], list[index1]) = (list[index1], list[index2]);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void RemoveLast<T>(this IList<T> list)
        {
            list.RemoveAt( list.Count - 1 );
        }
    }
}
