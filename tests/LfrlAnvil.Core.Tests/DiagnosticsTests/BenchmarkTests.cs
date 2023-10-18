using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class BenchmarkTests : TestsBase
{
    [Theory]
    [InlineData( -1, 1, 1, 1 )]
    [InlineData( 1, -1, 1, 1 )]
    [InlineData( 1, 1, -1, 1 )]
    [InlineData( 1, 1, 1, -1 )]
    public void Ctor_ShouldThrowArgumentOutOfRangeException_WhenAnySampleCountOrStepsPerSampleIsLessThanZero(
        int sampleCount,
        int stepsPerSample,
        int warmupSampleCount,
        int warmupStepsPerSample)
    {
        var action = Lambda.Of(
            () => new BenchmarkMock(
                new BenchmarkOptions(
                    Samples: new BenchmarkSampleOptions( sampleCount, stepsPerSample ),
                    StandardScoreCutoff: 1.5 ),
                new BenchmarkSampleOptions( warmupSampleCount, warmupStepsPerSample ) ) );

        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WarmupOptions_ShouldBeAssignedFromActualOptions_WhenWarmupOptionsAreNull()
    {
        var expected = new BenchmarkSampleOptions( Count: 3, StepsPerSample: 10 );
        var sut = new BenchmarkMock(
            new BenchmarkOptions(
                Samples: expected,
                StandardScoreCutoff: 1.5 ),
            null );

        sut.WarmupOptions.Should().Be( expected );
    }

    [Fact]
    public void GetSampleOptions_ShouldReturnCorrectOptionsForWarmup()
    {
        var expected = new BenchmarkSampleOptions( Count: 2, StepsPerSample: 9, CollectGarbage: false );
        var sut = new BenchmarkMock(
            new BenchmarkOptions(
                Samples: new BenchmarkSampleOptions( Count: 3, StepsPerSample: 10 ),
                StandardScoreCutoff: 1.5 ),
            expected );

        var result = sut.GetSampleOptions( BenchmarkSampleType.Warmup );

        result.Should().Be( expected );
    }

    [Fact]
    public void GetSampleOptions_ShouldReturnCorrectOptionsForActual()
    {
        var expected = new BenchmarkSampleOptions( Count: 2, StepsPerSample: 9, CollectGarbage: false );
        var sut = new BenchmarkMock(
            new BenchmarkOptions(
                Samples: expected,
                StandardScoreCutoff: 1.5 ),
            new BenchmarkSampleOptions( Count: 3, StepsPerSample: 10 ) );

        var result = sut.GetSampleOptions( BenchmarkSampleType.Actual );

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 3, 0 )]
    [InlineData( 0, 10 )]
    public void Run_ShouldDoNothingAndReturnEmptyResult_WhenSampleOptionsAreEmpty(int sampleCount, int stepsPerSample)
    {
        var sut = new BenchmarkMock(
            new BenchmarkOptions(
                Samples: new BenchmarkSampleOptions( sampleCount, stepsPerSample ),
                StandardScoreCutoff: 1.5 ),
            new BenchmarkSampleOptions( Count: 2, StepsPerSample: 9 ) );

        var result = sut.Run();

        using ( new AssertionScope() )
        {
            sut.Events.Should().BeEmpty();
            result.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( 0, 0 )]
    [InlineData( 3, 0 )]
    [InlineData( 0, 10 )]
    public void Run_ShouldSkipWarmup_WhenWarmupSampleOptionsAreEmpty(int sampleCount, int stepsPerSample)
    {
        var sut = new BenchmarkMock(
            new BenchmarkOptions(
                Samples: new BenchmarkSampleOptions( Count: 2, StepsPerSample: 10 ),
                StandardScoreCutoff: 1.5 ),
            new BenchmarkSampleOptions( sampleCount, stepsPerSample ) );

        var result = sut.Run();

        using ( new AssertionScope() )
        {
            sut.Events.Should().HaveCount( 13 );

            new BenchmarkEvent( "OnBeforeAll" )
                .Assert( sut.Events.ElementAtOrDefault( 0 ) );

            new BenchmarkEvent( "CreateState" )
                .Assert( sut.Events.ElementAtOrDefault( 1 ) );

            new BenchmarkEvent( "OnBeforeAll", SampleType: BenchmarkSampleType.Actual )
                .Assert( sut.Events.ElementAtOrDefault( 2 ) );

            var args = new BenchmarkSampleArgs( BenchmarkSampleType.Actual, 0, 2, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 3 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 4 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 5 ) );

            args = new BenchmarkSampleArgs( BenchmarkSampleType.Actual, 1, 2, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 6 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 7 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 8 ) );

            var elapsedTime = sut.Events.ElementAtOrDefault( 5 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 8 )?.ElapsedTime;

            new BenchmarkEvent( "OnAfterAll", SampleType: BenchmarkSampleType.Actual, ElapsedTime: elapsedTime )
                .Assert( sut.Events.ElementAtOrDefault( 9 ) );

            new BenchmarkEvent( "OnBeforeResultExtraction" )
                .Assert( sut.Events.ElementAtOrDefault( 10 ) );

            new BenchmarkEvent( "OnAfterResultExtraction", ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 11 ) );

            elapsedTime = sut.Events.ElementAtOrDefault( 9 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 11 )?.ElapsedTime;

            new BenchmarkEvent( "OnAfterAll", ElapsedTime: elapsedTime )
                .Assert( sut.Events.ElementAtOrDefault( 12 ) );

            result.Should().HaveCount( sut.Options.Samples.StepsPerSample );
        }
    }

    [Fact]
    public void Run_ShouldPerformEventsInCorrectOrderAndReturnCorrectResult()
    {
        var sut = new BenchmarkMock(
            new BenchmarkOptions(
                Samples: new BenchmarkSampleOptions( Count: 3, StepsPerSample: 10 ),
                StandardScoreCutoff: 1.5 ),
            new BenchmarkSampleOptions( Count: 2, StepsPerSample: 10 ) );

        var result = sut.Run();

        using ( new AssertionScope() )
        {
            sut.Events.Should().HaveCount( 24 );

            new BenchmarkEvent( "OnBeforeAll" )
                .Assert( sut.Events.ElementAtOrDefault( 0 ) );

            new BenchmarkEvent( "CreateState" )
                .Assert( sut.Events.ElementAtOrDefault( 1 ) );

            new BenchmarkEvent( "OnBeforeAll", SampleType: BenchmarkSampleType.Warmup )
                .Assert( sut.Events.ElementAtOrDefault( 2 ) );

            var args = new BenchmarkSampleArgs( BenchmarkSampleType.Warmup, 0, 2, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 3 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 4 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 5 ) );

            args = new BenchmarkSampleArgs( BenchmarkSampleType.Warmup, 1, 2, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 6 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 7 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 8 ) );

            var elapsedTime = sut.Events.ElementAtOrDefault( 5 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 8 )?.ElapsedTime;

            new BenchmarkEvent( "OnAfterAll", SampleType: BenchmarkSampleType.Warmup, ElapsedTime: elapsedTime )
                .Assert( sut.Events.ElementAtOrDefault( 9 ) );

            new BenchmarkEvent( "OnBeforeAll", SampleType: BenchmarkSampleType.Actual )
                .Assert( sut.Events.ElementAtOrDefault( 10 ) );

            args = new BenchmarkSampleArgs( BenchmarkSampleType.Actual, 0, 3, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 11 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 12 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 13 ) );

            args = new BenchmarkSampleArgs( BenchmarkSampleType.Actual, 1, 3, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 14 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 15 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 16 ) );

            args = new BenchmarkSampleArgs( BenchmarkSampleType.Actual, 2, 3, 10 );
            new BenchmarkEvent( "OnBeforeSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 17 ) );

            new BenchmarkEvent( "OnSample", State: sut.State, Args: args )
                .Assert( sut.Events.ElementAtOrDefault( 18 ) );

            new BenchmarkEvent( "OnAfterSample", State: sut.State, Args: args, ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 19 ) );

            elapsedTime = sut.Events.ElementAtOrDefault( 13 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 16 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 19 )?.ElapsedTime;

            new BenchmarkEvent( "OnAfterAll", SampleType: BenchmarkSampleType.Actual, ElapsedTime: elapsedTime )
                .Assert( sut.Events.ElementAtOrDefault( 20 ) );

            new BenchmarkEvent( "OnBeforeResultExtraction" )
                .Assert( sut.Events.ElementAtOrDefault( 21 ) );

            new BenchmarkEvent( "OnAfterResultExtraction", ElapsedTime: TimeSpan.Zero )
                .Assert( sut.Events.ElementAtOrDefault( 22 ) );

            elapsedTime = sut.Events.ElementAtOrDefault( 9 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 20 )?.ElapsedTime +
                sut.Events.ElementAtOrDefault( 22 )?.ElapsedTime;

            new BenchmarkEvent( "OnAfterAll", ElapsedTime: elapsedTime )
                .Assert( sut.Events.ElementAtOrDefault( 23 ) );

            result.Should().HaveCount( sut.Options.Samples.StepsPerSample );

            var objBytes = result.ElementAtOrDefault( 0 ).AllocatedBytes.Mean.Bytes;
            var expectedBytes = Enumerable.Range( 1, sut.Options.Samples.StepsPerSample ).Select( i => objBytes * i ).ToArray();

            result.Select( r => r.AllocatedBytes.Variance ).Should().AllBeEquivalentTo( MemorySize.Zero );
            result.Select( r => r.AllocatedBytes.StandardDeviation ).Should().AllBeEquivalentTo( MemorySize.Zero );
            result.Select( r => r.AllocatedBytes.StandardError ).Should().AllBeEquivalentTo( MemorySize.Zero );
            result.Select( r => r.AllocatedBytes.Min ).Should().BeSequentiallyEqualTo( result.Select( r => r.AllocatedBytes.Max ) );
            result.Select( r => r.AllocatedBytes.Mean ).Should().BeSequentiallyEqualTo( result.Select( r => r.AllocatedBytes.Max ) );
            result.Select( r => r.AllocatedBytes.Mean.Bytes ).Should().BeSequentiallyEqualTo( expectedBytes );

            for ( var i = 0; i < result.Length; ++i )
            {
                result[i].ElapsedTimeOutliers.Should().BeInRange( 0, sut.Options.Samples.Count );
                result[i].ElapsedTime.Min.Should().BeGreaterOrEqualTo( TimeSpan.Zero );
                result[i].ElapsedTime.Max.Should().BeGreaterOrEqualTo( result[i].ElapsedTime.Min );

                result[i]
                    .ElapsedTime.Mean.Should()
                    .BeGreaterOrEqualTo( result[i].ElapsedTime.Min )
                    .And.BeLessOrEqualTo( result[i].ElapsedTime.Max );

                result[i].ElapsedTime.Variance.Should().BeGreaterOrEqualTo( result[i].ElapsedTime.StandardDeviation );
                result[i].ElapsedTime.StandardDeviation.Should().BeGreaterOrEqualTo( TimeSpan.Zero );
                result[i].ElapsedTime.StandardError.Should().BeGreaterOrEqualTo( TimeSpan.Zero );

                result[i]
                    .ElapsedTimeWithOutliers.Min.Should()
                    .BeGreaterOrEqualTo( TimeSpan.Zero )
                    .And.BeLessOrEqualTo( result[i].ElapsedTime.Min );

                result[i].ElapsedTimeWithOutliers.Max.Should().BeGreaterOrEqualTo( result[i].ElapsedTime.Max );

                result[i]
                    .ElapsedTimeWithOutliers.Mean.Should()
                    .BeGreaterOrEqualTo( result[i].ElapsedTimeWithOutliers.Min )
                    .And.BeLessOrEqualTo( result[i].ElapsedTimeWithOutliers.Max );

                result[i].ElapsedTimeWithOutliers.Variance.Should().BeGreaterOrEqualTo( result[i].ElapsedTime.Variance );
                result[i].ElapsedTimeWithOutliers.StandardDeviation.Should().BeGreaterOrEqualTo( result[i].ElapsedTime.StandardDeviation );
                result[i].ElapsedTimeWithOutliers.StandardError.Should().BeGreaterOrEqualTo( result[i].ElapsedTime.StandardError );
            }
        }
    }

    private sealed record BenchmarkEvent(
        string Title,
        object? State = null,
        BenchmarkSampleType? SampleType = null,
        BenchmarkOptions? Options = null,
        BenchmarkSampleArgs? Args = null,
        TimeSpan? ElapsedTime = null)
    {
        public void Assert(BenchmarkEvent? actual)
        {
            actual.Should().NotBeNull();
            actual?.Title.Should().Be( Title );
            actual?.State.Should().BeSameAs( State );
            actual?.SampleType.Should().Be( SampleType );
            actual?.Options.Should().Be( Options );
            actual?.Args.Should().Be( Args );

            if ( ElapsedTime is null )
                actual?.ElapsedTime.Should().BeNull();
            else
                actual?.ElapsedTime.Should().BeGreaterOrEqualTo( ElapsedTime.Value );
        }
    }

    private sealed class BenchmarkMock : Benchmark<object>
    {
        public readonly object State = new object();
        public readonly List<BenchmarkEvent> Events = new List<BenchmarkEvent>();

        public BenchmarkMock(BenchmarkOptions options, BenchmarkSampleOptions? warmupOptions)
            : base( options, warmupOptions ) { }

        [Pure]
        protected override object CreateState()
        {
            Events.Add( new BenchmarkEvent( nameof( CreateState ) ) );
            return State;
        }

        protected override void OnSample(object state, BenchmarkSampleArgs args, ref BenchmarkStatisticsCollection statistics)
        {
            Events.Add( new BenchmarkEvent( nameof( OnSample ), State: state, Args: args ) );

            for ( var i = 0; i < args.Steps; ++i )
            {
                var count = i + 1;
                var start = GetStartInfo();
                DoWork( count );
                statistics.Add( start );
            }
        }

        protected override void OnBeforeAll(BenchmarkSampleType? sampleType)
        {
            base.OnBeforeAll( sampleType );
            Events.Add( new BenchmarkEvent( nameof( OnBeforeAll ), SampleType: sampleType ) );
        }

        protected override void OnAfterAll(BenchmarkSampleType? sampleType, TimeSpan elapsedTime)
        {
            base.OnAfterAll( sampleType, elapsedTime );
            Events.Add( new BenchmarkEvent( nameof( OnAfterAll ), SampleType: sampleType, ElapsedTime: elapsedTime ) );
        }

        protected override void OnBeforeSample(object state, BenchmarkSampleArgs args)
        {
            base.OnBeforeSample( state, args );
            Events.Add( new BenchmarkEvent( nameof( OnBeforeSample ), State: state, Args: args ) );
        }

        protected override void OnAfterSample(object state, BenchmarkSampleArgs args, TimeSpan elapsedTime)
        {
            base.OnAfterSample( state, args, elapsedTime );
            Events.Add( new BenchmarkEvent( nameof( OnAfterSample ), State: state, Args: args, ElapsedTime: elapsedTime ) );
        }

        protected override void OnBeforeResultExtraction()
        {
            base.OnBeforeResultExtraction();
            Events.Add( new BenchmarkEvent( nameof( OnBeforeResultExtraction ) ) );
        }

        protected override void OnAfterResultExtraction(TimeSpan elapsedTime)
        {
            base.OnAfterResultExtraction( elapsedTime );
            Events.Add( new BenchmarkEvent( nameof( OnAfterResultExtraction ), ElapsedTime: elapsedTime ) );
        }

        [MethodImpl( MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization )]
        private static void DoWork(int count)
        {
            for ( var i = 0; i < count; ++i )
                _ = new object();
        }
    }
}
