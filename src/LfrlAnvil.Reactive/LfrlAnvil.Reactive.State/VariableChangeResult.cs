namespace LfrlAnvil.Reactive.State;

public enum VariableChangeResult : byte
{
    Changed = 0,
    NotChanged = 1,
    ReadOnly = 2
}
