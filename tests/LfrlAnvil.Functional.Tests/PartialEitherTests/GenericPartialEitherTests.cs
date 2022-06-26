using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.TestExtensions;
using Xunit;

namespace LfrlAnvil.Functional.Tests.PartialEitherTests;

public abstract class GenericPartialEitherTests<T1, T2> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateWithCorrectValue()
    {
        var value = Fixture.Create<T1>();
        var sut = new PartialEither<T1>( value );
        sut.Value.Should().Be( value );
    }

    [Fact]
    public void WithFirst_ShouldCreateCorrectEither()
    {
        var value = Fixture.Create<T1>();

        var sut = new PartialEither<T1>( value );
        var result = sut.WithFirst<T2>();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeFalse();
            result.Second.Should().Be( value );
        }
    }

    [Fact]
    public void WithSecond_ShouldCreateCorrectEither()
    {
        var value = Fixture.Create<T1>();

        var sut = new PartialEither<T1>( value );
        var result = sut.WithSecond<T2>();

        using ( new AssertionScope() )
        {
            result.HasFirst.Should().BeTrue();
            result.First.Should().Be( value );
        }
    }
}
