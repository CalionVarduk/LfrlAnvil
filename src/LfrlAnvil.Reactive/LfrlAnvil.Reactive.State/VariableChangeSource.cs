namespace LfrlAnvil.Reactive.State;

public enum VariableChangeSource : byte
{
    Change = 0,
    TryChange = 1,
    Refresh = 2,
    Reset = 3,
    SetReadOnly = 4,
    ChildNode = 5
}
