using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public static class CollectionVariableRoot
{
    public static class WithoutValidators<TValidationResult>
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null)
            where TKey : notnull
            where TElement : VariableNode
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
            return CollectionVariableRoot.Create( initialElements, keySelector, keyComparer, validator, validator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            CollectionVariableRootChanges<TKey, TElement> elementChanges,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null)
            where TKey : notnull
            where TElement : VariableNode
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
            return CollectionVariableRoot.Create( initialElements, elementChanges, keySelector, keyComparer, validator, validator );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? warningsValidator = null)
        where TKey : notnull
        where TElement : VariableNode
    {
        return new CollectionVariableRoot<TKey, TElement, TValidationResult>(
            initialElements,
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        CollectionVariableRootChanges<TKey, TElement> elementChanges,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? warningsValidator = null)
        where TKey : notnull
        where TElement : VariableNode
    {
        return new CollectionVariableRoot<TKey, TElement, TValidationResult>(
            initialElements,
            elementChanges,
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );
    }
}
