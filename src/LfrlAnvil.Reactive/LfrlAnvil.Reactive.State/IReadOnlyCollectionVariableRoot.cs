using System;
using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public interface IReadOnlyCollectionVariableRoot : IVariableNode
{
    Type KeyType { get; }
    Type ElementType { get; }
    Type ValidationResultType { get; }
    ICollectionVariableRootElements Elements { get; }
    IEnumerable InitialElements { get; }
    IEnumerable Errors { get; }
    IEnumerable Warnings { get; }
    new IEventStream<ICollectionVariableRootChangeEvent> OnChange { get; }
    new IEventStream<ICollectionVariableRootValidationEvent> OnValidate { get; }
}

public interface IReadOnlyCollectionVariableRoot<TKey, TElement> : IReadOnlyCollectionVariableRoot
    where TKey : notnull
    where TElement : VariableNode
{
    Func<TElement, TKey> KeySelector { get; }
    new ICollectionVariableRootElements<TKey, TElement> Elements { get; }
    new IReadOnlyDictionary<TKey, TElement> InitialElements { get; }
    new IEventStream<ICollectionVariableRootChangeEvent<TKey, TElement>> OnChange { get; }
}

public interface IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> : IReadOnlyCollectionVariableRoot<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    new Chain<TValidationResult> Errors { get; }
    new Chain<TValidationResult> Warnings { get; }
    IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult> ErrorsValidator { get; }
    IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult> WarningsValidator { get; }
    new IEventStream<CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>> OnChange { get; }
    new IEventStream<CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>> OnValidate { get; }
}
