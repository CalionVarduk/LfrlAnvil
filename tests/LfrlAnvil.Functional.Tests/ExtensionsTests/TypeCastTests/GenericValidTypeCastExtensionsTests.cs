using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.TypeCastTests;

public abstract class GenericValidTypeCastExtensionsTests<TSource, TDestination> : TestsBase
    where TSource : TDestination
    where TDestination : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnWithValue_WhenIsValid()
    {
        var value = Fixture.CreateNotDefault<TSource>();

        var sut = ( TypeCast<TSource, TDestination> )value;

        var result = sut.ToMaybe();

        Assertion.All(
                result.HasValue.TestTrue(),
                result.Value.TestEquals( value ) )
            .Go();
    }
}
