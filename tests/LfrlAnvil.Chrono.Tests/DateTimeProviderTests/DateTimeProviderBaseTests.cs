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

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IGenericGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator<DateTime> sut = source;

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void IGeneratorGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator sut = source;

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
    {
        var expected = Fixture.Create<DateTime>();
        var source = Substitute.For<DateTimeProviderBase>( expected.Kind );
        source.GetNow().Returns( expected );
        IGenerator sut = source;

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }
}
