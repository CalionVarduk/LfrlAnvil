using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlSoft.NET.TestExtensions;
using LfrlSoft.NET.TestExtensions.Attributes;
using Xunit;
using AutoFixture;

namespace LfrlSoft.NET.Core.Tests.Chrono.Timestamp
{
    [TestClass( typeof( TimestampTestsData ) )]
    public class TimestampTests : TestsBase
    {
        [Fact]
        public void Zero_ShouldReturnCorrectResult()
        {
            var result = Core.Chrono.Timestamp.Zero;

            using ( new AssertionScope() )
            {
                result.UnixEpochTicks.Should().Be( 0 );
                result.UtcValue.Should().Be( DateTime.UnixEpoch );
                result.UtcValue.Kind.Should().Be( DateTimeKind.Utc );
            }
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetTicksCtorData ) )]
        public void Ctor_WithTicks_ShouldReturnCorrectResult(long ticks, DateTime expectedUtcValue)
        {
            var sut = new Core.Chrono.Timestamp( ticks );

            using ( new AssertionScope() )
            {
                sut.UnixEpochTicks.Should().Be( ticks );
                sut.UtcValue.Should().Be( expectedUtcValue );
                sut.UtcValue.Kind.Should().Be( DateTimeKind.Utc );
            }
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetUtcDateTimeCtorData ) )]
        public void Ctor_WithUtcValue_ShouldReturnCorrectResult(DateTime utcValue, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( utcValue );

            using ( new AssertionScope() )
            {
                sut.UnixEpochTicks.Should().Be( expectedTicks );
                sut.UtcValue.Should().Be( utcValue );
                sut.UtcValue.Kind.Should().Be( DateTimeKind.Utc );
            }
        }

        [Fact]
        public void ToString_ShouldReturnCorrectResult()
        {
            var ticks = Fixture.Create<int>();
            var sut = new Core.Chrono.Timestamp( ticks );

            var result = sut.ToString();

            result.Should().Be( $"{ticks} ticks" );
        }

        [Fact]
        public void GetHashCode_ShouldReturnCorrectResult()
        {
            var ticks = Fixture.Create<int>();
            var sut = new Core.Chrono.Timestamp( ticks );

            var result = sut.GetHashCode();

            result.Should().Be( ticks.GetHashCode() );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetEqualsData ) )]
        public void Equals_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a.Equals( b );

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetCompareToData ) )]
        public void CompareTo_ShouldReturnCorrectResult(long ticks1, long ticks2, int expectedSign)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a.CompareTo( b );

            Math.Sign( result ).Should().Be( expectedSign );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetAddData ) )]
        public void Add_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( ticks1 );
            var value = new Core.Chrono.Duration( ticks2 );

            var result = sut.Add( value );

            result.UnixEpochTicks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetSubtractData ) )]
        public void Subtract_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( ticks1 );
            var value = new Core.Chrono.Duration( ticks2 );

            var result = sut.Subtract( value );

            result.UnixEpochTicks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetSubtractData ) )]
        public void Subtract_WithTimestamp_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( ticks1 );
            var other = new Core.Chrono.Timestamp( ticks2 );

            var result = sut.Subtract( other );

            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetTicksCtorData ) )]
        public void DateTimeConversionOperator_ShouldReturnCorrectResult(long ticks, DateTime expected)
        {
            var sut = new Core.Chrono.Timestamp( ticks );

            var result = (DateTime) sut;

            using ( new AssertionScope() )
            {
                result.Should().Be( expected );
                result.Kind.Should().Be( DateTimeKind.Utc );
            }
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetAddData ) )]
        public void AddOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( ticks1 );
            var value = new Core.Chrono.Duration( ticks2 );

            var result = sut + value;

            result.UnixEpochTicks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetSubtractData ) )]
        public void SubtractOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( ticks1 );
            var value = new Core.Chrono.Duration( ticks2 );

            var result = sut - value;

            result.UnixEpochTicks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetSubtractData ) )]
        public void SubtractOperator_WithTimestamp_ShouldReturnCorrectResult(long ticks1, long ticks2, long expectedTicks)
        {
            var sut = new Core.Chrono.Timestamp( ticks1 );
            var other = new Core.Chrono.Timestamp( ticks2 );

            var result = sut - other;

            result.Ticks.Should().Be( expectedTicks );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetEqualsData ) )]
        public void EqualityOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a == b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetNotEqualsData ) )]
        public void InequalityOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a != b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetGreaterThanComparisonData ) )]
        public void GreaterThanOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a > b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetLessThanOrEqualToComparisonData ) )]
        public void LessThanOrEqualToOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a <= b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetLessThanComparisonData ) )]
        public void LessThanOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a < b;

            result.Should().Be( expected );
        }

        [Theory]
        [MethodData( nameof( TimestampTestsData.GetGreaterThanOrEqualToComparisonData ) )]
        public void GreaterThanOrEqualToOperator_ShouldReturnCorrectResult(long ticks1, long ticks2, bool expected)
        {
            var a = new Core.Chrono.Timestamp( ticks1 );
            var b = new Core.Chrono.Timestamp( ticks2 );

            var result = a >= b;

            result.Should().Be( expected );
        }
    }
}
