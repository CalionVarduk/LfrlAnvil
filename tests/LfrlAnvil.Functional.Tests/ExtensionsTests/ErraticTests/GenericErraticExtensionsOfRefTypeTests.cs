using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.ErraticTests;

public abstract class GenericErraticExtensionsOfRefTypeTests<T> : GenericErraticExtensionsTests<T>
    where T : class
{
    [Fact]
    public void ToMaybe_ShouldReturnWithoutValue_WhenHasNullValue()
    {
        var value = default( T );
        var sut = ( Erratic<T> )value!;

        var result = sut.ToMaybe();

        result.HasValue.Should().BeFalse();
    }
}
