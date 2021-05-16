using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LfrlSoft.NET.Common.Tests.Extensions
{
    public static class IFixtureExtensions
    {
        public static T CreateDefault<T>(this IFixture fixture)
        {
            return default;
        }

        public static T CreateNotDefault<T>(this IFixture fixture)
        {
            return fixture.Create<Generator<T>>().First( v => !EqualityComparer<T>.Default.Equals( v, default ) );
        }

        public static (T Lesser, T Greater) CreateDistinctPair<T>(this IFixture fixture)
            where T : IComparable<T>
        {
            var first = fixture.Create<T>();
            var second = fixture.Create<Generator<T>>().First( v => !EqualityComparer<T>.Default.Equals( v, first ) );
            return first.CompareTo( second ) > 0 ? (second, first) : (first, second);
        }

        public static (T Lesser, T Between, T Greater) CreateDistinctTriple<T>(this IFixture fixture)
            where T : IComparable<T>
        {
            var first = fixture.Create<T>();
            var second = fixture.Create<Generator<T>>().First( v => !EqualityComparer<T>.Default.Equals( v, first ) );
            var third = fixture.Create<Generator<T>>()
                .First( v => !EqualityComparer<T>.Default.Equals( v, first ) && !EqualityComparer<T>.Default.Equals( v, second ) );

            var result = new[] { first, second, third };
            Array.Sort( result );
            return (result[0], result[1], result[2]);
        }

        public static T? CreateNullable<T>(this IFixture fixture)
            where T : struct
        {
            return fixture.Create<T>();
        }

        public static T? CreateDefaultNullable<T>(this IFixture fixture)
            where T : struct
        {
            return fixture.CreateDefault<T>();
        }
    }
}
