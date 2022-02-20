using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions
{
    public static class MaybeExtensions
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Maybe<T> Reduce<T>(this Maybe<Maybe<T>> source)
            where T : notnull
        {
            return source.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T3 MatchWith<T1, T2, T3>(
            this Maybe<T1> source,
            Maybe<T2> other,
            Func<T1, T2, T3> both,
            Func<T1, T3> first,
            Func<T2, T3> second,
            Func<T3> none)
            where T1 : notnull
            where T2 : notnull
        {
            if ( source.HasValue )
                return other.HasValue ? both( source.Value!, other.Value! ) : first( source.Value! );

            return other.HasValue ? second( other.Value! ) : none();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Nil MatchWith<T1, T2>(
            this Maybe<T1> source,
            Maybe<T2> other,
            Action<T1, T2> both,
            Action<T1> first,
            Action<T2> second,
            Action none)
            where T1 : notnull
            where T2 : notnull
        {
            if ( source.HasValue )
            {
                if ( other.HasValue )
                    both( source.Value!, other.Value! );
                else
                    first( source.Value! );
            }
            else if ( other.HasValue )
                second( other.Value! );
            else
                none();

            return Nil.Instance;
        }
    }
}
