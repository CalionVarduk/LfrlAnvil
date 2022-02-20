using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Extensions
{
    public static class ComparerExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static IComparer<T> Invert<T>(this IComparer<T> source)
        {
            return new InvertedComparer<T>( source );
        }
    }
}
