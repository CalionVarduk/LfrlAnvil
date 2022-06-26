using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.Generators;
using LfrlAnvil.Identifiers.Exceptions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;
using NSubstitute;
using Xunit;

namespace LfrlAnvil.Identifiers.Tests.IdentifierGeneratorTests;

[TestClass( typeof( IdentifierGeneratorTestsData ) )]
public class IdentifierGeneratorTests : TestsBase
{
    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetCtorData ) )]
    public void Ctor_ShouldReturnCorrectResult(Timestamp startTimestamp, Timestamp expectedStartTimestamp, ulong expectedLastHighValue)
    {
        var expectedTimeEpsilon = Duration.FromMilliseconds( 1 );
        var expectedLowValueBounds = Bounds.Create( ushort.MinValue, ushort.MaxValue );
        var expectedMaxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );

        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        using ( new AssertionScope() )
        {
            sut.BaseTimestamp.Should().Be( Timestamp.Zero );
            sut.LastTimestamp.Should().Be( expectedStartTimestamp );
            sut.StartTimestamp.Should().Be( expectedStartTimestamp );
            sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
            sut.TimeEpsilon.Should().Be( expectedTimeEpsilon );
            sut.LowValueOverflowStrategy.Should().Be( LowValueOverflowStrategy.Forbidden );
            sut.LowValueBounds.Should().Be( expectedLowValueBounds );
            sut.LastHighValue.Should().Be( expectedLastHighValue );
            sut.LastLowValue.Should().Be( -1 );
        }
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartTimestampIsLessThanUnixEpoch()
    {
        var startTimestamp = new Timestamp( -1 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetCtorWithParamsData ) )]
    public void Ctor_WithParams_ShouldReturnCorrectResult(
        IdentifierGeneratorParams @params,
        Timestamp startTimestamp,
        Timestamp expectedBaseTimestamp,
        Timestamp expectedStartTimestamp,
        Timestamp expectedMaxTimestamp,
        ulong expectedLastHighValue,
        int expectedLastLowValue)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        using ( new AssertionScope() )
        {
            sut.BaseTimestamp.Should().Be( expectedBaseTimestamp );
            sut.LastTimestamp.Should().Be( expectedStartTimestamp );
            sut.StartTimestamp.Should().Be( expectedStartTimestamp );
            sut.MaxTimestamp.Should().Be( expectedMaxTimestamp );
            sut.TimeEpsilon.Should().Be( @params.TimeEpsilon );
            sut.LowValueOverflowStrategy.Should().Be( @params.LowValueOverflowStrategy );
            sut.LowValueBounds.Should().Be( @params.LowValueBounds );
            sut.LastHighValue.Should().Be( expectedLastHighValue );
            sut.LastLowValue.Should().Be( expectedLastLowValue );
        }
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenStartTimestampIsLessThanUnixEpoch()
    {
        var startTimestamp = new Timestamp( -1 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams() ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenTimeEpsilonIsNegative()
    {
        var @params = new IdentifierGeneratorParams { TimeEpsilon = Duration.FromTicks( -1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenTimeEpsilonIsZero()
    {
        var @params = new IdentifierGeneratorParams { TimeEpsilon = Duration.Zero };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenTimeEpsilonIsLargerThanThreeMs()
    {
        var @params = new IdentifierGeneratorParams { TimeEpsilon = Duration.FromMilliseconds( 3 ).AddTicks( 1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenBaseTimestampIsLessThanUnixEpoch()
    {
        var @params = new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( -1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenBaseTimestampIsGreaterThanStartTimestamp()
    {
        var @params = new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( 1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateFirstTimeForTheCurrentHighValueData ) )]
    public void Generate_ShouldReturnCorrectIdentifier_WhenGeneratingFirstTimeForTheCurrentHighValue(
        IdentifierGeneratorParams @params,
        Timestamp startTimestamp,
        Identifier expected)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheCurrentHighValueWithoutExceedingLowValueBoundsData ) )]
    public void Generate_ShouldReturnCorrectIdentifier_WhenGeneratingNextTimeForTheCurrentHighValueWithoutExceedingLowValueBounds(
        IdentifierGeneratorParams @params,
        Timestamp startTimestamp,
        int previousCount,
        Identifier expected)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, previousCount );

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheFutureHighValueData ) )]
    public void Generate_ShouldReturnCorrectIdentifier_WhenGeneratingNextTimeForTheFutureHighValue(
        Bounds<ushort> lowValueBounds,
        Timestamp futureTimestamp,
        Identifier expected)
    {
        var startTimestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock(
            startTimestamp,
            startTimestamp,
            startTimestamp,
            startTimestamp,
            futureTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );
        GenerateRange( sut, 3 );

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsData ) )]
    public void
        Generate_ShouldReturnCorrectIdentifier_WhenGeneratingNextTimeForTheCurrentHighValueAndExceedingLowValueBounds(
            IdentifierGeneratorParams @params,
            Timestamp startTimestamp,
            Timestamp futureTimestamp,
            Identifier expected)
    {
        var lowValueCount = @params.LowValueBounds.Max - @params.LowValueBounds.Min + 1;
        var timestampProvider = GetTimestampProviderMock(
            Repeat( startTimestamp, lowValueCount + 2 ).Append( futureTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Fact]
    public void Generate_ShouldReturnCorrectFirstIdentifier_WhenNextHighValueIsEqualToMaxHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var expected = new Identifier( (ulong)maxTimestamp.Subtract( startTimestamp ).FullMilliseconds, 0 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, maxTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Fact]
    public void
        Generate_ShouldThrowIdentifierGenerationException_WhenGeneratingNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsAndForbiddenStrategy()
    {
        var timestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( timestamp );

        var sut = new IdentifierGenerator(
            timestampProvider,
            new IdentifierGeneratorParams
            {
                LowValueOverflowStrategy = LowValueOverflowStrategy.Forbidden,
                LowValueBounds = new Bounds<ushort>( 0, 0 )
            } );

        sut.Generate();

        var action = Lambda.Of( () => sut.Generate() );

        action.Should().ThrowExactly<IdentifierGenerationException>();
    }

    [Fact]
    public void Generate_ShouldThrowIdentifierGenerationException_WhenNextHighValueIsGreaterThanLastAndMaxHighValueIsExceeded()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = new Timestamp( DateTime.MaxValue ).Add( Duration.FromTicks( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var action = Lambda.Of( () => sut.Generate() );

        action.Should().ThrowExactly<IdentifierGenerationException>();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheCurrentMaxHighValueAndExceedingLowValueBoundsData ) )]
    public void
        Generate_ShouldThrowIdentifierGenerationException_WhenGeneratingNextTimeForTheCurrentMaxHighValueAndExceedingLowValueBounds(
            IdentifierGeneratorParams @params,
            Timestamp maxTimestamp)
    {
        var startTimestamp = @params.BaseTimestamp;
        var futureTimestamp = maxTimestamp.Add( @params.TimeEpsilon );
        var lowValueCount = @params.LowValueBounds.Max - @params.LowValueBounds.Min + 1;
        var timestampProvider = GetTimestampProviderMock(
            Repeat( maxTimestamp, lowValueCount + 1 ).Prepend( startTimestamp ).Append( futureTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        var action = Lambda.Of( () => sut.Generate() );

        action.Should().ThrowExactly<IdentifierGenerationException>();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateFirstTimeForTheCurrentHighValueData ) )]
    public void TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenGeneratingFirstTimeForTheCurrentHighValue(
        IdentifierGeneratorParams @params,
        Timestamp startTimestamp,
        Identifier expected)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheCurrentHighValueWithoutExceedingLowValueBoundsData ) )]
    public void
        TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenGeneratingNextTimeForTheCurrentHighValueWithoutExceedingLowValueBounds(
            IdentifierGeneratorParams @params,
            Timestamp startTimestamp,
            int previousCount,
            Identifier expected)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, previousCount );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheFutureHighValueData ) )]
    public void TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenGeneratingNextTimeForTheFutureHighValue(
        Bounds<ushort> lowValueBounds,
        Timestamp futureTimestamp,
        Identifier expected)
    {
        var startTimestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock(
            startTimestamp,
            startTimestamp,
            startTimestamp,
            startTimestamp,
            futureTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );
        GenerateRange( sut, 3 );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsData ) )]
    public void
        TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenGeneratingNextTimeForTheCurrentHighValueAndExceedingLowValueBounds(
            IdentifierGeneratorParams @params,
            Timestamp startTimestamp,
            Timestamp futureTimestamp,
            Identifier expected)
    {
        var lowValueCount = @params.LowValueBounds.Max - @params.LowValueBounds.Min + 1;
        var timestampProvider = GetTimestampProviderMock(
            Repeat( startTimestamp, lowValueCount + 2 ).Append( futureTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }

    [Fact]
    public void TryGenerate_ShouldReturnTrueAndCorrectFirstIdentifier_WhenNextHighValueIsEqualToMaxHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var expected = new Identifier( (ulong)maxTimestamp.Subtract( startTimestamp ).FullMilliseconds, 0 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, maxTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeTrue();
            outResult.Should().Be( expected );
        }
    }

    [Fact]
    public void
        TryGenerate_ShouldReturnFalse_WhenGeneratingNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsAndForbiddenStrategy()
    {
        var timestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( timestamp );

        var sut = new IdentifierGenerator(
            timestampProvider,
            new IdentifierGeneratorParams
            {
                LowValueOverflowStrategy = LowValueOverflowStrategy.Forbidden,
                LowValueBounds = new Bounds<ushort>( 0, 0 )
            } );

        sut.Generate();

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Fact]
    public void TryGenerate_ShouldReturnFalse_WhenNextHighValueIsGreaterThanLastAndMaxHighValueIsExceeded()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = new Timestamp( DateTime.MaxValue ).Add( Duration.FromTicks( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheCurrentMaxHighValueAndExceedingLowValueBoundsData ) )]
    public void TryGenerate_ShouldReturnFalse_WhenGeneratingNextTimeForTheCurrentMaxHighValueAndExceedingLowValueBounds(
        IdentifierGeneratorParams @params,
        Timestamp maxTimestamp)
    {
        var startTimestamp = @params.BaseTimestamp;
        var futureTimestamp = maxTimestamp.Add( @params.TimeEpsilon );
        var lowValueCount = @params.LowValueBounds.Max - @params.LowValueBounds.Min + 1;
        var timestampProvider = GetTimestampProviderMock(
            Repeat( maxTimestamp, lowValueCount + 1 ).Prepend( startTimestamp ).Append( futureTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        var result = sut.TryGenerate( out var outResult );

        using ( new AssertionScope() )
        {
            result.Should().BeFalse();
            outResult.Should().Be( default );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGetTimestampData ) )]
    public void GetTimestamp_ShouldReturnCorrectResult(IdentifierGeneratorParams @params, Identifier identifier, Timestamp expected)
    {
        var startTimestamp = @params.BaseTimestamp;
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.GetTimestamp( identifier );

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetCalculateThroughputData ) )]
    public void CalculateThroughput_ShouldReturnCorrectResult(IdentifierGeneratorParams @params, Duration duration, ulong expected)
    {
        var startTimestamp = @params.BaseTimestamp;
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.CalculateThroughput( duration );

        result.Should().Be( expected );
    }

    [Fact]
    public void GeneratorState_ShouldBeUpdatedCorrectly_WhenGeneratingFirstTimeForTheCurrentHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var expected = sut.Generate();

        using ( new AssertionScope() )
        {
            sut.LastLowValue.Should().Be( expected.Low );
            sut.LastHighValue.Should().Be( expected.High );
            sut.LastTimestamp.Should().Be( nextTimestamp );
        }
    }

    [Fact]
    public void GeneratorState_ShouldBeUpdatedCorrectly_WhenGeneratingNextTimeForTheCurrentHighValueWithoutExceedingLowValueBounds()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        sut.Generate();
        var expected = sut.Generate();

        using ( new AssertionScope() )
        {
            sut.LastLowValue.Should().Be( expected.Low );
            sut.LastHighValue.Should().Be( expected.High );
            sut.LastTimestamp.Should().Be( nextTimestamp );
        }
    }

    [Fact]
    public void GeneratorState_ShouldBeUpdatedCorrectly_WhenGeneratingNextTimeForTheFutureHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock(
            startTimestamp,
            startTimestamp,
            startTimestamp,
            startTimestamp,
            nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );
        GenerateRange( sut, 3 );

        var expected = sut.Generate();

        using ( new AssertionScope() )
        {
            sut.LastLowValue.Should().Be( expected.Low );
            sut.LastHighValue.Should().Be( expected.High );
            sut.LastTimestamp.Should().Be( nextTimestamp );
        }
    }

    [Theory]
    [MethodData(
        nameof( IdentifierGeneratorTestsData.GetStateUpdateGenerateNextTimeForTheCurrentHighValueAndExceedingLowValueBoundsData ) )]
    public void
        GeneratorState_ShouldBeUpdatedCorrectly_WhenGeneratingNextTimeForTheCurrentHighValueAndExceedingLowValueBounds(
            IdentifierGeneratorParams @params,
            Timestamp nextTimestamp,
            Identifier expectedId,
            Timestamp expectedLastTimestamp)
    {
        var startTimestamp = @params.BaseTimestamp;
        var lowValueCount = @params.LowValueBounds.Max - @params.LowValueBounds.Min + 1;
        var timestampProvider = GetTimestampProviderMock(
            Repeat( startTimestamp, lowValueCount + 2 ).Append( nextTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        sut.Generate();

        using ( new AssertionScope() )
        {
            sut.LastLowValue.Should().Be( expectedId.Low );
            sut.LastHighValue.Should().Be( expectedId.High );
            sut.LastTimestamp.Should().Be( expectedLastTimestamp );
        }
    }

    [Fact]
    public void GeneratorState_ShouldNotChange_WhenFailedToGenerateNextIdentifier()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator(
            timestampProvider,
            new IdentifierGeneratorParams
            {
                LowValueBounds = new Bounds<ushort>( 0, 0 ),
                LowValueOverflowStrategy = LowValueOverflowStrategy.Forbidden
            } );

        var expected = sut.Generate();
        sut.TryGenerate( out _ );

        using ( new AssertionScope() )
        {
            sut.LastLowValue.Should().Be( expected.Low );
            sut.LastHighValue.Should().Be( expected.High );
            sut.LastTimestamp.Should().Be( startTimestamp );
        }
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetLowValuesLeftAtTheStartOfHighValueData ) )]
    public void
        LowValuesLeft_ShouldReturnCorrectResult_WhenNothingHasBeenGeneratedYetAndQueryHappensAtTheInstantOfGeneratorConstruction(
            Bounds<ushort> lowValueBounds,
            int expected)
    {
        var startTimestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );

        var result = sut.LowValuesLeft;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetLowValuesLeftAtTheInstantOfLastIdentifierGenerationData ) )]
    public void LowValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensAtTheInstantOfLastIdentifierGeneration(
        Bounds<ushort> lowValueBounds,
        int generatedAmount,
        int expected)
    {
        var startTimestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );
        GenerateRange( sut, generatedAmount );

        var result = sut.LowValuesLeft;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetLowValuesLeftAtTheStartOfHighValueData ) )]
    public void LowValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensInTheFuture(
        Bounds<ushort> lowValueBounds,
        int expected)
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );

        var result = sut.LowValuesLeft;

        result.Should().Be( expected );
    }

    [Fact]
    public void LowValuesLeft_ShouldReturnZero_WhenMaxHighValueHasBeenExceeded()
    {
        var timestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var exceededTimestamp = maxTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.LowValuesLeft;

        result.Should().Be( 0 );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetHighValuesLeftAtTheInstantOfGeneratorConstructionData ) )]
    public void
        HighValuesLeft_ShouldReturnCorrectResult_WhenNothingHasBeenGeneratedYetAndQueryHappensAtTheInstantOfGeneratorConstruction(
            IdentifierGeneratorParams @params,
            Timestamp startTimestamp,
            ulong expected)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );
        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.HighValuesLeft;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetHighValuesLeftAtTheInstantOfLastIdentifierGenerationData ) )]
    public void HighValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensAtTheInstantOfLastIdentifierGeneration(
        IdentifierGeneratorParams @params,
        Timestamp timestamp,
        int generatedAmount,
        ulong expected)
    {
        var timestampProvider = GetTimestampProviderMock( @params.BaseTimestamp, timestamp );
        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, generatedAmount );

        var result = sut.HighValuesLeft;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetHighValuesLeftInTheFutureData ) )]
    public void HighValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensInTheFuture(
        IdentifierGeneratorParams @params,
        Timestamp futureTimestamp,
        ulong expected)
    {
        var timestampProvider = GetTimestampProviderMock( @params.BaseTimestamp, futureTimestamp );
        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.HighValuesLeft;

        result.Should().Be( expected );
    }

    [Fact]
    public void HighValuesLeft_ShouldReturnZero_WhenMaxHighValueHasBeenExceeded()
    {
        var timestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var exceededTimestamp = maxTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.HighValuesLeft;

        result.Should().Be( 0 );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetValuesLeftAtTheInstantOfGeneratorConstructionData ) )]
    public void ValuesLeft_ShouldReturnCorrectResult_WhenNothingHasBeenGeneratedYetAndQueryHappensAtTheInstantOfGeneratorConstruction(
        IdentifierGeneratorParams @params,
        Timestamp startTimestamp,
        ulong expected)
    {
        var timestampProvider = GetTimestampProviderMock( startTimestamp );
        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.ValuesLeft;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetValuesLeftAtTheInstantOfLastIdentifierGenerationData ) )]
    public void ValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensAtTheInstantOfLastIdentifierGeneration(
        IdentifierGeneratorParams @params,
        Timestamp timestamp,
        int generatedAmount,
        ulong expected)
    {
        var timestampProvider = GetTimestampProviderMock( @params.BaseTimestamp, timestamp );
        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, generatedAmount );

        var result = sut.ValuesLeft;

        result.Should().Be( expected );
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetValuesLeftInTheFutureData ) )]
    public void ValuesLeft_ShouldReturnCorrectResult_WhenQueryHappensInTheFuture(
        IdentifierGeneratorParams @params,
        Timestamp futureTimestamp,
        ulong expected)
    {
        var timestampProvider = GetTimestampProviderMock( @params.BaseTimestamp, futureTimestamp );
        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.ValuesLeft;

        result.Should().Be( expected );
    }

    [Fact]
    public void ValuesLeft_ShouldReturnZero_WhenMaxHighValueHasBeenExceeded()
    {
        var timestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var exceededTimestamp = maxTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.ValuesLeft;

        result.Should().Be( 0 );
    }

    [Fact]
    public void IsOutOfValues_ShouldReturnFalse_WhenMaxHighValueHasNotBeenExceeded()
    {
        var timestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var timestampProvider = GetTimestampProviderMock( timestamp, maxTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.IsOutOfValues;

        result.Should().BeFalse();
    }

    [Fact]
    public void IsOutOfValues_ShouldReturnTrue_WhenMaxHighValueHasBeenExceeded()
    {
        var timestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var exceededTimestamp = maxTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( timestamp, exceededTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.IsOutOfValues;

        result.Should().BeTrue();
    }

    [Fact]
    public void IGeneratorGenerate_ShouldBeEquivalentToGenerate()
    {
        var timestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( timestamp );

        IGenerator sut = new IdentifierGenerator( timestampProvider );
        var other = new IdentifierGenerator( timestampProvider );
        var expected = other.Generate();

        var result = sut.Generate();

        result.Should().Be( expected );
    }

    [Fact]
    public void IGeneratorGenerate_ShouldThrowIdentifierGenerationException_WhenOutOfLowValues()
    {
        var timestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( timestamp );

        IGenerator sut = new IdentifierGenerator(
            timestampProvider,
            new IdentifierGeneratorParams
            {
                LowValueBounds = new Bounds<ushort>( 0, 0 ),
                LowValueOverflowStrategy = LowValueOverflowStrategy.Forbidden
            } );

        sut.Generate();

        var action = Lambda.Of( () => sut.Generate() );

        action.Should().ThrowExactly<IdentifierGenerationException>();
    }

    [Fact]
    public void IGeneratorTryGenerate_ShouldBeEquivalentToTryGenerate_WhenReturnedValueIsTrue()
    {
        var timestamp = Timestamp.Zero;
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
    public void IGeneratorTryGenerate_ShouldReturnFalse_WhenOutOfLowValues()
    {
        var timestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( timestamp );

        IGenerator sut = new IdentifierGenerator(
            timestampProvider,
            new IdentifierGeneratorParams
            {
                LowValueBounds = new Bounds<ushort>( 0, 0 ),
                LowValueOverflowStrategy = LowValueOverflowStrategy.Forbidden
            } );

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

        mock.GetNow().Returns( returnValues );
        return mock;
    }

    private static void GenerateRange(IdentifierGenerator sut, int count)
    {
        foreach ( var _ in Enumerable.Range( 0, count ) )
            sut.Generate();
    }

    private static IEnumerable<Timestamp> Repeat(Timestamp timestamp, int count)
    {
        return Enumerable.Range( 0, count ).Select( _ => timestamp );
    }
}
