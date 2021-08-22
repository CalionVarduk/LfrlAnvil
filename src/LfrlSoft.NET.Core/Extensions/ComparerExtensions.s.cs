using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core.Extensions
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
