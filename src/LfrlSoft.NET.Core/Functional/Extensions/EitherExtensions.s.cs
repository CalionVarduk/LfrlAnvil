using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Functional.Extensions
{
    public static class EitherExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Maybe<T1> ToMaybe<T1, T2>(this Either<T1, T2> source)
            where T1 : notnull
        {
            return source.HasFirst ? source.First : Maybe<T1>.None;
        }
    }
}
