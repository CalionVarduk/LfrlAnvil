using System;
using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public interface IReadOnlyCollectionVariable : IVariableNode
{
    Type KeyType { get; }
    Type ElementType { get; }
    Type ValidationResultType { get; }
    ICollectionVariableElements Elements { get; }
    IEnumerable InitialElements { get; }
    IEnumerable Errors { get; }
    IEnumerable Warnings { get; }
    new IEventStream<ICollectionVariableChangeEvent> OnChange { get; }
    new IEventStream<ICollectionVariableValidationEvent> OnValidate { get; }
}

public interface IReadOnlyCollectionVariable<TKey, TElement> : IReadOnlyCollectionVariable
    where TKey : notnull
    where TElement : notnull
{
    Func<TElement, TKey> KeySelector { get; }
    new ICollectionVariableElements<TKey, TElement> Elements { get; }
    new IReadOnlyDictionary<TKey, TElement> InitialElements { get; }
    new IEventStream<ICollectionVariableChangeEvent<TKey, TElement>> OnChange { get; }
}

public interface IReadOnlyCollectionVariable<TKey, TElement, TValidationResult> : IReadOnlyCollectionVariable<TKey, TElement>
    where TKey : notnull
    where TElement : notnull
{
    new Chain<TValidationResult> Errors { get; }
    new Chain<TValidationResult> Warnings { get; }
    new ICollectionVariableElements<TKey, TElement, TValidationResult> Elements { get; }
    IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult> ErrorsValidator { get; }
    IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult> WarningsValidator { get; }
    new IEventStream<CollectionVariableChangeEvent<TKey, TElement, TValidationResult>> OnChange { get; }
    new IEventStream<CollectionVariableValidationEvent<TKey, TElement, TValidationResult>> OnValidate { get; }
}
