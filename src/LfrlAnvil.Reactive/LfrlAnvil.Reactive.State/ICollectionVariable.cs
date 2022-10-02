using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State;

public interface ICollectionVariable<TKey, TElement, TValidationResult> : IReadOnlyCollectionVariable<TKey, TElement, TValidationResult>
    where TKey : notnull
    where TElement : notnull
{
    VariableChangeResult TryChange(IEnumerable<TElement> elements);
    VariableChangeResult Change(IEnumerable<TElement> elements);
    VariableChangeResult Add(TElement element);
    VariableChangeResult Add(IEnumerable<TElement> elements);
    VariableChangeResult TryReplace(TElement element);
    VariableChangeResult TryReplace(IEnumerable<TElement> elements);
    VariableChangeResult Replace(TElement element);
    VariableChangeResult Replace(IEnumerable<TElement> elements);
    VariableChangeResult AddOrTryReplace(TElement element);
    VariableChangeResult AddOrTryReplace(IEnumerable<TElement> elements);
    VariableChangeResult AddOrReplace(TElement element);
    VariableChangeResult AddOrReplace(IEnumerable<TElement> elements);
    VariableChangeResult Remove(TKey key);
    VariableChangeResult Remove(IEnumerable<TKey> keys);
    VariableChangeResult Clear();
    void Refresh();
    void Refresh(TKey key);
    void Refresh(IEnumerable<TKey> keys);
    void RefreshValidation();
    void RefreshValidation(TKey key);
    void RefreshValidation(IEnumerable<TKey> keys);
    void ClearValidation();
    void ClearValidation(TKey key);
    void ClearValidation(IEnumerable<TKey> keys);
}
