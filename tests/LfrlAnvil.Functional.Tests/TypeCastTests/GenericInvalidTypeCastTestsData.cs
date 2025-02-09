using System.Collections.Generic;

namespace LfrlAnvil.Functional.Tests.TypeCastTests;

public class GenericInvalidTypeCastTestsData<TSource, TDestination>
{
    public static TheoryData<TSource, TSource, bool> CreateEqualsTestData(Fixture fixture)
    {
        var (_1, _2) = fixture.CreateManyDistinct<TSource>( count: 2 );

        return new TheoryData<TSource, TSource, bool>
        {
            { _1, _1, true },
            { _1, _2, false }
        };
    }

    public static IEnumerable<object?[]> CreateNotEqualsTestData(Fixture fixture)
    {
        return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
    }
}
