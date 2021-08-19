using System.Collections.Generic;
using AutoFixture;
using LfrlSoft.NET.TestExtensions;

namespace LfrlSoft.NET.Core.Tests.Functional.TypeCast
{
    public class GenericValidTypeCastTestsData<TSource, TDestination>
        where TSource : TDestination
    {
        public static IEnumerable<object?[]> CreateEqualsTestData(IFixture fixture)
        {
            var (_1, _2) = fixture.CreateDistinctCollection<TSource>( 2 );

            return new[]
            {
                new object?[] { _1, _1, true },
                new object?[] { _1, _2, false },
                new object?[] { _1, null, false },
                new object?[] { null, _1, false },
            };
        }

        public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
        {
            return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
        }
    }
}
