using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Functional.Extensions
{
    public static class TypeCastExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Maybe<TDestination> ToMaybe<TSource, TDestination>(this TypeCast<TSource, TDestination> source)
            where TDestination : notnull
        {
            return source.IsValid ? new Maybe<TDestination>( source.Result! ) : Maybe<TDestination>.None;
        }
    }
}
