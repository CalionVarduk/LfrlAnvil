using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public static class CollectionVariable
{
    public static class WithoutValidators<TValidationResult>
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TElement>? elementComparer = null)
            where TKey : notnull
            where TElement : notnull
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();
            var elementValidator = Validators<TValidationResult>.Pass<TElement>();
            return CollectionVariable.Create(
                initialElements,
                keySelector,
                keyComparer,
                elementComparer,
                validator,
                validator,
                elementValidator,
                elementValidator );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            IEnumerable<TElement> elements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TElement>? elementComparer = null)
            where TKey : notnull
            where TElement : notnull
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();
            var elementValidator = Validators<TValidationResult>.Pass<TElement>();
            return CollectionVariable.Create(
                initialElements,
                elements,
                keySelector,
                keyComparer,
                elementComparer,
                validator,
                validator,
                elementValidator,
                elementValidator );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TElement>? elementComparer = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? warningsValidator = null,
        IValidator<TElement, TValidationResult>? elementErrorsValidator = null,
        IValidator<TElement, TValidationResult>? elementWarningsValidator = null)
        where TKey : notnull
        where TElement : notnull
    {
        return new CollectionVariable<TKey, TElement, TValidationResult>(
            initialElements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        IEnumerable<TElement> elements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TElement>? elementComparer = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? warningsValidator = null,
        IValidator<TElement, TValidationResult>? elementErrorsValidator = null,
        IValidator<TElement, TValidationResult>? elementWarningsValidator = null)
        where TKey : notnull
        where TElement : notnull
    {
        return new CollectionVariable<TKey, TElement, TValidationResult>(
            initialElements,
            elements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );
    }
}
