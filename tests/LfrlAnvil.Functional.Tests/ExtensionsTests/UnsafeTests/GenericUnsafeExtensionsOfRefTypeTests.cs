using LfrlAnvil.Functional.Extensions;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.UnsafeTests;

public abstract class GenericUnsafeExtensionsOfRefTypeTests<T> : GenericUnsafeExtensionsTests<T>
    where T : class
{
    [Fact]
    public void ToMaybe_ShouldReturnWithoutValue_WhenHasNullValue()
    {
        var value = default( T );
        var sut = ( Unsafe<T> )value!;

        var result = sut.ToMaybe();

        result.HasValue.Should().BeFalse();
    }
}
