using System;
using System.Collections.Generic;

namespace LfrlAnvil.Collections.Internal;

internal static class CollectionCopying
{
    internal static void CopyTo<T>(ICollection<T> source, T[] array, int arrayIndex)
    {
        var count = Math.Min( source.Count, array.Length - arrayIndex );
        var maxArrayIndex = arrayIndex + count - 1;

        if ( maxArrayIndex < 0 )
            return;

        using var enumerator = source.GetEnumerator();
        var index = arrayIndex;

        while ( index < 0 && enumerator.MoveNext() )
            ++index;

        while ( enumerator.MoveNext() && index <= maxArrayIndex )
            array[index++] = enumerator.Current;
    }
}
