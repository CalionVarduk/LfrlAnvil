namespace LfrlAnvil.Chrono.Tests.ZonedClockTests;

public class ZonedClockTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = ZonedClock.Utc;
        sut.TimeZone.TestEquals( TimeZoneInfo.Utc ).Go();
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = ZonedClock.Local;
        sut.TimeZone.TestEquals( TimeZoneInfo.Local ).Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new ZonedClock( timeZone );
        sut.TimeZone.TestEquals( timeZone ).Go();
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new ZonedClock( timeZone );

        var expectedMinTimestamp = new Timestamp( DateTime.UtcNow );
        var result = sut.GetNow();
        var expectedMaxTimestamp = new Timestamp( DateTime.UtcNow );

        Assertion.All(
                result.Timestamp.TestInRange( expectedMinTimestamp, expectedMaxTimestamp ),
                result.TimeZone.TestEquals( timeZone ) )
            .Go();
    }
}
