using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono.Exceptions;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.ZonedDateTime
{
    [TestClass( typeof( ZonedDateTimeTestsData ) )]
    public class ZonedDateTimeTests : TestsBase
    {
        [Fact]
        public void Default_ShouldReturnCorrectResult()
        {
            var result = default( Core.Chrono.ZonedDateTime );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( Core.Chrono.Timestamp.Zero );
                result.Value.Should().Be( result.Timestamp.UtcValue );
                result.Value.Kind.Should().Be( DateTimeKind.Utc );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.UtcOffset.Should().Be( Core.Chrono.Duration.Zero );
                result.TimeOfDay.Should().Be( Core.Chrono.TimeOfDay.Start );
                result.IsLocal.Should().BeFalse();
                result.IsUtc.Should().BeTrue();
                result.IsInDaylightSavingTime.Should().BeFalse();
                result.IsAmbiguous.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateUtcData ) )]
        public void CreateUtc_ShouldReturnCorrectResult(long ticks)
        {
            var timestamp = new Core.Chrono.Timestamp( ticks );
            var sut = Core.Chrono.ZonedDateTime.CreateUtc( timestamp );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( timestamp );
                sut.Value.Should().Be( timestamp.UtcValue );
                sut.Value.Kind.Should().Be( DateTimeKind.Utc );
                sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
                sut.UtcOffset.Should().Be( Core.Chrono.Duration.Zero );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( timestamp.UtcValue.TimeOfDay ) );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeTrue();
                sut.IsInDaylightSavingTime.Should().BeFalse();
                sut.IsAmbiguous.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateUtcWithDateTimeData ) )]
        public void CreateUtc_WithDateTime_ShouldReturnCorrectResult(DateTime dateTime)
        {
            var sut = Core.Chrono.ZonedDateTime.CreateUtc( dateTime );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Utc );
                sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
                sut.UtcOffset.Should().Be( Core.Chrono.Duration.Zero );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeTrue();
                sut.IsInDaylightSavingTime.Should().BeFalse();
                sut.IsAmbiguous.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateLocalData ) )]
        public void CreateLocal_ShouldReturnCorrectResult(DateTime dateTime)
        {
            var sut = Core.Chrono.ZonedDateTime.CreateLocal( dateTime );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( TimeZoneInfo.ConvertTimeToUtc( dateTime, TimeZoneInfo.Local ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Local );
                sut.TimeZone.Should().Be( TimeZoneInfo.Local );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
                sut.IsLocal.Should().BeTrue();
                sut.IsUtc.Should().BeFalse();
                sut.IsInDaylightSavingTime.Should().Be( TimeZoneInfo.Local.IsDaylightSavingTime( dateTime ) );
                sut.IsAmbiguous.Should().Be( TimeZoneInfo.Local.IsAmbiguousTime( dateTime ) );
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithUtcTimeZoneData ) )]
        public void Create_WithUtcTimeZone_ShouldReturnCorrectResult(DateTime dateTime)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, TimeZoneInfo.Utc );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Utc );
                sut.TimeZone.Should().Be( TimeZoneInfo.Utc );
                sut.UtcOffset.Should().Be( Core.Chrono.Duration.Zero );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeTrue();
                sut.IsInDaylightSavingTime.Should().BeFalse();
                sut.IsAmbiguous.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithLocalTimeZoneData ) )]
        public void Create_WithLocalTimeZone_ShouldReturnCorrectResult(DateTime dateTime)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, TimeZoneInfo.Local );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( TimeZoneInfo.ConvertTimeToUtc( dateTime, TimeZoneInfo.Local ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Local );
                sut.TimeZone.Should().Be( TimeZoneInfo.Local );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                sut.TimeZone.Should().Be( timeZone );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( timeZone.BaseUtcOffset ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                sut.TimeZone.Should().Be( timeZone );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( timeZone.BaseUtcOffset ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ).AddHours( -1 ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                sut.TimeZone.Should().Be( timeZone );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( timeZone.BaseUtcOffset ).AddHours( 1 ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
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
            var action = Lambda.Of( () => Core.Chrono.ZonedDateTime.Create( dateTime, timeZone ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<InvalidZonedDateTimeException>().Subject.FirstOrDefault();
                exception?.DateTime.Should().Be( dateTime );
                exception?.TimeZone.Should().Be( timeZone );
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithDateTimeOnTheEdgeOfInvalidityData ) )]
        public void Create_ShouldReturnCorrectResult_WhenDateTimeIsOnTheEdgeOfInvalidityInTheProvidedTimeZone(
            DateTime dateTime,
            TimeZoneInfo timeZone,
            bool isInDaylightSavingTime)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var expectedUtcOffset = timeZone.BaseUtcOffset.Add(
                isInDaylightSavingTime ? timeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime.Add( -expectedUtcOffset ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                sut.TimeZone.Should().Be( timeZone );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( expectedUtcOffset ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
                sut.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
                sut.IsAmbiguous.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateWithAmbiguousDateTimeData ) )]
        public void Create_ShouldReturnCorrectResult_WhenDateTimeIsAmbiguousInTheProvidedTimeZone(
            DateTime dateTime,
            TimeZoneInfo timeZone)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime.Add( -timeZone.BaseUtcOffset ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                sut.TimeZone.Should().Be( timeZone );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( timeZone.BaseUtcOffset ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var expectedUtcOffset = timeZone.BaseUtcOffset.Add(
                isInDaylightSavingTime ? timeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

            using ( new AssertionScope() )
            {
                sut.Timestamp.Should().Be( new Core.Chrono.Timestamp( dateTime.Add( -expectedUtcOffset ) ) );
                sut.Value.Should().Be( dateTime );
                sut.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                sut.TimeZone.Should().Be( timeZone );
                sut.UtcOffset.Should().Be( new Core.Chrono.Duration( expectedUtcOffset ) );
                sut.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( dateTime.TimeOfDay ) );
                sut.IsLocal.Should().BeFalse();
                sut.IsUtc.Should().BeFalse();
                sut.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
                sut.IsAmbiguous.Should().BeFalse();
            }
        }

        [Fact]
        public void TryCreate_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.Create<int>() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var expected = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = Core.Chrono.ZonedDateTime.TryCreate( dateTime, timeZone, out var zonedDateTime );

            using ( new AssertionScope() )
            {
                zonedDateTime.Should().Be( expected );
                result.Should().BeTrue();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCreateShouldThrowInvalidZonedDateTimeExceptionData ) )]
        public void TryCreate_ShouldReturnFalse_WhenDateTimeIsInvalidInTheProvidedTimeZone(
            DateTime dateTime,
            TimeZoneInfo timeZone)
        {
            var result = Core.Chrono.ZonedDateTime.TryCreate( dateTime, timeZone, out var zonedDateTime );

            using ( new AssertionScope() )
            {
                zonedDateTime.Should().Be( default );
                result.Should().BeFalse();
            }
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetToStringData ) )]
        public void ToString_ShouldReturnCorrectResult(DateTime dateTime, TimeZoneInfo timeZone, string expected)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var result = sut.ToString();
            result.Should().Be( expected );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var dateTime = Fixture.Create<DateTime>();
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( "1", 1 );
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var expected = Core.Hash.Default.Add( sut.Timestamp ).Add( timeZone.Id ).Value;

            var result = sut.GetHashCode();

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, bool expected)
        {
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(DateTime dt1, TimeZoneInfo tz1, DateTime dt2, TimeZoneInfo tz2, int expectedSign)
        {
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( TimeZoneInfo.Utc );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Utc );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.UtcOffset.Should().Be( Core.Chrono.Duration.Zero );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( TimeZoneInfo.Local );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Local );
                result.TimeZone.Should().Be( TimeZoneInfo.Local );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                result.TimeZone.Should().Be( targetTimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( targetTimeZone.BaseUtcOffset ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                result.TimeZone.Should().Be( targetTimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( targetTimeZone.BaseUtcOffset ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                result.TimeZone.Should().Be( targetTimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( targetTimeZone.BaseUtcOffset ).AddHours( 1 ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            var expectedUtcOffset = targetTimeZone.BaseUtcOffset.Add(
                isInDaylightSavingTime ? targetTimeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                result.TimeZone.Should().Be( targetTimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( expectedUtcOffset ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                result.TimeZone.Should().Be( targetTimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( targetTimeZone.BaseUtcOffset ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToTimeZone( targetTimeZone );

            var expectedUtcOffset = targetTimeZone.BaseUtcOffset.Add(
                isInDaylightSavingTime ? targetTimeZone.GetAdjustmentRules()[0].DaylightDelta : TimeSpan.Zero );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Unspecified );
                result.TimeZone.Should().Be( targetTimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( expectedUtcOffset ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToUtcTimeZone();

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Utc );
                result.TimeZone.Should().Be( TimeZoneInfo.Utc );
                result.UtcOffset.Should().Be( Core.Chrono.Duration.Zero );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );

            var result = sut.ToLocalTimeZone();

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( DateTimeKind.Local );
                result.TimeZone.Should().Be( TimeZoneInfo.Local );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( TimeZoneInfo.Local.GetUtcOffset( sut.Timestamp.UtcValue ) ) );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var durationToAdd = new Core.Chrono.Duration( ticksToAdd );

            var result = sut.Add( durationToAdd );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp.Add( durationToAdd ) );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( sut.Value.Kind );
                result.TimeZone.Should().Be( sut.TimeZone );
                result.UtcOffset.Should().Be( sut.UtcOffset );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var durationToAdd = new Core.Chrono.Duration( ticksToAdd );

            var result = sut.Add( durationToAdd );

            var daylightSavingOffset = timeZone.GetAdjustmentRules()[0].DaylightDelta;
            var expectedUtcOffset = sut.UtcOffset +
                new Core.Chrono.Duration( sut.IsInDaylightSavingTime ? -daylightSavingOffset : daylightSavingOffset );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp.Add( durationToAdd ) );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( sut.Value.Kind );
                result.TimeZone.Should().Be( sut.TimeZone );
                result.UtcOffset.Should().Be( expectedUtcOffset );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
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
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var durationToAdd = new Core.Chrono.Duration( ticksToAdd );

            var result = sut.Add( durationToAdd );

            var daylightSavingOffset = timeZone.GetAdjustmentRules()[0].DaylightDelta;
            var expectedUtcOffset = new Core.Chrono.Duration(
                timeZone.BaseUtcOffset.Ticks + (isInDaylightSavingTime ? daylightSavingOffset.Ticks : 0) );

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( sut.Timestamp.Add( durationToAdd ) );
                result.Value.Should().Be( expectedValue );
                result.Value.Kind.Should().Be( sut.Value.Kind );
                result.TimeZone.Should().Be( sut.TimeZone );
                result.UtcOffset.Should().Be( expectedUtcOffset );
                result.TimeOfDay.Should().Be( new Core.Chrono.TimeOfDay( expectedValue.TimeOfDay ) );
                result.IsLocal.Should().Be( sut.IsLocal );
                result.IsUtc.Should().Be( sut.IsUtc );
                result.IsInDaylightSavingTime.Should().Be( isInDaylightSavingTime );
                result.IsAmbiguous.Should().BeTrue();
            }
        }

        [Fact]
        public void Subtract_ShouldReturnCorrectResult()
        {
            var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
            var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

            var timeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var durationToSubtract = new Core.Chrono.Duration( Fixture.Create<int>() );

            var sut = Core.Chrono.ZonedDateTime.Create( value, timeZone );
            var expected = sut.Add( -durationToSubtract );

            var result = sut.Subtract( durationToSubtract );

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetDurationOffset_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var otherValue = Fixture.Create<DateTime>();
            var otherTimeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var otherTimeZone = ZonedDateTimeTestsData.GetTimeZone( $"{otherTimeZoneOffset}", otherTimeZoneOffset );

            var sut = Core.Chrono.ZonedDateTime.Create( value, timeZone );
            var other = Core.Chrono.ZonedDateTime.Create( otherValue, otherTimeZone );
            var expected = sut.Timestamp - other.Timestamp;

            var result = sut.GetDurationOffset( other );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetGetOppositeAmbiguousDateTimeWithUnambiguousData ) )]
        public void GetOppositeAmbiguousDateTime_WithUnambiguousDateTime_ShouldReturnCorrectResult(
            DateTime dateTime,
            TimeZoneInfo timeZone)
        {
            var sut = Core.Chrono.ZonedDateTime.Create( dateTime, timeZone );
            var result = sut.GetOppositeAmbiguousDateTime();
            result.Should().Be( sut );
        }

        [Theory]
        [MethodData( nameof( ZonedDateTimeTestsData.GetGetOppositeAmbiguousDateTimeData ) )]
        public void GetOppositeAmbiguousDateTime_ShouldReturnCorrectResult(
            DateTime utcDateTime,
            TimeZoneInfo timeZone,
            DateTime expectedUtcDateTime)
        {
            var sut = Core.Chrono.ZonedDateTime.CreateUtc( utcDateTime ).ToTimeZone( timeZone );

            var result = sut.GetOppositeAmbiguousDateTime();

            var daylightSavingOffset = timeZone.GetAdjustmentRules()[0].DaylightDelta;
            var expectedUtcOffset = sut.IsInDaylightSavingTime
                ? timeZone.BaseUtcOffset
                : timeZone.BaseUtcOffset + daylightSavingOffset;

            using ( new AssertionScope() )
            {
                result.Timestamp.Should().Be( new Core.Chrono.Timestamp( expectedUtcDateTime ) );
                result.Value.Should().Be( sut.Value );
                result.Value.Kind.Should().Be( sut.Value.Kind );
                result.TimeZone.Should().Be( sut.TimeZone );
                result.UtcOffset.Should().Be( new Core.Chrono.Duration( expectedUtcOffset ) );
                result.TimeOfDay.Should().Be( sut.TimeOfDay );
                result.IsLocal.Should().Be( sut.IsLocal );
                result.IsUtc.Should().Be( sut.IsUtc );
                result.IsInDaylightSavingTime.Should().Be( ! sut.IsInDaylightSavingTime );
                result.IsAmbiguous.Should().BeTrue();
            }
        }

        [Fact]
        public void DateTimeConversionOperator_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDateTime.Create( value, timeZone );

            var result = (DateTime)sut;

            result.Should().Be( sut.Value );
        }

        [Fact]
        public void TimestampConversionOperator_ShouldReturnCorrectResult()
        {
            var value = Fixture.Create<DateTime>();
            var timeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );

            var sut = Core.Chrono.ZonedDateTime.Create( value, timeZone );

            var result = (Core.Chrono.Timestamp)sut;

            result.Should().Be( sut.Timestamp );
        }

        [Fact]
        public void AddOperator_WithDuration_ShouldReturnCorrectResult()
        {
            var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
            var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

            var timeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var durationToSubtract = new Core.Chrono.Duration( Fixture.Create<int>() );

            var sut = Core.Chrono.ZonedDateTime.Create( value, timeZone );
            var expected = sut.Add( durationToSubtract );

            var result = sut + durationToSubtract;

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void SubtractOperator_WithDuration_ShouldReturnCorrectResult()
        {
            var dateTimeTicks = Math.Abs( Fixture.Create<int>() );
            var value = new DateTime( DateTime.UnixEpoch.Ticks + dateTimeTicks );

            var timeZoneOffset = Fixture.CreatePositiveInt32() % 12;
            var timeZone = ZonedDateTimeTestsData.GetTimeZone( $"{timeZoneOffset}", timeZoneOffset );
            var durationToSubtract = new Core.Chrono.Duration( Fixture.Create<int>() );

            var sut = Core.Chrono.ZonedDateTime.Create( value, timeZone );
            var expected = sut.Add( -durationToSubtract );

            var result = sut - durationToSubtract;

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
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

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
            var a = Core.Chrono.ZonedDateTime.Create( dt1, tz1 );
            var b = Core.Chrono.ZonedDateTime.Create( dt2, tz2 );

            var result = a >= b;

            result.Should().Be( expected );
        }
    }
}
