﻿using System;
using System.Collections.Generic;
using LfrlSoft.NET.Common.Collections;
using LfrlSoft.NET.Common.Collections.Internal;

namespace LfrlSoft.NET.Common.Extensions
{
    public static class ObjectExtensions
    {
        public static One<T> ToOne<T>(this T source)
        {
            return One.Create( source );
        }

        public static IEnumerable<T2> Memoize<T1, T2>(this T1 source, Func<T1, IEnumerable<T2>> selector)
        {
            return new MemoizedEnumeration<T2>( selector( source ) );
        }

        public static IEnumerable<T> Visit<T>(this T? source, Func<T, T?> nodeSelector)
            where T : class
        {
            return source.Visit( nodeSelector!, e => e is null )!;
        }

        public static IEnumerable<T> Visit<T>(this T source, Func<T, T> nodeSelector, Func<T, bool> breakPredicate)
        {
            if ( breakPredicate( source ) )
                yield break;

            var current = nodeSelector( source );

            while ( ! breakPredicate( current ) )
            {
                yield return current;

                current = nodeSelector( current );
            }
        }

        public static IEnumerable<T> VisitMany<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            return nodeRangeSelector( source ).VisitMany( nodeRangeSelector );
        }

        public static IEnumerable<T> VisitWithSelf<T>(this T? source, Func<T, T?> nodeSelector)
            where T : class
        {
            return source.VisitWithSelf( nodeSelector!, e => e is null )!;
        }

        public static IEnumerable<T> VisitWithSelf<T>(this T source, Func<T, T> nodeSelector, Func<T, bool> breakPredicate)
        {
            if ( breakPredicate( source ) )
                yield break;

            yield return source;

            var current = nodeSelector( source );

            while ( ! breakPredicate( current ) )
            {
                yield return current;

                current = nodeSelector( current );
            }
        }

        public static IEnumerable<T> VisitManyWithSelf<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
        {
            yield return source;

            foreach ( var node in nodeRangeSelector( source ).VisitMany( nodeRangeSelector ) )
                yield return node;
        }
    }
}
