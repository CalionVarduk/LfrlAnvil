using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Chrono.Tests.ZonedClockTests;

public class ZonedClockBaseTests : TestsBase
{
    [Fact]
    public void IGenericGeneratorGenerate_ShouldBeEquivalentToGetNow()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedDateTime.Create( Fixture.Create<DateTime>(), timeZone );
        var source = Substitute.For<ZonedClockBase>( timeZone );
        source.GetNow().Returns( expected );
        IGenerator<ZonedDateTime> sut = source;

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IGenericGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedDateTime.Create( Fixture.Create<DateTime>(), timeZone );
        var source = Substitute.For<ZonedClockBase>( timeZone );
        source.GetNow().Returns( expected );
        IGenerator<ZonedDateTime> sut = source;

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void IGeneratorGenerate_ShouldBeEquivalentToGetNow()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedDateTime.Create( Fixture.Create<DateTime>(), timeZone );
        var source = Substitute.For<ZonedClockBase>( timeZone );
        source.GetNow().Returns( expected );
        IGenerator sut = source;

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void IGeneratorTryGenerate_ShouldBeEquivalentToGetNow()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedDateTime.Create( Fixture.Create<DateTime>(), timeZone );
        var source = Substitute.For<ZonedClockBase>( timeZone );
        source.GetNow().Returns( expected );
        IGenerator sut = source;

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }
}
