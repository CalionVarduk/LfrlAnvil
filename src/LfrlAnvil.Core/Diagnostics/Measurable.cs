using System;
using System.Diagnostics;
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

        var stopwatch = new Stopwatch();

        try
        {
            State = MeasurableState.Preparing;
            stopwatch.Start();
            Prepare();
            stopwatch.Stop();
            Measurement = new TimeMeasurement( stopwatch.Elapsed, Measurement.Invocation, Measurement.Teardown );

            State = MeasurableState.Running;
            stopwatch.Restart();
            Run();
            stopwatch.Stop();
            Measurement = new TimeMeasurement( Measurement.Preparation, stopwatch.Elapsed, Measurement.Teardown );
        }
        finally
        {
            State = MeasurableState.TearingDown;
            stopwatch.Restart();
            Teardown();
            stopwatch.Stop();
            Measurement = new TimeMeasurement( Measurement.Preparation, Measurement.Invocation, stopwatch.Elapsed );

            State = MeasurableState.Done;
            Done();
        }
    }

    protected virtual void Prepare() { }
    protected abstract void Run();
    protected virtual void Teardown() { }
    protected virtual void Done() { }
}
