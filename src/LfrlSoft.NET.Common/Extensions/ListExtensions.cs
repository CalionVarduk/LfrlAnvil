using System.Collections.Generic;

namespace LfrlSoft.NET.Common.Extensions
{
    public static class ListExtensions
    {
        public static void SwapItems<T>(this IList<T> list, int index1, int index2)
        {
            var temp = list[index2];
            list[index2] = list[index1];
            list[index1] = temp;
        }
    }
}
