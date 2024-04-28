using System;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// Represents an operation whose elapsed time can be measured.
/// </summary>
public abstract class Measurable
{
    /// <summary>
    /// Creates a new <see cref="Measurable"/> instance in <see cref="MeasurableState.Ready"/> state.
    /// </summary>
    protected Measurable()
    {
        Measurement = TimeMeasurement.Zero;
        State = MeasurableState.Ready;
    }

    /// <summary>
    /// Current state.
    /// </summary>
    public MeasurableState State { get; private set; }

    /// <summary>
    /// Current time measurement.
    /// </summary>
    public TimeMeasurement Measurement { get; private set; }

    /// <summary>
    /// Invokes this measurable instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// When <see cref="State"/> is not equal to <see cref="MeasurableState.Ready"/>.
    /// </exception>
    public void Invoke()
    {
        if ( State != MeasurableState.Ready )
            throw new InvalidOperationException( ExceptionResources.MeasurableHasAlreadyBeenInvoked );

        State = MeasurableState.Preparing;
        var stopwatch = StopwatchSlim.Create();
        Prepare();
        Measurement = Measurement.SetPreparation( stopwatch.ElapsedTime );

        try
        {
            State = MeasurableState.Running;
            stopwatch = StopwatchSlim.Create();
            Run();
            Measurement = Measurement.SetInvocation( stopwatch.ElapsedTime );
        }
        catch
        {
            Measurement = Measurement.SetInvocation( stopwatch.ElapsedTime );
            throw;
        }
        finally
        {
            State = MeasurableState.TearingDown;
            stopwatch = StopwatchSlim.Create();
            Teardown();
            Measurement = Measurement.SetTeardown( stopwatch.ElapsedTime );
            State = MeasurableState.Done;
            Done();
        }
    }

    /// <summary>
    /// This method is invoked first, before the <see cref="Run()"/> method.
    /// Measurable instance will be in the <see cref="MeasurableState.Preparing"/> state.
    /// Time elapsed during an invocation of this method will be stored in the <see cref="TimeMeasurement.Preparation"/> property.
    /// Does nothing by default.
    /// </summary>
    protected virtual void Prepare() { }

    /// <summary>
    /// This method is invoked after the <see cref="Prepare()"/> method.
    /// Measurable instance will be in the <see cref="MeasurableState.Running"/> state.
    /// Time elapsed during an invocation of this method will be stored in the <see cref="TimeMeasurement.Invocation"/> property.
    /// </summary>
    protected abstract void Run();

    /// <summary>
    /// This method is invoked after the <see cref="Run()"/> method.
    /// Measurable instance will be in the <see cref="MeasurableState.TearingDown"/> state.
    /// Time elapsed during an invocation of this method will be stored in the <see cref="TimeMeasurement.Teardown"/> property.
    /// Does nothing by default.
    /// </summary>
    protected virtual void Teardown() { }

    /// <summary>
    /// This method is invoked last, after the <see cref="Teardown()"/> method.
    /// Measurable instance will be in the <see cref="MeasurableState.Done"/> state.
    /// Does nothing by default.
    /// </summary>
    protected virtual void Done() { }
}
