using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.TypeCastTests
{
    public class GenericValidTypeCastTestsData<TSource, TDestination>
        where TSource : TDestination
    {
        public static TheoryData<object?, object?, bool> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<TSource>( 2 );

            return new TheoryData<object?, object?, bool>
            {
                { _1, _1, true },
                { _1, _2, false },
                { _1, null, false },
                { null, _1, false },
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
