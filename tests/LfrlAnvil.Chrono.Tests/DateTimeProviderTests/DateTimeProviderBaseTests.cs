using FluentAssertions.Execution;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Tests.DateTimeProviderTests;

public class DateTimeProviderBaseTests : TestsBase
{
    [Fact]
    public void IGenericGeneratorGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator<DateTime> sut = source;

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Fact]
    public void IGenericGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator<DateTime> sut = source;

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }

    [Fact]
    public void IGeneratorGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator sut = source;

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Fact]
    public void IGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator sut = source;

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }
}
