namespace LfrlAnvil.Reactive.State;

public interface IVariable<TValue, TValidationResult> : IReadOnlyVariable<TValue, TValidationResult>
{
    VariableChangeResult TryChange(TValue value);
    VariableChangeResult Change(TValue value);
    void Refresh();
    void RefreshValidation();
    void ClearValidation();
}
