namespace LfrlAnvil.Diagnostics;

public enum MeasurableState : byte
{
    Ready = 0,
    Preparing = 1,
    Running = 2,
    TearingDown = 3,
    Done = 4
}
