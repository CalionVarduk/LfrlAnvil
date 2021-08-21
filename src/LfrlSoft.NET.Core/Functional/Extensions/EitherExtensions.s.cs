using System;
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

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Unsafe<T> ToUnsafe<T>(this Either<T, Exception> source)
        {
            return source.HasFirst ? new Unsafe<T>( source.First! ) : new Unsafe<T>( source.Second! );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Either<T1, T2> Reduce<T1, T2>(this Either<Either<T1, T2>, Either<T1, T2>> source)
        {
            return source.HasFirst ? source.First : source.Second;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Either<T1, T2> Reduce<T1, T2>(this Either<Either<T1, T2>, T1> source)
        {
            return source.HasFirst ? source.First : source.Second!;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Either<T1, T2> Reduce<T1, T2>(this Either<Either<T1, T2>, T2> source)
        {
            return source.HasFirst ? source.First : source.Second!;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Either<T1, T2> Reduce<T1, T2>(this Either<T1, Either<T1, T2>> source)
        {
            return source.HasFirst ? source.First! : source.Second;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Either<T1, T2> Reduce<T1, T2>(this Either<T2, Either<T1, T2>> source)
        {
            return source.HasFirst ? source.First! : source.Second;
        }
    }
}
