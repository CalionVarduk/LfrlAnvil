using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

public interface ICollectionVariableRoot<TKey, TElement, TValidationResult>
    : IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult>
    where TKey : notnull
    where TElement : VariableNode
{
    VariableChangeResult Change(CollectionVariableRootChanges<TKey, TElement> changes);
    VariableChangeResult Add(TElement element);
    VariableChangeResult Add(IEnumerable<TElement> elements);
    VariableChangeResult Restore(TKey key);
    VariableChangeResult Restore(IEnumerable<TKey> keys);
    VariableChangeResult Remove(TKey key);
    VariableChangeResult Remove(IEnumerable<TKey> keys);
    VariableChangeResult Clear();
    void Refresh();
    void RefreshValidation();
    void ClearValidation();
}
