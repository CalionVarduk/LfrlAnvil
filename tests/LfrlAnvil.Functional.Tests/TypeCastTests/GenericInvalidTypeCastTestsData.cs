using System.Collections.Generic;
using AutoFixture;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.TypeCastTests;

public class GenericInvalidTypeCastTestsData<TSource, TDestination>
{
    public static TheoryData<TSource, TSource, bool> CreateEqualsTestData(IFixture fixture)
    {
        var (_1, _2) = fixture.CreateDistinctCollection<TSource>( 2 );

        return new TheoryData<TSource, TSource, bool>
        {
            { _1, _1, true },
            { _1, _2, false }
        };
    }

    public static IEnumerable<object?[]> CreateNotEqualsTestData(IFixture fixture)
    {
        return CreateEqualsTestData( fixture ).ConvertResult( (bool r) => ! r );
    }
}