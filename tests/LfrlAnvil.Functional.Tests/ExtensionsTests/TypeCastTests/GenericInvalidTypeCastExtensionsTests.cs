using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Functional.Extensions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.ExtensionsTests.TypeCastTests;

public abstract class GenericInvalidTypeCastExtensionsTests<TSource, TDestination> : TestsBase
    where TDestination : notnull
{
    [Fact]
    public void ToMaybe_ShouldReturnWithoutValue_WhenIsInvalid()
    {
        var value = Fixture.Create<TSource>();

        var sut = (TypeCast<TSource, TDestination>)value;

        var result = sut.ToMaybe();

        result.HasValue.Should().BeFalse();
    }
}