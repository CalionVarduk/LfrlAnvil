using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Chrono;
using LfrlAnvil.Functional;
using LfrlAnvil.Generators;
using LfrlAnvil.Identifiers.Exceptions;
using LfrlAnvil.TestExtensions.Attributes;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Identifiers.Tests;

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

        Assertion.All(
                sut.BaseTimestamp.TestEquals( Timestamp.Zero ),
                sut.LastTimestamp.TestEquals( expectedStartTimestamp ),
                sut.StartTimestamp.TestEquals( expectedStartTimestamp ),
                sut.MaxTimestamp.TestEquals( expectedMaxTimestamp ),
                sut.TimeEpsilon.TestEquals( expectedTimeEpsilon ),
                sut.LowValueOverflowStrategy.TestEquals( LowValueOverflowStrategy.Forbidden ),
                sut.LowValueBounds.TestEquals( expectedLowValueBounds ),
                sut.LastHighValue.TestEquals( expectedLastHighValue ),
                sut.LastLowValue.TestEquals( -1 ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenStartTimestampIsLessThanUnixEpoch()
    {
        var startTimestamp = new Timestamp( -1 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        Assertion.All(
                sut.BaseTimestamp.TestEquals( expectedBaseTimestamp ),
                sut.LastTimestamp.TestEquals( expectedStartTimestamp ),
                sut.StartTimestamp.TestEquals( expectedStartTimestamp ),
                sut.MaxTimestamp.TestEquals( expectedMaxTimestamp ),
                sut.TimeEpsilon.TestEquals( @params.TimeEpsilon ),
                sut.LowValueOverflowStrategy.TestEquals( @params.LowValueOverflowStrategy ),
                sut.LowValueBounds.TestEquals( @params.LowValueBounds ),
                sut.LastHighValue.TestEquals( expectedLastHighValue ),
                sut.LastLowValue.TestEquals( expectedLastLowValue ) )
            .Go();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenStartTimestampIsLessThanUnixEpoch()
    {
        var startTimestamp = new Timestamp( -1 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenTimeEpsilonIsNegative()
    {
        var @params = new IdentifierGeneratorParams { TimeEpsilon = Duration.FromTicks( -1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenTimeEpsilonIsZero()
    {
        var @params = new IdentifierGeneratorParams { TimeEpsilon = Duration.Zero };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenBaseTimestampIsLessThanUnixEpoch()
    {
        var @params = new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( -1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Ctor_WithParams_ShouldThrowArgumentOutOfRangeException_WhenBaseTimestampIsGreaterThanStartTimestamp()
    {
        var @params = new IdentifierGeneratorParams { BaseTimestamp = new Timestamp( 1 ) };
        var timestampProvider = GetTimestampProviderMock( Timestamp.Zero );

        var action = Lambda.Of( () => new IdentifierGenerator( timestampProvider, @params ) );

        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheFutureHighValueData ) )]
    public void Generate_ShouldReturnCorrectIdentifier_WhenGeneratingNextTimeForTheFutureHighValue(
        Bounds<ushort> lowValueBounds,
        Timestamp futureTimestamp,
        Identifier expected)
    {
        var startTimestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, startTimestamp, futureTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );
        GenerateRange( sut, 3 );

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
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
        var timestampProvider = GetTimestampProviderMock( Repeat( startTimestamp, lowValueCount + 2 ).Append( futureTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void Generate_ShouldReturnCorrectFirstIdentifier_WhenNextHighValueIsEqualToMaxHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var expected = new Identifier( ( ulong )maxTimestamp.Subtract( startTimestamp ).FullMilliseconds, 0 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, maxTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.Generate();

        result.TestEquals( expected ).Go();
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

        action.Test( exc => exc.TestType().Exact<IdentifierGenerationException>() ).Go();
    }

    [Fact]
    public void Generate_ShouldThrowIdentifierGenerationException_WhenNextHighValueIsGreaterThanLastAndMaxHighValueIsExceeded()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = new Timestamp( DateTime.MaxValue ).Add( Duration.FromTicks( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var action = Lambda.Of( () => sut.Generate() );

        action.Test( exc => exc.TestType().Exact<IdentifierGenerationException>() ).Go();
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

        action.Test( exc => exc.TestType().Exact<IdentifierGenerationException>() ).Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
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

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGenerateNextTimeForTheFutureHighValueData ) )]
    public void TryGenerate_ShouldReturnTrueAndCorrectIdentifier_WhenGeneratingNextTimeForTheFutureHighValue(
        Bounds<ushort> lowValueBounds,
        Timestamp futureTimestamp,
        Identifier expected)
    {
        var startTimestamp = Timestamp.Zero;
        var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, startTimestamp, futureTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, new IdentifierGeneratorParams { LowValueBounds = lowValueBounds } );
        GenerateRange( sut, 3 );

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
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
        var timestampProvider = GetTimestampProviderMock( Repeat( startTimestamp, lowValueCount + 2 ).Append( futureTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
    }

    [Fact]
    public void TryGenerate_ShouldReturnTrueAndCorrectFirstIdentifier_WhenNextHighValueIsEqualToMaxHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var expected = new Identifier( ( ulong )maxTimestamp.Subtract( startTimestamp ).FullMilliseconds, 0 );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, maxTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( expected ) )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void TryGenerate_ShouldReturnFalse_WhenNextHighValueIsGreaterThanLastAndMaxHighValueIsExceeded()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = new Timestamp( DateTime.MaxValue ).Add( Duration.FromTicks( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.TryGenerate( out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetGetTimestampData ) )]
    public void GetTimestamp_ShouldReturnCorrectResult(IdentifierGeneratorParams @params, Identifier identifier, Timestamp expected)
    {
        var startTimestamp = @params.BaseTimestamp;
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.GetTimestamp( identifier );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [MethodData( nameof( IdentifierGeneratorTestsData.GetCalculateThroughputData ) )]
    public void CalculateThroughput_ShouldReturnCorrectResult(IdentifierGeneratorParams @params, Duration duration, ulong expected)
    {
        var startTimestamp = @params.BaseTimestamp;
        var timestampProvider = GetTimestampProviderMock( startTimestamp );

        var sut = new IdentifierGenerator( timestampProvider, @params );

        var result = sut.CalculateThroughput( duration );

        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void GeneratorState_ShouldBeUpdatedCorrectly_WhenGeneratingFirstTimeForTheCurrentHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var expected = sut.Generate();

        Assertion.All(
                sut.LastLowValue.TestEquals( expected.Low ),
                sut.LastHighValue.TestEquals( expected.High ),
                sut.LastTimestamp.TestEquals( nextTimestamp ) )
            .Go();
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

        Assertion.All(
                sut.LastLowValue.TestEquals( expected.Low ),
                sut.LastHighValue.TestEquals( expected.High ),
                sut.LastTimestamp.TestEquals( nextTimestamp ) )
            .Go();
    }

    [Fact]
    public void GeneratorState_ShouldBeUpdatedCorrectly_WhenGeneratingNextTimeForTheFutureHighValue()
    {
        var startTimestamp = Timestamp.Zero;
        var nextTimestamp = startTimestamp.Add( Duration.FromMilliseconds( 1 ) );
        var timestampProvider = GetTimestampProviderMock( startTimestamp, startTimestamp, startTimestamp, startTimestamp, nextTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );
        GenerateRange( sut, 3 );

        var expected = sut.Generate();

        Assertion.All(
                sut.LastLowValue.TestEquals( expected.Low ),
                sut.LastHighValue.TestEquals( expected.High ),
                sut.LastTimestamp.TestEquals( nextTimestamp ) )
            .Go();
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
        var timestampProvider = GetTimestampProviderMock( Repeat( startTimestamp, lowValueCount + 2 ).Append( nextTimestamp ).ToArray() );

        var sut = new IdentifierGenerator( timestampProvider, @params );
        GenerateRange( sut, lowValueCount );

        sut.Generate();

        Assertion.All(
                sut.LastLowValue.TestEquals( expectedId.Low ),
                sut.LastHighValue.TestEquals( expectedId.High ),
                sut.LastTimestamp.TestEquals( expectedLastTimestamp ) )
            .Go();
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

        Assertion.All(
                sut.LastLowValue.TestEquals( expected.Low ),
                sut.LastHighValue.TestEquals( expected.High ),
                sut.LastTimestamp.TestEquals( startTimestamp ) )
            .Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( 0 ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( 0UL ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( expected ).Go();
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

        result.TestEquals( 0UL ).Go();
    }

    [Fact]
    public void IsOutOfValues_ShouldReturnFalse_WhenMaxHighValueHasNotBeenExceeded()
    {
        var timestamp = Timestamp.Zero;
        var maxTimestamp = new Timestamp( DateTime.MaxValue ).Subtract( Duration.FromMilliseconds( 1 ).SubtractTicks( 1 ) );
        var timestampProvider = GetTimestampProviderMock( timestamp, maxTimestamp );

        var sut = new IdentifierGenerator( timestampProvider );

        var result = sut.IsOutOfValues;

        result.TestFalse().Go();
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

        result.TestTrue().Go();
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

        result.TestEquals( expected ).Go();
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

        action.Test( exc => exc.TestType().Exact<IdentifierGenerationException>() ).Go();
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

        Assertion.All(
                result.TestEquals( expected ),
                outResult.TestEquals( outExpected ) )
            .Go();
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

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
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
