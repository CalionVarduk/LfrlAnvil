using System;
using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Events;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased read-only collection variable of <see cref="IVariableNode"/> elements
/// that listens to its children's events and propagates them.
/// </summary>
public interface IReadOnlyCollectionVariableRoot : IVariableNode
{
    /// <summary>
    /// Key type.
    /// </summary>
    Type KeyType { get; }

    /// <summary>
    /// Element type.
    /// </summary>
    Type ElementType { get; }

    /// <summary>
    /// Validation result type.
    /// </summary>
    Type ValidationResultType { get; }

    /// <summary>
    /// Current collection of elements.
    /// </summary>
    ICollectionVariableRootElements Elements { get; }

    /// <summary>
    /// Initial collection of elements.
    /// </summary>
    IEnumerable InitialElements { get; }

    /// <summary>
    /// Collection of current validation errors.
    /// </summary>
    IEnumerable Errors { get; }

    /// <summary>
    /// Collection of current validation warnings.
    /// </summary>
    IEnumerable Warnings { get; }

    /// <summary>
    /// Event stream that emits events when variable's elements change.
    /// </summary>
    new IEventStream<ICollectionVariableRootChangeEvent> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<ICollectionVariableRootValidationEvent> OnValidate { get; }
}

/// <summary>
/// Represents a generic read-only collection variable of <see cref="VariableNode"/> elements
/// that listens to its children's events and propagates them.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public interface IReadOnlyCollectionVariableRoot<TKey, TElement> : IReadOnlyCollectionVariableRoot
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Element's key selector.
    /// </summary>
    Func<TElement, TKey> KeySelector { get; }

    /// <summary>
    /// Current collection of elements.
    /// </summary>
    new ICollectionVariableRootElements<TKey, TElement> Elements { get; }

    /// <summary>
    /// Initial collection of elements.
    /// </summary>
    new IReadOnlyDictionary<TKey, TElement> InitialElements { get; }

    /// <summary>
    /// Event stream that emits events when variable's elements change.
    /// </summary>
    new IEventStream<ICollectionVariableRootChangeEvent<TKey, TElement>> OnChange { get; }
}

/// <summary>
/// Represents a generic read-only collection variable of <see cref="VariableNode"/> elements
/// that listens to its children's events and propagates them.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface IReadOnlyCollectionVariableRoot<TKey, TElement, TValidationResult> : IReadOnlyCollectionVariableRoot<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Collection of current validation errors.
    /// </summary>
    new Chain<TValidationResult> Errors { get; }

    /// <summary>
    /// Collection of current validation warnings.
    /// </summary>
    new Chain<TValidationResult> Warnings { get; }

    /// <summary>
    /// Collection of elements validator that marks result as errors.
    /// </summary>
    IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult> ErrorsValidator { get; }

    /// <summary>
    /// Collection of elements validator that marks result as warnings.
    /// </summary>
    IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult> WarningsValidator { get; }

    /// <summary>
    /// Event stream that emits events when variable's elements change.
    /// </summary>
    new IEventStream<CollectionVariableRootChangeEvent<TKey, TElement, TValidationResult>> OnChange { get; }

    /// <summary>
    /// Event stream that emits events when variable's validation state changes.
    /// </summary>
    new IEventStream<CollectionVariableRootValidationEvent<TKey, TElement, TValidationResult>> OnValidate { get; }
}
