using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Functional.Maybe
{
    public class GenericMaybeTestsData<T>
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var _0 = default( T );
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new[]
            {
                new object?[] { _1, true, _1, true, true },
                new object?[] { _1, true, _2, true, false },
                new object?[] { _0, false, _0, false, true },
                new object?[] { _0, false, _1, true, false },
                new object?[] { _1, true, _0, false, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
