using System.Threading.Tasks;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.ZonedClockTests;

public class PreciseZonedClockTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = PreciseZonedClock.Utc;

        Assertion.All(
                sut.TimeZone.TestEquals( TimeZoneInfo.Utc ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromMinutes( 1 ) ) )
            .Go();
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = PreciseZonedClock.Local;

        Assertion.All(
                sut.TimeZone.TestEquals( TimeZoneInfo.Local ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromMinutes( 1 ) ) )
            .Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var action = Lambda.Of( () => new PreciseZonedClock( timeZone, Duration.FromTicks( value ) ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ShouldReturnCorrectResult(long value)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone, Duration.FromTicks( value ) );

        Assertion.All(
                sut.TimeZone.TestEquals( timeZone ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromTicks( value ) ) )
            .Go();
    }

    [Fact]
    public void DefaultCtor_ShouldReturnCorrectResult()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone );

        Assertion.All(
                sut.TimeZone.TestEquals( timeZone ),
                sut.PrecisionResetTimeout.TestEquals( Duration.FromMinutes( 1 ) ) )
            .Go();
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var expectedMinTimestamp = new Timestamp( DateTime.UtcNow );
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone, Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMaxTimestamp = new Timestamp( DateTime.UtcNow );

        Assertion.All(
                result.Timestamp.TestInRange( expectedMinTimestamp, expectedMaxTimestamp ),
                result.TimeZone.TestEquals( timeZone ) )
            .Go();
    }

    [Fact]
    public async Task GetNow_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone, Duration.FromTicks( 1 ) );

        await Task.Delay( TimeSpan.FromMilliseconds( 1 ) );

        var expectedMinTimestamp = new Timestamp( DateTime.UtcNow );
        var result = sut.GetNow();
        var expectedMaxTimestamp = new Timestamp( DateTime.UtcNow );

        Assertion.All(
                result.Timestamp.TestInRange( expectedMinTimestamp, expectedMaxTimestamp ),
                result.TimeZone.TestEquals( timeZone ) )
            .Go();
    }
}
