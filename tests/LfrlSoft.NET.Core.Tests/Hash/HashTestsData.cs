using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Hash
{
    public class HashTestsData
    {
        public static TheoryData<int, int, bool> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<int>( 2 );

            return new TheoryData<int, int, bool>
            {
                { _1, _1, true },
                { _1, _2, false }
            };
        }

        public static TheoryData<int, int, int> CreateCompareToTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctSortedCollection<int>( 2 );

            return new TheoryData<int, int, int>
            {
                { _1, _1, 0 },
                { _1, _2, -1 },
                { _2, _1, 1 }
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
