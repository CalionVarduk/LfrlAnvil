using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Extensions;

/// <summary>
/// Contains <see cref="IValidator{T,TResult}"/> extension methods.
/// </summary>
public static class ValidatorExtensions
{
    /// <summary>
    /// Creates a new <see cref="ForEachValidator{T,TElementResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying element validator.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <typeparam name="TResult">Element result type.</typeparam>
    /// <returns>New <see cref="ForEachValidator{T,TElementResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ElementValidatorResult<T, TResult>> ForCollectionElement<T, TResult>(
        this IValidator<T, TResult> validator)
    {
        return new ForEachValidator<T, TResult>( validator );
    }

    /// <summary>
    /// Creates a new <see cref="MemberValidator{T,TMember,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="memberSelector">Member selector.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TTarget">Object's member type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="MemberValidator{T,TMember,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<TTarget, TResult> ForMember<T, TTarget, TResult>(
        this IValidator<T, TResult> validator,
        Func<TTarget, T> memberSelector)
    {
        return new MemberValidator<TTarget, T, TResult>( validator, memberSelector );
    }

    /// <summary>
    /// Creates a new <see cref="MappedValidator{T,TSourceResult,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="resultMapper">Underlying validator's result mapper.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TSourceResult">Underlying validator's result type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="MappedValidator{T,TSourceResult,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Map<T, TSourceResult, TResult>(
        this IValidator<T, TSourceResult> validator,
        Func<Chain<TSourceResult>, Chain<TResult>> resultMapper)
    {
        return new MappedValidator<T, TSourceResult, TResult>( validator, resultMapper );
    }

    /// <summary>
    /// Creates a new <see cref="FormattedValidator{T,TResource}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="formatter">Underlying validation message formatter.</param>
    /// <param name="formatProvider">Optional format provider factory.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <typeparam name="TResource">Message's resource type.</typeparam>
    /// <returns>New <see cref="FormattedValidator{T,TResource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, FormattedValidatorResult<TResource>> Format<T, TResource>(
        this IValidator<T, ValidationMessage<TResource>> validator,
        IValidationMessageFormatter<TResource> formatter,
        Func<IFormatProvider?>? formatProvider = null)
    {
        return new FormattedValidator<T, TResource>( validator, formatter, formatProvider );
    }
}
