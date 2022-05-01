using System;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Chrono;
using LfrlAnvil.TestExtensions;
using NSubstitute;
using Xunit;
using AutoFixture;
using FluentAssertions;
using LfrlAnvil.Functional;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Identifiers.Tests.IdentifierGeneratorTests
{
    public class IdentifierGeneratorTests : TestsBase
    {
        [Fact]
        public void Ctor_WithTimestampProvider_ShouldReturnCorrectResult()
        {
            var expectedTimestamp = GetAnyTimestamp();
            var timestamp = GetTimestampWithAnyTicks( expectedTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedTimestamp );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
                sut.StartTimestamp.Should().Be( expectedTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( Bounds.Create( ushort.MinValue, ushort.MaxValue ) );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( -1 );
            }
        }

        [Fact]
        public void Ctor_WithTimestampProvider_ShouldReturnCorrectResult_WhenMaxTimestampIsLessThanMaxDateTime()
        {
            var timestamp = new Timestamp( new DateTime( 1000, 1, 1 ) );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp( timestamp );
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( timestamp );
                sut.LastTimestamp.Should().Be( timestamp );
                sut.StartTimestamp.Should().Be( timestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( Bounds.Create( ushort.MinValue, ushort.MaxValue ) );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( -1 );
            }
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBaseTimestamp_ShouldReturnCorrectResult_WhenStartIsEqualToBaseTimestamp()
        {
            var expectedBaseTimestamp = GetAnyTimestamp();
            var baseTimestamp = GetTimestampWithAnyTicks( expectedBaseTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var timestampProvider = GetTimestampProviderMock( baseTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
                sut.LastTimestamp.Should().Be( expectedBaseTimestamp );
                sut.StartTimestamp.Should().Be( expectedBaseTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( Bounds.Create( ushort.MinValue, ushort.MaxValue ) );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( -1 );
            }
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBaseTimestamp_ShouldReturnCorrectResult_WhenStartIsGreaterThanBaseTimestamp()
        {
            var expectedBaseTimestamp = GetAnyTimestamp();
            var baseTimestamp = GetTimestampWithAnyTicks( expectedBaseTimestamp );
            var timestampDifference = Duration.FromMilliseconds( Fixture.CreateNotDefault<uint>() );
            var expectedTimestamp = expectedBaseTimestamp.Add( timestampDifference );
            var timestamp = GetTimestampWithAnyTicks( expectedTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
                sut.StartTimestamp.Should().Be( expectedTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( Bounds.Create( ushort.MinValue, ushort.MaxValue ) );
                sut.LastHighValue.Should().Be( (ulong)timestampDifference.FullMilliseconds );
                sut.LastLowValue.Should().Be( -1 );
            }
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBaseTimestamp_ShouldThrowArgumentOutOfRangeException_WhenStartIsLessThanBaseTimestamp()
        {
            var baseTimestamp = GetAnyTimestamp();
            var timestamp = baseTimestamp.Subtract( Duration.FromTicks( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, baseTimestamp ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBounds_ShouldReturnCorrectResult()
        {
            var expectedTimestamp = GetAnyTimestamp();
            var timestamp = GetTimestampWithAnyTicks( expectedTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedTimestamp );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
                sut.StartTimestamp.Should().Be( expectedTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( lowValueBounds );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( lowValueBounds.Min - 1 );
            }
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBaseTimestampAndBounds_ShouldReturnCorrectResult_WhenStartIsEqualToBaseTimestamp()
        {
            var expectedBaseTimestamp = GetAnyTimestamp();
            var baseTimestamp = GetTimestampWithAnyTicks( expectedBaseTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( baseTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
                sut.LastTimestamp.Should().Be( expectedBaseTimestamp );
                sut.StartTimestamp.Should().Be( expectedBaseTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( lowValueBounds );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( lowValueBounds.Min - 1 );
            }
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBaseTimestampAndBounds_ShouldReturnCorrectResult_WhenStartIsGreaterThanBaseTimestamp()
        {
            var expectedBaseTimestamp = GetAnyTimestamp();
            var baseTimestamp = GetTimestampWithAnyTicks( expectedBaseTimestamp );
            var timestampDifference = Duration.FromMilliseconds( Fixture.CreateNotDefault<uint>() );
            var expectedTimestamp = expectedBaseTimestamp.Add( timestampDifference );
            var timestamp = GetTimestampWithAnyTicks( expectedTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
                sut.StartTimestamp.Should().Be( expectedTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( LowValueExceededHandlingStrategy.Forbidden );
                sut.LowValueBounds.Should().Be( lowValueBounds );
                sut.LastHighValue.Should().Be( (ulong)timestampDifference.FullMilliseconds );
                sut.LastLowValue.Should().Be( lowValueBounds.Min - 1 );
            }
        }

        [Fact]
        public void
            Ctor_WithTimestampProviderAndBaseTimestampAndBounds_ShouldThrowArgumentOutOfRangeException_WhenStartIsLessThanBaseTimestamp()
        {
            var baseTimestamp = GetAnyTimestamp();
            var timestamp = baseTimestamp.Subtract( Duration.FromTicks( 1 ) );
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Ctor_WithTimestampProviderAndBoundsAndStrategy_ShouldReturnCorrectResult()
        {
            var expectedTimestamp = GetAnyTimestamp();
            var timestamp = GetTimestampWithAnyTicks( expectedTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var strategy = Fixture.Create<LowValueExceededHandlingStrategy>();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds, strategy );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedTimestamp );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
                sut.StartTimestamp.Should().Be( expectedTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( strategy );
                sut.LowValueBounds.Should().Be( lowValueBounds );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( lowValueBounds.Min - 1 );
            }
        }

        [Fact]
        public void
            Ctor_WithTimestampProviderAndBaseTimestampAndBoundsAndStrategy_ShouldReturnCorrectResult_WhenStartIsEqualToBaseTimestamp()
        {
            var expectedBaseTimestamp = GetAnyTimestamp();
            var baseTimestamp = GetTimestampWithAnyTicks( expectedBaseTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var strategy = Fixture.Create<LowValueExceededHandlingStrategy>();
            var timestampProvider = GetTimestampProviderMock( baseTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds, strategy );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
                sut.LastTimestamp.Should().Be( expectedBaseTimestamp );
                sut.StartTimestamp.Should().Be( expectedBaseTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( strategy );
                sut.LowValueBounds.Should().Be( lowValueBounds );
                sut.LastHighValue.Should().Be( 0 );
                sut.LastLowValue.Should().Be( lowValueBounds.Min - 1 );
            }
        }

        [Fact]
        public void
            Ctor_WithTimestampProviderAndBaseTimestampAndBoundsAndStrategy_ShouldReturnCorrectResult_WhenStartIsGreaterThanBaseTimestamp()
        {
            var expectedBaseTimestamp = GetAnyTimestamp();
            var baseTimestamp = GetTimestampWithAnyTicks( expectedBaseTimestamp );
            var timestampDifference = Duration.FromMilliseconds( Fixture.CreateNotDefault<uint>() );
            var expectedTimestamp = expectedBaseTimestamp.Add( timestampDifference );
            var timestamp = GetTimestampWithAnyTicks( expectedTimestamp );
            var expectedMaxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var strategy = Fixture.Create<LowValueExceededHandlingStrategy>();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds, strategy );

            using ( new AssertionScope() )
            {
                sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
                sut.StartTimestamp.Should().Be( expectedTimestamp );
                sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
                sut.LowValueExceededHandlingStrategy.Should().Be( strategy );
                sut.LowValueBounds.Should().Be( lowValueBounds );
                sut.LastHighValue.Should().Be( (ulong)timestampDifference.FullMilliseconds );
                sut.LastLowValue.Should().Be( lowValueBounds.Min - 1 );
            }
        }

        [Fact]
        public void
            Ctor_WithTimestampProviderAndBaseTimestampAndBoundsAndStrategy_ShouldThrowArgumentOutOfRangeException_WhenStartIsLessThanBaseTimestamp()
        {
            var baseTimestamp = GetAnyTimestamp();
            var timestamp = baseTimestamp.Subtract( Duration.FromTicks( 1 ) );
            var lowValueBounds = GetAnyLowValueBounds();
            var strategy = Fixture.Create<LowValueExceededHandlingStrategy>();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds, strategy ) );

            action.Should().ThrowExactly<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Generate_ShouldBeEquivalentToTryGenerate_WhenIdentifierIsSafelyGenerated()
        {
            var timestamp = GetAnyTimestamp();
            var lowValueBounds = GetAnyLowValueBounds();
            var strategy = Fixture.CreateNotDefault<LowValueExceededHandlingStrategy>();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds, strategy );
            var other = new IdentifierGenerator( timestampProvider, lowValueBounds, strategy );
            other.TryGenerate( out var expected );

            var result = sut.Generate();

            result.Should().Be( expected );
        }

        [Fact]
        public void
            Generate_ShouldThrowInvalidOperationException_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndStrategyIsForbidden()
        {
            var timestamp = GetAnyTimestamp();
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator(
                timestampProvider,
                lowValueBounds,
                LowValueExceededHandlingStrategy.Forbidden );

            sut.Generate();

            var action = Lambda.Of( () => sut.Generate() );

            action.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenNextMillisecondIsDifferent()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var expectedTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var expectedHighValue = (ulong)(expectedTimestamp - baseTimestamp).FullMilliseconds;
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp, expectedTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( lowValueBounds.Min );
            }
        }

        [Fact]
        public void TryGenerate_ShouldReturnTrueAndCorrectFirstIdentifier_WhenNextMillisecondIsTheSame()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var expectedHighValue = (ulong)(startTimestamp - baseTimestamp).FullMilliseconds;
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( lowValueBounds.Min );
            }
        }

        [Fact]
        public void TryGenerate_ShouldReturnTrueAndCorrectSecondIdentifier_WhenNextMillisecondIsTheSameAndLowValueBoundsAreNotExceeded()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var expectedHighValue = (ulong)(startTimestamp - baseTimestamp).FullMilliseconds;
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );
            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( (ushort)(lowValueBounds.Min + 1) );
            }
        }

        [Fact]
        public void
            TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndStrategyIsAddMs()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var unusedTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 10 ) );
            var expectedHighValue = (ulong)(startTimestamp - baseTimestamp).FullMilliseconds + 1;
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, unusedTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds, LowValueExceededHandlingStrategy.AddMs );
            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( lowValueBounds.Min );
            }
        }

        [Fact]
        public void
            TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndStrategyIsBusyWait()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 10 ) );
            var expectedHighValue = (ulong)(nextTimestamp - baseTimestamp).FullMilliseconds;
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, nextTimestamp );

            var sut = new IdentifierGenerator(
                timestampProvider,
                baseTimestamp,
                lowValueBounds,
                LowValueExceededHandlingStrategy.BusyWait );

            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( lowValueBounds.Min );
            }
        }

        [Fact]
        public void
            TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndStrategyIsSleep()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 10 ) );
            var expectedHighValue = (ulong)(nextTimestamp - baseTimestamp).FullMilliseconds;
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, nextTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds, LowValueExceededHandlingStrategy.Sleep );
            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( lowValueBounds.Min );
            }
        }

        [Fact]
        public void TryGenerate_ShouldReturnTrueAndCorrectFirstIdentifier_WhenNextMillisecondIsEqualToMaxHighValue()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = GetExpectedMaxTimestamp();
            var expectedHighValue = (ulong)(startTimestamp - baseTimestamp).FullMilliseconds;
            var lowValueBounds = GetAnyLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( startTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.High.Should().Be( expectedHighValue );
                outResult.Low.Should().Be( lowValueBounds.Min );
            }
        }

        [Fact]
        public void TryGenerate_ShouldReturnFalse_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndStrategyIsForbidden()
        {
            var timestamp = GetAnyTimestamp();
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator(
                timestampProvider,
                lowValueBounds,
                LowValueExceededHandlingStrategy.Forbidden );

            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void TryGenerate_ShouldReturnFalse_WhenHighValueIsGreaterThanLastAndMaxIsExceeded()
        {
            var timestamp = GetAnyTimestamp();
            var exceededTimestamp = GetExpectedMaxTimestamp().Add( Duration.FromMilliseconds( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

            var sut = new IdentifierGenerator( timestampProvider, timestamp );

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void
            TryGenerate_ShouldReturnFalse_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndMaxHighValueIsExceededAndStrategyIsAddMs()
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp, maxTimestamp );

            var sut = new IdentifierGenerator(
                timestampProvider,
                lowValueBounds,
                LowValueExceededHandlingStrategy.AddMs );

            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void
            TryGenerate_ShouldReturnFalse_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndMaxHighValueIsExceededAndStrategyIsBusyWait()
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var exceededTimestamp = maxTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp, maxTimestamp, maxTimestamp, exceededTimestamp );

            var sut = new IdentifierGenerator(
                timestampProvider,
                lowValueBounds,
                LowValueExceededHandlingStrategy.BusyWait );

            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void
            TryGenerate_ShouldReturnFalse_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndMaxHighValueIsExceededAndStrategyIsSleep()
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var exceededTimestamp = maxTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp, maxTimestamp, maxTimestamp, exceededTimestamp );

            var sut = new IdentifierGenerator(
                timestampProvider,
                lowValueBounds,
                LowValueExceededHandlingStrategy.Sleep );

            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void GetTimestamp_ShouldReturnCorrectResult()
        {
            var offset = Duration.FromMilliseconds( Fixture.CreateNotDefault<uint>() );
            var id = new Identifier( (ulong)offset.FullMilliseconds, 0 );
            var timestamp = GetAnyTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.GetTimestamp( id );

            result.Should().Be( timestamp + offset );
        }

        [Fact]
        public void GeneratorState_ShouldBeUpdatedCorrectly_AfterGeneratingFirstIdentifier()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var expectedTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 10 ) );
            var lowValueBounds = new Bounds<ushort>( 0, 1 );
            var timestampProvider = GetTimestampProviderMock( startTimestamp, expectedTimestamp );
            var expectedLastHighValue = (ulong)(expectedTimestamp - baseTimestamp).FullMilliseconds;

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            sut.Generate();

            using ( new AssertionScope() )
            {
                sut.LastLowValue.Should().Be( lowValueBounds.Min );
                sut.LastHighValue.Should().Be( expectedLastHighValue );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
            }
        }

        [Fact]
        public void GeneratorState_ShouldBeUpdatedCorrectly_AfterGeneratingNextIdentifierWhichCausesLowValueOverflow()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var expectedTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 10 ) );
            var lowValueBounds = new Bounds<ushort>( 0, 1 );
            var timestampProvider = GetTimestampProviderMock( startTimestamp, expectedTimestamp );
            var expectedLastHighValue = (ulong)(expectedTimestamp - baseTimestamp).FullMilliseconds;

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            sut.Generate();
            sut.Generate();

            using ( new AssertionScope() )
            {
                sut.LastLowValue.Should().Be( lowValueBounds.Max );
                sut.LastHighValue.Should().Be( expectedLastHighValue );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
            }
        }

        [Fact]
        public void GeneratorState_ShouldBeUpdatedCorrectly_AfterGeneratingNextIdentifierWhichFixesLowValueOverflow()
        {
            var baseTimestamp = GetAnyTimestamp();
            var startTimestamp = baseTimestamp.Add( Duration.FromMilliseconds( 1 ) );
            var firstIntermediateTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 5 ) );
            var secondIntermediateTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 7 ) );
            var expectedTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 10 ) );
            var lowValueBounds = new Bounds<ushort>( 0, 1 );

            var timestampProvider = GetTimestampProviderMock(
                startTimestamp,
                firstIntermediateTimestamp,
                secondIntermediateTimestamp,
                expectedTimestamp );

            var expectedLastHighValue = (ulong)(expectedTimestamp - baseTimestamp).FullMilliseconds;

            var sut = new IdentifierGenerator( timestampProvider, baseTimestamp, lowValueBounds );

            sut.Generate();
            sut.Generate();
            sut.Generate();

            using ( new AssertionScope() )
            {
                sut.LastLowValue.Should().Be( lowValueBounds.Min );
                sut.LastHighValue.Should().Be( expectedLastHighValue );
                sut.LastTimestamp.Should().Be( expectedTimestamp );
            }
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 65536 )]
        [InlineData( 0, 0, 1 )]
        [InlineData( 10, 20, 11 )]
        public void LowValuesLeft_ShouldReturnFullRange_WhenNothingHasBeenGeneratedYetAndQueryHappensAtTheInstantOfGeneratorConstruction(
            ushort min,
            ushort max,
            int expected)
        {
            var timestamp = GetAnyTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );
            var lowValueBounds = new Bounds<ushort>( min, max );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );

            var result = sut.LowValuesLeft;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 1, 65535 )]
        [InlineData( ushort.MinValue, ushort.MaxValue, 10, 65526 )]
        [InlineData( 0, 0, 1, 0 )]
        [InlineData( 10, 20, 1, 10 )]
        [InlineData( 10, 20, 10, 1 )]
        [InlineData( 10, 20, 11, 0 )]
        public void LowValuesLeft_ShouldReturnCorrectRange_WhenQueryHappensAtTheInstantOfLastIdentifierGeneration(
            ushort min,
            ushort max,
            int generatedAmount,
            int expected)
        {
            var timestamp = GetAnyTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );
            var lowValueBounds = new Bounds<ushort>( min, max );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );
            foreach ( var _ in Enumerable.Range( 0, generatedAmount ) )
                sut.Generate();

            var result = sut.LowValuesLeft;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 65536 )]
        [InlineData( 0, 0, 1 )]
        [InlineData( 10, 20, 11 )]
        public void LowValuesLeft_ShouldReturnFullRange_WhenQueryHappensInTheFuture(
            ushort min,
            ushort max,
            int expected)
        {
            var timestamp = GetAnyTimestamp();
            var futureTimestamp = timestamp.Add( Duration.FromMilliseconds( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp, futureTimestamp );
            var lowValueBounds = new Bounds<ushort>( min, max );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );

            var result = sut.LowValuesLeft;

            result.Should().Be( expected );
        }

        [Fact]
        public void LowValuesLeft_ShouldReturnZero_WhenMaxHighValueHasBeenExceeded()
        {
            var timestamp = GetAnyTimestamp();
            var exceededTimestamp = GetExpectedMaxTimestamp().Add( Duration.FromMilliseconds( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.LowValuesLeft;

            result.Should().Be( 0 );
        }

        [Fact]
        public void
            HighValuesLeft_ShouldReturnCorrectResult_WhenNothingHasBeenGeneratedYetAndQueryHappensAtTheInstantOfGeneratorConstruction()
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var expected = (ulong)maxTimestamp.Subtract( timestamp ).FullMilliseconds + 1;
            var timestampProvider = GetTimestampProviderMock( timestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.HighValuesLeft;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 1, 1 )]
        [InlineData( ushort.MinValue, ushort.MaxValue, 10, 1 )]
        [InlineData( 0, 0, 1, 0 )]
        [InlineData( 10, 20, 1, 1 )]
        [InlineData( 10, 20, 10, 1 )]
        [InlineData( 10, 20, 11, 0 )]
        public void HighValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensAtTheInstantOfLastIdentifierGeneration(
            ushort lowMin,
            ushort lowMax,
            int generatedAmount,
            int expectationOffset)
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var expected = (ulong)(maxTimestamp.Subtract( timestamp ).FullMilliseconds + expectationOffset);
            var timestampProvider = GetTimestampProviderMock( timestamp );
            var lowValueBounds = new Bounds<ushort>( lowMin, lowMax );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );
            foreach ( var _ in Enumerable.Range( 0, generatedAmount ) )
                sut.Generate();

            var result = sut.HighValuesLeft;

            result.Should().Be( expected );
        }

        [Fact]
        public void HighValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensInTheFuture()
        {
            var timestamp = GetAnyTimestamp();
            var futureTimestamp = timestamp.Add( Duration.FromMilliseconds( 1 ) );
            var maxTimestamp = GetExpectedMaxTimestamp();
            var expected = (ulong)maxTimestamp.Subtract( futureTimestamp ).FullMilliseconds + 1;
            var timestampProvider = GetTimestampProviderMock( timestamp, futureTimestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.HighValuesLeft;

            result.Should().Be( expected );
        }

        [Fact]
        public void HighValuesLeft_ShouldReturnZero_WhenMaxHighValueHasBeenExceeded()
        {
            var timestamp = GetAnyTimestamp();
            var exceededTimestamp = GetExpectedMaxTimestamp().Add( Duration.FromMilliseconds( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.HighValuesLeft;

            result.Should().Be( 0 );
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 65536 )]
        [InlineData( 0, 0, 1 )]
        [InlineData( 10, 20, 11 )]
        public void ValuesLeft_ShouldReturnCorrectResult_WhenNothingHasBeenGeneratedYetAndQueryHappensAtTheInstantOfGeneratorConstruction(
            ushort lowMin,
            ushort lowMax,
            uint expectedPerHigh)
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var expectedHigh = (ulong)maxTimestamp.Subtract( timestamp ).FullMilliseconds + 1;
            var expected = expectedHigh * expectedPerHigh;
            var timestampProvider = GetTimestampProviderMock( timestamp );
            var lowValueBounds = new Bounds<ushort>( lowMin, lowMax );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );

            var result = sut.ValuesLeft;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 1, 65536, 65535 )]
        [InlineData( ushort.MinValue, ushort.MaxValue, 10, 65536, 65526 )]
        [InlineData( 0, 0, 1, 1, 0 )]
        [InlineData( 10, 20, 1, 11, 10 )]
        [InlineData( 10, 20, 10, 11, 1 )]
        [InlineData( 10, 20, 11, 11, 0 )]
        public void ValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensAtTheInstantOfLastIdentifierGeneration(
            ushort lowMin,
            ushort lowMax,
            int generatedAmount,
            uint expectedPerHigh,
            uint expectedLow)
        {
            var timestamp = GetAnyTimestamp();
            var maxTimestamp = GetExpectedMaxTimestamp();
            var expectedHigh = (ulong)maxTimestamp.Subtract( timestamp ).FullMilliseconds;
            var expected = expectedHigh * expectedPerHigh + expectedLow;
            var timestampProvider = GetTimestampProviderMock( timestamp );
            var lowValueBounds = new Bounds<ushort>( lowMin, lowMax );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );
            foreach ( var _ in Enumerable.Range( 0, generatedAmount ) )
                sut.Generate();

            var result = sut.ValuesLeft;

            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( ushort.MinValue, ushort.MaxValue, 65536 )]
        [InlineData( 0, 0, 1 )]
        [InlineData( 10, 20, 11 )]
        public void ValuesLeft_ShouldReturnFullRange_WhenQueryHappensInTheFuture(
            ushort lowMin,
            ushort lowMax,
            uint expectedPerHigh)
        {
            var timestamp = GetAnyTimestamp();
            var futureTimestamp = timestamp.Add( Duration.FromMilliseconds( 1 ) );
            var maxTimestamp = GetExpectedMaxTimestamp();
            var expectedHigh = (ulong)maxTimestamp.Subtract( futureTimestamp ).FullMilliseconds + 1;
            var expected = expectedHigh * expectedPerHigh;
            var timestampProvider = GetTimestampProviderMock( timestamp, futureTimestamp );
            var lowValueBounds = new Bounds<ushort>( lowMin, lowMax );

            var sut = new IdentifierGenerator( timestampProvider, lowValueBounds );

            var result = sut.ValuesLeft;

            result.Should().Be( expected );
        }

        [Fact]
        public void ValuesLeft_ShouldReturnZero_WhenMaxHighValueHasBeenExceeded()
        {
            var timestamp = GetAnyTimestamp();
            var exceededTimestamp = GetExpectedMaxTimestamp().Add( Duration.FromMilliseconds( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.ValuesLeft;

            result.Should().Be( 0 );
        }

        [Fact]
        public void IsOutOfValues_ShouldReturnFalse_WhenMaxHighValueHasNotBeenExceeded()
        {
            var timestamp = GetAnyTimestamp();
            var exceededTimestamp = GetExpectedMaxTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.IsOutOfValues;

            result.Should().BeFalse();
        }

        [Fact]
        public void IsOutOfValues_ShouldReturnTrue_WhenMaxHighValueHasBeenExceeded()
        {
            var timestamp = GetAnyTimestamp();
            var exceededTimestamp = GetExpectedMaxTimestamp().Add( Duration.FromMilliseconds( 1 ) );
            var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

            var sut = new IdentifierGenerator( timestampProvider );

            var result = sut.IsOutOfValues;

            result.Should().BeTrue();
        }

        [Fact]
        public void IGeneratorGenerate_ShouldBeEquivalentToGenerate()
        {
            var timestamp = GetAnyTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            IGenerator sut = new IdentifierGenerator( timestampProvider );
            var other = new IdentifierGenerator( timestampProvider );
            var expected = other.Generate();

            var result = sut.Generate();

            result.Should().Be( expected );
        }

        [Fact]
        public void IGeneratorTryGenerate_ShouldBeEquivalentToTryGenerate_WhenReturnedValueIsTrue()
        {
            var timestamp = GetAnyTimestamp();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            IGenerator sut = new IdentifierGenerator( timestampProvider );
            var other = new IdentifierGenerator( timestampProvider );
            var expected = other.TryGenerate( out var outExpected );

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().Be( expected );
                outResult.Should().Be( outExpected );
            }
        }

        [Fact]
        public void IGeneratorTryGenerate_ShouldReturnFalse_WhenNextMillisecondIsTheSameAndLowValueBoundsAreExceededAndStrategyIsForbidden()
        {
            var timestamp = GetAnyTimestamp();
            var lowValueBounds = GetAnySingleLowValueBounds();
            var timestampProvider = GetTimestampProviderMock( timestamp );

            IGenerator sut = new IdentifierGenerator(
                timestampProvider,
                lowValueBounds,
                LowValueExceededHandlingStrategy.Forbidden );

            sut.Generate();

            var result = sut.TryGenerate( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        private static ITimestampProvider GetTimestampProviderMock(params Timestamp[] returnValues)
        {
            var mock = Substitute.For<ITimestampProvider>();
            if ( returnValues.Length == 0 )
                return mock;

            mock.GetNow().Returns( returnValues[0], returnValues.Skip( 1 ).ToArray() );
            return mock;
        }

        private Bounds<ushort> GetAnyLowValueBounds()
        {
            var (min, max) = Fixture.CreateDistinctSortedCollection<ushort>( 2 );
            return Bounds.Create( min, max );
        }

        private Bounds<ushort> GetAnySingleLowValueBounds()
        {
            var value = Fixture.Create<ushort>();
            return Bounds.Create( value, value );
        }

        private static Timestamp GetExpectedMaxTimestamp()
        {
            return new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromTicks( ChronoConstants.TicksPerMillisecond - 1 ) );
        }

        private static Timestamp GetExpectedMaxTimestamp(Timestamp @base)
        {
            return @base.Add( Duration.FromMilliseconds( (long)Identifier.MaxHighValue ) );
        }

        private Timestamp GetAnyTimestamp()
        {
            return Timestamp.Zero.Add( Duration.FromMilliseconds( Fixture.Create<int>() ) );
        }

        private Timestamp GetTimestampWithAnyTicks(Timestamp @base)
        {
            var ticks = Fixture.Create<uint>() % ChronoConstants.TicksPerMillisecond;
            return @base.Add( Duration.FromTicks( ticks ) );
        }
    }
}
