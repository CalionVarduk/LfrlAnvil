namespace LfrlAnvil.Reactive.State;

public interface IVariableRoot<TKey> : IReadOnlyVariableRoot<TKey>
    where TKey : notnull
{
    void Refresh();
    void RefreshValidation();
    void ClearValidation();
}
