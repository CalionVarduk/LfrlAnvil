using System.Collections.Generic;
using System.Threading;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.DiagnosticsTests;

public class MeasurableTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldStartAsReady()
    {
        var sut = new MeasurableMock();

        Assertion.All(
                sut.State.TestEquals( MeasurableState.Ready ),
                sut.Measurement.TestEquals( TimeMeasurement.Zero ) )
            .Go();
    }

    [Fact]
    public void Invoke_ShouldPerformActionsInCorrectOrderWithCorrectInformation()
    {
        var sut = new MeasurableMock();

        sut.Invoke();

        Assertion.All(
                sut.State.TestEquals( MeasurableState.Done ),
                sut.Measurement.TestNotEquals( TimeMeasurement.Zero ),
                sut.Actions.TestSequence(
                [
                    ("Prepare", MeasurableState.Preparing, TimeMeasurement.Zero),
                    ("Run", MeasurableState.Running, new TimeMeasurement( sut.Measurement.Preparation, TimeSpan.Zero, TimeSpan.Zero )),
                    ("Teardown", MeasurableState.TearingDown,
                        new TimeMeasurement( sut.Measurement.Preparation, sut.Measurement.Invocation, TimeSpan.Zero )),
                    ("Done", MeasurableState.Done, sut.Measurement)
                ] ) )
            .Go();
    }

    [Fact]
    public void Invoke_ShouldPerformActionsInCorrectOrderWithCorrectInformation_EvenWhenRunImplementationThrows()
    {
        var sut = new MeasurableThrowingMock();

        var action = Lambda.Of( () => sut.Invoke() );

        action.Test( exc => Assertion.All(
                exc.TestNotNull(),
                sut.State.TestEquals( MeasurableState.Done ),
                sut.Measurement.TestNotEquals( TimeMeasurement.Zero ),
                sut.Actions.TestSequence(
                [
                    ("Prepare", MeasurableState.Preparing, TimeMeasurement.Zero),
                    ("Run", MeasurableState.Running, new TimeMeasurement( sut.Measurement.Preparation, TimeSpan.Zero, TimeSpan.Zero )),
                    ("Teardown", MeasurableState.TearingDown,
                        new TimeMeasurement( sut.Measurement.Preparation, sut.Measurement.Invocation, TimeSpan.Zero )),
                    ("Done", MeasurableState.Done, sut.Measurement)
                ] ) ) )
            .Go();
    }

    [Fact]
    public void Invoke_ShouldThrowInvalidOperationException_WhenMeasurableHasAlreadyBeenInvoked()
    {
        var sut = new MeasurableMock();
        sut.Invoke();

        var action = Lambda.Of( () => sut.Invoke() );

        action.Test( exc => exc.TestType().Exact<InvalidOperationException>() ).Go();
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
