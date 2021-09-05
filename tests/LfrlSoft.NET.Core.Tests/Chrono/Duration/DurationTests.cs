﻿using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.Core.Chrono;
using LfrlSoft.NET.Core.Functional;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;

namespace LfrlSoft.NET.Core.Tests.Chrono.Duration
{
    [TestClass( typeof( DurationTestsData ) )]
    public class DurationTests : TestsBase
    {
        [Fact]
        public void Zero_ShouldReturnCorrectResult()
        {
            var sut = Core.Chrono.Duration.Zero;
            sut.Ticks.Should().Be( 0 );
        }

        [Fact]
        public void MinValue_ShouldReturnCorrectResult()
        {
            var sut = Core.Chrono.Duration.MinValue;
            sut.Ticks.Should().Be( long.MinValue );
        }

        [Fact]
        public void MaxValue_ShouldReturnCorrectResult()
        {
            var sut = Core.Chrono.Duration.MaxValue;
            sut.Ticks.Should().Be( long.MaxValue );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTicksData ) )]
        public void Ctor_WithTicks_ShouldCreateCorrectly(long ticks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            sut.Ticks.Should().Be( ticks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetCtorWithSecondsPrecisionData ) )]
        public void Ctor_WithSecondsPrecision_ShouldCreateCorrectly(int hours, int minutes, int seconds, long expectedSeconds)
        {
            var sut = new Core.Chrono.Duration( hours, minutes, seconds );
            sut.Ticks.Should().Be( expectedSeconds * Constants.TicksPerSecond );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetCtorWithMsPrecisionData ) )]
        public void Ctor_WithMsPrecision_ShouldCreateCorrectly(
            int hours,
            int minutes,
            int seconds,
            int milliseconds,
            long expectedMilliseconds)
        {
            var sut = new Core.Chrono.Duration( hours, minutes, seconds, milliseconds );
            sut.Ticks.Should().Be( expectedMilliseconds * Constants.TicksPerMillisecond );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetCtorWithTicksPrecisionData ) )]
        public void Ctor_WithTicksPrecision_ShouldCreateCorrectly(
            int hours,
            int minutes,
            int seconds,
            int milliseconds,
            int ticks,
            long expectedMilliseconds)
        {
            var sut = new Core.Chrono.Duration( hours, minutes, seconds, milliseconds, ticks );
            sut.Ticks.Should().Be( expectedMilliseconds * Constants.TicksPerMillisecond + ticks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetCtorWithTimeSpanData ) )]
        public void Ctor_WithTimeSpan_ShouldCreateCorrectly(TimeSpan timeSpan)
        {
            var sut = new Core.Chrono.Duration( timeSpan );
            sut.Ticks.Should().Be( timeSpan.Ticks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFullMillisecondsData ) )]
        public void FullMilliseconds_ShouldReturnCorrectResult(int milliseconds, int ticks, long expected)
        {
            var sut = new Core.Chrono.Duration( 0, 0, 0, milliseconds, ticks );
            sut.FullMilliseconds.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFullSecondsData ) )]
        public void FullSeconds_ShouldReturnCorrectResult(int seconds, int milliseconds, int ticks, long expected)
        {
            var sut = new Core.Chrono.Duration( 0, 0, seconds, milliseconds, ticks );
            sut.FullSeconds.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFullMinutesData ) )]
        public void FullMinutes_ShouldReturnCorrectResult(int minutes, int seconds, int milliseconds, int ticks, long expected)
        {
            var sut = new Core.Chrono.Duration( 0, minutes, seconds, milliseconds, ticks );
            sut.FullMinutes.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFullHoursData ) )]
        public void FullHours_ShouldReturnCorrectResult(int hours, int minutes, int seconds, int milliseconds, int ticks, long expected)
        {
            var sut = new Core.Chrono.Duration( hours, minutes, seconds, milliseconds, ticks );
            sut.FullHours.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTicksInMillisecondData ) )]
        public void TicksInMillisecond_ShouldReturnCorrectResult(int milliseconds, int ticks, int expected)
        {
            var sut = new Core.Chrono.Duration( 0, 0, 0, milliseconds, ticks );
            sut.TicksInMillisecond.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetMillisecondsInSecondData ) )]
        public void MillisecondsInSecond_ShouldReturnCorrectResult(int seconds, int milliseconds, int ticks, int expected)
        {
            var sut = new Core.Chrono.Duration( 0, 0, seconds, milliseconds, ticks );
            sut.MillisecondsInSecond.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSecondsInMinuteData ) )]
        public void SecondsInMinute_ShouldReturnCorrectResult(int minutes, int seconds, int milliseconds, int ticks, int expected)
        {
            var sut = new Core.Chrono.Duration( 0, minutes, seconds, milliseconds, ticks );
            sut.SecondsInMinute.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetMinutesInHourData ) )]
        public void MinutesInHour_ShouldReturnCorrectResult(int hours, int minutes, int seconds, int milliseconds, int ticks, int expected)
        {
            var sut = new Core.Chrono.Duration( hours, minutes, seconds, milliseconds, ticks );
            sut.MinutesInHour.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTotalMillisecondsData ) )]
        public void TotalMilliseconds_ShouldReturnCorrectResult(int milliseconds, int ticks, double expected)
        {
            var sut = new Core.Chrono.Duration( 0, 0, 0, milliseconds, ticks );
            sut.TotalMilliseconds.Should().BeApproximately( expected, 0.0000000001 );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTotalSecondsData ) )]
        public void TotalSeconds_ShouldReturnCorrectResult(int seconds, int milliseconds, int ticks, double expected)
        {
            var sut = new Core.Chrono.Duration( 0, 0, seconds, milliseconds, ticks );
            sut.TotalSeconds.Should().BeApproximately( expected, 0.0000000001 );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTotalMinutesData ) )]
        public void TotalMinutes_ShouldReturnCorrectResult(int minutes, int seconds, int milliseconds, int ticks, double expected)
        {
            var sut = new Core.Chrono.Duration( 0, minutes, seconds, milliseconds, ticks );
            sut.TotalMinutes.Should().BeApproximately( expected, 0.0000000001 );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTotalHoursData ) )]
        public void TotalHours_ShouldReturnCorrectResult(int hours, int minutes, int seconds, int milliseconds, int ticks, double expected)
        {
            var sut = new Core.Chrono.Duration( hours, minutes, seconds, milliseconds, ticks );
            sut.TotalHours.Should().BeApproximately( expected, 0.0000000001 );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTicksData ) )]
        public void FromTicks_ShouldReturnCorrectResult(long ticks)
        {
            var sut = Core.Chrono.Duration.FromTicks( ticks );
            sut.Ticks.Should().Be( ticks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromMillisecondsWithDoubleData ) )]
        public void FromMilliseconds_WithDouble_ShouldReturnCorrectResult(double milliseconds, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromMilliseconds( milliseconds );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromMillisecondsWithLongData ) )]
        public void FromMilliseconds_WithLong_ShouldReturnCorrectResult(long milliseconds, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromMilliseconds( milliseconds );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromSecondsWithDoubleData ) )]
        public void FromSeconds_WithDouble_ShouldReturnCorrectResult(double seconds, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromSeconds( seconds );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromSecondsWithLongData ) )]
        public void FromSeconds_WithLong_ShouldReturnCorrectResult(long seconds, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromSeconds( seconds );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromMinutesWithDoubleData ) )]
        public void FromMinutes_WithDouble_ShouldReturnCorrectResult(double minutes, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromMinutes( minutes );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromMinutesWithLongData ) )]
        public void FromMinutes_WithLong_ShouldReturnCorrectResult(long minutes, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromMinutes( minutes );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromHoursWithDoubleData ) )]
        public void FromHours_WithDouble_ShouldReturnCorrectResult(double hours, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromHours( hours );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetFromHoursWithLongData ) )]
        public void FromHours_WithLong_ShouldReturnCorrectResult(long hours, long expectedTicks)
        {
            var sut = Core.Chrono.Duration.FromHours( hours );
            sut.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetToStringData ) )]
        public void ToString_ShouldReturnCorrectResult(long ticks, string expected)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.ToString();
            result.Should().Be( expected );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var ticks = Fixture.Create<long>();
            var sut = new Core.Chrono.Duration( ticks );

            var result = sut.GetHashCode();

            result.Should().Be( ticks.GetHashCode() );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(long ticks1, long ticks2, int expectedSign)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetNegateData ) )]
        public void Negate_ShouldReturnCorrectResult(long ticks, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.Negate();
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddTicksData ) )]
        public void Add_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks1 );
            var other = new Core.Chrono.Duration( ticks2 );

            var result = sut.Add( other );

            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddTicksData ) )]
        public void AddTicks_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks1 );
            var result = sut.AddTicks( ticks2 );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddMillisecondsWithDoubleData ) )]
        public void AddMilliseconds_WithDouble_ShouldReturnCorrectResult(long ticks, double milliseconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddMilliseconds( milliseconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddMillisecondsWithLongData ) )]
        public void AddMilliseconds_WithLong_ShouldReturnCorrectResult(long ticks, long milliseconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddMilliseconds( milliseconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddSecondsWithDoubleData ) )]
        public void AddSeconds_WithDouble_ShouldReturnCorrectResult(long ticks, double seconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddSeconds( seconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddSecondsWithLongData ) )]
        public void AddSeconds_WithLong_ShouldReturnCorrectResult(long ticks, long seconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddSeconds( seconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddMinutesWithDoubleData ) )]
        public void AddMinutes_WithDouble_ShouldReturnCorrectResult(long ticks, double minutes, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddMinutes( minutes );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddMinutesWithLongData ) )]
        public void AddMinutes_WithLong_ShouldReturnCorrectResult(long ticks, long minutes, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddMinutes( minutes );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddHoursWithDoubleData ) )]
        public void AddHours_WithDouble_ShouldReturnCorrectResult(long ticks, double hours, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddHours( hours );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddHoursWithLongData ) )]
        public void AddHours_WithLong_ShouldReturnCorrectResult(long ticks, long hours, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.AddHours( hours );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractTicksData ) )]
        public void Subtract_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks1 );
            var other = new Core.Chrono.Duration( ticks2 );

            var result = sut.Subtract( other );

            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractTicksData ) )]
        public void SubtractTicks_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks1 );
            var result = sut.SubtractTicks( ticks2 );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractMillisecondsWithDoubleData ) )]
        public void SubtractMilliseconds_WithDouble_ShouldReturnCorrectResult(long ticks, double milliseconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractMilliseconds( milliseconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractMillisecondsWithLongData ) )]
        public void SubtractMilliseconds_WithLong_ShouldReturnCorrectResult(long ticks, long milliseconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractMilliseconds( milliseconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractSecondsWithDoubleData ) )]
        public void SubtractSeconds_WithDouble_ShouldReturnCorrectResult(long ticks, double seconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractSeconds( seconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractSecondsWithLongData ) )]
        public void SubtractSeconds_WithLong_ShouldReturnCorrectResult(long ticks, long seconds, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractSeconds( seconds );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractMinutesWithDoubleData ) )]
        public void SubtractMinutes_WithDouble_ShouldReturnCorrectResult(long ticks, double minutes, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractMinutes( minutes );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractMinutesWithLongData ) )]
        public void SubtractMinutes_WithLong_ShouldReturnCorrectResult(long ticks, long minutes, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractMinutes( minutes );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractHoursWithDoubleData ) )]
        public void SubtractHours_WithDouble_ShouldReturnCorrectResult(long ticks, double hours, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractHours( hours );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractHoursWithLongData ) )]
        public void SubtractHours_WithLong_ShouldReturnCorrectResult(long ticks, long hours, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SubtractHours( hours );
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTrimToMillisecondData ) )]
        public void TrimToMillisecond_ShouldReturnCorrectResult(long ticks, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.TrimToMillisecond();
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTrimToSecondData ) )]
        public void TrimToSecond_ShouldReturnCorrectResult(long ticks, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.TrimToSecond();
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTrimToMinuteData ) )]
        public void TrimToMinute_ShouldReturnCorrectResult(long ticks, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.TrimToMinute();
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTrimToHourData ) )]
        public void TrimToHour_ShouldReturnCorrectResult(long ticks, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.TrimToHour();
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetTicksInMillisecondThrowData ) )]
        public void SetTicksInMillisecond_ShouldThrow_WhenValueIsInvalid(long ticks, int value)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var action = Lambda.Of( () => sut.SetTicksInMillisecond( value ) );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetTicksInMillisecondData ) )]
        public void SetTicksInMillisecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SetTicksInMillisecond( value );

            using ( new AssertionScope() )
            {
                result.TicksInMillisecond.Should().Be( value );
                result.Ticks.Should().Be( expectedTicks );
            }
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetMillisecondsInSecondThrowData ) )]
        public void SetMillisecondsInSecond_ShouldThrow_WhenValueIsInvalid(long ticks, int value)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var action = Lambda.Of( () => sut.SetMillisecondsInSecond( value ) );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetMillisecondsInSecondData ) )]
        public void SetMillisecondsInSecond_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SetMillisecondsInSecond( value );

            using ( new AssertionScope() )
            {
                result.MillisecondsInSecond.Should().Be( value );
                result.Ticks.Should().Be( expectedTicks );
            }
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetSecondsInMinuteThrowData ) )]
        public void SetSecondsInMinute_ShouldThrow_WhenValueIsInvalid(long ticks, int value)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var action = Lambda.Of( () => sut.SetSecondsInMinute( value ) );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetSecondsInMinuteData ) )]
        public void SetSecondsInMinute_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SetSecondsInMinute( value );

            using ( new AssertionScope() )
            {
                result.SecondsInMinute.Should().Be( value );
                result.Ticks.Should().Be( expectedTicks );
            }
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetMinutesInHourThrowData ) )]
        public void SetMinutesInHour_ShouldThrow_WhenValueIsInvalid(long ticks, int value)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var action = Lambda.Of( () => sut.SetMinutesInHour( value ) );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetMinutesInHourData ) )]
        public void SetMinutesInHour_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SetMinutesInHour( value );

            using ( new AssertionScope() )
            {
                result.MinutesInHour.Should().Be( value );
                result.Ticks.Should().Be( expectedTicks );
            }
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetHoursThrowData ) )]
        public void SetHours_ShouldThrow_WhenValueIsInvalid(long ticks, int value)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var action = Lambda.Of( () => sut.SetHours( value ) );
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSetHoursData ) )]
        public void SetHours_ShouldReturnCorrectResult(long ticks, int value, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = sut.SetHours( value );

            using ( new AssertionScope() )
            {
                result.FullHours.Should().Be( value );
                result.Ticks.Should().Be( expectedTicks );
            }
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetTicksData ) )]
        public void TimeSpanConversionOperator_ShouldReturnCorrectResult(long ticks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = (TimeSpan) sut;
            result.Ticks.Should().Be( sut.Ticks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetNegateData ) )]
        public void NegateOperator_ShouldReturnCorrectResult(long ticks, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks );
            var result = -sut;
            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetAddTicksData ) )]
        public void AddOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks1 );
            var other = new Core.Chrono.Duration( ticks2 );

            var result = sut + other;

            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetSubtractTicksData ) )]
        public void SubtractOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Duration( ticks1 );
            var other = new Core.Chrono.Duration( ticks2 );

            var result = sut - other;

            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetEqualsData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetNotEqualsData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetGreaterThanComparisonData ) )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetLessThanOrEqualToComparisonData ) )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetLessThanComparisonData ) )]
        public void LessThanOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( DurationTestsData.GetGreaterThanOrEqualToComparisonData ) )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Duration( ticks1 );
            var b = new Core.Chrono.Duration( ticks2 );

            var result = a >= b;

            result.Should().Be( expected );
        }
    }
}