using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Equality
{
    public class GenericEqualityTestsData<T>
    {
        public static TheoryData<T, T, bool> CreateCtorTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new TheoryData<T, T, bool>
            {
                { _1, _1, true },
                { _1, _2, false }
            };
        }

        public static TheoryData<T, T, T, T, bool> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2, _3, _4) = fixture.CreateDistinctCollection<T>( 4 );

            return new TheoryData<T, T, T, T, bool>
            {
                { _1, _2, _1, _2, true },
                { _1, _2, _1, _3, false },
                { _1, _3, _2, _3, false },
                { _1, _2, _3, _4, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }

        public static TheoryData<T, T> CreateConversionOperatorTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new TheoryData<T, T>
            {
                { _1, _1 },
                { _1, _2 }
            };
        }
    }
}
