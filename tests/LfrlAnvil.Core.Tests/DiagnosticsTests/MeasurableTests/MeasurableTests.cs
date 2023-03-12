using System.Collections.Generic;
using System.Threading;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.DiagnosticsTests.MeasurableTests;

public class MeasurableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldStartAsReady()
    {
        var sut = new MeasurableMock();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( MeasurableState.Ready );
            sut.Measurement.Should().BeEquivalentTo( TimeMeasurement.Zero );
        }
    }

    [Fact]
    public void Invoke_ShouldPerformActionsInCorrectOrderWithCorrectInformation()
    {
        var sut = new MeasurableMock();

        sut.Invoke();

        using ( new AssertionScope() )
        {
            sut.State.Should().Be( MeasurableState.Done );
            sut.Measurement.Should().NotBeEquivalentTo( TimeMeasurement.Zero );
            sut.Actions.Should()
                .BeEquivalentTo(
                    ("Prepare", MeasurableState.Preparing, TimeMeasurement.Zero),
                    ("Run", MeasurableState.Running, new TimeMeasurement( sut.Measurement.Preparation, TimeSpan.Zero, TimeSpan.Zero )),
                    ("Teardown", MeasurableState.TearingDown,
                     new TimeMeasurement( sut.Measurement.Preparation, sut.Measurement.Invocation, TimeSpan.Zero )),
                    ("Done", MeasurableState.Done, sut.Measurement) );
        }
    }

    [Fact]
    public void Invoke_ShouldPerformActionsInCorrectOrderWithCorrectInformation_EvenWhenRunImplementationThrows()
    {
        var sut = new MeasurableThrowingMock();

        var action = Lambda.Of( () => sut.Invoke() );

        using ( new AssertionScope() )
        {
            action.Should().Throw<Exception>();
            sut.State.Should().Be( MeasurableState.Done );
            sut.Measurement.Should().NotBeEquivalentTo( TimeMeasurement.Zero );
            sut.Actions.Should()
                .BeEquivalentTo(
                    ("Prepare", MeasurableState.Preparing, TimeMeasurement.Zero),
                    ("Run", MeasurableState.Running, new TimeMeasurement( sut.Measurement.Preparation, TimeSpan.Zero, TimeSpan.Zero )),
                    ("Teardown", MeasurableState.TearingDown,
                     new TimeMeasurement( sut.Measurement.Preparation, sut.Measurement.Invocation, TimeSpan.Zero )),
                    ("Done", MeasurableState.Done, sut.Measurement) );
        }
    }

    [Fact]
    public void Invoke_ShouldThrowInvalidOperationException_WhenMeasurableHasAlreadyBeenInvoked()
    {
        var sut = new MeasurableMock();
        sut.Invoke();

        var action = Lambda.Of( () => sut.Invoke() );

        action.Should().ThrowExactly<InvalidOperationException>();
    }
}

public class MeasurableMock : Measurable
{
    public readonly List<(string Name, MeasurableState State, TimeMeasurement Measurement)> Actions =
        new List<(string, MeasurableState, TimeMeasurement)>();

    protected sealed override void Prepare()
    {
        base.Prepare();
        Actions.Add( (nameof( Prepare ), State, Measurement) );
        Thread.Sleep( 1 );
    }

    protected override void Run()
    {
        Actions.Add( (nameof( Run ), State, Measurement) );
        Thread.Sleep( 1 );
    }

    protected sealed override void Teardown()
    {
        base.Teardown();
        Actions.Add( (nameof( Teardown ), State, Measurement) );
        Thread.Sleep( 1 );
    }

    protected sealed override void Done()
    {
        base.Done();
        Actions.Add( (nameof( Done ), State, Measurement) );
    }
}

public sealed class MeasurableThrowingMock : MeasurableMock
{
    protected override void Run()
    {
        base.Run();
        throw new Exception();
    }
}
