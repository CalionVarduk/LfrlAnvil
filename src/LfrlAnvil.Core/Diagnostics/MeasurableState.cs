namespace LfrlAnvil.Diagnostics;

/// <summary>
/// Represents the state of a <see cref="Measurable"/> instance.
/// </summary>
public enum MeasurableState : byte
{
    /// <summary>
    /// Specifies that a measurable has not yet been invoked.
    /// </summary>
    Ready = 0,

    /// <summary>
    /// Specifies that a measurable has been invoked and is in its preparation stage.
    /// </summary>
    Preparing = 1,

    /// <summary>
    /// Specifies that a measurable has been invoked and is in its main state.
    /// </summary>
    Running = 2,

    /// <summary>
    /// Specifies that a measurable has been invoked and is in its teardown stage.
    /// </summary>
    TearingDown = 3,

    /// <summary>
    /// Specifies that a measurable instance invocation has finished.
    /// </summary>
    Done = 4
}
