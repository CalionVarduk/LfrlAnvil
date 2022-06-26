using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.TypeCastTests;

public abstract class GenericValidTypeCastExtensionsTests<TSource, TDestination> : TestsBase
    where TSource : TDestination
    where TDestination : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnWithValue_WhenIsValid()
    {
        var value = Fixture.CreateNotDefault<TSource>();

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.ToMaybe();

        using ( new AssertionScope() )
        {
            result.HasValue.Should().BeTrue();
            result.Value.Should().Be( value );
        }
    }
}