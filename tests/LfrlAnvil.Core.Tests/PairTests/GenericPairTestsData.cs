using System.Collections.Generic;

namespace LfrlAnvil.Tests.PairTests;

public class GenericPairTestsData<T1, T2>
{
    public static TheoryData<T1, T2, T1, T2, bool> CreateEqualsTestData(Fixture fixture)
    {
        var (f1, f2) = fixture.CreateManyDistinct<T1>( count: 2 );
        var (s1, s2) = fixture.CreateManyDistinct<T2>( count: 2 );

        return new TheoryData<T1, T2, T1, T2, bool>
        {
            { f1, s1, f1, s1, true },
            { f1, s1, f1, s2, false },
            { f1, s1, f2, s1, false },
            { f1, s1, f2, s2, false }
        };
    }

    public static IEnumerable<object?[]> CreateNotEqualsTestData(Fixture fixture)
    {
        return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
    }
}
