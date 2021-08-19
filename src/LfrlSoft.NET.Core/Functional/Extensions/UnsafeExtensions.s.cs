﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Functional.Extensions
{
    public static class UnsafeExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Maybe<T> ToMaybe<T>(this Unsafe<T> source)
            where T : notnull
        {
            return source.IsOk ? source.Value : Maybe<T>.None;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Either<T, Exception> ToEither<T>(this Unsafe<T> source)
        {
            return source.IsOk ? new Either<T, Exception>( source.Value! ) : new Either<T, Exception>( source.Error! );
        }
    }
}
