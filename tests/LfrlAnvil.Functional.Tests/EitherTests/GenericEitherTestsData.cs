using System.Collections.Generic;

namespace LfrlAnvil.Functional.Tests.EitherTests;

public class GenericEitherTestsData<T1, T2>
{
    public static TheoryData<object, bool, object, bool, bool> CreateEqualsTestData(Fixture fixture)
    {
        var (_11, _12) = fixture.CreateManyDistinct<T1>( count: 2 );
        var (_21, _22) = fixture.CreateManyDistinct<T2>( count: 2 );

        return new TheoryData<object, bool, object, bool, bool>
        {
            { _11!, true, _11!, true, true },
            { _11!, true, _12!, true, false },
            { _21!, false, _21!, false, true },
            { _21!, false, _22!, false, false },
            { _11!, true, _21!, false, false },
            { _21!, false, _11!, true, false }
        };
    }

    public static IEnumerable<object?[]> CreateNotEqualsTestData(Fixture fixture)
    {
        return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
    }
}
