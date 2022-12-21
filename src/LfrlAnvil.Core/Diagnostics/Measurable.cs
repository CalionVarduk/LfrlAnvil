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

        State = MeasurableState.Preparing;
        var start = Stopwatch.GetTimestamp();
        Prepare();
        var end = Stopwatch.GetTimestamp();
        Measurement = new TimeMeasurement( TimeSpan.FromTicks( end - start ), Measurement.Invocation, Measurement.Teardown );

        try
        {
            State = MeasurableState.Running;
            start = Stopwatch.GetTimestamp();
            Run();
            end = Stopwatch.GetTimestamp();
            Measurement = new TimeMeasurement( Measurement.Preparation, TimeSpan.FromTicks( end - start ), Measurement.Teardown );
        }
        catch
        {
            end = Stopwatch.GetTimestamp();
            Measurement = new TimeMeasurement( Measurement.Preparation, TimeSpan.FromTicks( end - start ), Measurement.Teardown );
            throw;
        }
        finally
        {
            State = MeasurableState.TearingDown;
            start = Stopwatch.GetTimestamp();
            Teardown();
            end = Stopwatch.GetTimestamp();
            Measurement = new TimeMeasurement( Measurement.Preparation, Measurement.Invocation, TimeSpan.FromTicks( end - start ) );

            State = MeasurableState.Done;
            Done();
        }
    }

    protected virtual void Prepare() { }
    protected abstract void Run();
    protected virtual void Teardown() { }
    protected virtual void Done() { }
}
