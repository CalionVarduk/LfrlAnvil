using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.BitmaskTests
{
    public class GenericBitmaskTestsData<T>
        where T : struct, IConvertible, IComparable
    {
        public static TheoryData<T, T, bool> GetEqualsData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new TheoryData<T, T, bool>
            {
                { _1, _1, true },
                { _1, _2, false }
            };
        }

        public static IEnumerable<object?[]> GetNotEqualsData(IFixture fixture)
        {
            return GetEqualsData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static TheoryData<T, T, int> GetCompareToData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctSortedCollection<T>( 2 );

            return new TheoryData<T, T, int>
            {
                { _1, _1, 0 },
                { _1, _2, -1 },
                { _2, _1, 1 }
            };
        }

        public static IEnumerable<object?[]> CreateGreaterThanComparisonTestData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r > 0 );
        }

        public static IEnumerable<object?[]> CreateGreaterThanOrEqualToComparisonTestData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r >= 0 );
        }

        public static IEnumerable<object?[]> CreateLessThanComparisonTestData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r < 0 );
        }

        public static IEnumerable<object?[]> CreateLessThanOrEqualToComparisonTestData(IFixture fixture)
        {
            return GetCompareToData( fixture ).ConvertResult( (int r) => r <= 0 );
        }

        public static TheoryData<T, T, bool> GetContainsAnyData(IFixture fixture)
        {
            return new TheoryData<T, T, bool>
            {
                { Convert( 0 ), Convert( 0 ), true },
                { Convert( 0 ), Convert( 1 ), false },
                { Convert( 1 ), Convert( 0 ), true },
                { Convert( 1 ), Convert( 1 ), true },
                { Convert( 1 ), Convert( 2 ), false },
                { Convert( 1 ), Convert( 3 ), true },
                { Convert( 2 ), Convert( 1 ), false },
                { Convert( 2 ), Convert( 2 ), true },
                { Convert( 2 ), Convert( 3 ), true },
                { Convert( 3 ), Convert( 1 ), true },
                { Convert( 3 ), Convert( 2 ), true },
                { Convert( 3 ), Convert( 3 ), true }
            };
        }

        public static TheoryData<T, T, bool> GetContainsAllData(IFixture fixture)
        {
            return new TheoryData<T, T, bool>
            {
                { Convert( 0 ), Convert( 0 ), true },
                { Convert( 0 ), Convert( 1 ), false },
                { Convert( 1 ), Convert( 0 ), true },
                { Convert( 1 ), Convert( 1 ), true },
                { Convert( 1 ), Convert( 2 ), false },
                { Convert( 1 ), Convert( 3 ), false },
                { Convert( 2 ), Convert( 1 ), false },
                { Convert( 2 ), Convert( 2 ), true },
                { Convert( 2 ), Convert( 3 ), false },
                { Convert( 3 ), Convert( 1 ), true },
                { Convert( 3 ), Convert( 2 ), true },
                { Convert( 3 ), Convert( 3 ), true }
            };
        }

        public static TheoryData<T, int, bool> GetContainsBitData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = 1UL << maxBitIndex;

            return new TheoryData<T, int, bool>
            {
                { Convert( 0 ), 0, false },
                { Convert( 0 ), 1, false },
                { Convert( 0 ), 2, false },
                { Convert( 0 ), maxBitIndex, false },
                { Convert( 1 ), 0, true },
                { Convert( 1 ), 1, false },
                { Convert( 1 ), 2, false },
                { Convert( 1 ), maxBitIndex, false },
                { Convert( 2 ), 0, false },
                { Convert( 2 ), 1, true },
                { Convert( 2 ), 2, false },
                { Convert( 2 ), maxBitIndex, false },
                { Convert( 3 ), 0, true },
                { Convert( 3 ), 1, true },
                { Convert( 3 ), 2, false },
                { Convert( 3 ), maxBitIndex, false },
                { Convert( maxValue ), 0, false },
                { Convert( maxValue ), 1, false },
                { Convert( maxValue ), 2, false },
                { Convert( maxValue ), maxBitIndex, true }
            };
        }

        public static TheoryData<T, T, T> GetSetData(IFixture fixture)
        {
            return new TheoryData<T, T, T>
            {
                { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                { Convert( 0 ), Convert( 1 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 0 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 1 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 2 ), Convert( 3 ) },
                { Convert( 1 ), Convert( 3 ), Convert( 3 ) },
                { Convert( 2 ), Convert( 1 ), Convert( 3 ) },
                { Convert( 2 ), Convert( 2 ), Convert( 2 ) },
                { Convert( 2 ), Convert( 3 ), Convert( 3 ) },
                { Convert( 3 ), Convert( 1 ), Convert( 3 ) },
                { Convert( 3 ), Convert( 2 ), Convert( 3 ) },
                { Convert( 3 ), Convert( 3 ), Convert( 3 ) }
            };
        }

        public static TheoryData<T, int, T> GetSetBitData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = 1UL << maxBitIndex;

            return new TheoryData<T, int, T>
            {
                { Convert( 0 ), 0, Convert( 1 ) },
                { Convert( 0 ), 1, Convert( 2 ) },
                { Convert( 0 ), 2, Convert( 4 ) },
                { Convert( 0 ), maxBitIndex, Convert( maxValue ) },
                { Convert( 1 ), 0, Convert( 1 ) },
                { Convert( 1 ), 1, Convert( 3 ) },
                { Convert( 1 ), 2, Convert( 5 ) },
                { Convert( 1 ), maxBitIndex, Convert( 1 | maxValue ) },
                { Convert( 2 ), 0, Convert( 3 ) },
                { Convert( 2 ), 1, Convert( 2 ) },
                { Convert( 2 ), 2, Convert( 6 ) },
                { Convert( 2 ), maxBitIndex, Convert( 2 | maxValue ) },
                { Convert( 3 ), 0, Convert( 3 ) },
                { Convert( 3 ), 1, Convert( 3 ) },
                { Convert( 3 ), 2, Convert( 7 ) },
                { Convert( 3 ), maxBitIndex, Convert( 3 | maxValue ) },
                { Convert( maxValue ), 0, Convert( 1 | maxValue ) },
                { Convert( maxValue ), 1, Convert( 2 | maxValue ) },
                { Convert( maxValue ), 2, Convert( 4 | maxValue ) },
                { Convert( maxValue ), maxBitIndex, Convert( maxValue ) }
            };
        }

        public static TheoryData<T, T, T> GetUnsetData(IFixture fixture)
        {
            return new TheoryData<T, T, T>
            {
                { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                { Convert( 0 ), Convert( 1 ), Convert( 0 ) },
                { Convert( 1 ), Convert( 0 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 1 ), Convert( 0 ) },
                { Convert( 1 ), Convert( 2 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 3 ), Convert( 0 ) },
                { Convert( 2 ), Convert( 1 ), Convert( 2 ) },
                { Convert( 2 ), Convert( 2 ), Convert( 0 ) },
                { Convert( 2 ), Convert( 3 ), Convert( 0 ) },
                { Convert( 3 ), Convert( 1 ), Convert( 2 ) },
                { Convert( 3 ), Convert( 2 ), Convert( 1 ) },
                { Convert( 3 ), Convert( 3 ), Convert( 0 ) }
            };
        }

        public static TheoryData<T, int, T> GetUnsetBitData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = 1UL << maxBitIndex;

            return new TheoryData<T, int, T>
            {
                { Convert( 0 ), 0, Convert( 0 ) },
                { Convert( 0 ), 1, Convert( 0 ) },
                { Convert( 0 ), 2, Convert( 0 ) },
                { Convert( 0 ), maxBitIndex, Convert( 0 ) },
                { Convert( 1 ), 0, Convert( 0 ) },
                { Convert( 1 ), 1, Convert( 1 ) },
                { Convert( 1 ), 2, Convert( 1 ) },
                { Convert( 1 ), maxBitIndex, Convert( 1 ) },
                { Convert( 2 ), 0, Convert( 2 ) },
                { Convert( 2 ), 1, Convert( 0 ) },
                { Convert( 2 ), 2, Convert( 2 ) },
                { Convert( 2 ), maxBitIndex, Convert( 2 ) },
                { Convert( 3 ), 0, Convert( 2 ) },
                { Convert( 3 ), 1, Convert( 1 ) },
                { Convert( 3 ), 2, Convert( 3 ) },
                { Convert( 3 ), maxBitIndex, Convert( 3 ) },
                { Convert( maxValue ), 0, Convert( maxValue ) },
                { Convert( maxValue ), 1, Convert( maxValue ) },
                { Convert( maxValue ), 2, Convert( maxValue ) },
                { Convert( maxValue ), maxBitIndex, Convert( 0 ) }
            };
        }

        public static TheoryData<T, T, T> GetIntersectData(IFixture fixture)
        {
            return new TheoryData<T, T, T>
            {
                { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                { Convert( 0 ), Convert( 1 ), Convert( 0 ) },
                { Convert( 1 ), Convert( 0 ), Convert( 0 ) },
                { Convert( 1 ), Convert( 1 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 2 ), Convert( 0 ) },
                { Convert( 1 ), Convert( 3 ), Convert( 1 ) },
                { Convert( 2 ), Convert( 1 ), Convert( 0 ) },
                { Convert( 2 ), Convert( 2 ), Convert( 2 ) },
                { Convert( 2 ), Convert( 3 ), Convert( 2 ) },
                { Convert( 3 ), Convert( 1 ), Convert( 1 ) },
                { Convert( 3 ), Convert( 2 ), Convert( 2 ) },
                { Convert( 3 ), Convert( 3 ), Convert( 3 ) }
            };
        }

        public static TheoryData<T, T, T> GetAlternateData(IFixture fixture)
        {
            return new TheoryData<T, T, T>
            {
                { Convert( 0 ), Convert( 0 ), Convert( 0 ) },
                { Convert( 0 ), Convert( 1 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 0 ), Convert( 1 ) },
                { Convert( 1 ), Convert( 1 ), Convert( 0 ) },
                { Convert( 1 ), Convert( 2 ), Convert( 3 ) },
                { Convert( 1 ), Convert( 3 ), Convert( 2 ) },
                { Convert( 2 ), Convert( 1 ), Convert( 3 ) },
                { Convert( 2 ), Convert( 2 ), Convert( 0 ) },
                { Convert( 2 ), Convert( 3 ), Convert( 1 ) },
                { Convert( 3 ), Convert( 1 ), Convert( 2 ) },
                { Convert( 3 ), Convert( 2 ), Convert( 1 ) },
                { Convert( 3 ), Convert( 3 ), Convert( 0 ) }
            };
        }

        public static TheoryData<T, T> GetNegateData(IFixture fixture)
        {
            var maxBitIndex = Bitmask<T>.BitCount - 1;
            var maxValue = ((1UL << maxBitIndex) - 1) | 1UL << maxBitIndex;

            return new TheoryData<T, T>
            {
                { Convert( 0 ), Convert( maxValue ) },
                { Convert( 1 ), Convert( maxValue - 1 ) },
                { Convert( 2 ), Convert( maxValue - 2 ) },
                { Convert( 3 ), Convert( maxValue - 3 ) },
                { Convert( maxValue - 3 ), Convert( 3 ) },
                { Convert( maxValue - 2 ), Convert( 2 ) },
                { Convert( maxValue - 1 ), Convert( 1 ) },
                { Convert( maxValue ), Convert( 0 ) }
            };
        }

        public static TheoryData<T, int> GetCountData(IFixture fixture)
        {
            return new TheoryData<T, int>
            {
                { Convert( 0 ), 0 },
                { Convert( 1 ), 1 },
                { Convert( 2 ), 1 },
                { Convert( 3 ), 2 },
                { Convert( 31 ), 5 }
            };
        }

        public static TheoryData<T, IEnumerable<T>> GetEnumeratorData(IFixture fixture)
        {
            return new TheoryData<T, IEnumerable<T>>
            {
                { Convert( 0 ), Enumerable.Empty<T>() },
                { Convert( 1 ), new[] { Convert( 1 ) } },
                { Convert( 2 ), new[] { Convert( 2 ) } },
                { Convert( 3 ), new[] { Convert( 1 ), Convert( 2 ) } },
                { Convert( 31 ), new[] { Convert( 1 ), Convert( 2 ), Convert( 4 ), Convert( 8 ), Convert( 16 ) } }
            };
        }

        public static T Convert(ulong value)
        {
            return Bitmask<T>.FromLongValue( value );
        }
    }
}
