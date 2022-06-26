using AutoFixture;
using FluentAssertions;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.PartialTypeCastTests;

public abstract class GenericPartialTypeCastTests<TSource> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithCorrectValue()
    {
        var value = Fixture.Create<TSource>();
        var sut = new PartialTypeCast<TSource>( value );
        sut.Value.Should().Be( value );
    }
}
