using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.Common.Tests.Extensions;

namespace LfrlSoft.NET.Common.Tests.Bitmask
{
    public class BitmaskTestsData<T>
        where T : struct, IConvertible, IComparable
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new[]
            {
                new object?[] { _1, _1, true },
                new object?[] { _1, _2, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static IEnumerable<object?[]> CreateCompareToTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctSortedCollection<T>( 2 );

            return new[]
            {
                new object?[] { _1, _1, 0 },
                new object?[] { _1, _2, -1 },
                new object?[] { _2, _1, 1 }
            };
        }

        public static IEnumerable<object?[]> CreateContainsAnyTestData(IFixture fixture)
        {
            return new[]
            {
                new object?[] { Convert( 0 ), Convert( 0 ), true },
                new object?[] { Convert( 0 ), Convert( 1 ), false },
                new object?[] { Convert( 1 ), Convert( 0 ), true },
                new object?[] { Convert( 1 ), Convert( 1 ), true },
                new object?[] { Convert( 1 ), Convert( 2 ), false },
                new object?[] { Convert( 1 ), Convert( 3 ), true },
                new object?[] { Convert( 2 ), Convert( 1 ), false },
                new object?[] { Convert( 2 ), Convert( 2 ), true },
                new object?[] { Convert( 2 ), Convert( 3 ), true },
                new object?[] { Convert( 3 ), Convert( 1 ), true },
                new object?[] { Convert( 3 ), Convert( 2 ), true },
                new object?[] { Convert( 3 ), Convert( 3 ), true }
            };
        }

        public static IEnumerable<object?[]> CreateContainsAllTestData(IFixture fixture)
        {
            return new[]
            {
                new object?[] { Convert( 0 ), Convert( 0 ), true },
                new object?[] { Convert( 0 ), Convert( 1 ), false },
                new object?[] { Convert( 1 ), Convert( 0 ), true },
                new object?[] { Convert( 1 ), Convert( 1 ), true },
                new object?[] { Convert( 1 ), Convert( 2 ), false },
                new object?[] { Convert( 1 ), Convert( 3 ), false },
                new object?[] { Convert( 2 ), Convert( 1 ), false },
                new object?[] { Convert( 2 ), Convert( 2 ), true },
                new object?[] { Convert( 2 ), Convert( 3 ), false },
                new object?[] { Convert( 3 ), Convert( 1 ), true },
                new object?[] { Convert( 3 ), Convert( 2 ), true },
                new object?[] { Convert( 3 ), Convert( 3 ), true }
            };
        }

        public static IEnumerable<object?[]> CreateContainsBitTestData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = 1UL << maxBitIndex;

            return new[]
            {
                new object?[] { Convert( 0 ), 0, false },
                new object?[] { Convert( 0 ), 1, false },
                new object?[] { Convert( 0 ), 2, false },
                new object?[] { Convert( 0 ), maxBitIndex, false },
                new object?[] { Convert( 1 ), 0, true },
                new object?[] { Convert( 1 ), 1, false },
                new object?[] { Convert( 1 ), 2, false },
                new object?[] { Convert( 1 ), maxBitIndex, false },
                new object?[] { Convert( 2 ), 0, false },
                new object?[] { Convert( 2 ), 1, true },
                new object?[] { Convert( 2 ), 2, false },
                new object?[] { Convert( 2 ), maxBitIndex, false },
                new object?[] { Convert( 3 ), 0, true },
                new object?[] { Convert( 3 ), 1, true },
                new object?[] { Convert( 3 ), 2, false },
                new object?[] { Convert( 3 ), maxBitIndex, false },
                new object?[] { Convert( maxValue ), 0, false },
                new object?[] { Convert( maxValue ), 1, false },
                new object?[] { Convert( maxValue ), 2, false },
                new object?[] { Convert( maxValue ), maxBitIndex, true }
            };
        }

        public static IEnumerable<object?[]> CreateSetTestData(IFixture fixture)
        {
            return new[]
            {
                new object?[] { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                new object?[] { Convert( 0 ), Convert( 1 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 0 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 1 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 2 ), Convert( 3 ) },
                new object?[] { Convert( 1 ), Convert( 3 ), Convert( 3 ) },
                new object?[] { Convert( 2 ), Convert( 1 ), Convert( 3 ) },
                new object?[] { Convert( 2 ), Convert( 2 ), Convert( 2 ) },
                new object?[] { Convert( 2 ), Convert( 3 ), Convert( 3 ) },
                new object?[] { Convert( 3 ), Convert( 1 ), Convert( 3 ) },
                new object?[] { Convert( 3 ), Convert( 2 ), Convert( 3 ) },
                new object?[] { Convert( 3 ), Convert( 3 ), Convert( 3 ) }
            };
        }

        public static IEnumerable<object?[]> CreateSetBitTestData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = 1UL << maxBitIndex;

            return new[]
            {
                new object?[] { Convert( 0 ), 0, Convert( 1 ) },
                new object?[] { Convert( 0 ), 1, Convert( 2 ) },
                new object?[] { Convert( 0 ), 2, Convert( 4 ) },
                new object?[] { Convert( 0 ), maxBitIndex, Convert( maxValue ) },
                new object?[] { Convert( 1 ), 0, Convert( 1 ) },
                new object?[] { Convert( 1 ), 1, Convert( 3 ) },
                new object?[] { Convert( 1 ), 2, Convert( 5 ) },
                new object?[] { Convert( 1 ), maxBitIndex, Convert( 1 | maxValue ) },
                new object?[] { Convert( 2 ), 0, Convert( 3 ) },
                new object?[] { Convert( 2 ), 1, Convert( 2 ) },
                new object?[] { Convert( 2 ), 2, Convert( 6 ) },
                new object?[] { Convert( 2 ), maxBitIndex, Convert( 2 | maxValue ) },
                new object?[] { Convert( 3 ), 0, Convert( 3 ) },
                new object?[] { Convert( 3 ), 1, Convert( 3 ) },
                new object?[] { Convert( 3 ), 2, Convert( 7 ) },
                new object?[] { Convert( 3 ), maxBitIndex, Convert( 3 | maxValue ) },
                new object?[] { Convert( maxValue ), 0, Convert( 1 | maxValue ) },
                new object?[] { Convert( maxValue ), 1, Convert( 2 | maxValue ) },
                new object?[] { Convert( maxValue ), 2, Convert( 4 | maxValue ) },
                new object?[] { Convert( maxValue ), maxBitIndex, Convert( maxValue ) }
            };
        }

        public static IEnumerable<object?[]> CreateUnsetTestData(IFixture fixture)
        {
            return new[]
            {
                new object?[] { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                new object?[] { Convert( 0 ), Convert( 1 ), Convert( 0 ) },
                new object?[] { Convert( 1 ), Convert( 0 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 1 ), Convert( 0 ) },
                new object?[] { Convert( 1 ), Convert( 2 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 3 ), Convert( 0 ) },
                new object?[] { Convert( 2 ), Convert( 1 ), Convert( 2 ) },
                new object?[] { Convert( 2 ), Convert( 2 ), Convert( 0 ) },
                new object?[] { Convert( 2 ), Convert( 3 ), Convert( 0 ) },
                new object?[] { Convert( 3 ), Convert( 1 ), Convert( 2 ) },
                new object?[] { Convert( 3 ), Convert( 2 ), Convert( 1 ) },
                new object?[] { Convert( 3 ), Convert( 3 ), Convert( 0 ) }
            };
        }

        public static IEnumerable<object?[]> CreateUnsetBitTestData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = 1UL << maxBitIndex;

            return new[]
            {
                new object?[] { Convert( 0 ), 0, Convert( 0 ) },
                new object?[] { Convert( 0 ), 1, Convert( 0 ) },
                new object?[] { Convert( 0 ), 2, Convert( 0 ) },
                new object?[] { Convert( 0 ), maxBitIndex, Convert( 0 ) },
                new object?[] { Convert( 1 ), 0, Convert( 0 ) },
                new object?[] { Convert( 1 ), 1, Convert( 1 ) },
                new object?[] { Convert( 1 ), 2, Convert( 1 ) },
                new object?[] { Convert( 1 ), maxBitIndex, Convert( 1 ) },
                new object?[] { Convert( 2 ), 0, Convert( 2 ) },
                new object?[] { Convert( 2 ), 1, Convert( 0 ) },
                new object?[] { Convert( 2 ), 2, Convert( 2 ) },
                new object?[] { Convert( 2 ), maxBitIndex, Convert( 2 ) },
                new object?[] { Convert( 3 ), 0, Convert( 2 ) },
                new object?[] { Convert( 3 ), 1, Convert( 1 ) },
                new object?[] { Convert( 3 ), 2, Convert( 3 ) },
                new object?[] { Convert( 3 ), maxBitIndex, Convert( 3 ) },
                new object?[] { Convert( maxValue ), 0, Convert( maxValue ) },
                new object?[] { Convert( maxValue ), 1, Convert( maxValue ) },
                new object?[] { Convert( maxValue ), 2, Convert( maxValue ) },
                new object?[] { Convert( maxValue ), maxBitIndex, Convert( 0 ) }
            };
        }

        public static IEnumerable<object?[]> CreateIntersectTestData(IFixture fixture)
        {
            return new[]
            {
                new object?[] { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                new object?[] { Convert( 0 ), Convert( 1 ), Convert( 0 ) },
                new object?[] { Convert( 1 ), Convert( 0 ), Convert( 0 ) },
                new object?[] { Convert( 1 ), Convert( 1 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 2 ), Convert( 0 ) },
                new object?[] { Convert( 1 ), Convert( 3 ), Convert( 1 ) },
                new object?[] { Convert( 2 ), Convert( 1 ), Convert( 0 ) },
                new object?[] { Convert( 2 ), Convert( 2 ), Convert( 2 ) },
                new object?[] { Convert( 2 ), Convert( 3 ), Convert( 2 ) },
                new object?[] { Convert( 3 ), Convert( 1 ), Convert( 1 ) },
                new object?[] { Convert( 3 ), Convert( 2 ), Convert( 2 ) },
                new object?[] { Convert( 3 ), Convert( 3 ), Convert( 3 ) }
            };
        }

        public static IEnumerable<object?[]> CreateAlternateTestData(IFixture fixture)
        {
            return new[]
            {
                new object?[] { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                new object?[] { Convert( 0 ), Convert( 1 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 0 ), Convert( 1 ) },
                new object?[] { Convert( 1 ), Convert( 1 ), Convert( 0 ) },
                new object?[] { Convert( 1 ), Convert( 2 ), Convert( 3 ) },
                new object?[] { Convert( 1 ), Convert( 3 ), Convert( 2 ) },
                new object?[] { Convert( 2 ), Convert( 1 ), Convert( 3 ) },
                new object?[] { Convert( 2 ), Convert( 2 ), Convert( 0 ) },
                new object?[] { Convert( 2 ), Convert( 3 ), Convert( 1 ) },
                new object?[] { Convert( 3 ), Convert( 1 ), Convert( 2 ) },
                new object?[] { Convert( 3 ), Convert( 2 ), Convert( 1 ) },
                new object?[] { Convert( 3 ), Convert( 3 ), Convert( 0 ) }
            };
        }

        public static IEnumerable<object?[]> CreateNegateTestData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = ((1UL << maxBitIndex) - 1) | 1UL << maxBitIndex;

            return new[]
            {
                new object?[] { Convert( 0 ), Convert( maxValue ) },
                new object?[] { Convert( 1 ), Convert( maxValue - 1 ) },
                new object?[] { Convert( 2 ), Convert( maxValue - 2 ) },
                new object?[] { Convert( 3 ), Convert( maxValue - 3 ) },
                new object?[] { Convert( maxValue - 3 ), Convert( 3 ) },
                new object?[] { Convert( maxValue - 2 ), Convert( 2 ) },
                new object?[] { Convert( maxValue - 1 ), Convert( 1 ) },
                new object?[] { Convert( maxValue ), Convert( 0 ) }
            };
        }

        public static IEnumerable<object[]> CreateCountTestData(IFixture fixture)
        {
            return new[]
            {
                new object[] { Convert( 0 ), 0 },
                new object[] { Convert( 1 ), 1 },
                new object[] { Convert( 2 ), 1 },
                new object[] { Convert( 3 ), 2 },
                new object[] { Convert( 31 ), 5 }
            };
        }

        public static IEnumerable<object[]> CreateEnumeratorTestData(IFixture fixture)
        {
            return new[]
            {
                new object[] { Convert( 0 ), Enumerable.Empty<T>() },
                new object[] { Convert( 1 ), new[] { Convert( 1 ) } },
                new object[] { Convert( 2 ), new[] { Convert( 2 ) } },
                new object[] { Convert( 3 ), new[] { Convert( 1 ), Convert( 2 ) } },
                new object[] { Convert( 31 ), new[] { Convert( 1 ), Convert( 2 ), Convert( 4 ), Convert( 8 ), Convert( 16 ) } }
            };
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

        public static T Convert(ulong value)
        {
            return Bitmask<T>.FromLongValue( value );
        }
    }
}
