namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents available levels of state machine optimizations.
/// </summary>
public enum StateMachineOptimization : byte
{
    /// <summary>
    /// Specifies that no optimization should take place.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies that all unreachable states should be removed.
    /// </summary>
    RemoveUnreachableStates = 1,

    /// <summary>
    /// Specifies that all unreachable states should be removed and all equivalent states should be merged together.
    /// </summary>
    Minimize = 2
}
