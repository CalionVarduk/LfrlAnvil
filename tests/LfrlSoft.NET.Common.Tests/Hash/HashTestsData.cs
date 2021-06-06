using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.Common.Tests.Extensions;

namespace LfrlSoft.NET.Common.Tests.Hash
{
    public class HashTestsData
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<int>( 2 );

            return new[]
            {
                new object?[] { _1, _1, true },
                new object?[] { _1, _2, false }
            };
        }

        public static IEnumerable<object?[]> CreateCompareToTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctSortedCollection<int>( 2 );

            return new[]
            {
                new object?[] { _1, _1, 0 },
                new object?[] { _1, _2, -1 },
                new object?[] { _2, _1, 1 }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static IEnumerable<object?[]> CreateGreaterThanComparisonTestData(IFixture fixture)
        {
            return CreateCompareToTestData( fixture ).ConvertResult( (int r) => r > 0 );
        }

        public static IEnumerable<object?[]> CreateGreaterThanOrEqualToComparisonTestData(IFixture fixture)
        {
            return CreateCompareToTestData( fixture ).ConvertResult( (int r) => r >= 0 );
        }

        public static IEnumerable<object?[]> CreateLessThanComparisonTestData(IFixture fixture)
        {
            return CreateCompareToTestData( fixture ).ConvertResult( (int r) => r < 0 );
        }

        public static IEnumerable<object?[]> CreateLessThanOrEqualToComparisonTestData(IFixture fixture)
        {
            return CreateCompareToTestData( fixture ).ConvertResult( (int r) => r <= 0 );
        }
    }
}
