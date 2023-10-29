using System.Threading.Tasks;

namespace LfrlAnvil.Chrono.Tests.ZonedClockTests;

public class FrozenZonedClockTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = new FrozenZonedClock( ZonedDateTime.Create( value, timeZone ) );

        sut.TimeZone.Should().Be( timeZone );
    }

    [Fact]
    public async Task GetNow_ShouldReturnCorrectResult()
    {
        var value = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedDateTime.Create( value, timeZone );
        var sut = new FrozenZonedClock( expected );

        var firstResult = sut.GetNow();
        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );
        var secondResult = sut.GetNow();

        using ( new AssertionScope() )
        {
            firstResult.Should().Be( expected );
            secondResult.Should().Be( expected );
        }
    }
}
