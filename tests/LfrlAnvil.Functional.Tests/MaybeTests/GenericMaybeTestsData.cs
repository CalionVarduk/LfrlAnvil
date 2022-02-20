using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.MaybeTests
{
    public class GenericMaybeTestsData<T>
    {
        public static TheoryData<T?, bool, T?, bool, bool> CreateEqualsTestData(IFixture fixture)
        {
            var _0 = default( T );
            var (_1, _2) = fixture.CreateDistinctCollection<T>( 2 );

            return new TheoryData<T?, bool, T?, bool, bool>
            {
                { _1, true, _1, true, true },
                { _1, true, _2, true, false },
                { _0, false, _0, false, true },
                { _0, false, _1, true, false },
                { _1, true, _0, false, false }
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
