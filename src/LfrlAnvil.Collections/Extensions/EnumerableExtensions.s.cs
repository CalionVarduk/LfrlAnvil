using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Extensions
{
    public static class EnumerableExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static MultiSet<T> ToMultiSet<T>(this IEnumerable<T> source)
            where T : notnull
        {
            return source.ToMultiSet( EqualityComparer<T>.Default );
        }

        [Pure]
        public static MultiSet<T> ToMultiSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
            where T : notnull
        {
            var result = new MultiSet<T>( comparer );
            foreach ( var e in source )
                result.Add( e );

            return result;
        }
    }
}
