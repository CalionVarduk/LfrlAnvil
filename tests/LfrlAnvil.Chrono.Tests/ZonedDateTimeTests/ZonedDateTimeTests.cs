using FluentAssertions.Execution;
using LfrlAnvil.Chrono.Exceptions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Chrono.Tests.ZonedDateTimeTests;

[TestClass( typeof( ZonedDateTimeTestsData ) )]
public class ZonedDateTimeTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnStartOfUnixEpochInUtcTimeZone()
    {
        var result = default( ZonedDateTime );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( Timestamp.Zero );
            AssertValueDateCorrectness( result, result.Timestamp.UtcValue, DateTimeKind.Utc );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.UtcOffset.Should().Be( Duration.Zero );
            result.TimeOfDay.Should().Be( TimeOfDay.Start );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
            result.IsInDaylightSavingTime.Should().BeFalse();
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateUtcData ) )]
    public void CreateUtc_ShouldReturnCorrectUtcDateTime(long ticks)
    {
        var timestamp = new Timestamp( ticks );
        var sut = ZonedDateTime.CreateUtc( timestamp );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( timestamp );
            AssertValueDateCorrectness( sut, timestamp.UtcValue, DateTimeKind.Utc );
            sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
            sut.UtcOffset.Should().Be( Duration.Zero );
            sut.TimeOfDay.Should().Be( new TimeOfDay( timestamp.UtcValue.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeTrue();
            sut.IsInDaylightSavingTime.Should().BeFalse();
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateUtcWithDateTimeData ) )]
    public void CreateUtc_WithDateTime_ShouldReturnCorrectUtcDateTime(DateTime dateTime)
    {
        var sut = ZonedDateTime.CreateUtc( dateTime );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Utc );
            sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
            sut.UtcOffset.Should().Be( Duration.Zero );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeTrue();
            sut.IsInDaylightSavingTime.Should().BeFalse();
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateLocalData ) )]
    public void CreateLocal_ShouldReturnCorrectLocalDateTime(DateTime dateTime)
    {
        var sut = ZonedDateTime.CreateLocal( dateTime );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( TimeZoneInfo.ConvertTimeToUtc( dateTime, TimeZoneInfo.Local ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Local );
            sut.TimeZone.Should().Be( TimeZoneInfo.Local );
            sut.UtcOffset.Should().Be( new Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeTrue();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().Be( TimeZoneInfo.Local.IsDaylightSavingTime( dateTime ) );
            sut.IsAmbiguous.Should().Be( TimeZoneInfo.Local.IsAmbiguousTime( dateTime ) );
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithUtcTimeZoneData ) )]
    public void Create_WithUtcTimeZone_ShouldReturnCorrectUtcDateTime(DateTime dateTime)
    {
        var sut = ZonedDateTime.Create( dateTime, TimeZoneInfo.Utc );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Utc );
            sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
            sut.UtcOffset.Should().Be( Duration.Zero );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeTrue();
            sut.IsInDaylightSavingTime.Should().BeFalse();
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithLocalTimeZoneData ) )]
    public void Create_WithLocalTimeZone_ShouldReturnCorrectLocalDateTime(DateTime dateTime)
    {
        var sut = ZonedDateTime.Create( dateTime, TimeZoneInfo.Local );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( TimeZoneInfo.ConvertTimeToUtc( dateTime, TimeZoneInfo.Local ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Local );
            sut.TimeZone.Should().Be( TimeZoneInfo.Local );
            sut.UtcOffset.Should().Be( new Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeTrue();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().Be( TimeZoneInfo.Local.IsDaylightSavingTime( dateTime ) );
            sut.IsAmbiguous.Should().Be( TimeZoneInfo.Local.IsAmbiguousTime( dateTime ) );
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithoutDaylightSavingData ) )]
    public void Create_WithoutDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Unspecified );
            sut.TimeZone.Should().Be( timeZone );
            sut.UtcOffset.Should().Be( new Duration( timeZone.BaseUtcOffset ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().BeFalse();
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithInactiveDaylightSavingData ) )]
    public void Create_WithInactiveDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Unspecified );
            sut.TimeZone.Should().Be( timeZone );
            sut.UtcOffset.Should().Be( new Duration( timeZone.BaseUtcOffset ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().BeFalse();
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithActiveDaylightSavingData ) )]
    public void Create_WithActiveDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ).AddHours( -1 ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Unspecified );
            sut.TimeZone.Should().Be( timeZone );
            sut.UtcOffset.Should().Be( new Duration( timeZone.BaseUtcOffset ).AddHours( 1 ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().BeTrue();
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateShouldThrowInvalidZonedDateTimeExceptionData ) )]
    public void Create_ShouldThrowInvalidZonedDateTimeException_WhenDateTimeIsInvalidInTheProvidedTimeZone(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var action = Lambda.Of( () => ZonedDateTime.Create( dateTime, timeZone ) );

        action.Should()
            .ThrowExactly<InvalidZonedDateTimeException>()
            .AndMatch( e => e.DateTime == dateTime && ReferenceEquals( e.TimeZone, timeZone ) );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithDateTimeOnTheEdgeOfInvalidityData ) )]
    public void Create_ShouldReturnCorrectResult_WhenDateTimeIsOnTheEdgeOfInvalidityInTheProvidedTimeZone(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool isInDaylightSavingTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var expectedUtcOffset = timeZone.BaseUtcOffset.Add(
            isInDaylightSavingTime ? timeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime.Add( -expectedUtcOffset ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Unspecified );
            sut.TimeZone.Should().Be( timeZone );
            sut.UtcOffset.Should().Be( new Duration( expectedUtcOffset ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithAmbiguousDateTimeData ) )]
    public void Create_ShouldReturnCorrectResultInStandardTime_WhenDateTimeIsAmbiguousInTheProvidedTimeZone(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Unspecified );
            sut.TimeZone.Should().Be( timeZone );
            sut.UtcOffset.Should().Be( new Duration( timeZone.BaseUtcOffset ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().BeFalse();
            sut.IsAmbiguous.Should().BeTrue();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithDateTimeOnTheEdgeOfAmbiguityData ) )]
    public void Create_ShouldReturnCorrectResult_WhenDateTimeIsOnTheEdgeOfAmbiguityInTheProvidedTimeZone(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool isInDaylightSavingTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var expectedUtcOffset = timeZone.BaseUtcOffset.Add(
            isInDaylightSavingTime ? timeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

        using ( new AssertionScope() )
        {
            sut.Timestamp.Should().Be( new Timestamp( dateTime.Add( -expectedUtcOffset ) ) );
            AssertValueDateCorrectness( sut, dateTime, DateTimeKind.Unspecified );
            sut.TimeZone.Should().Be( timeZone );
            sut.UtcOffset.Should().Be( new Duration( expectedUtcOffset ) );
            sut.TimeOfDay.Should().Be( new TimeOfDay( dateTime.TimeOfDay ) );
            sut.IsLocal.Should().BeFalse();
            sut.IsUtc.Should().BeFalse();
            sut.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
            sut.IsAmbiguous.Should().BeFalse();
        }
    }

    [Fact]
    public void TryCreate_ShouldBeEquivalentToCreate_WithSafeDateTimeAndTimeZone()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var expected = ZonedDateTime.Create( dateTime, timeZone );

        var result = ZonedDateTime.TryCreate( dateTime, timeZone );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCreateShouldThrowInvalidZonedDateTimeExceptionData ) )]
    public void TryCreate_ShouldReturnNull_WhenDateTimeIsInvalidInTheProvidedTimeZone(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var result = ZonedDateTime.TryCreate( dateTime, timeZone );
        result.Should().BeNull();
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToStringData ) )]
    public void ToString_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone, string expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = Hash.Default.Add( sut.Timestamp ).Add( timeZone.Id ).Value;

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetEqualsData ) )]
    public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a.Equals( b );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetCompareToData ) )]
    public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a.CompareTo( b );

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithUtcTimeZoneData ) )]
    public void ToTimeZone_WithUtcTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( TimeZoneInfo.Utc );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Utc );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.UtcOffset.Should().Be( Duration.Zero );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
            result.IsInDaylightSavingTime.Should().BeFalse();
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithLocalTimeZoneData ) )]
    public void ToTimeZone_WithLocalTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( TimeZoneInfo.Local );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Local );
            result.TimeZone.Should().Be( TimeZoneInfo.Local );
            result.UtcOffset.Should().Be( new Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeTrue();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().Be( TimeZoneInfo.Local.IsDaylightSavingTime( expectedValue ) );
            result.IsAmbiguous.Should().Be( TimeZoneInfo.Local.IsAmbiguousTime( expectedValue ) );
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithoutTargetDaylightSavingData ) )]
    public void ToTimeZone_WithoutTargetDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeZoneInfo targetTimeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Unspecified );
            result.TimeZone.Should().Be( targetTimeZone );
            result.UtcOffset.Should().Be( new Duration( targetTimeZone.BaseUtcOffset ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().BeFalse();
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithInactiveTargetDaylightSavingData ) )]
    public void ToTimeZone_WithInactiveTargetDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeZoneInfo targetTimeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Unspecified );
            result.TimeZone.Should().Be( targetTimeZone );
            result.UtcOffset.Should().Be( new Duration( targetTimeZone.BaseUtcOffset ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().BeFalse();
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithActiveTargetDaylightSavingData ) )]
    public void ToTimeZone_WithActiveTargetDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeZoneInfo targetTimeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Unspecified );
            result.TimeZone.Should().Be( targetTimeZone );
            result.UtcOffset.Should().Be( new Duration( targetTimeZone.BaseUtcOffset ).AddHours( 1 ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().BeTrue();
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithDateTimeOnTheEdgeOfInvalidityData ) )]
    public void ToTimeZone_WhenDateTimeIsOnTheEdgeOfInvalidityInTargetTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeZoneInfo targetTimeZone,
        DateTime expectedValue,
        bool isInDaylightSavingTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        var expectedUtcOffset = targetTimeZone.BaseUtcOffset.Add(
            isInDaylightSavingTime ? targetTimeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Unspecified );
            result.TimeZone.Should().Be( targetTimeZone );
            result.UtcOffset.Should().Be( new Duration( expectedUtcOffset ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithAmbiguousDateTimeData ) )]
    public void ToTimeZone_WhenDateTimeIsAmbiguousInTargetTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeZoneInfo targetTimeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Unspecified );
            result.TimeZone.Should().Be( targetTimeZone );
            result.UtcOffset.Should().Be( new Duration( targetTimeZone.BaseUtcOffset ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().BeFalse();
            result.IsAmbiguous.Should().BeTrue();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToTimeZoneWithDateTimeOnTheEdgeOfAmbiguityData ) )]
    public void ToTimeZone_WhenDateTimeIsOnTheEdgeOfAmbiguityInTargetTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeZoneInfo targetTimeZone,
        DateTime expectedValue,
        bool isInDaylightSavingTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToTimeZone( targetTimeZone );

        var expectedUtcOffset = targetTimeZone.BaseUtcOffset.Add(
            isInDaylightSavingTime ? targetTimeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Unspecified );
            result.TimeZone.Should().Be( targetTimeZone );
            result.UtcOffset.Should().Be( new Duration( expectedUtcOffset ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToUtcTimeZoneData ) )]
    public void ToUtcTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToUtcTimeZone();

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Utc );
            result.TimeZone.Should().Be( TimeZoneInfo.Utc );
            result.UtcOffset.Should().Be( Duration.Zero );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeFalse();
            result.IsUtc.Should().BeTrue();
            result.IsInDaylightSavingTime.Should().BeFalse();
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetToLocalTimeZoneData ) )]
    public void ToLocalTimeZone_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var result = sut.ToLocalTimeZone();

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp );
            AssertValueDateCorrectness( result, expectedValue, DateTimeKind.Local );
            result.TimeZone.Should().Be( TimeZoneInfo.Local );
            result.UtcOffset.Should().Be( new Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().BeTrue();
            result.IsUtc.Should().BeFalse();
            result.IsInDaylightSavingTime.Should().Be( TimeZoneInfo.Local.IsDaylightSavingTime( expectedValue ) );
            result.IsAmbiguous.Should().Be( TimeZoneInfo.Local.IsAmbiguousTime( expectedValue ) );
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithDurationAndNoChangesToDaylightSavingData ) )]
    public void Add_WithDurationAndNoChangesToDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        long ticksToAdd,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var durationToAdd = new Duration( ticksToAdd );

        var result = sut.Add( durationToAdd );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp.Add( durationToAdd ) );
            AssertValueDateCorrectness( result, expectedValue, sut.Value.Kind );
            result.TimeZone.Should().Be( sut.TimeZone );
            result.UtcOffset.Should().Be( sut.UtcOffset );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().Be( sut.IsLocal );
            result.IsUtc.Should().Be( sut.IsUtc );
            result.IsInDaylightSavingTime.Should().Be( sut.IsInDaylightSavingTime );
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithDurationAndChangesToDaylightSavingData ) )]
    public void Add_WithDurationAndChangesToDaylightSaving_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        long ticksToAdd,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var durationToAdd = new Duration( ticksToAdd );

        var result = sut.Add( durationToAdd );

        var daylightSavingOffset = timeZone.GetAdjustmentRules()[0].DaylightDelta;
        var expectedUtcOffset = sut.UtcOffset +
            new Duration( sut.IsInDaylightSavingTime ? -daylightSavingOffset : daylightSavingOffset );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp.Add( durationToAdd ) );
            AssertValueDateCorrectness( result, expectedValue, sut.Value.Kind );
            result.TimeZone.Should().Be( sut.TimeZone );
            result.UtcOffset.Should().Be( expectedUtcOffset );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().Be( sut.IsLocal );
            result.IsUtc.Should().Be( sut.IsUtc );
            result.IsInDaylightSavingTime.Should().Be( ! sut.IsInDaylightSavingTime );
            result.IsAmbiguous.Should().BeFalse();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithDurationAndAmbiguousDateTimeData ) )]
    public void Add_WithDurationAndAmbiguousDateTime_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        long ticksToAdd,
        DateTime expectedValue,
        bool isInDaylightSavingTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var durationToAdd = new Duration( ticksToAdd );

        var result = sut.Add( durationToAdd );

        var daylightSavingOffset = timeZone.GetAdjustmentRules()[0].DaylightDelta;
        var expectedUtcOffset = new Duration(
            timeZone.BaseUtcOffset.Ticks + (isInDaylightSavingTime ? daylightSavingOffset.Ticks : 0) );

        using ( new AssertionScope() )
        {
            result.Timestamp.Should().Be( sut.Timestamp.Add( durationToAdd ) );
            AssertValueDateCorrectness( result, expectedValue, sut.Value.Kind );
            result.TimeZone.Should().Be( sut.TimeZone );
            result.UtcOffset.Should().Be( expectedUtcOffset );
            result.TimeOfDay.Should().Be( new TimeOfDay( expectedValue.TimeOfDay ) );
            result.IsLocal.Should().Be( sut.IsLocal );
            result.IsUtc.Should().Be( sut.IsUtc );
            result.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
            result.IsAmbiguous.Should().BeTrue();
        }
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithPeriodData ) )]
    public void Add_WithPeriod_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Period period,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedDateTime.Create( expectedValue, timeZone );

        var result = sut.Add( period );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithPeriodAndAmbiguityData ) )]
    public void Add_WithPeriodAndAmbiguity_ShouldReturnCorrectResult_AndPreserveAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        Period period,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.Add( period );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithPeriodThrowData ) )]
    public void Add_WithPeriod_ShouldThrowInvalidZonedDateTimeException_WhenTimeIsInvalid(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Period period)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var action = Lambda.Of( () => sut.Add( period ) );

        action.Should()
            .ThrowExactly<InvalidZonedDateTimeException>()
            .AndMatch( e => e.DateTime == dateTime.Add( period ) && ReferenceEquals( e.TimeZone, timeZone ) );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithPeriodData ) )]
    public void TryAdd_WithPeriod_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Period period,
        DateTime expectedValue)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedDateTime.Create( expectedValue, timeZone );

        var result = sut.TryAdd( period );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithPeriodAndAmbiguityData ) )]
    public void TryAdd_WithPeriodAndAmbiguity_ShouldReturnCorrectResult_AndPreserveAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        Period period,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.TryAdd( period );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetAddWithPeriodThrowData ) )]
    public void TryAdd_WithPeriod_ShouldReturnNull_WhenTimeIsInvalid(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        Period period)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var result = sut.TryAdd( period );
        result.Should().BeNull();
    }

    [Fact]
    public void Subtract_WithDuration_ShouldBeEquivalentToAddWithNegatedDuration()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var durationToSubtract = new Duration( Fixture.Create<int>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.Add( -durationToSubtract );

        var result = sut.Subtract( durationToSubtract );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void Subtract_WithPeriod_ShouldBeEquivalentToAddWithNegatedPeriod()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut.Subtract( periodToSubtract );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void TrySubtract_WithPeriod_ShouldBeEquivalentToTryAddWithNegatedPeriod()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.TryAdd( -periodToSubtract );

        var result = sut.TrySubtract( periodToSubtract );

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetDurationOffset_ShouldReturnDifferenceBetweenTimestamps()
    {
        var value = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var otherValue = Fixture.Create<DateTime>();
        var otherTimeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDateTime.Create( value, timeZone );
        var other = ZonedDateTime.Create( otherValue, otherTimeZone );
        var expected = sut.Timestamp - other.Timestamp;

        var result = sut.GetDurationOffset( other );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime otherDateTime,
        PeriodUnits units,
        Period expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var start = ZonedDateTime.Create( otherDateTime, timeZone );

        var result = sut.GetPeriodOffset( start, units );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGetPeriodOffsetData ) )]
    public void GetPeriodOffset_WithStartGreaterThanCaller_ShouldReturnCorrectResult(
        DateTime otherDateTime,
        TimeZoneInfo timeZone,
        DateTime dateTime,
        PeriodUnits units,
        Period expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var start = ZonedDateTime.Create( otherDateTime, timeZone );

        var result = sut.GetPeriodOffset( start, units );

        result.Should().BeEquivalentTo( -expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_ShouldReturnCorrectResult(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        DateTime otherDateTime,
        PeriodUnits units,
        Period expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var start = ZonedDateTime.Create( otherDateTime, timeZone );

        var result = sut.GetGreedyPeriodOffset( start, units );

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGetGreedyPeriodOffsetData ) )]
    public void GetGreedyPeriodOffset_WithStartGreaterThanCaller_ShouldReturnCorrectResult(
        DateTime otherDateTime,
        TimeZoneInfo timeZone,
        DateTime dateTime,
        PeriodUnits units,
        Period expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var start = ZonedDateTime.Create( otherDateTime, timeZone );

        var result = sut.GetGreedyPeriodOffset( start, units );

        result.Should().BeEquivalentTo( -expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetYearData ) )]
    public void SetYear_ShouldReturnTargetWithChangedYear(DateTime dateTime, TimeZoneInfo timeZone, int newYear, DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expectedResult = ZonedDateTime.Create( expected, timeZone );

        var result = sut.SetYear( newYear );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetYearWithAmbiguityData ) )]
    public void SetYear_WithAmbiguity_ShouldReturnTargetWithChangedYear_AndPreservedAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        int newYear,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.SetYear( newYear );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetYearThrowData ) )]
    public void SetYear_ShouldThrowArgumentOutOfRangeException_WhenYearIsInvalid(DateTime dateTime, TimeZoneInfo timeZone, int newYear)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.SetYear( newYear ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetMonthData ) )]
    public void SetMonth_ShouldReturnTargetWithChangedMonth(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        IsoMonthOfYear newMonth,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expectedResult = ZonedDateTime.Create( expected, timeZone );

        var result = sut.SetMonth( newMonth );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetMonthWithAmbiguityData ) )]
    public void SetMonth_WithAmbiguity_ShouldReturnTargetWithChangedMonth_AndPreservedAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        IsoMonthOfYear newMonth,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.SetMonth( newMonth );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetDayOfMonthData ) )]
    public void SetDayOfMonth_ShouldReturnTargetWithChangedDayOfMonth(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int newDay,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expectedResult = ZonedDateTime.Create( expected, timeZone );

        var result = sut.SetDayOfMonth( newDay );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetDayOfMonthWithAmbiguityData ) )]
    public void SetDayOfMonth_WithAmbiguity_ShouldReturnTargetWithChangedDayOfMonth_AndPreservedAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        int newDay,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.SetDayOfMonth( newDay );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetDayOfMonthThrowData ) )]
    public void SetDayOfMonth_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int newDay)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.SetDayOfMonth( newDay ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetDayOfYearData ) )]
    public void SetDayOfYear_ShouldReturnTargetWithChangedDayOfYear(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int newDay,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expectedResult = ZonedDateTime.Create( expected, timeZone );

        var result = sut.SetDayOfYear( newDay );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetDayOfYearWithAmbiguityData ) )]
    public void SetDayOfYear_WithAmbiguity_ShouldReturnTargetWithChangedDayOfYear_AndPreservedAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        int newDay,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.SetDayOfYear( newDay );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetDayOfYearThrowData ) )]
    public void SetDayOfYear_ShouldThrowArgumentOutOfRangeException_WhenDayIsInvalid(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        int newDay)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var action = Lambda.Of( () => sut.SetDayOfYear( newDay ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetTimeOfDayData ) )]
    public void SetTimeOfDay_ShouldReturnTargetWithChangedTimeOfDay(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeOfDay newTime,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expectedResult = ZonedDateTime.Create( expected, timeZone );

        var result = sut.SetTimeOfDay( newTime );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetTimeOfDayWithAmbiguityData ) )]
    public void SetTimeOfDay_WithAmbiguity_ShouldReturnTargetWithChangedTimeOfDay_AndPreservedAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        TimeOfDay newTime,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.SetTimeOfDay( newTime );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetTimeOfDayThrowData ) )]
    public void SetTimeOfDay_ShouldThrowInvalidZonedDateTimeException_WhenTimeIsInvalid(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeOfDay newTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );

        var action = Lambda.Of( () => sut.SetTimeOfDay( newTime ) );

        action.Should()
            .ThrowExactly<InvalidZonedDateTimeException>()
            .AndMatch( e => e.DateTime == dateTime.Date + (TimeSpan)newTime && ReferenceEquals( e.TimeZone, timeZone ) );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetTimeOfDayData ) )]
    public void TrySetTimeOfDay_ShouldReturnTargetWithChangedTimeOfDay(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeOfDay newTime,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expectedResult = ZonedDateTime.Create( expected, timeZone );

        var result = sut.TrySetTimeOfDay( newTime );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetTimeOfDayWithAmbiguityData ) )]
    public void TrySetTimeOfDay_WithAmbiguity_ShouldReturnTargetWithChangedTimeOfDay_AndPreservedAmbiguitySetting(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        bool forceInDaylightSavingTime,
        TimeOfDay newTime,
        DateTime expected)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        if ( forceInDaylightSavingTime )
            sut = sut.GetOppositeAmbiguousDateTime() ?? sut;

        var expectedResult = ZonedDateTime.Create( expected, timeZone );
        if ( forceInDaylightSavingTime )
            expectedResult = expectedResult.GetOppositeAmbiguousDateTime() ?? expectedResult;

        var result = sut.TrySetTimeOfDay( newTime );

        result.Should().BeEquivalentTo( expectedResult );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetSetTimeOfDayThrowData ) )]
    public void TrySetTimeOfDay_ShouldReturnNull_WhenTimeIsInvalid(
        DateTime dateTime,
        TimeZoneInfo timeZone,
        TimeOfDay newTime)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var result = sut.TrySetTimeOfDay( newTime );
        result.Should().BeNull();
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGetOppositeAmbiguousDateTimeWithUnambiguousData ) )]
    public void GetOppositeAmbiguousDateTime_WithUnambiguousDateTime_ShouldReturnNull(
        DateTime dateTime,
        TimeZoneInfo timeZone)
    {
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var result = sut.GetOppositeAmbiguousDateTime();
        result.Should().BeNull();
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGetOppositeAmbiguousDateTimeData ) )]
    public void GetOppositeAmbiguousDateTime_ShouldReturnCorrectResult(
        DateTime utcDateTime,
        TimeZoneInfo timeZone,
        DateTime expectedUtcDateTime)
    {
        var sut = ZonedDateTime.CreateUtc( utcDateTime ).ToTimeZone( timeZone );

        var result = sut.GetOppositeAmbiguousDateTime();

        var daylightSavingOffset = timeZone.GetAdjustmentRules()[0].DaylightDelta;
        var expectedUtcOffset = sut.IsInDaylightSavingTime
            ? timeZone.BaseUtcOffset
            : timeZone.BaseUtcOffset + daylightSavingOffset;

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            if ( result is null )
                return;

            result.Value.Timestamp.Should().Be( new Timestamp( expectedUtcDateTime ) );
            AssertValueDateCorrectness( result.Value, sut.Value, sut.Value.Kind );
            result.Value.TimeZone.Should().Be( sut.TimeZone );
            result.Value.UtcOffset.Should().Be( new Duration( expectedUtcOffset ) );
            result.Value.TimeOfDay.Should().Be( sut.TimeOfDay );
            result.Value.IsLocal.Should().Be( sut.IsLocal );
            result.Value.IsUtc.Should().Be( sut.IsUtc );
            result.Value.IsInDaylightSavingTime.Should().Be( ! sut.IsInDaylightSavingTime );
            result.Value.IsAmbiguous.Should().BeTrue();
        }
    }

    [Fact]
    public void GetDay_ShouldBeEquivalentToZonedDayCreate()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedDay.Create( dateTime, timeZone );

        var result = sut.GetDay();

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( IsoDayOfWeek.Monday )]
    [InlineData( IsoDayOfWeek.Tuesday )]
    [InlineData( IsoDayOfWeek.Wednesday )]
    [InlineData( IsoDayOfWeek.Thursday )]
    [InlineData( IsoDayOfWeek.Friday )]
    [InlineData( IsoDayOfWeek.Saturday )]
    [InlineData( IsoDayOfWeek.Sunday )]
    public void GetWeek_ShouldBeEquivalentToZonedWeekCreate(IsoDayOfWeek weekStart)
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedWeek.Create( dateTime, timeZone, weekStart );

        var result = sut.GetWeek( weekStart );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetMonth_ShouldBeEquivalentToZonedMonthCreate()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedMonth.Create( dateTime, timeZone );

        var result = sut.GetMonth();

        result.Should().Be( expected );
    }

    [Fact]
    public void GetYear_ShouldBeEquivalentToZonedYearCreate()
    {
        var dateTime = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var sut = ZonedDateTime.Create( dateTime, timeZone );
        var expected = ZonedYear.Create( dateTime, timeZone );

        var result = sut.GetYear();

        result.Should().Be( expected );
    }

    [Fact]
    public void DateTimeConversionOperator_ShouldReturnUnderlyingValue()
    {
        var value = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDateTime.Create( value, timeZone );

        var result = (DateTime)sut;

        result.Should().Be( sut.Value );
    }

    [Fact]
    public void TimestampConversionOperator_ShouldReturnUnderlyingTimestamp()
    {
        var value = Fixture.Create<DateTime>();
        var timeZone = TimeZoneFactory.CreateRandom( Fixture );

        var sut = ZonedDateTime.Create( value, timeZone );

        var result = (Timestamp)sut;

        result.Should().Be( sut.Timestamp );
    }

    [Fact]
    public void AddOperator_WithDuration_ShouldBeEquivalentToAddWithDuration()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var durationToAdd = new Duration( Fixture.Create<int>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.Add( durationToAdd );

        var result = sut + durationToAdd;

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void AddOperator_WithPeriod_ShouldBeEquivalentToAddWithPeriod()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var periodToAdd = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.Add( periodToAdd );

        var result = sut + periodToAdd;

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractOperator_WithDuration_ShouldBeEquivalentToAddWithNegatedDuration()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var durationToSubtract = new Duration( Fixture.Create<int>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.Add( -durationToSubtract );

        var result = sut - durationToSubtract;

        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void SubtractOperator_WithPeriod_ShouldBeEquivalentToAddWithNegatedPeriod()
    {
        var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
        var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

        var timeZone = TimeZoneFactory.CreateRandom( Fixture );
        var periodToSubtract = new Period(
            years: Fixture.Create<sbyte>(),
            months: Fixture.Create<sbyte>(),
            weeks: Fixture.Create<short>(),
            days: Fixture.Create<short>(),
            hours: Fixture.Create<short>(),
            minutes: Fixture.Create<short>(),
            seconds: Fixture.Create<short>(),
            milliseconds: Fixture.Create<short>(),
            ticks: Fixture.Create<short>() );

        var sut = ZonedDateTime.Create( value, timeZone );
        var expected = sut.Add( -periodToSubtract );

        var result = sut - periodToSubtract;

        result.Should().BeEquivalentTo( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetEqualsData ) )]
    public void EqualityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a == b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetNotEqualsData ) )]
    public void InequalityOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a != b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGreaterThanComparisonData ) )]
    public void GreaterThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a > b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetLessThanOrEqualToComparisonData ) )]
    public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a <= b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetLessThanComparisonData ) )]
    public void LessThanOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a < b;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( ZonedDateTimeTestsData.GetGreaterThanOrEqualToComparisonData ) )]
    public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(
        DateTime dt1,
        TimeZoneInfo tz1,
        DateTime dt2,
        TimeZoneInfo tz2,
        bool expected)
    {
        var a = ZonedDateTime.Create( dt1, tz1 );
        var b = ZonedDateTime.Create( dt2, tz2 );

        var result = a >= b;

        result.Should().Be( expected );
    }

    private static void AssertValueDateCorrectness(ZonedDateTime result, DateTime expected, DateTimeKind expectedKind)
    {
        result.Value.Should().Be( expected );
        result.Value.Kind.Should().Be( expectedKind );
        result.Year.Should().Be( expected.Year );
        result.Month.Should().Be( expected.Month );
        result.DayOfMonth.Should().Be( expected.Day );
        result.DayOfYear.Should().Be( expected.DayOfYear );
        result.DayOfWeek.Should().Be( expected.DayOfWeek.ToIso() );
    }
}
