using System;
using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Bounds
{
    public class GenericBoundsTestsData<T>
        where T : IComparable<T>
    {
        public static TheoryData<T, T, T, T, bool> GetEqualsData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new TheoryData<T, T, T, T, bool>
            {
                { _1, _2, _1, _2, true },
                { _1, _2, _1, _3, false },
                { _1, _3, _2, _3, false },
                { _1, _2, _3, _4, false }
            };
        }

        public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
        {
            return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static TheoryData<T, T, T, T> GetClampData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new TheoryData<T, T, T, T>
            {
                { _2, _3, _1, _2 },
                { _1, _2, _1, _1 },
                { _1, _3, _2, _2 },
                { _1, _2, _2, _2 },
                { _1, _2, _3, _2 }
            };
        }

        public static TheoryData<T, T, T, bool> GetContainsData(IFixture fixture)
        {
            var (_1, _2, _3) = fixture.CreateDistinctSortedCollection<T>( 3 );

            return new TheoryData<T, T, T, bool>
            {
                { _2, _3, _1, false },
                { _1, _3, _1, true },
                { _1, _3, _2, true },
                { _1, _3, _3, true },
                { _1, _2, _3, false }
            };
        }

        public static TheoryData<T, T, T, T, bool> GetContainsForBoundsData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new TheoryData<T, T, T, T, bool>
            {
                { _1, _2, _1, _2, true },
                { _1, _3, _1, _2, true },
                { _1, _3, _2, _3, true },
                { _1, _4, _2, _3, true },
                { _2, _3, _1, _3, false },
                { _1, _2, _1, _3, false },
                { _2, _3, _1, _4, false },
                { _3, _4, _1, _2, false },
                { _2, _3, _1, _2, false },
                { _1, _2, _3, _4, false },
                { _1, _2, _2, _3, false }
            };
        }

        public static TheoryData<T, T, T, T, bool> GetIntersectsForBoundsData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new TheoryData<T, T, T, T, bool>
            {
                { _1, _2, _1, _2, true },
                { _1, _3, _1, _2, true },
                { _1, _3, _2, _3, true },
                { _1, _4, _2, _3, true },
                { _2, _3, _1, _3, true },
                { _1, _2, _1, _3, true },
                { _2, _3, _1, _4, true },
                { _3, _4, _1, _2, false },
                { _2, _3, _1, _2, true },
                { _1, _2, _3, _4, false },
                { _1, _2, _2, _3, true }
            };
        }

        public static TheoryData<T, T, T, T, Bounds<T>?> GetIntersectionData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctSortedCollection<T>( 4 );

            return new TheoryData<T, T, T, T, Bounds<T>?>
            {
                { _1, _2, _1, _2, new Bounds<T>( _1, _2 ) },
                { _1, _3, _1, _2, new Bounds<T>( _1, _2 ) },
                { _1, _3, _2, _3, new Bounds<T>( _2, _3 ) },
                { _1, _4, _2, _3, new Bounds<T>( _2, _3 ) },
                { _2, _3, _1, _3, new Bounds<T>( _2, _3 ) },
                { _1, _2, _1, _3, new Bounds<T>( _1, _2 ) },
                { _2, _3, _1, _4, new Bounds<T>( _2, _3 ) },
                { _3, _4, _1, _2, null },
                { _2, _3, _1, _2, new Bounds<T>( _2, _2 ) },
                { _1, _2, _3, _4, null },
                { _1, _2, _2, _3, new Bounds<T>( _2, _2 ) }
            };
        }
    }
}
