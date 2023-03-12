using System.Threading.Tasks;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Chrono.Tests.ZonedClockTests;

public class PreciseZonedClockTests : TestsBase
{
    [Fact]
    public void Utc_ShouldReturnCorrectResult()
    {
        var sut = PreciseZonedClock.Utc;

        using ( new AssertionScope() )
        {
            sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromMinutes( 1 ) );
        }
    }

    [Fact]
    public void Local_ShouldReturnCorrectResult()
    {
        var sut = PreciseZonedClock.Local;

        using ( new AssertionScope() )
        {
            sut.TimeZone.Should().Be( TimeZoneInfo.Local );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromMinutes( 1 ) );
        }
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    [InlineData( -2 )]
    [InlineData( ChronoConstants.DaysInYear * ChronoConstants.TicksPerStandardDay )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenPrecisionResetTimeoutIsInvalid(long value)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var action = Lambda.Of( () => new PreciseZonedClock( timeZone, Duration.FromTicks( value ) ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 100 )]
    [InlineData( 12345 )]
    public void Ctor_ShouldReturnCorrectResult(long value)
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone, Duration.FromTicks( value ) );

        using ( new AssertionScope() )
        {
            sut.TimeZone.Should().Be( timeZone );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromTicks( value ) );
        }
    }

    [Fact]
    public void DefaultCtor_ShouldReturnCorrectResult()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone );

        using ( new AssertionScope() )
        {
            sut.TimeZone.Should().Be( timeZone );
            sut.PrecisionResetTimeout.Should().Be( Duration.FromMinutes( 1 ) );
        }
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult()
    {
        var expectedMinTimestamp = new Timestamp( DateTime.UtcNow );
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone, Duration.FromHours( 24 ) );

        var result = sut.GetNow();
        var expectedMaxTimestamp = new Timestamp( DateTime.UtcNow );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().BeGreaterOrEqualTo( expectedMinTimestamp ).And.BeLessOrEqualTo( expectedMaxTimestamp );
            result.TimeZone.Should().Be( timeZone );
        }
    }

    [Fact]
    public void GetNow_ShouldReturnCorrectResult_WhenPrecisionResetTimeoutIsExceeded()
    {
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = new PreciseZonedClock( timeZone, Duration.FromTicks( 1 ) );

        Task.Delay( TimeSpan.FromMilliseconds( 1 ) ).Wait();

        var expectedMinTimestamp = new Timestamp( DateTime.UtcNow );
        var result = sut.GetNow();
        var expectedMaxTimestamp = new Timestamp( DateTime.UtcNow );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().BeGreaterOrEqualTo( expectedMinTimestamp ).And.BeLessOrEqualTo( expectedMaxTimestamp );
            result.TimeZone.Should().Be( timeZone );
        }
    }
}
