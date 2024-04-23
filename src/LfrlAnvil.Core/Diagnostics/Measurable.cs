using System;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Diagnostics;

public abstract class Measurable
{
    protected Measurable()
    {
        Measurement = TimeMeasurement.Zero;
        State = MeasurableState.Ready;
    }

    public MeasurableState State { get; private set; }
    public TimeMeasurement Measurement { get; private set; }

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

    protected virtual void Prepare() { }
    protected abstract void Run();
    protected virtual void Teardown() { }
    protected virtual void Done() { }
}
