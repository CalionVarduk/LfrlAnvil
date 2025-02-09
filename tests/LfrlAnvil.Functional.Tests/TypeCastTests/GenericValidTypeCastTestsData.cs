using System.Collections.Generic;

namespace LfrlAnvil.Functional.Tests.TypeCastTests;

public class GenericValidTypeCastTestsData<TSource, TDestination>
    where TSource : TDestination
{
    public static TheoryData<object?, object?, bool> CreateEqualsTestData(Fixture fixture)
    {
        var (_1, _2) = fixture.CreateManyDistinct<TSource>( count: 2 );

        return new TheoryData<object?, object?, bool>
        {
            { _1, _1, true },
            { _1, _2, false },
            { _1, null, false },
            { null, _1, false },
        };
    }

    public static IEnumerable<object?[]> CreateNotEqualsTestData(Fixture fixture)
    {
        return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
    }
}
