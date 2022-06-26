using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace LfrlAnvil.Functional.Tests.PartialTypeCastTests;

public abstract class GenericValidPartialTypeCastTests<TSource, TDestination> : GenericPartialTypeCastTests<TSource>
    where TSource : TDestination
{
    [Fact]
    public void To_ShouldReturnCorrectTypeCast()
    {
        var value = Fixture.Create<TSource>();

        var sut = new PartialTypeCast<TSource>( value );

        var result = sut.To<TDestination>();

        using ( new AssertionScope() )
        {
            result.IsValid.Should().BeTrue();
            result.IsInvalid.Should().BeFalse();
            result.Source.Should().Be( value );
            result.Result.Should().Be( value );
        }
    }
}
