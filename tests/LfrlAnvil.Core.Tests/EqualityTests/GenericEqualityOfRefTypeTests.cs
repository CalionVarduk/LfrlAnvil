using System.Collections.Generic;
using LfrlAnvil.TestExtensions.Attributes;

namespace LfrlAnvil.Tests.EqualityTests;

public abstract class GenericEqualityOfRefTypeTests<T> : GenericEqualityTests<T>
    where T : class
{
    [Theory]
    [GenericMethodData( nameof( CreateCtorNullTestData ) )]
    public void Ctor_ShouldNotThrowWhenValueIsNull(T? first, T? second, bool expected)
    {
        var sut = new Equality<T>( first, second );

        sut.Should()
            .BeEquivalentTo(
                new
                {
                    First = first,
                    Second = second,
                    Result = expected
                } );
    }

    public static IEnumerable<object?[]> CreateCtorNullTestData(IFixture fixture)
    {
        var result = new List<object?[]>();
        var value = fixture.CreateNotDefault<T>();

        result.Add( new object?[] { null, value, false } );
        result.Add( new object?[] { value, null, false } );
        result.Add( new object?[] { null, null, true } );

        return result;
    }
}
