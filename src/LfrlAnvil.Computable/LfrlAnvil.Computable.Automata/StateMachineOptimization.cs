namespace LfrlAnvil.Computable.Automata;

public enum StateMachineOptimization : byte
{
    None = 0,
    RemoveUnreachableStates = 1,
    Minimize = 2
}
