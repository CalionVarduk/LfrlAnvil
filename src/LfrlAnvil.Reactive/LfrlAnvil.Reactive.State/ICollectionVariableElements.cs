using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public interface ICollectionVariableElements : IEnumerable
{
    int Count { get; }
    IEnumerable Keys { get; }
    IEnumerable Values { get; }
    IEnumerable InvalidElementKeys { get; }
    IEnumerable WarningElementKeys { get; }
    IEnumerable ModifiedElementKeys { get; }
}

public interface ICollectionVariableElements<TKey, TElement> : ICollectionVariableElements, IReadOnlyDictionary<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
    new int Count { get; }
    new IReadOnlyCollection<TKey> Keys { get; }
    new IReadOnlyCollection<TElement> Values { get; }
    new IReadOnlySet<TKey> InvalidElementKeys { get; }
    new IReadOnlySet<TKey> WarningElementKeys { get; }
    new IReadOnlySet<TKey> ModifiedElementKeys { get; }
    IEqualityComparer<TKey> KeyComparer { get; }
    IEqualityComparer<TElement> ElementComparer { get; }

    [Pure]
    IEnumerable GetErrors(TKey key);

    [Pure]
    IEnumerable GetWarnings(TKey key);

    [Pure]
    CollectionVariableElementState GetState(TKey key);
}

public interface ICollectionVariableElements<TKey, TElement, TValidationResult> : ICollectionVariableElements<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
    IValidator<TElement, TValidationResult> ErrorsValidator { get; }
    IValidator<TElement, TValidationResult> WarningsValidator { get; }

    [Pure]
    new Chain<TValidationResult> GetErrors(TKey key);

    [Pure]
    new Chain<TValidationResult> GetWarnings(TKey key);
}
