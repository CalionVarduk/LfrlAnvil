using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace LfrlAnvil.Functional.Tests.PartialTypeCastTests;

public abstract class GenericInvalidPartialTypeCastTests<TSource, TDestination> : GenericPartialTypeCastTests<TSource>
{
    [Fact]
    public void To_ShouldReturnCorrectTypeCast()
    {
        var value = Fixture.Create<TSource>();

        var sut = new PartialTypeCast<TSource>( value );

        var result = sut.To<TDestination>();

        using ( new AssertionScope() )
        {
            result.IsValid.Should().BeFalse();
            result.IsInvalid.Should().BeTrue();
            result.Source.Should().Be( value );
            result.Result.Should().Be( default( TDestination ) );
        }
    }
}