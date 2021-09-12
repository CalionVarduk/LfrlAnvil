using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ListExtensions
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SwapItems<T>(this IList<T> list, int index1, int index2)
        {
            (list[index2], list[index1]) = (list[index1], list[index2]);
        }
    }
}
