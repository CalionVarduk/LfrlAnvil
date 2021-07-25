using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Equality
{
    public class EqualityTestsData<T>
    {
        public static IEnumerable<object?[]> CreateCtorTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new[]
            {
                new object?[] { _1, _1, true },
                new object?[] { _1, _2, false }
            };
        }

        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctCollection<T>( 4 );

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

        public static IEnumerable<object?[]> CreateConversionOperatorTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new[]
            {
                new object?[] { _1, _1 },
                new object?[] { _1, _2 }
            };
        }
    }
}
