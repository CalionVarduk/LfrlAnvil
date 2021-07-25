using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Bounds
{
    public class BoundsTestsData<T>
        where T : IComparable<T>
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new[]
            {
                new object?[] { _1, _2, _1, _2, true },
                new object?[] { _1, _2, _1, _3, false },
                new object?[] { _1, _3, _2, _3, false },
                new object?[] { _1, _2, _3, _4, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static IEnumerable<object?[]> CreateClampTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new[]
            {
                new object?[] { _2, _3, _1, _2 },
                new object?[] { _1, _2, _1, _1 },
                new object?[] { _1, _3, _2, _2 },
                new object?[] { _1, _2, _2, _2 },
                new object?[] { _1, _2, _3, _2 }
            };
        }

        public static IEnumerable<object?[]> CreateContainsTestData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new[]
            {
                new object?[] { _2, _3, _1, false },
                new object?[] { _1, _3, _1, true },
                new object?[] { _1, _3, _2, true },
                new object?[] { _1, _3, _3, true },
                new object?[] { _1, _2, _3, false }
            };
        }

        public static IEnumerable<object?[]> CreateContainsForBoundsTestData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new[]
            {
                new object?[] { _1, _2, _1, _2, true },
                new object?[] { _1, _3, _1, _2, true },
                new object?[] { _1, _3, _2, _3, true },
                new object?[] { _1, _4, _2, _3, true },
                new object?[] { _2, _3, _1, _3, false },
                new object?[] { _1, _2, _1, _3, false },
                new object?[] { _2, _3, _1, _4, false },
                new object?[] { _3, _4, _1, _2, false },
                new object?[] { _2, _3, _1, _2, false },
                new object?[] { _1, _2, _3, _4, false },
                new object?[] { _1, _2, _2, _3, false }
            };
        }

        public static IEnumerable<object?[]> CreateIntersectsForBoundsTestData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new[]
            {
                new object?[] { _1, _2, _1, _2, true },
                new object?[] { _1, _3, _1, _2, true },
                new object?[] { _1, _3, _2, _3, true },
                new object?[] { _1, _4, _2, _3, true },
                new object?[] { _2, _3, _1, _3, true },
                new object?[] { _1, _2, _1, _3, true },
                new object?[] { _2, _3, _1, _4, true },
                new object?[] { _3, _4, _1, _2, false },
                new object?[] { _2, _3, _1, _2, true },
                new object?[] { _1, _2, _3, _4, false },
                new object?[] { _1, _2, _2, _3, true }
            };
        }

        public static IEnumerable<object?[]> CreateIntersectionTestData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new[]
            {
                new object?[] { _1, _2, _1, _2, new Bounds<T>( _1, _2 ) },
                new object?[] { _1, _3, _1, _2, new Bounds<T>( _1, _2 ) },
                new object?[] { _1, _3, _2, _3, new Bounds<T>( _2, _3 ) },
                new object?[] { _1, _4, _2, _3, new Bounds<T>( _2, _3 ) },
                new object?[] { _2, _3, _1, _3, new Bounds<T>( _2, _3 ) },
                new object?[] { _1, _2, _1, _3, new Bounds<T>( _1, _2 ) },
                new object?[] { _2, _3, _1, _4, new Bounds<T>( _2, _3 ) },
                new object?[] { _3, _4, _1, _2, null },
                new object?[] { _2, _3, _1, _2, new Bounds<T>( _2, _2 ) },
                new object?[] { _1, _2, _3, _4, null },
                new object?[] { _1, _2, _2, _3, new Bounds<T>( _2, _2 ) }
            };
        }
    }
}
