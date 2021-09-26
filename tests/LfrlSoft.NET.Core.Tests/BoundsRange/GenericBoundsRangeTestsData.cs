using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.BoundsRange
{
    public class GenericBoundsRangeTestsData<T>
        where T : IComparable<T>
    {
        public static TheoryData<IEnumerable<Bounds<T>>, string> GetToStringData(IFixture fixture)
        {
            var (a, b, c, d) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new TheoryData<IEnumerable<Bounds<T>>, string>
            {
                { Array.Empty<Bounds<T>>(), "BoundsRange()" },
                { new[] { Bind( a, a ) }, $"BoundsRange([{a} : {a}])" },
                { new[] { Bind( a, b ) }, $"BoundsRange([{a} : {b}])" },
                { new[] { Bind( a, b ), Bind( c, d ) }, $"BoundsRange([{a} : {b}] & [{c} : {d}])" }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>> GetGetHashCodeData(IFixture fixture)
        {
            var (a, b, c, d) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new TheoryData<IEnumerable<Bounds<T>>>
            {
                Array.Empty<Bounds<T>>(),
                new[] { Bind( a, a ) },
                new[] { Bind( a, b ) },
                new[] { Bind( a, b ), Bind( c, d ) }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, bool> GetEqualsData(IFixture fixture)
        {
            var (a, b, c, d, e, f) = fixture.CreateDistinctSortedCollection<T>( 6 );

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, bool>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), true },
                { new[] { Bind( a, a ) }, new[] { Bind( a, a ) }, true },
                { new[] { Bind( a, b ), Bind( c, d ) }, new[] { Bind( a, b ), Bind( c, d ) }, true },
                { new[] { Bind( a, a ) }, new[] { Bind( a, b ) }, false },
                { new[] { Bind( a, b ), Bind( c, d ) }, new[] { Bind( a, b ), Bind( e, f ) }, false }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, T, int> GetFindBoundsIndexData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 13 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12]);

            var range = new[] { Bind( b, d ), Bind( f, h ), Bind( j, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, T, int>
            {
                { Array.Empty<Bounds<T>>(), a, ~0 },
                { range, a, ~0 },
                { range, b, 0 },
                { range, c, 0 },
                { range, d, 0 },
                { range, e, ~1 },
                { range, f, 1 },
                { range, g, 1 },
                { range, h, 1 },
                { range, i, ~2 },
                { range, j, 2 },
                { range, k, 2 },
                { range, l, 2 },
                { range, m, ~3 }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, T, Bounds<T>?> GetFindBoundsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 13 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12]);

            var range = new[] { Bind( b, d ), Bind( f, h ), Bind( j, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, T, Bounds<T>?>
            {
                { Array.Empty<Bounds<T>>(), a, null },
                { range, a, null },
                { range, b, Bind( b, d ) },
                { range, c, Bind( b, d ) },
                { range, d, Bind( b, d ) },
                { range, e, null },
                { range, f, Bind( f, h ) },
                { range, g, Bind( f, h ) },
                { range, h, Bind( f, h ) },
                { range, i, null },
                { range, j, Bind( j, l ) },
                { range, k, Bind( j, l ) },
                { range, l, Bind( j, l ) },
                { range, m, null }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, T, bool> GetContainsWithSingleValueData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 13 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12]);

            var range = new[] { Bind( b, d ), Bind( f, h ), Bind( j, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, T, bool>
            {
                { Array.Empty<Bounds<T>>(), a, false },
                { range, a, false },
                { range, b, true },
                { range, c, true },
                { range, d, true },
                { range, e, false },
                { range, f, true },
                { range, g, true },
                { range, h, true },
                { range, i, false },
                { range, j, true },
                { range, k, true },
                { range, l, true },
                { range, m, false }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, bool> GetContainsWithBoundsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, bool>
            {
                { Array.Empty<Bounds<T>>(), Bind( a, a ), false },
                { range, Bind( a, a ), false },
                { range, Bind( a, b ), false },
                { range, Bind( a, c ), false },
                { range, Bind( a, d ), false },
                { range, Bind( a, e ), false },
                { range, Bind( a, f ), false },
                { range, Bind( a, g ), false },
                { range, Bind( a, h ), false },
                { range, Bind( a, i ), false },
                { range, Bind( a, j ), false },
                { range, Bind( a, k ), false },
                { range, Bind( a, l ), false },
                { range, Bind( a, m ), false },
                { range, Bind( a, n ), false },
                { range, Bind( b, b ), false },
                { range, Bind( b, c ), false },
                { range, Bind( b, d ), false },
                { range, Bind( b, e ), false },
                { range, Bind( b, f ), false },
                { range, Bind( b, g ), false },
                { range, Bind( b, h ), false },
                { range, Bind( b, i ), false },
                { range, Bind( b, j ), false },
                { range, Bind( b, k ), false },
                { range, Bind( b, l ), false },
                { range, Bind( b, m ), false },
                { range, Bind( b, n ), false },
                { range, Bind( c, c ), true },
                { range, Bind( c, d ), true },
                { range, Bind( c, e ), true },
                { range, Bind( c, f ), true },
                { range, Bind( c, g ), false },
                { range, Bind( c, h ), false },
                { range, Bind( c, i ), false },
                { range, Bind( c, j ), false },
                { range, Bind( c, k ), false },
                { range, Bind( c, l ), false },
                { range, Bind( c, m ), false },
                { range, Bind( c, n ), false },
                { range, Bind( d, d ), true },
                { range, Bind( d, e ), true },
                { range, Bind( d, f ), true },
                { range, Bind( d, g ), false },
                { range, Bind( d, h ), false },
                { range, Bind( d, i ), false },
                { range, Bind( d, j ), false },
                { range, Bind( d, k ), false },
                { range, Bind( d, l ), false },
                { range, Bind( d, m ), false },
                { range, Bind( d, n ), false },
                { range, Bind( e, e ), true },
                { range, Bind( e, f ), true },
                { range, Bind( e, g ), false },
                { range, Bind( e, h ), false },
                { range, Bind( e, i ), false },
                { range, Bind( e, j ), false },
                { range, Bind( e, k ), false },
                { range, Bind( e, l ), false },
                { range, Bind( e, m ), false },
                { range, Bind( e, n ), false },
                { range, Bind( f, f ), true },
                { range, Bind( f, g ), false },
                { range, Bind( f, h ), false },
                { range, Bind( f, i ), false },
                { range, Bind( f, j ), false },
                { range, Bind( f, k ), false },
                { range, Bind( f, l ), false },
                { range, Bind( f, m ), false },
                { range, Bind( f, n ), false },
                { range, Bind( g, g ), false },
                { range, Bind( g, h ), false },
                { range, Bind( g, i ), false },
                { range, Bind( g, j ), false },
                { range, Bind( g, k ), false },
                { range, Bind( g, l ), false },
                { range, Bind( g, m ), false },
                { range, Bind( g, n ), false },
                { range, Bind( h, h ), false },
                { range, Bind( h, i ), false },
                { range, Bind( h, j ), false },
                { range, Bind( h, k ), false },
                { range, Bind( h, l ), false },
                { range, Bind( h, m ), false },
                { range, Bind( h, n ), false },
                { range, Bind( i, i ), true },
                { range, Bind( i, j ), true },
                { range, Bind( i, k ), true },
                { range, Bind( i, l ), true },
                { range, Bind( i, m ), false },
                { range, Bind( i, n ), false },
                { range, Bind( j, j ), true },
                { range, Bind( j, k ), true },
                { range, Bind( j, l ), true },
                { range, Bind( j, m ), false },
                { range, Bind( j, n ), false },
                { range, Bind( k, k ), true },
                { range, Bind( k, l ), true },
                { range, Bind( k, m ), false },
                { range, Bind( k, n ), false },
                { range, Bind( l, l ), true },
                { range, Bind( l, m ), false },
                { range, Bind( l, n ), false },
                { range, Bind( m, m ), false },
                { range, Bind( m, n ), false },
                { range, Bind( n, n ), false }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, bool> GetContainsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, bool>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), true },
                { range, Array.Empty<Bounds<T>>(), true },
                { Array.Empty<Bounds<T>>(), range, false },
                { range, range, true },
                { range, new[] { Bind( a, b ) }, false },
                { range, new[] { Bind( a, c ) }, false },
                { range, new[] { Bind( a, e ) }, false },
                { range, new[] { Bind( a, f ) }, false },
                { range, new[] { Bind( a, g ) }, false },
                { range, new[] { Bind( c, c ) }, true },
                { range, new[] { Bind( c, d ) }, true },
                { range, new[] { Bind( c, f ) }, true },
                { range, new[] { Bind( c, g ) }, false },
                { range, new[] { Bind( d, e ) }, true },
                { range, new[] { Bind( d, f ) }, true },
                { range, new[] { Bind( d, g ) }, false },
                { range, new[] { Bind( f, f ) }, true },
                { range, new[] { Bind( f, g ) }, false },
                { range, new[] { Bind( f, i ) }, false },
                { range, new[] { Bind( g, h ) }, false },
                { range, new[] { Bind( e, k ) }, false },
                { range, new[] { Bind( i, l ) }, true },
                { range, new[] { Bind( l, n ) }, false },
                { range, new[] { Bind( m, n ) }, false },
                { range, new[] { Bind( a, b ), Bind( g, h ) }, false },
                { range, new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) }, false },
                { range, new[] { Bind( g, h ), Bind( m, n ) }, false },
                { range, new[] { Bind( b, d ), Bind( g, h ), Bind( k, n ) }, false },
                { range, new[] { Bind( d, e ), Bind( i, k ) }, true },
                { range, new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ), Bind( m, m ) }, false },
                { range, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) }, true },
                { range, new[] { Bind( b, k ), Bind( l, m ) }, false },
                { new[] { Bind( a, n ) }, range, true },
                { new[] { Bind( a, n ) }, new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }, true },
                { new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }, new[] { Bind( a, n ) }, false },
                { new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, d ), Bind( e, f ) }, false },
                { new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( i, j ), Bind( k, l ) }, false },
                { new[] { Bind( c, c ) }, new[] { Bind( c, c ) }, true },
                { new[] { Bind( c, d ) }, new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) }, false },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, h ) }, false },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, f ) }, true },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( g, g ) }, true }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, bool> GetIntersectsWithBoundsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, bool>
            {
                { Array.Empty<Bounds<T>>(), Bind( a, a ), false },
                { range, Bind( a, a ), false },
                { range, Bind( a, b ), false },
                { range, Bind( a, c ), true },
                { range, Bind( a, d ), true },
                { range, Bind( a, e ), true },
                { range, Bind( a, f ), true },
                { range, Bind( a, g ), true },
                { range, Bind( a, h ), true },
                { range, Bind( a, i ), true },
                { range, Bind( a, j ), true },
                { range, Bind( a, k ), true },
                { range, Bind( a, l ), true },
                { range, Bind( a, m ), true },
                { range, Bind( a, n ), true },
                { range, Bind( b, b ), false },
                { range, Bind( b, c ), true },
                { range, Bind( b, d ), true },
                { range, Bind( b, e ), true },
                { range, Bind( b, f ), true },
                { range, Bind( b, g ), true },
                { range, Bind( b, h ), true },
                { range, Bind( b, i ), true },
                { range, Bind( b, j ), true },
                { range, Bind( b, k ), true },
                { range, Bind( b, l ), true },
                { range, Bind( b, m ), true },
                { range, Bind( b, n ), true },
                { range, Bind( c, c ), true },
                { range, Bind( c, d ), true },
                { range, Bind( c, e ), true },
                { range, Bind( c, f ), true },
                { range, Bind( c, g ), true },
                { range, Bind( c, h ), true },
                { range, Bind( c, i ), true },
                { range, Bind( c, j ), true },
                { range, Bind( c, k ), true },
                { range, Bind( c, l ), true },
                { range, Bind( c, m ), true },
                { range, Bind( c, n ), true },
                { range, Bind( d, d ), true },
                { range, Bind( d, e ), true },
                { range, Bind( d, f ), true },
                { range, Bind( d, g ), true },
                { range, Bind( d, h ), true },
                { range, Bind( d, i ), true },
                { range, Bind( d, j ), true },
                { range, Bind( d, k ), true },
                { range, Bind( d, l ), true },
                { range, Bind( d, m ), true },
                { range, Bind( d, n ), true },
                { range, Bind( e, e ), true },
                { range, Bind( e, f ), true },
                { range, Bind( e, g ), true },
                { range, Bind( e, h ), true },
                { range, Bind( e, i ), true },
                { range, Bind( e, j ), true },
                { range, Bind( e, k ), true },
                { range, Bind( e, l ), true },
                { range, Bind( e, m ), true },
                { range, Bind( e, n ), true },
                { range, Bind( f, f ), true },
                { range, Bind( f, g ), true },
                { range, Bind( f, h ), true },
                { range, Bind( f, i ), true },
                { range, Bind( f, j ), true },
                { range, Bind( f, k ), true },
                { range, Bind( f, l ), true },
                { range, Bind( f, m ), true },
                { range, Bind( f, n ), true },
                { range, Bind( g, g ), false },
                { range, Bind( g, h ), false },
                { range, Bind( g, i ), true },
                { range, Bind( g, j ), true },
                { range, Bind( g, k ), true },
                { range, Bind( g, l ), true },
                { range, Bind( g, m ), true },
                { range, Bind( g, n ), true },
                { range, Bind( h, h ), false },
                { range, Bind( h, i ), true },
                { range, Bind( h, j ), true },
                { range, Bind( h, k ), true },
                { range, Bind( h, l ), true },
                { range, Bind( h, m ), true },
                { range, Bind( h, n ), true },
                { range, Bind( i, i ), true },
                { range, Bind( i, j ), true },
                { range, Bind( i, k ), true },
                { range, Bind( i, l ), true },
                { range, Bind( i, m ), true },
                { range, Bind( i, n ), true },
                { range, Bind( j, j ), true },
                { range, Bind( j, k ), true },
                { range, Bind( j, l ), true },
                { range, Bind( j, m ), true },
                { range, Bind( j, n ), true },
                { range, Bind( k, k ), true },
                { range, Bind( k, l ), true },
                { range, Bind( k, m ), true },
                { range, Bind( k, n ), true },
                { range, Bind( l, l ), true },
                { range, Bind( l, m ), true },
                { range, Bind( l, n ), true },
                { range, Bind( m, m ), false },
                { range, Bind( m, n ), false },
                { range, Bind( n, n ), false }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, bool> GetIntersectsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, bool>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), false },
                { range, Array.Empty<Bounds<T>>(), false },
                { Array.Empty<Bounds<T>>(), range, false },
                { range, range, true },
                { range, new[] { Bind( a, b ) }, false },
                { range, new[] { Bind( a, c ) }, true },
                { range, new[] { Bind( a, e ) }, true },
                { range, new[] { Bind( a, f ) }, true },
                { range, new[] { Bind( a, g ) }, true },
                { range, new[] { Bind( c, c ) }, true },
                { range, new[] { Bind( c, d ) }, true },
                { range, new[] { Bind( c, f ) }, true },
                { range, new[] { Bind( c, g ) }, true },
                { range, new[] { Bind( d, e ) }, true },
                { range, new[] { Bind( d, f ) }, true },
                { range, new[] { Bind( d, g ) }, true },
                { range, new[] { Bind( f, f ) }, true },
                { range, new[] { Bind( f, g ) }, true },
                { range, new[] { Bind( f, i ) }, true },
                { range, new[] { Bind( g, h ) }, false },
                { range, new[] { Bind( e, k ) }, true },
                { range, new[] { Bind( i, l ) }, true },
                { range, new[] { Bind( l, n ) }, true },
                { range, new[] { Bind( m, n ) }, false },
                { range, new[] { Bind( a, b ), Bind( g, h ) }, false },
                { range, new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) }, false },
                { range, new[] { Bind( g, h ), Bind( m, n ) }, false },
                { range, new[] { Bind( b, d ), Bind( g, h ), Bind( k, n ) }, true },
                { range, new[] { Bind( d, e ), Bind( i, k ) }, true },
                { range, new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ), Bind( m, m ) }, true },
                { range, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) }, true },
                { range, new[] { Bind( b, k ), Bind( l, m ) }, true },
                { new[] { Bind( a, n ) }, range, true },
                { new[] { Bind( a, n ) }, new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }, true },
                { new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }, new[] { Bind( a, n ) }, true },
                { new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, d ), Bind( e, f ) }, false },
                { new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( i, j ), Bind( k, l ) }, false },
                { new[] { Bind( c, c ) }, new[] { Bind( c, c ) }, true },
                { new[] { Bind( c, d ) }, new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) }, true },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, h ) }, true },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, f ) }, true },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( g, g ) }, true }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>> GetGetIntersectionWithBoundsData(
            IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Bind( a, a ), Array.Empty<Bounds<T>>() },
                { range, Bind( a, a ), Array.Empty<Bounds<T>>() },
                { range, Bind( a, b ), Array.Empty<Bounds<T>>() },
                { range, Bind( a, c ), new[] { Bind( c, c ) } },
                { range, Bind( a, f ), new[] { Bind( c, f ) } },
                { range, Bind( a, g ), new[] { Bind( c, f ) } },
                { range, Bind( c, c ), new[] { Bind( c, c ) } },
                { range, Bind( c, e ), new[] { Bind( c, e ) } },
                { range, Bind( c, f ), new[] { Bind( c, f ) } },
                { range, Bind( c, g ), new[] { Bind( c, f ) } },
                { range, Bind( d, e ), new[] { Bind( d, e ) } },
                { range, Bind( d, f ), new[] { Bind( d, f ) } },
                { range, Bind( d, g ), new[] { Bind( d, f ) } },
                { range, Bind( f, f ), new[] { Bind( f, f ) } },
                { range, Bind( f, g ), new[] { Bind( f, f ) } },
                { range, Bind( g, h ), Array.Empty<Bounds<T>>() },
                { range, Bind( m, n ), Array.Empty<Bounds<T>>() },
                { range, Bind( l, n ), new[] { Bind( l, l ) } },
                { range, Bind( i, l ), new[] { Bind( i, l ) } },
                { range, Bind( e, k ), new[] { Bind( e, f ), Bind( i, k ) } },
                {
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    Bind( a, n ),
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }
                }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>> GetGetIntersectionData(
            IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { range, Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { Array.Empty<Bounds<T>>(), range, Array.Empty<Bounds<T>>() },
                { range, range, range },
                { range, new[] { Bind( a, b ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( a, c ) }, new[] { Bind( c, c ) } },
                { range, new[] { Bind( a, e ) }, new[] { Bind( c, e ) } },
                { range, new[] { Bind( a, f ) }, new[] { Bind( c, f ) } },
                { range, new[] { Bind( a, g ) }, new[] { Bind( c, f ) } },
                { range, new[] { Bind( c, c ) }, new[] { Bind( c, c ) } },
                { range, new[] { Bind( c, d ) }, new[] { Bind( c, d ) } },
                { range, new[] { Bind( c, f ) }, new[] { Bind( c, f ) } },
                { range, new[] { Bind( c, g ) }, new[] { Bind( c, f ) } },
                { range, new[] { Bind( d, e ) }, new[] { Bind( d, e ) } },
                { range, new[] { Bind( d, f ) }, new[] { Bind( d, f ) } },
                { range, new[] { Bind( d, g ) }, new[] { Bind( d, f ) } },
                { range, new[] { Bind( f, f ) }, new[] { Bind( f, f ) } },
                { range, new[] { Bind( f, g ) }, new[] { Bind( f, f ) } },
                { range, new[] { Bind( f, i ) }, new[] { Bind( f, f ), Bind( i, i ) } },
                { range, new[] { Bind( g, h ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( e, k ) }, new[] { Bind( e, f ), Bind( i, k ) } },
                { range, new[] { Bind( i, l ) }, new[] { Bind( i, l ) } },
                { range, new[] { Bind( l, n ) }, new[] { Bind( l, l ) } },
                { range, new[] { Bind( m, n ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( a, b ), Bind( g, h ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( g, h ), Bind( m, n ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( b, d ), Bind( g, h ), Bind( k, n ) }, new[] { Bind( c, d ), Bind( k, l ) } },
                { range, new[] { Bind( d, e ), Bind( i, k ) }, new[] { Bind( d, e ), Bind( i, k ) } },
                {
                    range,
                    new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ), Bind( m, m ) },
                    new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ) }
                },
                { range, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) }, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) } },
                { range, new[] { Bind( b, k ), Bind( l, m ) }, new[] { Bind( c, f ), Bind( i, k ), Bind( l, l ) } },
                { new[] { Bind( a, n ) }, range, range },
                {
                    new[] { Bind( a, n ) },
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }
                },
                {
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( a, n ) },
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }
                },
                { new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, d ), Bind( e, f ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( i, j ), Bind( k, l ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( c, c ) }, new[] { Bind( c, c ) }, new[] { Bind( c, c ) } },
                { new[] { Bind( c, d ) }, new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) }, new[] { Bind( c, d ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, h ) }, new[] { Bind( c, d ), Bind( f, g ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, f ) }, new[] { Bind( c, d ), Bind( f, f ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( g, g ) }, new[] { Bind( c, d ), Bind( g, g ) } }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>> GetMergeWithWithBoundsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Bind( a, a ), new[] { Bind( a, a ) } },
                { range, Bind( a, a ), new[] { Bind( a, a ), Bind( c, f ), Bind( i, l ) } },
                { range, Bind( a, b ), new[] { Bind( a, b ), Bind( c, f ), Bind( i, l ) } },
                { range, Bind( a, c ), new[] { Bind( a, f ), Bind( i, l ) } },
                { range, Bind( a, f ), new[] { Bind( a, f ), Bind( i, l ) } },
                { range, Bind( a, g ), new[] { Bind( a, g ), Bind( i, l ) } },
                { range, Bind( c, c ), range },
                { range, Bind( c, e ), range },
                { range, Bind( c, f ), range },
                { range, Bind( c, g ), new[] { Bind( c, g ), Bind( i, l ) } },
                { range, Bind( d, e ), range },
                { range, Bind( d, f ), range },
                { range, Bind( d, g ), new[] { Bind( c, g ), Bind( i, l ) } },
                { range, Bind( f, f ), range },
                { range, Bind( f, g ), new[] { Bind( c, g ), Bind( i, l ) } },
                { range, Bind( g, h ), new[] { Bind( c, f ), Bind( g, h ), Bind( i, l ) } },
                { range, Bind( m, n ), new[] { Bind( c, f ), Bind( i, l ), Bind( m, n ) } },
                { range, Bind( l, n ), new[] { Bind( c, f ), Bind( i, n ) } },
                { range, Bind( i, l ), range },
                { range, Bind( e, k ), new[] { Bind( c, l ) } },
                { new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }, Bind( a, n ), new[] { Bind( a, n ) } }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>> GetMergeWithData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { range, Array.Empty<Bounds<T>>(), range },
                { Array.Empty<Bounds<T>>(), range, range },
                { range, range, range },
                { range, new[] { Bind( a, b ) }, new[] { Bind( a, b ), Bind( c, f ), Bind( i, l ) } },
                { range, new[] { Bind( a, c ) }, new[] { Bind( a, f ), Bind( i, l ) } },
                { range, new[] { Bind( a, e ) }, new[] { Bind( a, f ), Bind( i, l ) } },
                { range, new[] { Bind( a, f ) }, new[] { Bind( a, f ), Bind( i, l ) } },
                { range, new[] { Bind( a, g ) }, new[] { Bind( a, g ), Bind( i, l ) } },
                { range, new[] { Bind( c, c ) }, range },
                { range, new[] { Bind( c, d ) }, range },
                { range, new[] { Bind( c, f ) }, range },
                { range, new[] { Bind( c, g ) }, new[] { Bind( c, g ), Bind( i, l ) } },
                { range, new[] { Bind( d, e ) }, range },
                { range, new[] { Bind( d, f ) }, range },
                { range, new[] { Bind( d, g ) }, new[] { Bind( c, g ), Bind( i, l ) } },
                { range, new[] { Bind( f, f ) }, range },
                { range, new[] { Bind( f, g ) }, new[] { Bind( c, g ), Bind( i, l ) } },
                { range, new[] { Bind( f, i ) }, new[] { Bind( c, l ) } },
                { range, new[] { Bind( g, h ) }, new[] { Bind( c, f ), Bind( g, h ), Bind( i, l ) } },
                { range, new[] { Bind( e, k ) }, new[] { Bind( c, l ) } },
                { range, new[] { Bind( i, l ) }, range },
                { range, new[] { Bind( l, n ) }, new[] { Bind( c, f ), Bind( i, n ) } },
                { range, new[] { Bind( m, n ) }, new[] { Bind( c, f ), Bind( i, l ), Bind( m, n ) } },
                { range, new[] { Bind( a, b ), Bind( g, h ) }, new[] { Bind( a, b ), Bind( c, f ), Bind( g, h ), Bind( i, l ) } },
                {
                    range,
                    new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) },
                    new[] { Bind( a, b ), Bind( c, f ), Bind( g, h ), Bind( i, l ), Bind( m, n ) }
                },
                { range, new[] { Bind( g, h ), Bind( m, n ) }, new[] { Bind( c, f ), Bind( g, h ), Bind( i, l ), Bind( m, n ) } },
                { range, new[] { Bind( b, d ), Bind( g, h ), Bind( k, n ) }, new[] { Bind( b, f ), Bind( g, h ), Bind( i, n ) } },
                { range, new[] { Bind( d, e ), Bind( i, k ) }, range },
                {
                    range,
                    new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ), Bind( m, m ) },
                    new[] { Bind( c, f ), Bind( i, l ), Bind( m, m ) }
                },
                { range, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) }, range },
                { range, new[] { Bind( b, k ), Bind( l, m ) }, new[] { Bind( b, m ) } },
                { new[] { Bind( a, n ) }, range, new[] { Bind( a, n ) } },
                {
                    new[] { Bind( a, n ) },
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( a, n ) }
                },
                {
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( a, n ) },
                    new[] { Bind( a, n ) }
                },
                {
                    new[] { Bind( i, j ), Bind( k, l ) },
                    new[] { Bind( c, d ), Bind( e, f ) },
                    new[] { Bind( c, d ), Bind( e, f ), Bind( i, j ), Bind( k, l ) }
                },
                {
                    new[] { Bind( c, d ), Bind( e, f ) },
                    new[] { Bind( i, j ), Bind( k, l ) },
                    new[] { Bind( c, d ), Bind( e, f ), Bind( i, j ), Bind( k, l ) }
                },
                { new[] { Bind( c, c ) }, new[] { Bind( c, c ) }, new[] { Bind( c, c ) } },
                {
                    new[] { Bind( c, d ) },
                    new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) },
                    new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) }
                },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, h ) }, new[] { Bind( b, h ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, f ) }, new[] { Bind( b, g ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( g, g ) }, new[] { Bind( b, g ) } }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>> GetRemoveWithBoundsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Bind( a, a ), Array.Empty<Bounds<T>>() },
                { range, Bind( a, a ), range },
                { range, Bind( a, b ), range },
                { range, Bind( a, c ), range },
                { range, Bind( a, f ), new[] { Bind( i, l ) } },
                { range, Bind( a, g ), new[] { Bind( i, l ) } },
                { range, Bind( c, c ), range },
                { range, Bind( c, e ), new[] { Bind( e, f ), Bind( i, l ) } },
                { range, Bind( c, f ), new[] { Bind( i, l ) } },
                { range, Bind( c, g ), new[] { Bind( i, l ) } },
                { range, Bind( d, e ), new[] { Bind( c, d ), Bind( e, f ), Bind( i, l ) } },
                { range, Bind( d, f ), new[] { Bind( c, d ), Bind( i, l ) } },
                { range, Bind( d, g ), new[] { Bind( c, d ), Bind( i, l ) } },
                { range, Bind( f, f ), range },
                { range, Bind( f, g ), range },
                { range, Bind( g, h ), range },
                { range, Bind( m, n ), range },
                { range, Bind( l, n ), range },
                { range, Bind( i, l ), new[] { Bind( c, f ) } },
                { range, Bind( e, k ), new[] { Bind( c, e ), Bind( k, l ) } },
                { new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) }, Bind( a, n ), Array.Empty<Bounds<T>>() }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>> GetRemoveData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { range, Array.Empty<Bounds<T>>(), range },
                { Array.Empty<Bounds<T>>(), range, Array.Empty<Bounds<T>>() },
                { range, range, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( a, b ) }, range },
                { range, new[] { Bind( a, c ) }, range },
                { range, new[] { Bind( a, e ) }, new[] { Bind( e, f ), Bind( i, l ) } },
                { range, new[] { Bind( a, f ) }, new[] { Bind( i, l ) } },
                { range, new[] { Bind( a, g ) }, new[] { Bind( i, l ) } },
                { range, new[] { Bind( c, c ) }, range },
                { range, new[] { Bind( c, d ) }, new[] { Bind( d, f ), Bind( i, l ) } },
                { range, new[] { Bind( c, f ) }, new[] { Bind( i, l ) } },
                { range, new[] { Bind( c, g ) }, new[] { Bind( i, l ) } },
                { range, new[] { Bind( d, e ) }, new[] { Bind( c, d ), Bind( e, f ), Bind( i, l ) } },
                { range, new[] { Bind( d, f ) }, new[] { Bind( c, d ), Bind( i, l ) } },
                { range, new[] { Bind( d, g ) }, new[] { Bind( c, d ), Bind( i, l ) } },
                { range, new[] { Bind( f, f ) }, range },
                { range, new[] { Bind( f, g ) }, range },
                { range, new[] { Bind( f, i ) }, range },
                { range, new[] { Bind( g, h ) }, range },
                { range, new[] { Bind( e, k ) }, new[] { Bind( c, e ), Bind( k, l ) } },
                { range, new[] { Bind( i, l ) }, new[] { Bind( c, f ) } },
                { range, new[] { Bind( l, n ) }, range },
                { range, new[] { Bind( m, n ) }, range },
                { range, new[] { Bind( a, b ), Bind( g, h ) }, range },
                { range, new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) }, range },
                { range, new[] { Bind( g, h ), Bind( m, n ) }, range },
                { range, new[] { Bind( b, d ), Bind( g, h ), Bind( k, n ) }, new[] { Bind( d, f ), Bind( i, k ) } },
                { range, new[] { Bind( d, e ), Bind( i, k ) }, new[] { Bind( c, d ), Bind( e, f ), Bind( k, l ) } },
                { range, new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ), Bind( m, m ) }, new[] { Bind( c, f ), Bind( j, k ) } },
                { range, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, f ), Bind( j, k ) } },
                { range, new[] { Bind( b, k ), Bind( l, m ) }, new[] { Bind( k, l ) } },
                { new[] { Bind( a, n ) }, range, new[] { Bind( a, c ), Bind( f, i ), Bind( l, n ) } },
                {
                    new[] { Bind( a, n ) },
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( a, b ), Bind( c, d ), Bind( e, f ), Bind( g, i ), Bind( j, l ), Bind( m, n ) }
                },
                {
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( a, n ) },
                    Array.Empty<Bounds<T>>()
                },
                { new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( i, j ), Bind( k, l ) } },
                { new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, d ), Bind( e, f ) } },
                { new[] { Bind( c, c ) }, new[] { Bind( c, c ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( c, d ) }, new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, h ) }, new[] { Bind( b, c ), Bind( d, f ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, f ) }, new[] { Bind( b, c ), Bind( d, g ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( g, g ) }, new[] { Bind( b, c ), Bind( d, g ) } }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>> GetComplementData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 8 );
            var (a, b, c, d, e, f, g, h) = (data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]);

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { new[] { Bind( a, a ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( a, b ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( a, b ), Bind( c, d ) }, new[] { Bind( b, c ) } },
                { new[] { Bind( a, b ), Bind( c, d ), Bind( e, f ), Bind( g, h ) }, new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ) } },
                { new[] { Bind( a, a ), Bind( b, c ), Bind( d, e ) }, new[] { Bind( a, b ), Bind( c, d ) } },
                { new[] { Bind( a, b ), Bind( c, c ), Bind( d, e ) }, new[] { Bind( b, d ) } },
                { new[] { Bind( a, b ), Bind( c, d ), Bind( e, e ) }, new[] { Bind( b, c ), Bind( d, e ) } },
                { new[] { Bind( a, a ), Bind( b, c ), Bind( d, e ), Bind( f, f ) }, new[] { Bind( a, b ), Bind( c, d ), Bind( e, f ) } }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>> GetComplementWithBoundsData(IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, Bounds<T>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Bind( a, a ), new[] { Bind( a, a ) } },
                { range, Bind( a, a ), new[] { Bind( a, a ) } },
                { range, Bind( a, b ), new[] { Bind( a, b ) } },
                { range, Bind( a, c ), new[] { Bind( a, c ) } },
                { range, Bind( a, f ), new[] { Bind( a, c ) } },
                { range, Bind( a, g ), new[] { Bind( a, c ), Bind( f, g ) } },
                { range, Bind( c, c ), Array.Empty<Bounds<T>>() },
                { range, Bind( c, e ), Array.Empty<Bounds<T>>() },
                { range, Bind( c, f ), Array.Empty<Bounds<T>>() },
                { range, Bind( c, g ), new[] { Bind( f, g ) } },
                { range, Bind( d, e ), Array.Empty<Bounds<T>>() },
                { range, Bind( d, f ), Array.Empty<Bounds<T>>() },
                { range, Bind( d, g ), new[] { Bind( f, g ) } },
                { range, Bind( f, f ), Array.Empty<Bounds<T>>() },
                { range, Bind( f, g ), new[] { Bind( f, g ) } },
                { range, Bind( g, h ), new[] { Bind( g, h ) } },
                { range, Bind( m, n ), new[] { Bind( m, n ) } },
                { range, Bind( l, n ), new[] { Bind( l, n ) } },
                { range, Bind( i, l ), Array.Empty<Bounds<T>>() },
                { range, Bind( e, k ), new[] { Bind( f, i ) } },
                {
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    Bind( a, n ),
                    new[] { Bind( a, b ), Bind( c, d ), Bind( e, f ), Bind( g, i ), Bind( j, l ), Bind( m, n ) }
                }
            };
        }

        public static TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>> GetComplementWithRangeData(
            IFixture fixture)
        {
            var data = fixture.CreateDistinctSortedCollection<T>( 14 );
            var (a, b, c, d, e, f, g, h, i, j, k, l, m, n) = (
                data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9], data[10], data[11], data[12],
                data[13]);

            var range = new[] { Bind( c, f ), Bind( i, l ) };

            return new TheoryData<IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>, IEnumerable<Bounds<T>>>
            {
                { Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { range, Array.Empty<Bounds<T>>(), Array.Empty<Bounds<T>>() },
                { Array.Empty<Bounds<T>>(), range, range },
                { range, range, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( a, b ) }, new[] { Bind( a, b ) } },
                { range, new[] { Bind( a, c ) }, new[] { Bind( a, c ) } },
                { range, new[] { Bind( a, e ) }, new[] { Bind( a, c ) } },
                { range, new[] { Bind( a, f ) }, new[] { Bind( a, c ) } },
                { range, new[] { Bind( a, g ) }, new[] { Bind( a, c ), Bind( f, g ) } },
                { range, new[] { Bind( c, c ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( c, d ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( c, f ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( c, g ) }, new[] { Bind( f, g ) } },
                { range, new[] { Bind( d, e ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( d, f ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( d, g ) }, new[] { Bind( f, g ) } },
                { range, new[] { Bind( f, f ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( f, g ) }, new[] { Bind( f, g ) } },
                { range, new[] { Bind( f, i ) }, new[] { Bind( f, i ) } },
                { range, new[] { Bind( g, h ) }, new[] { Bind( g, h ) } },
                { range, new[] { Bind( e, k ) }, new[] { Bind( f, i ) } },
                { range, new[] { Bind( i, l ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( l, n ) }, new[] { Bind( l, n ) } },
                { range, new[] { Bind( m, n ) }, new[] { Bind( m, n ) } },
                { range, new[] { Bind( a, b ), Bind( g, h ) }, new[] { Bind( a, b ), Bind( g, h ) } },
                { range, new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) }, new[] { Bind( a, b ), Bind( g, h ), Bind( m, n ) } },
                { range, new[] { Bind( g, h ), Bind( m, n ) }, new[] { Bind( g, h ), Bind( m, n ) } },
                { range, new[] { Bind( b, d ), Bind( g, h ), Bind( k, n ) }, new[] { Bind( b, c ), Bind( g, h ), Bind( l, n ) } },
                { range, new[] { Bind( d, e ), Bind( i, k ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( c, c ), Bind( i, j ), Bind( k, l ), Bind( m, m ) }, new[] { Bind( m, m ) } },
                { range, new[] { Bind( d, d ), Bind( i, j ), Bind( k, l ) }, Array.Empty<Bounds<T>>() },
                { range, new[] { Bind( b, k ), Bind( l, m ) }, new[] { Bind( b, c ), Bind( f, i ), Bind( l, m ) } },
                { new[] { Bind( a, n ) }, range, Array.Empty<Bounds<T>>() },
                {
                    new[] { Bind( a, n ) },
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    Array.Empty<Bounds<T>>()
                },
                {
                    new[] { Bind( b, c ), Bind( d, e ), Bind( f, g ), Bind( i, j ), Bind( l, m ) },
                    new[] { Bind( a, n ) },
                    new[] { Bind( a, b ), Bind( c, d ), Bind( e, f ), Bind( g, i ), Bind( j, l ), Bind( m, n ) }
                },
                { new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( c, d ), Bind( e, f ) } },
                { new[] { Bind( c, d ), Bind( e, f ) }, new[] { Bind( i, j ), Bind( k, l ) }, new[] { Bind( i, j ), Bind( k, l ) } },
                { new[] { Bind( c, c ) }, new[] { Bind( c, c ) }, Array.Empty<Bounds<T>>() },
                {
                    new[] { Bind( c, d ) },
                    new[] { Bind( a, d ), Bind( e, f ), Bind( g, h ) },
                    new[] { Bind( a, c ), Bind( e, f ), Bind( g, h ) }
                },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, h ) }, new[] { Bind( g, h ) } },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( f, f ) }, Array.Empty<Bounds<T>>() },
                { new[] { Bind( b, g ) }, new[] { Bind( c, d ), Bind( g, g ) }, Array.Empty<Bounds<T>>() }
            };
        }

        public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
        {
            return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
        }

        private static Bounds<T> Bind(T min, T max)
        {
            return new Bounds<T>( min, max );
        }
    }
}
