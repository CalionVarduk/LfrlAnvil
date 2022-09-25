namespace LfrlAnvil.Reactive.State.Internal;

public interface IMutableVariableNode : IVariableNode
{
    void Refresh();
    void RefreshValidation();
    void ClearValidation();
    void SetReadOnly(bool enabled);
}
